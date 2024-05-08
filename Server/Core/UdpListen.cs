using System.Net;
using System.Net.Sockets;
using Timer = System.Timers.Timer;

namespace Server.Core;

internal class UdpListen
{

    private readonly List<(IPAddress IP_Addr, TcpClient TCP_Client)> _initAddrList = new();
    private readonly List<(IPEndPoint IP_EndPoint, UdpServer Udp_Server)> _udpProxyList = new();
    private readonly UdpClient _udpClient;
    private readonly Timer _timer = new(5000);
    public static int SurplusProxyNum { get; private set; } = 3000;
    public UdpListen(int port)
    {
        _udpClient = new UdpClient(port);
        _ = UdpReceiveAsync();
        //检查并剔除已经失效的TcpClient
        _timer.Elapsed += (s, e) => TcpUsabilityCheck();
        _timer.Start();
    }


    /// <summary>
    /// 增加一条UDP代理隧道
    /// </summary>
    /// <param name="ipEndPoint">客户端远程地址</param>
    /// <param name="tcpClient">TCP依赖</param>
    public void AddServer(IPEndPoint ipEndPoint, TcpClient tcpClient)
    {
        if (ipEndPoint.Port == 0)
        {
            _initAddrList.Add((ipEndPoint.Address, tcpClient));
        }
        else
        {
            var udpServer = new UdpServer(ipEndPoint, tcpClient, BackToSourceAsync);
            _udpProxyList.Add((udpServer.ClientPoint, udpServer));
        }
        SurplusProxyNum--;
        WriteLog($"Open the udp proxy tunnel to {ipEndPoint}");

    }

    /// <summary>
    /// 定时器:检查可用性
    /// </summary>
    /// <param name="state"></param>
    private void TcpUsabilityCheck()
    {
        //循环时不要在原有列表中移除
        foreach (var i in _udpProxyList.ToArray())
        {
            if (!CheckTcpClientUsability(i.Udp_Server.TcpClient))
            {
                i.Udp_Server.Close();
                _udpProxyList.Remove(i);
                SurplusProxyNum++;
            }
        }
    }

    /// <summary>
    /// 回源
    /// </summary>
    /// <param name="clientPoint">客户端远程地址</param>
    /// <param name="data">待发送数据</param>
    private async Task BackToSourceAsync(IPEndPoint clientPoint, byte[] data)
    {
        if (data.Length > 0)
        {
            await _udpClient.SendAsync(data, data.Length, clientPoint);
        }
    }

    /// <summary>
    /// UDP接收回调
    /// </summary>
    /// <param name="ar">回调对象</param>
    public async Task UdpReceiveAsync()
    {
        while (true)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            var recResult = await _udpClient.ReceiveAsync(cts.Token);
            var data = DecodeBytes(recResult.Buffer);
            int headerLen = 0;
            var rs = GetWhichClient(recResult.RemoteEndPoint);
            if (rs is not null && data[2] == 0)
            {
                switch (data[3])
                {
                    case 1://IPV4
                        headerLen = 10;
                        break;
                    case 3://域名
                        headerLen = 7 + data[4];
                        break;
                    case 4://IPV6
                        headerLen = 22;
                        break;
                }
                var udpAddr = GetUdpAddr(data);
                if (udpAddr is not null)
                {
                    await rs.UdpSendAsync(udpAddr, data.Skip(headerLen).ToArray());
                }
            }
        }

    }

    /// <summary>
    /// 分辨远程端口来源
    /// </summary>
    /// <param name="remoteEndPoint"></param>
    /// <returns></returns>
    private UdpServer? GetWhichClient(IPEndPoint remoteEndPoint)
    {
        //直接转发
        for (int i = 0; i < _udpProxyList.Count; i++)
        {
            if (_udpProxyList[i].IP_EndPoint == remoteEndPoint)
            {
                //记录转发头
                return _udpProxyList[i].Udp_Server;
            }
        }
        //需要初始化
        for (int i = 0; i < _initAddrList.Count; i++)
        {
            if (_initAddrList[i].IP_Addr.Equals(remoteEndPoint.Address))
            {
                var udpServer = new UdpServer(remoteEndPoint, _initAddrList[i].TCP_Client, BackToSourceAsync);
                _initAddrList.Remove(_initAddrList[i]);
                if (udpServer.UdpClient != null)
                {
                    _udpProxyList.Add((udpServer.ClientPoint, udpServer));
                    return udpServer;
                }
            }
        }
        return null;
    }
}
