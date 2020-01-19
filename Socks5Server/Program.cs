using System;
using Socks5Server.Core;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
namespace Socks5Server
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var JsonSpan = File.ReadAllBytes("Config.json").AsSpan();
                var JsonReader = new Utf8JsonReader(JsonSpan);
                var JsonObject = new List<string>();
                while (JsonReader.Read())
                {
                    if (JsonReader.TokenType == JsonTokenType.EndObject)
                    {
                        var ObjectInfo = JsonObject.ToArray();
                        new TCP_Listen(IPAddress.Any, Convert.ToInt16(ObjectInfo[0]),ObjectInfo[1], Convert.ToBoolean(ObjectInfo[2]));
                        JsonObject = new List<string>();
                    }
                    else if (JsonReader.TokenType == JsonTokenType.String)
                    {
                        JsonObject.Add(JsonReader.GetString());
                    }
                }
                Task.Delay(-1).Wait();
            }
            catch (FileNotFoundException)
            {
                DataHandle.WriteLog("未找到配置文件,程序已退出");
            }
            catch {
                DataHandle.WriteLog("配置文件载入错误,程序已退出");
            }
           
        }

   
    }
}
