global using static Server.DataHandle;
global using static Share.Utils;
using Server.Core;
using System.Net;
using System.Text.Json.Nodes;

try
{
    JsonArray config = (JsonArray)JsonNode.Parse(await File.ReadAllBytesAsync("Config.json"))!;
    var tasks = new List<Task>();
    foreach (var node in config)
    {
        var pass = (string?)node?["Pass"] ?? string.Empty;
        var port = (int?)node?["Port"] ?? 0;
        var enableUdp = (bool?)node?["Udp"] ?? false;
        var listener = new TcpListen(IPAddress.Any, port, pass, enableUdp);
        tasks.Add(listener.StartAsync());
    }
    await Task.WhenAll(tasks);
    
}
catch (FileNotFoundException)
{
    WriteLog("未找到配置文件(Config.json),程序已退出");
}