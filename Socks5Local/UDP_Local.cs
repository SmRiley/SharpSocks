using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace Socks5Local
{
    class UDP_Local
    {
        UdpClient UDP_Client = new UdpClient(0);
        IPEndPoint Local_Point,Proxy_Point;
        public int UDP_Port { get { return (UDP_Client.Client.LocalEndPoint as IPEndPoint).Port; } }
        
        public UDP_Local(IPEndPoint Proxy_EndPoint,IPEndPoint IP_EndPoint=null) {
            Proxy_Point = Proxy_EndPoint;
            if (IP_EndPoint != null)
            {
                Local_Point = IP_EndPoint;
            }
            UDP_Client.BeginReceive(UDP_Receive,null);
        }

        void UDP_Receive(IAsyncResult ar) {
            try
            {
                IPEndPoint Remote_Point = new IPEndPoint(0, 0);
                var Data = UDP_Client.EndReceive(ar, ref Remote_Point);
                if (Local_Point == null)
                {
                    Local_Point = Remote_Point;
                }
                if (Remote_Point.Equals(Local_Point))
                {
                    UDP_Send(DataHandle.En_Bytes(Data), Proxy_Point);
                }
                else if (Remote_Point.Equals(Proxy_Point))
                {
                    UDP_Send(DataHandle.De_Bytes(Data), Local_Point);
                }
                UDP_Client.BeginReceive(UDP_Receive, null);
            }
            catch(Exception) { 
            
            }
        }

        void UDP_Send(byte[] Data,IPEndPoint Remote_Point) {
            UDP_Client.Send(Data, Data.Length, Remote_Point);
        }

        public void Dispose() {
            UDP_Client.Close();
        }
    }
}
