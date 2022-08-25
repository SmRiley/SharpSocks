using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using Timer = System.Timers.Timer;

namespace Server.Core;

class UdpListen
{

    private readonly List<(IPAddress IP_Addr, TcpClient TCP_Client)> _initAddrList = new();
    private readonly List<(IPEndPoint IP_EndPoint, UdpServer Udp_Server)> _udpProxyList = new();
    private readonly UdpClient _udpClient;
    private readonly Timer _timer = new(5000);
    public static int SurplusProxyNum { get; private set; } = 3000;
    public UdpListen(int Port)
    {
        _udpClient = new UdpClient(Port);
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
            var Udp_Server = new UdpServer(ipEndPoint, tcpClient, BackToSourceAsync);
            _udpProxyList.Add((Udp_Server.ClientPoint, Udp_Server));
        }
        SurplusProxyNum--;
        WriteLog($"已开启对{ipEndPoint}的UDP代理隧道");

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
            if (!CheckTcpUsability(i.Udp_Server.TcpClient))
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
        await _udpClient.SendAsync(data, data.Length, clientPoint);
    }

    /// <summary>
    /// UDP接收回调
    /// </summary>
    /// <param name="ar">回调对象</param>
    public async Task UdpReceiveAsync()
    {
        while (true)
        {
            var recResult = await _udpClient.ReceiveAsync();
            var data = DeBytes(recResult.Buffer);
            int header_len = 0;
            var rs = GetWhichClient(recResult.RemoteEndPoint);
            if (rs is not null && data[2] == 0)
            {
                switch (data[3])
                {
                    case 1://IPV4
                        header_len = 10;
                        break;
                    case 3://域名
                        header_len = 7 + data[4];
                        break;
                    case 4://IPV6
                        header_len = 22;
                        break;
                }
                var udpAddr = GetUdpAddr(data);
                if (udpAddr is not null)
                {
                    await rs.UdpSendAsync(udpAddr, data.Skip(header_len).ToArray());
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
                var Udp_Server = new UdpServer(remoteEndPoint, _initAddrList[i].TCP_Client, BackToSourceAsync);
                _initAddrList.Remove(_initAddrList[i]);
                if (Udp_Server.UdpClient != null)
                {
                    _udpProxyList.Add((Udp_Server.ClientPoint, Udp_Server));
                    return Udp_Server;
                }
            }
        }
        return null;
    }
}
