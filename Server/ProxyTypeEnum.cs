namespace Server;

public enum ProxyTypeEnum
{
    Connection = 1,
    TcpProxyIpv4,
    TcpProxyDomain,
    TcpProxyIpv6,
    UdpProxyIpv4,
    UdpProxyDomain,
    UdpProxyIpv6,
    Unknown = 500
}
