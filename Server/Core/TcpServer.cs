using System.Net.Sockets;
using System;

namespace Server.Core;

class TcpServer
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
        WriteLog($"开启对{_client.Client.RemoteEndPoint}的TCP代理隧道");
    }

    /// <summary>
    /// 发送数据
    /// </summary>
    /// <param name="TcpStream">TCP流</param>
    /// <param name="data">数据</param>
    private async Task TcpSendAsync(NetworkStream TcpStream, byte[] data)
    {
        try
        {
            await TcpStream.WriteAsync(data);
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
        try
        {
            while (true)
            {
                var recLen = await _clientStream.ReadAsync(_clientBuff.AsMemory(0, _buffSize));
                if (recLen > 0)
                {
                    await TcpSendAsync(_proxyStream, DeBytes(_clientBuff[..recLen]));
                }
                else
                {
                    ProxyClose();
                }
            }
        }
        catch (SocketException)
        {
            ProxyClose();
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
                var recLen = await _proxyStream.ReadAsync(_proxyBuff.AsMemory(0, _buffSize));
                if (recLen > 0)
                {
                    await TcpSendAsync(_clientStream, EnBytes(_proxyBuff[..recLen]));
                }
                else
                {
                    ProxyClose();
                }
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
        try
        {
            if (_client.Connected)
            {

                WriteLog($"已断开客户端{_client.Client.RemoteEndPoint}的连接");
                _clientStream.Close();
                _client.Close();
            }
            if (_proxy.Connected)
            {
                WriteLog($"已断开代理端{_proxy.Client.RemoteEndPoint}的连接");
                _proxyStream.Close();
                _proxy.Close();
            }
        }
        catch (Exception)
        {

        }
    }

    ~TcpServer()
    {
        _client?.Dispose();
        _clientStream?.Dispose();
        _proxy?.Dispose();
        _proxyStream?.Dispose();
    }
}