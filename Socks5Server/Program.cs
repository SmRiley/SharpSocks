using Socks5Server.Core;
using System.Net;
using System.Threading.Tasks;
namespace Socks5Server
{
    class Program
    {
        static void Main(string[] args)
        {
            new TCP_Listen(IPAddress.Any, 1080);
            Task.Delay(-1).Wait();
        }

   
    }
}
