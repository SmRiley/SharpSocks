using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;

namespace Socks5Server.Core
{
    class UDP_Server
    {
        public IPEndPoint Client_Point;
        public TcpClient TCP_Client;
        public UdpClient UDP_Client;
        List<IPEndPoint> Proxy_Point_List = new List<IPEndPoint>();
        public delegate void Return_Source_Dg(IPEndPoint Client_Point, byte[] Send_Data);
        Return_Source_Dg Return_Source;

        public UDP_Server(IPEndPoint IP_Point, TcpClient Tcp_Client, Return_Source_Dg RS) {
            Client_Point = IP_Point;
            TCP_Client = Tcp_Client;
            try{
                UDP_Client = new UdpClient(0);
                UDP_Client.Client.ReceiveTimeout = 1000 * 5;
                UDP_Client.Client.SendTimeout = 1000 * 5;
                Return_Source = new Return_Source_Dg(RS);
                UDP_Client.BeginReceive(new AsyncCallback(UDP_Recieve), null);
            }catch(SocketException) {
                UDP_Client = null;
            }
        }

        /// <summary>
        /// UDP接收回调
        /// </summary>
        /// <param name="ar"></param>
        private void UDP_Recieve(IAsyncResult ar) {
            try
            {
                IPEndPoint Remote_Point = null;
                byte[] Receive_Data = UDP_Client.EndReceive(ar, ref Remote_Point);
                if (Receive_Data.Length > 0)
                {
                    if (Proxy_Point_List.Contains(Remote_Point))
                    {
                        var Header = DataHandle.Get_UDP_Header(Remote_Point);
                        var Send_Data = Header.Concat(Receive_Data).ToArray();
                        Return_Source(Client_Point, Send_Data);
                    }

                }
                UDP_Client.BeginReceive(new AsyncCallback(UDP_Recieve), null);
            }
            catch (Exception){

            }
        }


        public void UDP_Send(byte[] Send_Data,IPEndPoint IP_EndPoint) {
            UDP_Client.Send(Send_Data, Send_Data.Length, IP_EndPoint);
            Proxy_Point_List.Add(IP_EndPoint);
        }

        /// <summary>
        /// 关闭UDP对象及TCP依赖
        /// </summary>
        public void Close() {
            try
            {
                DataHandle.WriteLog($"已关闭{Client_Point}的UDP代理隧道");
                UDP_Client.Close();
                TCP_Client.GetStream().Close();
                TCP_Client.Close();
            }
            catch (Exception){ 
            
            }
        }
    }
}
