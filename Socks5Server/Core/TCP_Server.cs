using System;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Socks5Server.Core
{
    class TCP_Server
    {
   
      
        static int Data_Size { get { return 1024 * 50; } }
        byte[] Client_Data = new byte[Data_Size];
        byte[] Proxy_Data = new byte[Data_Size];
        TcpClient TCP_Client, TCP_Proxy;
        NetworkStream Client_Stream, Proxy_Stream;
        public TCP_Server(TcpClient Tcp_Client, TcpClient Tcp_Proxy)
        {
            try
            {
                TCP_Proxy = Tcp_Proxy;
                TCP_Client = Tcp_Client;
                Client_Stream = TCP_Client.GetStream();
                Proxy_Stream = TCP_Proxy.GetStream();
                Client_Stream.BeginRead(Client_Data, 0, Data_Size, TCP_Client_Receive, null);
                Proxy_Stream.BeginRead(Proxy_Data, 0, Data_Size, TCP_Proxy_Receive,null);
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

                byte[] Data = DataHandle.De_Bytes(Client_Data.Take(Client_Stream.EndRead(ar)).ToArray());

                if (Data.Length > 0)
                {
                    TCP_Socks_Send(Proxy_Stream, Data);
                    Client_Stream.BeginRead(Client_Data, 0, Data_Size, TCP_Client_Receive, null);
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
                byte[] Data = DataHandle.En_Bytes(Proxy_Data.Take(Proxy_Stream.EndRead(ar)).ToArray());
                if (Data.Length > 0)
                {
                    TCP_Socks_Send(Client_Stream,Data);
                    Proxy_Stream.BeginRead(Proxy_Data, 0, Data_Size, TCP_Proxy_Receive, null);
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
