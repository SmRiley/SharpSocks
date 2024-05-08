using System.Net;
using System.Net.Sockets;

namespace Client.Core;

internal class UdpLocal : IDisposable
{
    private readonly UdpClient _udpClient = new(0);
    private IPEndPoint? _localPoint;
    private readonly IPEndPoint _proxyPoint;

    public int UdpPort => ((IPEndPoint)_udpClient.Client.LocalEndPoint!).Port;

    public UdpLocal(IPEndPoint proxyIpEndpoint, IPEndPoint? localIpEndpoint = null)
    {
        _proxyPoint = proxyIpEndpoint;
        if (localIpEndpoint != null)
        {
            _localPoint = localIpEndpoint;
        }
        _ = UdpReceiveAsync();
    }

    private async Task UdpReceiveAsync()
    {
        try
        {
            while (true)
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                var recResult = await _udpClient.ReceiveAsync(cts.Token);
                if (recResult.Buffer.Length <= 0)
                {
                    throw new SocketException();
                }
                _localPoint ??= recResult.RemoteEndPoint;
                if (_localPoint.Equals(recResult.RemoteEndPoint))
                {
                    await UdpSendAsync(_proxyPoint, EncodeBytes(recResult.Buffer));
                }
                else
                {
                    await UdpSendAsync(_localPoint, DecodeBytes(recResult.Buffer));
                }
            }
        }
        catch (Exception ex) when (ex is SocketException or ObjectDisposedException)
        {
            Console.WriteLine(ex.Message);
            Dispose();
        }
    }

    private async Task UdpSendAsync(IPEndPoint remotePoint, byte[] data)
    {
        try
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            await _udpClient.SendAsync(data, remotePoint, cts.Token);
        }
        catch (Exception ex) when (ex is SocketException or ObjectDisposedException)
        {
            // 记录或处理异常
            Console.WriteLine(ex.Message);
            Dispose();
        }
    }

    public void Dispose()
    {
        _udpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
