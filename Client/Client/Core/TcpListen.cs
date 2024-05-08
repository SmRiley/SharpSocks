using System.Net;
using System.Net.Sockets;
using Timer = System.Timers.Timer;
namespace Client.Core;

internal class TcpListen : IDisposable
{
    private readonly int _port;
    private readonly int _localPort;
    private readonly string _ip;

    private static readonly Timer CheckerTimer = new(10000);

    private readonly TcpListener _tcpListener;
    public TcpListen(string ipAddr, int port, string pass, int localPort)
    {
        _ip = ipAddr;
        _port = port;
        _localPort = localPort;
        Key = GenerateUniqueRandomBytes(pass);
        _tcpListener = new TcpListener(IPAddress.Any, _localPort);
        CheckerTimer.Elapsed += (s, e) => CheckTcpTimer();
    }

    public void Start()
    {
        _tcpListener.Start();
        _ = AcceptTcpAsync();
        CheckerTimer.Start();
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
        catch (SocketException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static void CheckTcpTimer()
    {
        lock (TcpLocal.UdpList)
        {
            foreach (var i in TcpLocal.UdpList.ToArray())
            {
                if (!CheckTcpClientUsability(i.tcpClient))
                {
                    i.tcpClient.Dispose();
                    i.UdpLocal.Dispose();
                    TcpLocal.UdpList.Remove(i);
                }
            }
        }
    }

    public void Dispose()
    {
        _tcpListener.Stop();
        _tcpListener.Server.Close();
        GC.SuppressFinalize(this);
    }
}
