using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Net;

namespace Socks5Local
{
    class DataHandle
    {
        public static List<byte> Key { private get; set; }


        //这里的发送与接收是相对服务端的,不是对于本地端!



        public static bool Is_UDP_Init(byte[] Data) {
            if (Data.Take(3).SequenceEqual((new byte[] { 5, 0, 0 }))) {
                if (Data.Length == 10 && !Data.Skip(8).Take(2).SequenceEqual(new byte[] { 0, 0 })) {

                    return true;
                }
                else if (Data.Length == 22 && !Data.Skip(20).Take(2).SequenceEqual(new byte[] { 0, 0 }))
                {
                    return true;
                }

            }
            return false;
        }

        public static byte[] Ex_UDP(byte[] Data, int UDP_Port) {
            try
            {
                if (Is_UDP_Init(Data))
                {

                    var Rs = (new byte[] { 5, 0, 0, 1 }).Concat(IPAddress.Parse("127.0.0.1").GetAddressBytes()).
                        Concat(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(UDP_Port)).Skip(2));
                    return Rs.ToArray();
                }

                return Data;
            }
            catch (Exception) {
                return Data;
            }
        }

        /// <summary>
        /// 得到密匙
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static List<byte> Get_Pass(string Key)
        {
            int Bytes_Key = 0;
            foreach (var i in Encoding.UTF8.GetBytes(Key))
            {
                Bytes_Key += i;
            }
            var random = new Random(Bytes_Key);
            //这里不用byte[]是因为会填充0.当然也可以用,不过需要跳过byte[]的0下标..
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
        /// <param name="Bytes"></param>
        /// <returns></returns>
        public static byte[] En_Bytes(byte[] Bytes)
        {
            var List_Byte = new List<byte>();
            foreach (var b in Bytes)
            {
                List_Byte.Add((byte)Key.IndexOf(b));
            }
            return List_Byte.ToArray();
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="Bytes"></param>
        /// <returns></returns>
        public static byte[] De_Bytes(byte[] Bytes)
        {
            var List_Byte = new List<byte>();
            foreach (var b in Bytes)
            {
                List_Byte.Add(Key[b]);
            }
            return List_Byte.ToArray();
        }

        public static bool TCP_Usability(TcpClient Tcp_Client)
        {
            try
            {
                Tcp_Client.GetStream().Write(new byte[] {0},0,1);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
