using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace Share;
public class Utils
{
    public static List<byte> Key { set; private get; } = new();

    /// <summary>
    /// 得到密匙
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static List<byte> GenerateUniqueRandomBytes(string key)
    {
        int seed = Encoding.UTF8.GetBytes(key).Sum(b => (int)b);
        var random = new Random(seed);
        var uniqueBytes = new HashSet<byte>();

        while (uniqueBytes.Count < 256)
        {
            byte randomByte = (byte)random.Next(256);
            uniqueBytes.Add(randomByte);
        }

        return uniqueBytes.ToList();
    }

    /// <summary>
    /// 加密
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static byte[] EncodeBytes(byte[] bytes)
    {
        return bytes.Select(b => (byte)Key.IndexOf(b)).ToArray();
    }

    /// <summary>
    /// 解密
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static byte[] DecodeBytes(byte[] bytes)
    {
        return bytes.Select(b => Key[b]).ToArray();
    }

    /// <summary>
    /// TCP可用性检查
    /// </summary>
    /// <param name="tcpClient">待检测对象</param>
    /// <returns></returns>
    public static bool CheckTcpClientUsability(TcpClient tcpClient)
    {
        try
        {
            var activeConnections = IPGlobalProperties.GetIPGlobalProperties()?.GetActiveTcpConnections();
            var matchingConnection = activeConnections?.FirstOrDefault(x => x.LocalEndPoint.Equals(tcpClient.Client?.LocalEndPoint));
            return matchingConnection?.State == TcpState.Established;
        }
        catch (NetworkInformationException)
        {
            return false;
        }
    }
}
