using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Threading;

namespace Socks5Server.Core
{
    class UDP_Listen
    {

        List<(IPAddress IP_Addr, TcpClient TCP_Client)>  Init_Addr_List = new List<(IPAddress IP_Addr, TcpClient TCP_Client)>();
        List<(IPEndPoint IP_EndPoint, UDP_Server Udp_Server)> UDP_Proxy_List = new List<(IPEndPoint IP_EndPoint, UDP_Server Udp_Server)>();
        UdpClient UDP_Listener;
        //UDP并发限制
        static int Surplus_Count = 300;
        public static int Surplus_Proxy_Count { get { return Surplus_Count; } private set { Surplus_Count = value; } }
        Timer timer;
        public UDP_Listen(int Port){
            UDP_Listener = new UdpClient(Port);
            UDP_Listener.BeginReceive(UDP_Receive,null);
            //检查并剔除已经失效的TCPClient           
            timer = new Timer(new TimerCallback(Usability_Check), null, 5000, 5000);
        }


        /// <summary>
        /// 增加一条UDP代理隧道
        /// </summary>
        /// <param name="IP_EndPoint">客户端远程地址</param>
        /// <param name="TCP_Client">TCP依赖</param>
        public void Add_Server(IPEndPoint IP_EndPoint,TcpClient TCP_Client) {
            if (IP_EndPoint.Port == 0)
            {
                Init_Addr_List.Add((IP_EndPoint.Address, TCP_Client));
            }
            else {
                UDP_Server Udp_Server = new UDP_Server(IP_EndPoint, TCP_Client, Return_Source);
                UDP_Proxy_List.Add((Udp_Server.Client_Point, Udp_Server));
            }
            Surplus_Proxy_Count--;
            DataHandle.WriteLog($"已开启对{IP_EndPoint}的UDP代理隧道");
             
        }

        /// <summary>
        /// 定时器:检查可用性
        /// </summary>
        /// <param name="state"></param>
        private void Usability_Check(object state) {
            //不能直接在foreach中进行移除操作,不然会抛出异常
            //解决线程安全
            try
            {
                foreach (var i in UDP_Proxy_List.ToArray())
                {
                    if (!DataHandle.TCP_Usability(i.Udp_Server.TCP_Client))
                    {
                        i.Udp_Server.Close();
                        UDP_Proxy_List.Remove(i);
                        Surplus_Proxy_Count++;
                    }
                }
            }
            catch (Exception) { 
            
            }
        }

        /// <summary>
        /// 回源
        /// </summary>
        /// <param name="Client_Point">客户端远程地址</param>
        /// <param name="Send_Data">待发送数据</param>
        private void Return_Source(IPEndPoint Client_Point, byte[] Send_Data) {
            UDP_Listener.Send(Send_Data, Send_Data.Length, Client_Point);
        }

        /// <summary>
        /// UDP接收回调
        /// </summary>
        /// <param name="ar">回调对象</param>
        public void UDP_Receive(IAsyncResult ar) {
            IPEndPoint Remote_Point = null;
            byte[] Receive_Data = UDP_Listener.EndReceive(ar,ref Remote_Point);
            int header_len = 0;
            var Rs = Which_Client(Remote_Point);
            if (Rs != null && Receive_Data[2] == 0) {
                switch (Receive_Data[3])
                {
                    case 1://IPV4
                        header_len = 10;
                        break;
                    case 3://域名
                        header_len = 7 + Receive_Data[4];
                        break;
                    case 4://IPV6
                        header_len = 22;
                        break;
                }
                var UDP_ADDR = DataHandle.Get_UDP_ADDR(Receive_Data);
                Rs.UDP_Send(Receive_Data.Skip(header_len).ToArray(), UDP_ADDR);
            }
            UDP_Listener.BeginReceive(new AsyncCallback(UDP_Receive), null);
            
        }

        /// <summary>
        /// 分辨远程端口来源
        /// </summary>
        /// <param name="Remote_Point"></param>
        /// <returns></returns>
        private UDP_Server Which_Client(IPEndPoint Remote_Point) {
            //直接转发
            for (int i = 0; i < UDP_Proxy_List.Count; i++)
            {
                if (UDP_Proxy_List[i].IP_EndPoint == Remote_Point)
                {
                    //记录转发头
                    return UDP_Proxy_List[i].Udp_Server;
                }
            }
            //需要初始化
            for (int i = 0; i < Init_Addr_List.Count; i++)
            {
                if (Init_Addr_List[i].IP_Addr.Equals(Remote_Point.Address))
                {
                    UDP_Server Udp_Server = new UDP_Server(Remote_Point, Init_Addr_List[i].TCP_Client, Return_Source);
                    Init_Addr_List.Remove(Init_Addr_List[i]);
                    if (Udp_Server.UDP_Client != null) {                       
                        UDP_Proxy_List.Add((Udp_Server.Client_Point, Udp_Server));
                        return Udp_Server;
                    }
                }
            }
            return null;
        }
    }
}
