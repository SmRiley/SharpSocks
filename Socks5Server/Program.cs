using System;
using Socks5Server.Core;
using System.Net;
using System.Threading.Tasks;
namespace Socks5Server
{
    class Program
    {
        static void Main(string[] args)
        {
            new Socsk_Listen(IPAddress.Any, 1080);
            Task.Delay(-1).Wait();
        }

        public static void PrintLog(string str)
        {
            Console.WriteLine(DateTime.Now + string.Format("{0}{1}", ":", str));
        }
    }
}
