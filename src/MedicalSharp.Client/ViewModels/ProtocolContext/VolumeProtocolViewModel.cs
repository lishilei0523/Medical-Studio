using Caliburn.Micro;
using MedicalSharp.Client.ViewModels.VolumeContext;
using SD.Infrastructure.Avalonia.Caliburn.Base;

namespace MedicalSharp.Client.ViewModels.ProtocolContext
{
    /// <summary>
    /// 体积渲染协议视图模型
    /// </summary>
    public class VolumeProtocolViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗口管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public VolumeProtocolViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 体积渲染视图模型 —— VolumeViewModel VolumeViewModel
        /// <summary>
        /// 体积渲染视图模型
        /// </summary>
        public VolumeViewModel VolumeViewModel { get; set; }
        #endregion

        #endregion

        #region # 方法

        //TODO 

        #endregion
    }
}
