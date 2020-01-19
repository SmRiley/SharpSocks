using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Socks5Local
{
    class TCP_Local
    {

        byte[] Proxy_Data = new byte[1024 * 50];
        byte[] Local_Data = new byte[1024 * 50];

        public static List<(TcpClient TCP_Client, UDP_Local UDP_obj)> UDP_List = new List<(TcpClient TCP_Client, UDP_Local UDP_obj)>();
        TcpClient TCP_Client, TCP_Proxy;
        public TCP_Local(TcpClient Client,TcpClient Proxy)
        {
            TCP_Client = Client;
            TCP_Proxy = Proxy;
            TCP_Client.GetStream().BeginRead(Local_Data,0,Local_Data.Length,Client_Read,null);
            TCP_Proxy.GetStream().BeginRead(Proxy_Data, 0, Proxy_Data.Length, Proxy_Read_Init, 1);
        }

        public static void TCP_Send(TcpClient TCP_Client, byte[] Data)
        {
            try
            {
                TCP_Client.GetStream().Write(Data, 0, Data.Length);
            }
            catch (Exception)
            {

            }
        }

        private void Client_Read(IAsyncResult ar)
        {
            try
            {
                byte[] Data =DataHandle.En_Bytes(Local_Data.Take(TCP_Client.GetStream().EndRead(ar)).ToArray());
                if (Data.Length > 0)
                {
                    TCP_Send(TCP_Proxy, Data);
                    TCP_Client.GetStream().BeginRead(Local_Data, 0, Local_Data.Length, Client_Read,null);
                }
            }
            catch (Exception)
            {
                Dispose();
            }
        }


        private void Proxy_Read_Init(IAsyncResult ar)
        {
            var AR = (int)ar.AsyncState;
            try
            {
                byte[] Data =DataHandle.De_Bytes(Proxy_Data.Take(TCP_Proxy.GetStream().EndRead(ar)).ToArray());
                if (Data.Length > 0)
                {
                    if (AR == 2)
                    {
                        var UDP_Obj = new UDP_Local(TCP_Proxy.Client.RemoteEndPoint as IPEndPoint);
                        TCP_Send(TCP_Client, DataHandle.Ex_UDP(Data, UDP_Obj.UDP_Port));
                        if (DataHandle.Is_UDP_Init(Data))
                        {
                            UDP_List.Add((TCP_Client, UDP_Obj));
                        }
                        else
                        {
                            TCP_Proxy.GetStream().BeginRead(Proxy_Data, 0, Proxy_Data.Length, Proxy_Read, null);
                        }

                    }
                    else
                    {
                        TCP_Send(TCP_Client, Data);
                        TCP_Proxy.GetStream().BeginRead(Proxy_Data, 0, Proxy_Data.Length, Proxy_Read_Init, ++AR);
                    }

                }
                else {
                    throw (new SocketException());
                }

            }
            catch (Exception)
            {
                Dispose();
            }
        }


        private void Proxy_Read(IAsyncResult ar)
        {
            try
            {
                byte[] Data = Proxy_Data.Take(TCP_Proxy.GetStream().EndRead(ar)).ToArray();
                if (Data.Length > 0)
                {
                    Data = DataHandle.De_Bytes(Data);
                    TCP_Send(TCP_Client, Data);
                    TCP_Proxy.GetStream().BeginRead(Proxy_Data, 0, Proxy_Data.Length, Proxy_Read, null);
                }
                else {
                    throw (new SocketException());
                }

            }
            catch (Exception)
            {
                Dispose();
            }
        }

        private void Dispose() {
            try
            {
                Proxy_Data = null;
                Local_Data = null;
                TCP_Client.GetStream().Close();
                TCP_Client.Close();
                TCP_Proxy.GetStream().Close();
                TCP_Proxy.Close();
            }
            catch (Exception) { 
            }
        }
    }
}
