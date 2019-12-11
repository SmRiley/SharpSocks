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
        byte[] Client_Data, Proxy_Data;
        NetworkStream Client_Stream, Proxy_Stream;
        int Data_Size = 1024 * 30;
        DataHandle dataHandle;
        AsyncCallback C_Read_Delegate,P_Read_Delegate;
        TcpClient Tcp_Client, Tcp_Proxy;
        public Socks_Server(TcpClient TcpClient, TcpClient TcpProxy) {
            try
            {
                Client_Data = new byte[Data_Size];
                Proxy_Data = new byte[Data_Size];
                Tcp_Proxy = TcpProxy;
                Tcp_Client = TcpClient;
                Client_Stream = Tcp_Client.GetStream();
                Proxy_Stream = Tcp_Proxy.GetStream();
                dataHandle = new DataHandle();
                C_Read_Delegate = new AsyncCallback(Client_Receive);
                P_Read_Delegate = new AsyncCallback(Proxy_Receive);
                Client_Stream.BeginRead(Client_Data, 0, Data_Size, C_Read_Delegate, null);
                Proxy_Stream.BeginRead(Proxy_Data, 0, Data_Size, P_Read_Delegate, null);
            }
            catch (SocketException){
                close();
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="TcpStream">TCP流</param>
        /// <param name="Data">数据</param>
        private void Socks_Send(NetworkStream TcpStream, byte[] Data)
        {
            try
            {
                TcpStream.Write(Data);
            }
            catch {
                close();
            }
        }

        /// <summary>
        /// 客户端接收数据回调
        /// </summary>
        /// <param name="ar"></param>
        private void Client_Receive(IAsyncResult ar) {
            try
            {
                int Size = Client_Stream.EndRead(ar);

                if (Size > 0)
                {
                    Socks_Send(Proxy_Stream, Client_Data.Take(Size).ToArray());
                    Client_Stream.BeginRead(Client_Data, 0, Data_Size, C_Read_Delegate, null);
                }
                else {
                    close();
                }

            }
            catch {
                close();
            }
        }

        /// <summary>
        /// 代理端接收数据回调
        /// </summary>
        /// <param name="ar"></param>
        private void Proxy_Receive(IAsyncResult ar) {
            try
            {
                int Size = Proxy_Stream.EndRead(ar);

                if (Size > 0)
                {
                    Socks_Send(Client_Stream, Proxy_Data.Take(Size).ToArray());
                    Proxy_Stream.BeginRead(Proxy_Data, 0, Data_Size, P_Read_Delegate, null);
                }
                else {
                    close();
                }
                
            }
            catch {
                close();
            }
        }

        /// <summary>
        /// 清理
        /// </summary>
        void close() {
            if (Tcp_Client != null)
            {
                try
                {
                    Program.PrintLog(string.Format("已断开客户端{0}的连接", Tcp_Client.Client.RemoteEndPoint));
                    Client_Stream.Close();
                    Tcp_Client.Close();
                    Tcp_Client = null;
                }
                catch (NullReferenceException) { 
                    
                }
            }
            if (Tcp_Proxy != null) {
                try
                {
                    Program.PrintLog(string.Format("已断开代理端{0}的连接", Tcp_Proxy.Client.RemoteEndPoint));
                    Proxy_Stream.Close();
                    Tcp_Proxy.Close();
                    Tcp_Proxy = null;
                }
                catch (NullReferenceException){ 

                }
            }
            
        }
    }
}
