using Caliburn.Micro;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using SD.IOC.Core.Mediators;
using System.Threading.Tasks;

namespace MedicalSharp.Client.ViewModels.HomeContext
{
    /// <summary>
    /// 导航视图模型
    /// </summary>
    public class NavigationViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗口管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public NavigationViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
            this.DisplayName = "Sample.Avalonia.Host";
        }

        #endregion

        #region # 属性

        #region 标题 —— string DisplayName
        /// <summary>
        /// 标题
        /// </summary>
        [DependencyProperty]
        public override string DisplayName { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 打开OpenGL视图 —— async Task OpenGlView()
        /// <summary>
        /// 打开OpenGL视图
        /// </summary>
        public async Task OpenGlView()
        {
            GraphicViewModel viewModel = ResolveMediator.Resolve<GraphicViewModel>();
            await this._windowManager.ShowWindowAsync(viewModel);
        }
        #endregion

        #endregion
    }
}
