using Avalonia;
using Avalonia.Controls;
using System;

namespace MedicalSharp.Client
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            AppBuilder appBuilder = BuildAvaloniaApp();
            appBuilder.StartWithClassicDesktopLifetime(args, ShutdownMode.OnLastWindowClose);
        }

        static AppBuilder BuildAvaloniaApp()
        {
            AppBuilder appBuilder = AppBuilder.Configure<App>();
            appBuilder.UsePlatformDetect();
            appBuilder.With(new Win32PlatformOptions
            {
                RenderingMode = [Win32RenderingMode.Wgl]
            });
            appBuilder.With(new X11PlatformOptions
            {
                RenderingMode = [X11RenderingMode.Glx, X11RenderingMode.Egl, X11RenderingMode.Vulkan]
            });
            appBuilder.UseSkia();

            return appBuilder;
        }
    }
}
