using Caliburn.Micro;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;

namespace MedicalSharp.Client.ViewModels.CommonContext
{
    /// <summary>
    /// 容器视图模型
    /// </summary>
    public class ContainerViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗口管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public ContainerViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 标题 —— string Title
        /// <summary>
        /// 标题
        /// </summary>
        [DependencyProperty]
        public string Title { get; set; }
        #endregion

        #region 内容 —— Screen Content
        /// <summary>
        /// 内容
        /// </summary>
        [DependencyProperty]
        public Screen Content { get; set; }
        #endregion

        #endregion

        #region # 方法

        //Actions

        #endregion
    }
}
