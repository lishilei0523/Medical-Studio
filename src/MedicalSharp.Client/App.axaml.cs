using Avalonia;
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
        }
    }
}
