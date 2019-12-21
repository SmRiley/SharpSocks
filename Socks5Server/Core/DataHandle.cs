using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Socks5Server
{
    class Datahandle
    {

        /// <summary>
        /// 无需验证
        /// </summary>
        static public byte[] No_Authentication_Required = new byte[] { 5, 0 };

        static public byte[] Connect_Fail = new byte[] { 5, 0xFF };

        /// <summary>
        /// 需要身份验证
        /// </summary>
        static public byte[] Authentication_Required = new byte[] { 5, 2 };

        /// <summary>
        /// 身份验证成功
        /// </summary>
        static public byte[] Authentication_Success = new byte[] { 1, 0 };

        static public byte[] Proxy_Success = new byte[] { 5, 0, 0, 1, 0, 0, 0, 0, 0, 0 };
        static public byte[] Proxy_Error = new byte[] { 5, 1, 0, 1, 0, 0, 0, 0, 0, 0 };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Data"></param>
        /// <returns>
        /// 1.建立连接请求
        /// 2.转发请求TCP(IPV4)
        /// 3.转发请求TCP(域名)
        /// 4.转发请求TCP(IPV6)
        /// 5.转发请求UDP(IPV4)
        /// 6.转发请求UDP(域名)
        /// 7.转发请求UDP(IPV6)
        /// 0.其它
        /// </returns>
        static public int Get_Which_Type(byte[] Data)
        {
            int Type = 0;
            if (Data.Length > 2 && Data.Length == Data[1] + 2)
            {
                Type = 1;
            }
            else if (Data.Length > 8)
            {
                if (Data[1] == 1)
                {
                    ///TCP请求
                    if (Data[3] == 1 && Data.Length == 10)
                    {
                        Type = 2;
                    }
                    else if (Data[3] == 3 && Data.Length == (Data.Skip(5).Take(Data[4]).Count() + 7))
                    {
                        Type = 3;
                    }
                    else if (Data[3] == 4 && Data.Length == 22) {
                        Type = 4;
                    }
                } else if (Data[1] == 3) {
                    //UDP请求或转发
                    if (Data[3] == 1 && Data.Length == 10)
                    {
                        Type = 5;
                    }
                    else if (Data[3] == 3 && Data.Length == (Data.Skip(5).Take(Data[4]).Count() + 7))
                    {
                        Type = 6;
                    }
                    else if (Data[3] == 4 && Data.Length == 22)
                    {
                        Type = 7;
                    }
                }
            }

            return Type;


        }


        /// <summary>
        /// 得到可供进行验证的方式
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        static public byte[] Get_Checking_Method(byte[] Data)
        {
            if (Get_Which_Type(Data) == 1)
            {
                List<byte> Method_List = new List<byte>();
                Method_List.AddRange(Data.Skip(2).Take(Data[1]));
                return Method_List.ToArray();
            }
            else
            {
                return new byte[] { };
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="Data"></param>
        /// <returns>
        /// int type代理协议 -1 不明,1:TCP,3:UDP
        /// string IP
        /// int PORT
        /// </returns>
        static public(int type, IPAddress IP, int port) Get_Request_Info(byte[] Data)
        {
            IPAddress Host_IP = null;
            int Port = 0;
            byte[] Bytes_Port = new byte[2];
            int Which_Type = Get_Which_Type(Data);
            try
            {
                if (1 < Which_Type)
                {

                    if (new List<int> { 2, 5 }.Contains(Which_Type))
                    {
                        //IPV4
                        Host_IP = new IPAddress(Data.Skip(4).Take(4).ToArray());
                        Bytes_Port = (Data.Skip(8).Take(2).ToArray());
                    }
                    else if (new List<int> { 3, 6 }.Contains(Which_Type))
                    {
                        //域名解析IP
                        Host_IP = Dns.GetHostEntry(Encoding.UTF8.GetString(Data.Skip(5).Take(Data[4]).ToArray())).AddressList[0];
                        Bytes_Port = (Data.Skip(5 + Data[4]).Take(2).ToArray());
                    }
                    else if (new List<int> { 4, 7 }.Contains(Which_Type)) {
                        //IPV6
                        Host_IP = new IPAddress(Data.Skip(4).Take(16).ToArray());
                        Bytes_Port = (Data.Skip(8).Take(2).ToArray());
                    }
                    Port = (Bytes_Port[0] << 8) + Bytes_Port[1];
                    Console.WriteLine(string.Format("解析IP:{0},Port:{1}", Host_IP, Port));
                    return (Data[1], Host_IP, Port);
                }
            }
            catch
            {
                
            }
            return (0,IPAddress.Parse("127.0.0.1"), 0);
        }

        /// <summary>
        /// 得到UDP转发数据头
        /// </summary>
        /// <param name="IP_Endpoint">IPEndPoint</param>
        /// <returns></returns>
        static public byte[] Get_UDP_Header(IPEndPoint IP_Endpoint) {
            int IP_Len = IP_Endpoint.Address.GetAddressBytes().Length;
            List<byte> Bytes_IPEndPoint = new List<byte>();
            if (IP_Len == 4)
            {
                //IPV4
                Bytes_IPEndPoint.AddRange(new byte[] { 5, 0, 0, 1 });
                
            }
            else if(IP_Len == 16){ 
                //IPV6
                Bytes_IPEndPoint.AddRange(new byte[] { 5, 0, 0, 4 });
            }

            Bytes_IPEndPoint.AddRange(IP_Endpoint.Address.GetAddressBytes());
            //Port为int32储存,实际应用中为Uint16,需要去掉前两个字节
            Bytes_IPEndPoint.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(IP_Endpoint.Port)).Skip(2));
            
            return Bytes_IPEndPoint.ToArray();
        }

        /// <summary>
        /// 得到UDP转发的目标地址
        /// </summary>
        /// <param name="Data">待转发数据</param>
        /// <returns></returns>
        static public IPEndPoint Get_UDP_ADDR(byte[] Data)
        {
            IPAddress Host_IP = null;
            byte[] Bytes_Port = new byte[2];
            int Port = 0;
            switch (Data[3]){
                case 1:
                    Host_IP = new IPAddress(Data.Skip(4).Take(4).ToArray());
                    Bytes_Port = (Data.Skip(8).Take(2).ToArray());
                    break;
                case 3:
                    Host_IP = Dns.GetHostEntry(Encoding.UTF8.GetString(Data.Skip(5).Take(Data[4]).ToArray())).AddressList[0];
                    Bytes_Port = (Data.Skip(5 + Data[4]).Take(2).ToArray());
                    break;
                case 4:
                    Host_IP = new IPAddress(Data.Skip(4).Take(16).ToArray());
                    Bytes_Port = (Data.Skip(8).Take(2).ToArray());
                    break;
            }
            Port = (Bytes_Port[0] << 8) + Bytes_Port[1];
            IPEndPoint ADDR = new IPEndPoint(Host_IP, Port);
            return ADDR;
        }

        /// <summary>
        /// 建立TCP连接
        /// </summary>
        /// <param name="IP">IP</param>
        /// <param name="Port">PORT</param>
        /// <returns></returns>
        static public TcpClient Connecte_TCP(IPAddress IP, int Port)
        {
            TcpClient tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect(IP, Port);              
            }
            catch
            {

            }
            return tcpClient;
        }


        /// <summary>
        /// 对比两个数据
        /// </summary>
        /// <param name="array_First">数据1</param>
        /// <param name="array_Second">数据2</param>
        /// <returns></returns>
        static public bool Data_Pare(byte[] array_First, byte[] array_Second)
        {
            if (array_First.Length == array_Second.Length)
            {
                for (int i = 0; i < array_First.Length; i++)
                {
                    if (array_First[i] != array_Second[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }


        }


    }
}
