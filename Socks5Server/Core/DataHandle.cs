using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Socks5Server
{
    class DataHandle
    {

        /// <summary>
        /// 无需验证
        /// </summary>
        public byte[] No_Authentication_Required = new byte[] { 5, 0 };

        public byte[] Connect_Fail = new byte[] { 5, 0xFF };

        /// <summary>
        /// 需要身份验证
        /// </summary>
        public byte[] Authentication_Required = new byte[] { 5, 2 };

        /// <summary>
        /// 身份验证成功
        /// </summary>
        public byte[] Authentication_Success = new byte[] { 1, 0 };

        public byte[] Proxy_Success = new byte[] { 5, 0, 0, 1, 0, 0, 0, 0, 0, 0 };
        public byte[] Proxy_Error = new byte[] { 5, 1, 0, 1, 0, 0, 0, 0, 0, 0 };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Data"></param>
        /// <returns>
        /// 1.建立连接请求
        /// 2.转发请求TCP(IP)
        /// 3.转发请求TCP(域名)
        /// 4.转发请求UDP(IP)
        /// 5.转发请求UDP(域名)
        /// 6.其它
        /// </returns>
        public int Get_Which_Type(byte[] Data)
        {

            if (Data.Length > 2 && Data.Length == Data[1] + 2)
            {
                return 1;
            }
            else if (Data.Length > 8 && Data[1] == (byte)1)
            {
                ///TCP请求
                if (Data[3] == (byte)1 && Data.Length == 10)
                {
                    return 2;
                }
                else if (Data[3] == (byte)3 && +Data.Length == (Data.Skip(5).Take(Data[4]).Count() + 7))
                {
                    return 3;
                }
            }
            else if (Data.Length > 8 && Data[1] == (byte)3)
            {
                //UDP请求
                if (Data[3] == (byte)1 && Data.Length == 10)
                {
                    return 4;
                }
                else if (Data[3] == (byte)3 && +Data.Length == (Data.Skip(5).Take(Data[4]).Count() + 7))
                {
                    return 5;
                }

            }

            return 6;


        }


        /// <summary>
        /// 得到可供进行验证的方式
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public byte[] Get_Checking_Method(byte[] Data)
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
        public (int type, string IP, int port) Get_Request_Info(byte[] Data)
        {
            string Host_IP = "";
            int Port = 0;
            byte[] Bytes_Port = new byte[2];
            int Which_Type = Get_Which_Type(Data);
            try
            {
                if (1 < Which_Type && Which_Type < 6)
                {
                    if (Which_Type == 2 || Which_Type == 4)
                    {
                        byte[] IP = Data.Skip(4).Take(4).ToArray();
                        for (int i = 0; i < IP.Length; i++)
                        {
                            if (i < 3)
                            {
                                Host_IP += IP[i].ToString() + ".";
                            }
                            else
                            {
                                Host_IP += IP[i].ToString();
                            }
                        }
                        Bytes_Port = (Data.Skip(8).Take(2).ToArray());

                    }
                    else if (Which_Type == 5 || Which_Type == 3)
                    {
                        //域名解析IP
                        Host_IP = Dns.GetHostEntry(Encoding.UTF8.GetString(Data.Skip(5).Take(Data[4]).ToArray())).AddressList[0].ToString();
                        Bytes_Port = (Data.Skip(5 + Data[4]).Take(2).ToArray());
                    }
                    Port = (Bytes_Port[0] << 8) + Bytes_Port[1];
                    Console.WriteLine(Host_IP + "," + Port);
                    return (Data[1], Host_IP, Port);
                }
            }
            catch
            {

            }
            return (0, "", 0);
        }


        /// <summary>
        /// 转发请求包
        /// </summary>
        /// <param name="CMD">命令码</param>
        /// <param name="ATYP">类型</param>
        /// <param name="ADDR">目的地址</param>
        /// <param name="PORT">端口</param>
        /// <returns></returns>
        public byte[] Proxy_Data(byte CMD, byte ATYP, string ADDR, Int16 PORT)
        {
            List<byte> List_Byte = new List<byte>();
            List_Byte.Add(0X05);
            List_Byte.Add(CMD);
            List_Byte.Add(0X00);
            List_Byte.Add(ATYP);
            byte[] ADDR_Arr = Encoding.UTF8.GetBytes(ADDR);
            List_Byte.Add((byte)ADDR_Arr.Length);
            foreach (byte b in ADDR_Arr)
            {
                List_Byte.Add(b);
            }
            //端口转byte
            if (PORT > byte.MaxValue)
            {
                byte[] PORT_ARR = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(PORT));
                List_Byte.AddRange(PORT_ARR);
            }
            else
            {
                List_Byte.Add(0);
                List_Byte.Add(Convert.ToByte(PORT));
            }
            return new byte[] { 05, 01, 00, 01, 0XCA, 0X6C, 0X16, 0X05, 0X00, 0X50 };
            //return List_Byte.ToArray();
        }

        public TcpClient Connecte_Tcp(int type, string IP, int Port)
        {
            TcpClient tcpClient = new TcpClient();
            if (type == 1)
            {
                
                try
                {
                    tcpClient.Connect(IP, Port);
                    
                }
                catch
                {

                }
            }
            /* else (type == 3){

             }else { 

             }*/

            return tcpClient;
        }


        /// <summary>
        /// 对比两个数据
        /// </summary>
        /// <param name="array_First">数据1</param>
        /// <param name="array_Second">数据2</param>
        /// <returns></returns>
        public bool Data_Pare(byte[] array_First, byte[] array_Second)
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
