using Client.Core;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Client;
internal class MainPageViewModel : ObservableObject
{
    public string IpAddress { get; set; } = "127.0.0.1";
    public string Port { get; set; } = "3080";
    public string Pass { get; set; } = "123456";
    public string LocalPort { get; set; } = "1080";
    public string BtnContext { get; set; } = "启动";
    private TcpListen? tcpListen;
    public bool Enable { get; set; } = true;
    public MainPageViewModel()
    {

    }

    public void ExExecutionAsync()
    {
        if (BtnContext == "启动")
        {
            Key = GetPassBytes(Pass);
            tcpListen = new TcpListen(IpAddress, int.Parse(Port), Pass, int.Parse(LocalPort));
            tcpListen.Start();
            Enable = false;
            BtnContext = "停止";
        }
        else
        {
            tcpListen?.Stop();
            Enable = true;
            BtnContext = "启动";
        }
    }
}
