using System.Net;

namespace Client;

internal class DataHandle
{
    //这里的发送与接收是相对服务端的,不是对于本地端
    public static bool IsUdpInit(byte[] dat)
    {
        if (dat.Take(3).SequenceEqual(new byte[] { 5, 0, 0 }))
        {
            if (dat.Length == 10 && !dat.Skip(8).Take(2).SequenceEqual(new byte[] { 0, 0 }))
            {

                return true;
            }
            else if (dat.Length == 22 && !dat.Skip(20).Take(2).SequenceEqual(new byte[] { 0, 0 }))
            {
                return true;
            }

        }
        return false;
    }

    /// <summary>
    /// Udp是否有效
    /// </summary>
    /// <param name="data"></param>
    /// <param name="udpPort"></param>
    /// <returns></returns>
    public static byte[] ExUdp(byte[] data, int udpPort)
    {
        if (IsUdpInit(data))
        {

            var rs = (new byte[] { 5, 0, 0, 1 }).Concat(IPAddress.Parse("127.0.0.1").GetAddressBytes()).
                Concat(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(udpPort)).Skip(2));
            return rs.ToArray();
        }
        return data;
    }
}
