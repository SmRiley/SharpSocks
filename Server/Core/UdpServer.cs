using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Net.Http;

namespace Server.Core;

class UdpServer
{
    private readonly List<IPEndPoint> _proxyPointList = new();
    private readonly Func<IPEndPoint, byte[], Task> _callBackAsync;
    public IPEndPoint ClientPoint { get; set; }
    public TcpClient TcpClient { get; set; }
    public UdpClient UdpClient { get; set; }
    public UdpServer(IPEndPoint ipEndpoint, TcpClient tcpClient, Func<IPEndPoint, byte[], Task> callBackFunc)
    {
        ClientPoint = ipEndpoint;
        TcpClient = tcpClient;
        UdpClient = new UdpClient(0);
        _callBackAsync = callBackFunc;
        _ = UdpRecieveAsync();
    }

    /// <summary>
    /// UDP接收回调
    /// </summary>
    /// <param name="ar"></param>
    private async Task UdpRecieveAsync()
    {
        try
        {
            while (true)
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                var receiveInfo = await UdpClient.ReceiveAsync(cts.Token);
                if (receiveInfo.Buffer.Length > 0)
                {
                    if (_proxyPointList.Contains(receiveInfo.RemoteEndPoint))
                    {
                        var Header = GetUdpHeader(receiveInfo.RemoteEndPoint);
                        var Send_Data = Header.Concat(receiveInfo.Buffer).ToArray();
                        await _callBackAsync(ClientPoint, EnBytes(Send_Data));
                    }
                }
                else
                {
                    UdpClient.Dispose();
                    break;
                }
            }

        }
        catch (SocketException)
        {

        }
    }

    public async Task UdpSendAsync(IPEndPoint ipEndpoint, byte[] data)
    {
        await UdpClient.SendAsync(data, data.Length, ipEndpoint);
        _proxyPointList.Add(ipEndpoint);
    }

    /// <summary>
    /// 关闭UDP对象及TCP依赖
    /// </summary>
    public void Close()
    {
        try
        {
            WriteLog($"已关闭{ClientPoint}的UDP代理隧道");
            UdpClient.Close();
            TcpClient.GetStream().Close();
            TcpClient.Close();
        }
        catch (Exception)
        {

        }
    }

    ~UdpServer()
    {
        UdpClient.Dispose();
        TcpClient.Dispose();
    }
}
