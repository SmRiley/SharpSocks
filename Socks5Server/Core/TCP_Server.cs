using System;
using System.Net.Sockets;
using System.Linq;

namespace Socks5Server.Core
{
    class TCP_Server
    {
        byte[] Client_Data, Proxy_Data;
        NetworkStream Client_Stream, Proxy_Stream;
        int Data_Size { get { return 1024 * 30; } }
        AsyncCallback C_Read_Delegate, P_Read_Delegate;
        TcpClient TCP_Client, TCP_Proxy;
        
        public TCP_Server(TcpClient Tcp_Client, TcpClient Tcp_Proxy)
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
                DataHandle.WriteLog($"开启对{TCP_Client.Client.RemoteEndPoint}的TCP代理隧道");
            }
            catch (SocketException)
            {
                close();
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

                    DataHandle.WriteLog($"已断开客户端{TCP_Client.Client.RemoteEndPoint}的连接");
                    Client_Stream.Close();
                    TCP_Client.Close();
                    TCP_Client = null;
                }
                if (TCP_Proxy != null)
                {
                    DataHandle.WriteLog($"已断开代理端{TCP_Proxy.Client.RemoteEndPoint}的连接");
                    Proxy_Stream.Close();
                    TCP_Proxy.Close();
                    TCP_Proxy = null;
                }
            }
            catch (Exception)
            {

            }
        }


    }
}
