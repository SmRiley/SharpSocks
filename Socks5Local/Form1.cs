using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace Socks5Local
{
    public partial class Form1 : Form
    {
        TCP_Start TCP_Listener;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            ///四项是否为空
            
            try
            {
                if (button1.Text == "启动")
                {
                    TCP_Listener = new TCP_Start(IP_ADDR.Text, Convert.ToInt32(PORT.Text), Pass.Text, Convert.ToInt32(Local_PORT.Text));
                    if (TCP_Listener.state)
                    {
                        button1.Text = "关闭";
                        IP_ADDR.Enabled = PORT.Enabled = Pass.Enabled = Local_PORT.Enabled = false;
                    }
                }
                else {
                    TCP_Listener.Dispose();
                    TCP_Listener = null;
                    button1.Text = "启动";
                    IP_ADDR.Enabled = PORT.Enabled = Pass.Enabled = Local_PORT.Enabled = true;
                }
                
            }
            catch (FormatException){
                MessageBox.Show("输入非法项");
            }
        }

    }
}
