using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;

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
        UdpClient.Client.ReceiveTimeout = 1000 * 15;
        UdpClient.Client.SendTimeout = 1000 * 15;
        _callBackAsync = callBackFunc;
        _ = UDPRecieveAsync();
    }

    /// <summary>
    /// UDP接收回调
    /// </summary>
    /// <param name="ar"></param>
    private async Task UDPRecieveAsync()
    {
        try
        {
            while (true)
            {
                var receiveInfo = await UdpClient.ReceiveAsync();
                if (receiveInfo.Buffer.Length > 0)
                {
                    if (_proxyPointList.Contains(receiveInfo.RemoteEndPoint))
                    {
                        var Header = GetUdpHeader(receiveInfo.RemoteEndPoint);
                        var Send_Data = Header.Concat(receiveInfo.Buffer).ToArray();
                        await _callBackAsync(ClientPoint, EnBytes(Send_Data));
                    }
                }
            }

        }
        catch (Exception)
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
}
