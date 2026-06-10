using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace MedicalSharp.Client
{
    /// <summary>
    /// Avalonia应用程序
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// 框架初始化完成事件
        /// </summary>
        public override void OnFrameworkInitializationCompleted()
        {
            base.OnFrameworkInitializationCompleted();

            //Caliburn启动
            Startup startup = new Startup();
            startup.Initialize();

            //主窗口关闭退出应用程序
            if (base.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
            }
        }
    }
}
