using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace Server;

class DataHandle
{
    /// <summary>
    /// 无需验证
    /// </summary>
    public static byte[] No_Authentication_Required = new byte[] { 5, 0 };

    public static byte[] Connect_Fail = new byte[] { 5, 255 };

    /// <summary>
    /// 需要身份验证
    /// </summary>
    public static byte[] Authentication_Required = new byte[] { 5, 2 };

    /// <summary>
    /// 身份验证成功
    /// </summary>
    public static byte[] Authentication_Success = new byte[] { 1, 0 };

    public static byte[] Proxy_Success = new byte[] { 5, 0, 0, 1, 0, 0, 0, 0, 0, 0 };
    public static byte[] Proxy_Error = new byte[] { 5, 1, 0, 1, 0, 0, 0, 0, 0, 0 };

    /// <summary>
    /// 打印
    /// </summary>
    /// <param name="str">需打印字符串</param>
    public static void WriteLog(string str)
    {
        Console.WriteLine($"{DateTime.Now} : {str}\n\r");
    }

    /// <summary>
    /// 得到请求类型,参阅"https://zh.m.wikipedia.org/zh-hans/SOCKS#SOCKS5"
    /// </summary>
    /// <param name="Data"></param>
    /// <returns></returns>
    public static ProxyTypeEnum GetProxyType(byte[] Data)
    {
        if (Data.Length > 2 && Data.Length == Data[1] + 2)
        {
            return ProxyTypeEnum.Connection;
        }
        else if (Data.Length > 8)
        {
            if (Data[1] == 1)
            {
                ///TCP请求
                if (Data[3] == 1 && Data.Length == 10)
                {
                    return ProxyTypeEnum.TcpProxyIPV4;
                }
                else if (Data[3] == 3 && Data.Length == (Data.Skip(5).Take(Data[4]).Count() + 7))
                {
                    return ProxyTypeEnum.TcpProxyDomain;
                }
                else if (Data[3] == 4 && Data.Length == 22)
                {
                    return ProxyTypeEnum.TcpProxyIPV6;
                }
            }
            else if (Data[1] == 3)
            {
                //UDP请求或转发
                if (Data[3] == 1 && Data.Length == 10)
                {
                    return ProxyTypeEnum.UdpProxyIPV4;
                }
                else if (Data[3] == 3 && Data.Length == (Data.Skip(5).Take(Data[4]).Count() + 7))
                {
                    return ProxyTypeEnum.UdpProxyDomain;
                }
                else if (Data[3] == 4 && Data.Length == 22)
                {
                    return ProxyTypeEnum.TcpProxyIPV6;
                }
            }
        }
        return ProxyTypeEnum.Unknown;
    }


    /// <summary>
    /// 是否为无需账号密码模式
    /// </summary>
    /// <param name="Data"></param>
    /// <returns></returns>
    public static bool IsNoAuth(byte[] Data)
    {
        if (GetProxyType(Data) is ProxyTypeEnum.Connection)
        {
            var methodBytes = Data.Skip(2).Take(Data[1]);
            return methodBytes.Contains(byte.MinValue);
        }
        return false;
    }


