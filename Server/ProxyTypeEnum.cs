namespace Server;

public enum ProxyTypeEnum
{
    Connection = 1,
    TcpProxyIPV4,
    TcpProxyDomain,
    TcpProxyIPV6,
    UdpProxyIPV4,
    UdpProxyDomain,
    UdpProxyIPV6,
    Unknown = 500
}
