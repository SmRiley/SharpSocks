using System.Net;
using System.Net.Sockets;

namespace Server.Core;

class TcpListen
{
    private const int buffSize = 1024 * 15;
    private const int timeout = 1000 * 5;
    private readonly byte[] _dataBuff = new byte[buffSize];
    private readonly TcpListener _tcpListener;
    private readonly UdpListen? udpListen;

    public TcpListen(IPAddress ip, int port, string pass, bool udpSupport = true)
    {
        Key = GetPassBytes(pass);
        _tcpListener = new TcpListener(ip, port);
        if (udpSupport)
        {
            udpListen = new UdpListen(port);
        }
        WriteLog($"Socks Service init,Listen on port {port}, udp support status : {udpSupport}");
    }

    public async Task StartAsync()
    {
        try
        {
            _tcpListener.Start();
            while (true)
            {
                var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                tcpClient.ReceiveTimeout = timeout;
                _ = TcpConnectAsync(tcpClient);
            }
        }
        catch (SocketException ex)
        {
            WriteLog(ex.Message);
        }
    }

    /// <summary>
    /// 发送数据
    /// </summary>
    /// <param name="tcpClient">TCPClient</param>
    /// <param name="data">数据</param>
    private static async Task TcpSendAsync(TcpClient tcpClient, byte[] data)
    {
        await tcpClient.GetStream().WriteAsync(EnBytes(data));
    }

    /// <summary>
    /// 接受客户端连接
    /// </summary>
    /// <param name="tcpClient"></param>
    private async Task TcpConnectAsync(TcpClient tcpClient)
    {
        NetworkStream tcpStream = tcpClient.GetStream();
        try
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(35));
            var recLen = await tcpStream.ReadAsync(_dataBuff.AsMemory(0, buffSize),cts.Token);
            if(recLen == 0)
            {
                tcpClient.Dispose();
                return;
            }
            var data = DeBytes(_dataBuff[..recLen]);
            var isNoAuth = IsNoAuth(data);
            var type = GetProxyType(data);
            //首次请求建立连接
            if (isNoAuth)
            {
                WriteLog($"receive connection request from {tcpClient.Client.RemoteEndPoint}");
                await TcpSendAsync(tcpClient, No_Authentication_Required);
                _ = TcpConnectAsync(tcpClient);
            }
            //已建立连接,判断代理目标端信息
            else if (type is not ProxyTypeEnum.Connection or ProxyTypeEnum.Unknown)
            {
                var proxyInfo = GetProxyInfo(data);
                if (proxyInfo.Type is 1)
                {
                    //TCP
                    TcpClient tcpProxy = TcpConnecte(proxyInfo.IP, proxyInfo.Port);
                    if (tcpProxy.Connected)
                    {
                        _ = new TcpServer(tcpClient, tcpProxy);
                        await TcpSendAsync(tcpClient, Proxy_Success);
                    }
                    else
                    {
                        await TcpSendAsync(tcpClient, Connect_Fail);
                        throw new SocketException();
                    }
                }
                else if (proxyInfo.Type is 3)
                {
                    //UDP 
                    //判断是否开启UDP支持及UDP阈值
                    if (udpListen is not null && UdpListen.SurplusProxyNum > 0)
                    {
                        //得到客户端开放UDP端口
                        var ClientPoint = new IPEndPoint(((IPEndPoint)tcpClient.Client.RemoteEndPoint!).Address, proxyInfo.Port);
                        udpListen.AddServer(ClientPoint, tcpClient);
                        byte[] header = GetUdpHeader((IPEndPoint)tcpClient.Client.LocalEndPoint!);
                        await TcpSendAsync(tcpClient, header);
                    }
                    else
                    {
                        throw new NotSupportedException("UDP support is disenable,the udp proxy request will be throw away");
                    }
                }
            }
            else
            {
                //不为连接且不为转发,有可能是密码错误
                throw new NotSupportedException("Unknown forwarding type or wrong password, this connection will be closed.");
            }
        }
        catch (Exception ex) when (ex is SocketException or NotSupportedException)
        {
            Close(tcpClient);
            WriteLog(ex.Message);
        }
    }

    /// <summary>
    /// 关闭客户端连接
    /// </summary>
    /// <param name="tcpClient">客户端TCPClient</param>
    private static void Close(TcpClient tcpClient)
    {
        try
        {
            if (tcpClient.Connected)
            {
                WriteLog($"Close the client connection to {tcpClient.Client.RemoteEndPoint}");
            }
        }
        catch (SocketException sex)
        {
            WriteLog(sex.Message);
        }
        finally
        {
            tcpClient.Close();
        }
    }

}