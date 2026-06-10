using Avalonia;
using Avalonia.Controls;
using System;

namespace MedicalSharp.Client
{
    /// <summary>
    /// 主程序
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 主入口函数
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            AppBuilder appBuilder = BuildAvaloniaApp();
            appBuilder.StartWithClassicDesktopLifetime(args, ShutdownMode.OnLastWindowClose);
        }

        /// <summary>
        /// 构建Avalonia应用程序
        /// </summary>
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
                RenderingMode = [X11RenderingMode.Glx, X11RenderingMode.Egl]
            });
            appBuilder.UseSkia();

            return appBuilder;
        }
    }
}
