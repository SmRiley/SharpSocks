using System;
using System.Net;
using System.Net.Sockets;

namespace Client.Core;

class TcpLocal : IDisposable
{
    private readonly byte[] _proxyBuff = new byte[1024 * 50];
    private readonly byte[] _localBuff = new byte[1024 * 50];

    public static List<(TcpClient tcpClient, UdpLocal UdpLocal)> UdpList { get; private set; } = new();
    private readonly TcpClient _client;
    private readonly TcpClient _proxy;
    private readonly NetworkStream _clientStream;
    private readonly NetworkStream _proxyStream;
    public TcpLocal(TcpClient Client, TcpClient Proxy)
    {
        _client = Client;
        _proxy = Proxy;
        _clientStream = _client.GetStream();
        _proxyStream = _proxy.GetStream();
        _ = ClientReadAsync();
        _ = ProxyReadInitAsync(1);
    }

    public async Task TcpSendAsync(NetworkStream stream, byte[] data)
    {
        try
        {
            if(data.Length > 0)
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                await stream.WriteAsync(data,cts.Token);
            }
            else
            {
                throw new SocketException();
            }
        }
        catch (Exception ex)when(ex is SocketException or TimeoutException)
        {
            Dispose();
        }
    }

    /// <summary>
    /// 客户端加密后转发
    /// </summary>
    /// <returns></returns>
    private async Task ClientReadAsync()
    {
        try
        {
            while (true)
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(35));
                var recLen = await _clientStream.ReadAsync(_localBuff,cts.Token);
                var data = EnBytes(_localBuff[..recLen]);
                await TcpSendAsync(_proxyStream, data);
            }
        }
        catch (Exception ex) when (ex is SocketException or IOException or ObjectDisposedException or TimeoutException)
        {
            Dispose();
        }
    }

    /// <summary>
    /// 代理端接收初始化
    /// </summary>
    /// <param name="ar"></param>
    /// <returns></returns>
    private async Task ProxyReadInitAsync(int ar)
    {
        try
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(35));
            var recLen = await _proxyStream.ReadAsync(_proxyBuff,cts.Token);
            var data = DeBytes(_proxyBuff[..recLen]);
            if (ar == 2)
            {
                var remoteipEndpoint = _proxy.Client?.RemoteEndPoint as IPEndPoint;
                if (remoteipEndpoint is not null)
                {
                    var udpLocal = new UdpLocal(remoteipEndpoint);
                    await TcpSendAsync(_clientStream, ExUdp(data, udpLocal.UdpPort));
                    if (IsUdpInit(data))
                    {
                        UdpList.Add((_client, udpLocal));
                    }
                    else
                    {
                        _ = ProxyReadAsync();
                    }
                }
            }
            else
            {
                await TcpSendAsync(_clientStream, data);
                _ = ProxyReadInitAsync(++ar);
            }
        }
        catch (Exception ex) when (ex is SocketException or IOException or ObjectDisposedException or OperationCanceledException)
        {
            Dispose();
        }
    }

    /// <summary>
    /// 代理接收转发
    /// </summary>
    /// <returns></returns>
    private async Task ProxyReadAsync()
    {
        try
        {
            while (true)
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(35));
                var recLen = await _proxyStream.ReadAsync(_proxyBuff,cts.Token);
                var data = DeBytes(_proxyBuff[..recLen]);
                await TcpSendAsync(_clientStream, data);
            }
        }
        catch (Exception ex) when (ex is SocketException or IOException or ObjectDisposedException or OperationCanceledException)
        {
            Dispose();
        }
    }

    public void Dispose()
    {
        _client.Dispose();
        _proxy.Dispose();
        GC.SuppressFinalize(this);
    }
}
