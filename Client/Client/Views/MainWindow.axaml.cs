using Avalonia.Controls;

namespace Client.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        if (OperatingSystem.IsAndroid())
        {

        }

        if (OperatingSystem.IsWindows())
        {
            Width = 500;
            Height = 350;
        }

        InitializeComponent();
    }
}