    /// <summary>
    /// 获取请求转发信息
    /// </summary>
    /// <param name="Data"></param>
    /// <returns>
    /// int type代理协议 -1 未知,1:TCP,3:UDP
    /// string IP
    /// int PORT
    /// </returns>
    public static (int Type, IPAddress IP, int Port) GetProxyInfo(byte[] Data)
    {
        IPAddress? hostIp = null;
        int port = 0;
        var type = GetProxyType(Data);
        try
        {
            if (type is not ProxyTypeEnum.Connection)
            {
                byte[] portBytes = new byte[2];
                switch (type)
                {
                    case ProxyTypeEnum.TcpProxyIPV4 or ProxyTypeEnum.UdpProxyIPV4:
                        //IPV4
                        hostIp = new IPAddress(Data.Skip(4).Take(4).ToArray());
                        portBytes = (Data.Skip(8).Take(2).ToArray());
                        port = (portBytes[0] << 8) + portBytes[1];
                        WriteLog($"Receive tcp Ipv4 proxy request to {hostIp}:{port}");
                        break;
                    case ProxyTypeEnum.TcpProxyDomain or ProxyTypeEnum.UdpProxyDomain:
                        //域名解析IP
                        string Realm_Name = Encoding.UTF8.GetString(Data.Skip(5).Take(Data[4]).ToArray());
                        hostIp = Dns.GetHostEntry(Realm_Name).AddressList[0];
                        portBytes = (Data.Skip(5 + Data[4]).Take(2).ToArray());
                        port = (portBytes[0] << 8) + portBytes[1];
                        WriteLog($"Receive tcp proxy request to {Realm_Name}({hostIp}:{port})");
                        break;
                    case ProxyTypeEnum.TcpProxyIPV6 or ProxyTypeEnum.UdpProxyIPV6:
                        //IPV6
                        hostIp = new IPAddress(Data.Skip(4).Take(16).ToArray());
                        portBytes = (Data.Skip(8).Take(2).ToArray());
                        port = (portBytes[0] << 8) + portBytes[1];
                        WriteLog($"Receive tcp Ipv6 proxy request to {hostIp}:{port}的代理请求");
                        break;
                }

                if (hostIp != null)
                {
                    return (Data[1], hostIp, port);
                }

            }
        }
        catch (IndexOutOfRangeException)
        {

        }
        return (0, IPAddress.Parse("127.0.0.1"), 0);
    }

    /// <summary>
    /// 得到UDP转发数据头
    /// </summary>
    /// <param name="ipEndpoint">IPEndPoint</param>
    /// <param name="IsRsv">Ver(5)还是RSV(0)</param>
    /// <returns></returns>
    public static byte[] GetUdpHeader(IPEndPoint ipEndpoint, bool IsRsv = false)
    {
        int IP_Len = ipEndpoint.Address.GetAddressBytes().Length;
        List<byte> Bytes_IPEndPoint = new();
        if (IsRsv)
        {
            Bytes_IPEndPoint.AddRange(new byte[] { 0, 0 });
        }
        else
        {
            Bytes_IPEndPoint.AddRange(new byte[] { 5, 0 });
        }
        if (IP_Len == 4)
        {
            //IPV4
            Bytes_IPEndPoint.AddRange(new byte[] { 0, 1 });
        }
        else if (IP_Len == 16)
        {
            //IPV6
            Bytes_IPEndPoint.AddRange(new byte[] { 0, 4 });
        }

        Bytes_IPEndPoint.AddRange(ipEndpoint.Address.GetAddressBytes());
        //Port为int32储存,实际应用中为Uint16,需要去掉前两个字节
        Bytes_IPEndPoint.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(ipEndpoint.Port)).Skip(2));

        return Bytes_IPEndPoint.ToArray();
    }

    /// <summary>
    /// 得到UDP转发的目标地址
    /// </summary>
    /// <param name="data">待转发数据</param>
    /// <returns></returns>
    public static IPEndPoint? GetUdpAddr(byte[] data)
    {
        IPAddress? hostIp = null;
        byte[] portBytes = new byte[2];
        switch (data[3])
        {
            case 1:
                hostIp = new IPAddress(data.Skip(4).Take(4).ToArray());
                portBytes = (data.Skip(8).Take(2).ToArray());
                break;
            case 3:
                hostIp = Dns.GetHostEntry(Encoding.UTF8.GetString(data.Skip(5).Take(data[4]).ToArray())).AddressList[0];
                portBytes = (data.Skip(5 + data[4]).Take(2).ToArray());
                break;
            case 4:
                hostIp = new IPAddress(data.Skip(4).Take(16).ToArray());
                portBytes = (data.Skip(8).Take(2).ToArray());
                break;
        }
        var Port = (portBytes[0] << 8) + portBytes[1];
        if (hostIp != null)
        {
            return new(hostIp, Port);
        }
        return null;
    }

    /// <summary>
    /// 建立TCP连接
    /// </summary>
    /// <param name="ip">IP</param>
    /// <param name="port">PORT</param>
    /// <returns></returns>
    public static TcpClient TcpConnecte(IPAddress ip, int port)
    {
        TcpClient tcpClient = new();
        try
        {
            tcpClient.Connect(ip, port);
        }
        catch(SocketException)
        {
            tcpClient.Dispose();
        }
        return tcpClient;
    }

}
