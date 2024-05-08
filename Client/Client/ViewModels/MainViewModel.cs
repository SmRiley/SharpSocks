using Client.Core;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Client.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _ipAddress = "127.0.0.1";

    [ObservableProperty]
    private string _port = "3080";

    [ObservableProperty]
    private string _pass = "123456";

    [ObservableProperty]
    private string _localPort = "1080";

    [ObservableProperty]
    private string _btnContext = "Start";

    [ObservableProperty]
    private bool _enable = true;

    private TcpListen? _tcpListen;
    public void ExecutionProxy()
    {
        if (BtnContext == "Start")
        {
            Key = GenerateUniqueRandomBytes(Pass);
            _tcpListen = new(IpAddress, int.Parse((string)Port), Pass, int.Parse((string)LocalPort));
            _tcpListen.Start();
            Enable = false;
            BtnContext = "Stop";
        }
        else
        {
            _tcpListen?.Stop();
            Enable = true;
            BtnContext = "Start";
        }
    }
}