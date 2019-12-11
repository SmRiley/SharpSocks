using System;
using System.Text.Encodings;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using Socks5Server;

namespace Socks5Server.Core
{
    class Socsk_Listen
    {
        private int Data_Size = 1024;
        private byte[] Receive_Data;
        DataHandle dataHandle;

        public Socsk_Listen(IPAddress IP,int Port) {
            Receive_Data = new byte[Data_Size];
            dataHandle = new DataHandle();
            TcpListener Socks_Server = new TcpListener(IP, Port);
            Program.PrintLog(string.Format("Socks服务已启动,监听{0}端口中", Port));
            Socks_Server.Start();
            Socks_Server.BeginAcceptTcpClient(AcceptTcpClient,Socks_Server);
        }

    

        private void AcceptTcpClient(IAsyncResult ar) {
            TcpListener Socks_Server = ar.AsyncState as TcpListener;
            TcpClient Tcp_Client = Socks_Server.EndAcceptTcpClient(ar);
            Task task = new Task(()=> {
                Socks_Connect(Tcp_Client);
            });
            task.Start();
            Socks_Server.BeginAcceptTcpClient(new AsyncCallback(AcceptTcpClient),Socks_Server);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="tcpClient">TCPClient</param>
        /// <param name="Data">数据</param>
        private void Socks_Send(TcpClient tcpClient,byte[] Data) {
            tcpClient.GetStream().Write(Data);
        }


        private void Socks_Connect(object state) {
            TcpClient Tcp_Client = state as TcpClient;
            Program.PrintLog(string.Format("接收来自{0},当前线程数:{1}",Tcp_Client.Client.RemoteEndPoint,ThreadPool.ThreadCount));
            NetworkStream TCP_Stream = Tcp_Client.GetStream();
            var State_VT = (Tcp_Client, TCP_Stream);
            TCP_Stream.BeginRead(Receive_Data,0,Data_Size,new AsyncCallback(Socks_Receive),State_VT);
        }

        /// <summary>
        /// 读取回调
        /// </summary>
        /// <param name="ar"></param>
        private void Socks_Receive(IAsyncResult ar) {
            var State_Vt = (((TcpClient Tcp_Client, NetworkStream TCP_Stream))ar.AsyncState);
            try
            {
                int size = State_Vt.TCP_Stream.EndRead(ar);
                if (size > 0)
                {
                    Program.PrintLog(string.Format("收到客户端的{0}个字节数据", size));
                    byte[] Methods = dataHandle.Get_Checking_Method(Receive_Data.Take(size).ToArray());
                    int Data_Type = dataHandle.Get_Which_Type(Receive_Data.Take(size).ToArray());
                    //请求建立连接

                    if (Methods.Contains((byte)0))
                    {
                        Socks_Send(State_Vt.Tcp_Client, dataHandle.No_Authentication_Required);
                        Program.PrintLog("等待接受代理目标信息");
                        State_Vt.TCP_Stream.BeginRead(Receive_Data, 0, Data_Size, new AsyncCallback(Socks_Receive), State_Vt);
                    }
                    //接受代理目标端信息
                    else if (2 < Data_Type && Data_Type < 6)
                    {
                        var Request_Info = dataHandle.Get_Request_Info(Receive_Data.Take(size).ToArray());
                        TcpClient Tcp_Proxy = dataHandle.Connecte_Tcp(Request_Info.type, Request_Info.IP, Request_Info.port);
                        if (Tcp_Proxy.Connected)
                        {
                            new Socks_Server(State_Vt.Tcp_Client, Tcp_Proxy);
                            Socks_Send(State_Vt.Tcp_Client, dataHandle.Proxy_Success);
                            Program.PrintLog("正式建立开启代理");
                        }
                        else
                        {
                            Socks_Send(State_Vt.Tcp_Client, dataHandle.Connect_Fail);
                            Close(State_Vt.Tcp_Client);
                        }
                    }
                }else
                {
                    Close(State_Vt.Tcp_Client);
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
                    Program.PrintLog(string.Format("已关闭客户端{0}的连接", Tcp_Client.Client.RemoteEndPoint));
                    Tcp_Client.GetStream().Close();
                    Tcp_Client.Close();
                }
            }
            catch (SocketException){ 
            
            }
        }

    }
}
