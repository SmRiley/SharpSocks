global using static Client.DataHandle;
global using static Share.Utils;
#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
#endif
namespace Client;

public partial class App : Application
{
    private const int WindowWidth = 400;
    private const int WindowHeight = 550;
    public App()
    {
        InitializeComponent();
        Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
        {
#if WINDOWS
            var mauiWindow = handler.VirtualView;
            var nativeWindow = handler.PlatformView;
            nativeWindow.Activate();
            IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new SizeInt32(WindowWidth, WindowHeight));
#endif
        });
        MainPage = new AppShell();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);
        if (window != null)
        {
            window.Title = "SharpSocks";
        }
        return window!;
    }
}
