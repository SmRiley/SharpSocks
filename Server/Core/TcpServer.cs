using System.Net.Sockets;

namespace Server.Core;

internal class TcpServer
{
    private const int _buffSize = 1024 * 50;
    private readonly byte[] _clientBuff = new byte[_buffSize];
    private readonly byte[] _proxyBuff = new byte[_buffSize];
    private readonly TcpClient _client;
    private readonly TcpClient _proxy;
    private readonly NetworkStream _clientStream;
    private readonly NetworkStream _proxyStream;

    public TcpServer(TcpClient tcpClient, TcpClient tcpProxy)
    {
        _proxy = tcpProxy;
        _client = tcpClient;
        _clientStream = _client.GetStream();
        _proxyStream = _proxy.GetStream();
        _ = TcpClientReceive();
        _ = TcpProxyReceive();
        WriteLog($"Open the tcp proxy tunnel to {_client.Client.RemoteEndPoint}");
    }

    /// <summary>
    /// 发送数据
    /// </summary>
    /// <param name="stream">TCP流</param>
    /// <param name="data">数据</param>
    private async Task TcpSendAsync(NetworkStream stream, byte[] data)
    {
        try
        {
            if (data.Length > 0)
            {
                await stream.WriteAsync(data);
            }
            else
            {
                throw new SocketException();
            }
        }
        catch (SocketException)
        {
            ProxyClose();
        }
    }

    /// <summary>
    /// 客户端接收数据回调
    /// </summary>
    /// <param name="ar"></param>
    private async Task TcpClientReceive()
    {
        while (true)
        {
            try
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(35));
                var recLen = await _clientStream.ReadAsync(_clientBuff.AsMemory(0, _buffSize), cts.Token);
                await TcpSendAsync(_proxyStream, DeBytes(_clientBuff[..recLen]));

            }
            catch (SocketException)
            {
                ProxyClose();
            }
        }
    }

    /// <summary>
    /// 代理端接收数据回调
    /// </summary>
    /// <param name="ar"></param>
    private async Task TcpProxyReceive()
    {
        try
        {
            while (true)
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(35));
                var recLen = await _proxyStream.ReadAsync(_proxyBuff.AsMemory(0, _buffSize), cts.Token);
                await TcpSendAsync(_clientStream, EnBytes(_proxyBuff[..recLen]));
            }
        }
        catch (SocketException)
        {
            ProxyClose();
        }
    }

    /// <summary>
    /// 关闭代理隧道
    /// </summary>
    private void ProxyClose()
    {
        if (_client.Connected)
        {

            WriteLog($"Close the client connection to {_client.Client.RemoteEndPoint}");
            _client.Dispose();
        }
        if (_proxy.Connected)
        {
            WriteLog($"Close the proxy connection to {_proxy.Client.RemoteEndPoint}");
            _proxy.Dispose();
        }
    }

    ~TcpServer()
    {
        _client?.Dispose();
        _proxy?.Dispose();
    }
}