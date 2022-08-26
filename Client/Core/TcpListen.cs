using System.Net;
using System.Net.Sockets;
using Timer = System.Timers.Timer;
namespace Client.Core;

internal class TcpListen : IDisposable
{
    private int _port;
    private int _localPort;
    private readonly string _ip;

    private static Timer _checkerTimer = new(10000);

    private readonly TcpListener _tcpListener;
    public TcpListen(string ipAddr, int port, string pass, int localPort)
    {
        _ip = ipAddr;
        _port = port;
        _localPort = localPort;
        Key = GetPassBytes(pass);
        _tcpListener = new TcpListener(IPAddress.Any, _localPort);
        _checkerTimer.Elapsed += (s, e) => CheckTcpTimer();
    }

    public void Start()
    {
        _tcpListener.Start();
        _ = AcceptTcpAsync();
        _checkerTimer.Start();
    }

    public void Stop()
    {
        _tcpListener.Stop();
    }

    private async Task AcceptTcpAsync()
    {
        try
        {
            while (true)
            {
                var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                _ = new TcpLocal(tcpClient, new TcpClient(_ip, _port));
            }
        }
        catch (SocketException)
        {

        }
    }

    private static void CheckTcpTimer()
    {
        foreach (var i in TcpLocal.UdpList.ToArray())
        {
            if (!CheckTcpUsability(i.tcpClient))
            {
                i.tcpClient.Dispose();
                i.UdpLocal.Dispose();
                TcpLocal.UdpList.Remove(i);
            }
        }
    }

    public void Dispose()
    {
        _tcpListener.Stop();
        GC.SuppressFinalize(this);
    }
}
