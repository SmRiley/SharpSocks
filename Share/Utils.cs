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
    public static List<byte> GetPassBytes(string key)
    {
        int passBytes = 0;
        foreach (var i in Encoding.UTF8.GetBytes(key))
        {
            passBytes += i;
        }
        var random = new Random(passBytes);
        var Bytes_Pass = new List<byte>();
        for (int i = 0; i < 256; i++)
        {
            byte Random_int = (byte)random.Next(256);
            if (!Bytes_Pass.Contains(Random_int))
            {
                Bytes_Pass.Add(Random_int);
            }
            else
            {
                i--;
            }
        }
        return Bytes_Pass;
    }

    /// <summary>
    /// 加密
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static byte[] EnBytes(byte[] bytes)
    {
        var List_Byte = new List<byte>();
        foreach (var b in bytes)
        {
            List_Byte.Add((byte)Key.IndexOf(b));
        }
        return List_Byte.ToArray();
    }

    /// <summary>
    /// 解密
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static byte[] DeBytes(byte[] bytes)
    {
        var List_Byte = new List<byte>();
        foreach (var b in bytes)
        {
            List_Byte.Add(Key[b]);
        }
        return List_Byte.ToArray();
    }

    /// <summary>
    /// TCP可用性检查
    /// </summary>
    /// <param name="tcpClient">待检测对象</param>
    /// <returns></returns>
    public static bool CheckTcpUsability(TcpClient tcpClient)
    {
        try
        {
            var foos = IPGlobalProperties.GetIPGlobalProperties()?.GetActiveTcpConnections();
            var foo = foos?.FirstOrDefault(x => x.LocalEndPoint.Equals(tcpClient.Client?.LocalEndPoint));
            return foo?.State is TcpState.Established;
        }
        catch (NetworkInformationException)
        {
            return false;
        }
    }
}
