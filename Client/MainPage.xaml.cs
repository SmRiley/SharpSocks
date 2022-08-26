namespace Client;

public partial class MainPage : ContentPage
{

    public MainPage()
    {
        InitializeComponent();
    }

    private void StartBtn_Clicked(object sender, EventArgs e)
    {
        var context = (MainPageViewModel)BindingContext;
        if (new[] { context.IpAddress,
            context.Port,
            context.Pass,
            context.LocalPort
        }.Any(t => string.IsNullOrWhiteSpace(t)))
        {
            _ = DisplayAlert("提示", "请将信息填写完整再启用", "确定");
            return;
        }
        try
        {
            uint.Parse(context.Port);
            uint.Parse(context.LocalPort);
        }
        catch (Exception ex) when (ex is FormatException or OverflowException)
        {
            DisplayAlert("提示", $"端口和代理端口必须为数字,且范围为{uint.MinValue}-{uint.MaxValue}", "确定");
            return;
        }
        context.ExExecutionAsync();
    }
}

