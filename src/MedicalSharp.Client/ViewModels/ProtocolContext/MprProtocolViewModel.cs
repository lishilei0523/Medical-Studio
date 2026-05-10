using Caliburn.Micro;
using MedicalSharp.Client.ViewModels.VolumeContext;
using SD.Infrastructure.Avalonia.Caliburn.Base;

namespace MedicalSharp.Client.ViewModels.ProtocolContext
{
    /// <summary>
    /// MPR协议视图模型
    /// </summary>
    public class MprProtocolViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗口管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public MprProtocolViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region MPR视图模型 —— MprViewModel MprViewModel
        /// <summary>
        /// MPR视图模型
        /// </summary>
        public MprViewModel MprViewModel { get; set; }
        #endregion

        #endregion

        #region # 方法

        //TODO 

        #endregion
    }
}
