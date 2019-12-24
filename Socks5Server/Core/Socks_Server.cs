using System;
using System.Collections.Generic;
using System.Text.Encodings;
using System.Net.Sockets;
using System.Net;
using System.Linq;
using Socks5Server;
using System.IO;

namespace Socks5Server.Core
{
    class Socks_Server
    {
        byte[] Client_Data, Proxy_Data,UDP_Receive;
        NetworkStream Client_Stream, Proxy_Stream;
        int Data_Size { get { return 1024 * 30; } }
        AsyncCallback C_Read_Delegate, P_Read_Delegate, UDP_Receive_Delegate;
        TcpClient TCP_Client, TCP_Proxy;
        public UdpClient UDP_Client;
        IPEndPoint UDP_Client_Point, UDP_Remote_Point;
        List<IPEndPoint> Proxy_Point = new List<IPEndPoint>();

        public Socks_Server(TcpClient Tcp_Client, TcpClient Tcp_Proxy)
        {
            try
            {
                Client_Data = new byte[Data_Size];
                Proxy_Data = new byte[Data_Size];
                TCP_Proxy = Tcp_Proxy;
                TCP_Client = Tcp_Client;
                Client_Stream = TCP_Client.GetStream();
                Proxy_Stream = TCP_Proxy.GetStream();
                C_Read_Delegate = new AsyncCallback(TCP_Client_Receive);
                P_Read_Delegate = new AsyncCallback(TCP_Proxy_Receive);
                Client_Stream.BeginRead(Client_Data, 0, Data_Size, C_Read_Delegate, null);
                Proxy_Stream.BeginRead(Proxy_Data, 0, Data_Size, P_Read_Delegate, null);
            }
            catch (SocketException)
            {
                close();
            }
        }

        public Socks_Server(IPEndPoint Client_Point,TcpClient Tcp_Client)
        {
            try
            {
                TCP_Client = Tcp_Client;
                UDP_Client = new UdpClient(0);
                UDP_Client_Point = Client_Point;
                UDP_Client.Client.ReceiveTimeout = UDP_Client.Client.SendTimeout = 5000;
                UDP_Receive_Delegate += new AsyncCallback(UDP_Client_Receive);
                UDP_Client.BeginReceive(UDP_Receive_Delegate, null);
            }
            catch (Exception){
                UDP_Client = null;              
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="TcpStream">TCP流</param>
        /// <param name="Data">数据</param>
        private void TCP_Socks_Send(NetworkStream TcpStream, byte[] Data)
        {
            try
            {
                TcpStream.Write(Data);
            }
            catch
            {
                close();
            }
        }

        /// <summary>
        /// 客户端接收数据回调
        /// </summary>
        /// <param name="ar"></param>
        private void TCP_Client_Receive(IAsyncResult ar)
        {
            try
            {
                int Size = Client_Stream.EndRead(ar);

                if (Size > 0)
                {
                    TCP_Socks_Send(Proxy_Stream, Client_Data.Take(Size).ToArray());
                    Client_Stream.BeginRead(Client_Data, 0, Data_Size, C_Read_Delegate, null);
                }
                else
                {
                    close();
                }

            }
            catch
            {
                close();
            }
        }

        /// <summary>
        /// 代理端接收数据回调
        /// </summary>
        /// <param name="ar"></param>
        private void TCP_Proxy_Receive(IAsyncResult ar)
        {
            try
            {
                int Size = Proxy_Stream.EndRead(ar);

                if (Size > 0)
                {
                    TCP_Socks_Send(Client_Stream, Proxy_Data.Take(Size).ToArray());
                    Proxy_Stream.BeginRead(Proxy_Data, 0, Data_Size, P_Read_Delegate, null);
                }
                else
                {
                    close();
                }

            }
            catch
            {
                close();
            }
        }

        /// <summary>
        /// TCP清理
        /// </summary>
        private void close()
        {
            try
            {
                if (TCP_Client != null)
                {

                    Program.PrintLog(string.Format("已断开客户端{0}的连接", TCP_Client.Client.RemoteEndPoint));
                    Client_Stream.Close();
                    TCP_Client.Close();
                    TCP_Client = null;
                }
                if (TCP_Proxy != null)
                {
                    Program.PrintLog(string.Format("已断开代理端{0}的连接", TCP_Proxy.Client.RemoteEndPoint));
                    Proxy_Stream.Close();
                    TCP_Proxy.Close();
                    TCP_Proxy = null;
                }
            }
            catch (Exception)
            {

            }
        }

        private void UDP_Socks_Send(UdpClient UDP_Client, byte[] Send_Data, IPEndPoint ADDR)
        {
            try
            {
                if (Datahandle.TCP_Is_Connected(TCP_Client))
                {
                    UDP_Client.Send(Send_Data, Send_Data.Length, ADDR);
                }
                else
                {
                    UDP_Close();
                }
            }
            catch (Exception)
            {
                UDP_Close();
            }
        }

        private void UDP_Client_Receive(IAsyncResult ar)
        {
            try
            {           
                UDP_Receive = UDP_Client.EndReceive(ar, ref UDP_Remote_Point);
                //判断是否需要初始化
                if (UDP_Client_Point.Port == 0 && UDP_Client_Point.Address.Equals(UDP_Remote_Point.Address))
                {
                    UDP_Client_Point = UDP_Remote_Point;
                    Program.PrintLog(string.Format("正式开启UDP代理隧道,客户端{0},Port:{1}", UDP_Client_Point.Address, UDP_Client_Point.Port));
                }

                if (UDP_Receive.Length > 0)
                {

                    if (UDP_Remote_Point == UDP_Client_Point && UDP_Receive[2] == 0)
                    {

                        //记录转发头
                        int header_len = 0;
                        switch (UDP_Receive[3])
                        {
                            case 1://IPV4
                                header_len = 10;
                                break;
                            case 3://域名
                                header_len = 7 + UDP_Receive[4];
                                break;
                            case 4://IPV6
                                header_len = 22;
                                break;
                        }
                        var UDP_ADDR = Datahandle.Get_UDP_ADDR(UDP_Receive);
                        Proxy_Point.Add(UDP_ADDR);
                        UDP_Socks_Send(UDP_Client, UDP_Receive.Skip(header_len).ToArray(), UDP_ADDR);
                    }
                    else if (Proxy_Point.Contains(UDP_Remote_Point))
                    {
                        byte[] Header = Datahandle.Get_UDP_Header(UDP_Remote_Point, true);
                        UDP_Socks_Send(UDP_Client, Header.Concat(UDP_Receive).ToArray(), UDP_Client_Point);
                    }
                    UDP_Client.BeginReceive(UDP_Receive_Delegate, null);
                }
                else
                {
                    UDP_Close();
                }

            }
            catch(Exception) {                           
                UDP_Close();
            }
        }

        private void UDP_Close() {
            try
            {
                TCP_Client.GetStream().Close();
                TCP_Client.Close();
                UDP_Close();
            }
            catch(Exception) { 
            
            }
        }

    }
}
