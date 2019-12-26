using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace Socks5Server.Core
{
    class TCP_Listen
    {
        private int Data_Size = 1024;
        private byte[] Receive_Data;
        int TimeOut =1000 * 5;
        UDP_Listen UDP_Listener;
        bool UDP_Support = true;
        public TCP_Listen(IPAddress IP,int Port, bool Udp_Support = true) {
            Receive_Data = new byte[Data_Size];
            TcpListener Socks_Server = new TcpListener(IP, Port);
            DataHandle.WriteLog(string.Format("Socks服务已启动,监听{0}端口中", Port));
            Socks_Server.Start();
            Socks_Server.BeginAcceptTcpClient(AcceptTcpClient,Socks_Server);
            if (Udp_Support)
            {
                UDP_Listener = new UDP_Listen(Port);
            }
            else {
                UDP_Support = Udp_Support;
            }
        }

        private void AcceptTcpClient(IAsyncResult ar) {
            TcpListener Socks_Server = ar.AsyncState as TcpListener;
            TcpClient Tcp_Client = Socks_Server.EndAcceptTcpClient(ar);
            //超时设置
            Tcp_Client.ReceiveTimeout = Tcp_Client.SendTimeout = TimeOut;
            Task task = new Task(()=> {
                TCP_Connect(Tcp_Client);
            });
            task.Start();
            Socks_Server.BeginAcceptTcpClient(new AsyncCallback(AcceptTcpClient),Socks_Server);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="tcpClient">TCPClient</param>
        /// <param name="Data">数据</param>
        private void TCP_Send(TcpClient tcpClient,byte[] Data) {
            tcpClient.GetStream().Write(Data);
        }

        private void TCP_Connect(object state) {
            TcpClient Tcp_Client = state as TcpClient;
            DataHandle.WriteLog(string.Format("接收来自{0},当前线程数:{1}",Tcp_Client.Client.RemoteEndPoint,ThreadPool.ThreadCount));
            NetworkStream TCP_Stream = Tcp_Client.GetStream();
            var State_VT = (Tcp_Client, TCP_Stream);
            TCP_Stream.BeginRead(Receive_Data,0,Data_Size,new AsyncCallback(TCP_Receive),State_VT);
        }

        /// <summary>
        /// 读取回调
        /// </summary>
        /// <param name="ar"></param>
        private void TCP_Receive(IAsyncResult ar) {
            var State_Vt = (((TcpClient Tcp_Client, NetworkStream TCP_Stream))ar.AsyncState);
            try
            {
                int size = State_Vt.TCP_Stream.EndRead(ar);
                if (size > 0)
                {
                    byte[] Methods = DataHandle.Get_Checking_Method(Receive_Data.Take(size).ToArray());
                    int Data_Type = DataHandle.Get_Which_Type(Receive_Data.Take(size).ToArray());
                    //请求建立连接

                    if (Methods.Contains((byte)0))
                    {
                        TCP_Send(State_Vt.Tcp_Client, DataHandle.No_Authentication_Required);
                        State_Vt.TCP_Stream.BeginRead(Receive_Data, 0, Data_Size, new AsyncCallback(TCP_Receive), State_Vt);
                    }
                    //接受代理目标端信息
                    else if (1 < Data_Type && Data_Type < 8)
                    {
                        var Request_Info = DataHandle.Get_Request_Info(Receive_Data.Take(size).ToArray());
                        if (Request_Info.type == 1)
                        {
                            //TCP
                            TcpClient Tcp_Proxy = DataHandle.Connecte_TCP(Request_Info.IP, Request_Info.port);
                            if (Tcp_Proxy.Connected)
                            {
                                new TCP_Server(State_Vt.Tcp_Client, Tcp_Proxy);
                                TCP_Send(State_Vt.Tcp_Client, DataHandle.Proxy_Success);                             
                            }
                            else
                            {
                                TCP_Send(State_Vt.Tcp_Client, DataHandle.Connect_Fail);
                                throw (new SocketException());
                            }
                        }
                        else if (Request_Info.type == 3) {
                            //UDP 
                            //判断是否开启UDP支持及UDP阈值
                            if (UDP_Support && UDP_Listen.Surplus_Proxy_Count > 0)
                            {
                                //得到客户端开放UDP端口
                                IPEndPoint ClientPoint = new IPEndPoint((State_Vt.Tcp_Client.Client.RemoteEndPoint as IPEndPoint).Address, Request_Info.port);
                                UDP_Listener.Add_Server(ClientPoint, State_Vt.Tcp_Client);
                                byte[] header = DataHandle.Get_UDP_Header(State_Vt.Tcp_Client.Client.LocalEndPoint as IPEndPoint);
                                TCP_Send(State_Vt.Tcp_Client, header);
                            }
                            else
                            {
                                throw (new SocketException());
                            }
                        }
                    }
                }
                else
                {
                    throw (new SocketException());
                }
            }
            catch (Exception)
            {
                Close(State_Vt.Tcp_Client);
            }

        }



        /// <summary>
        /// 关闭客户端连接
        /// </summary>
        /// <param name="Tcp_Client">客户端TCPClient</param>
        private void Close(TcpClient Tcp_Client)
        {
            try
            {
                if (Tcp_Client.Connected)
                {
                    DataHandle.WriteLog(string.Format("已关闭客户端{0}的连接", Tcp_Client.Client.RemoteEndPoint));
                    Tcp_Client.GetStream().Close();
                    Tcp_Client.Close();
                }

            }
            catch (SocketException){ 
            
            }
        }

    }
}
