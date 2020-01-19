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
    class TCP_Start
    {
        public bool state { get; }
        string Keys { get; }
        int Port { get; }
        int Local_Port { get; }
        string IP;

        static System.Threading.Timer Checker_Timer;
        TcpListener tcpListener { get; }

        public TCP_Start(string IP_ADDR, int PORT, string Pass, int Local_PORT)
        {
            ///四项是否为空

            IP = IP_ADDR;
            DataHandle.Key = DataHandle.Get_Pass(Pass);
            Port = PORT;
            Local_Port = Local_PORT;
            if (IP != "" && Keys != "" && Port != 0 && Local_Port != 0)
            {
                tcpListener = new TcpListener(IPAddress.Any, Local_Port);
                tcpListener.Start();
                tcpListener.BeginAcceptTcpClient(Accept_TCP, tcpListener);
                Checker_Timer = new System.Threading.Timer(Check_Timer, null, 0, 1000 * 5);
                state = true;
            }
            else
            {
                state = false;
                MessageBox.Show("请检查输入项");
            }

        }

        private void Accept_TCP(IAsyncResult ar)
        {
            try
            {
                var TCP_Client = (ar.AsyncState as TcpListener).EndAcceptTcpClient(ar);
                new TCP_Local(TCP_Client,new TcpClient(IP,Port));
                (ar.AsyncState as TcpListener).BeginAcceptTcpClient(Accept_TCP, ar.AsyncState);
            }
            catch (Exception)
            {

            }
        }
      

        void Check_Timer(object state)
        {
            try
            {
                foreach (var i in TCP_Local.UDP_List.ToArray())
                {
                    if (!DataHandle.TCP_Usability(i.TCP_Client))
                    {
                        i.TCP_Client.Dispose();
                        i.UDP_obj.Dispose();
                        TCP_Local.UDP_List.Remove(i);
                    }
                }
            }
            catch (SocketException)
            {

            }
        }

        public void Dispose()
        {
            try
            {
                tcpListener.Stop();
            }
            catch
            {

            }
        }
    }
}
