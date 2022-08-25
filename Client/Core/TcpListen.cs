using System.Net;
using System.Net.Sockets;
using System.Runtime.Versioning;
using Timer = System.Timers.Timer;
namespace Client.Core;

class TcpListen:IDisposable
{
    private int _port;
    private int _localPort;
    private readonly string _ip;

    private static Timer _checkerTimer = new(10000);

    private readonly TcpListener _tcpListener;
    private CancellationTokenSource? ListenerCts;
    public TcpListen(string ipAddr, int port, string pass, int localPort)
    {
        _ip = ipAddr;
        _port = port;
        _localPort = localPort;
        Key = GetPassBytes(pass);
        _tcpListener = new TcpListener(IPAddress.Any, _localPort);
        _checkerTimer.Elapsed += (s, e) => TcpListen.CheckTcpTimer();
    }

    public void Start()
    {
        ListenerCts = new CancellationTokenSource();
        _tcpListener.Start();
        _ = AcceptTcpAsync();
        _checkerTimer.Start();
    }

    public void Stop()
    {
        _tcpListener.Stop();
        ListenerCts?.Cancel();
    }

    private async Task AcceptTcpAsync()
    {
        await Task.Run(async () =>
        {
            try
            {
                while (true)
                {
                    var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                    _ = new TCPLocal(tcpClient, new TcpClient(_ip, _port));
                }
            }
            catch (SocketException)
            {

            }
        },ListenerCts!.Token);
    }

    static void CheckTcpTimer()
    {
        foreach (var i in TCPLocal.UdpList.ToArray())
        {
            if (!CheckTcpUsability(i.tcpClient))
            {
                i.tcpClient.Dispose();
                i.UdpLocal.Dispose();
                TCPLocal.UdpList.Remove(i);
            }
        }
    }

    public void Dispose()
    {
        _tcpListener.Stop();
    }
}
