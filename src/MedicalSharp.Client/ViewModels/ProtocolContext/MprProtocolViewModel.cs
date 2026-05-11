using Avalonia.Collections;
using Caliburn.Micro;
using MedicalSharp.Client.ViewModels.VolumeContext;
using MedicalSharp.Primitives.Managers;
using MedicalSharp.Primitives.Models;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
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

            //默认值
            this.SelectedWindowLevel = WindowLevelManager.Default;
            this.WindowLevels =
            [
                WindowLevelManager.Default,
                WindowLevelManager.Brain,
                WindowLevelManager.Cardiac,
                WindowLevelManager.Liver,
                WindowLevelManager.Lung,
                WindowLevelManager.Abdomen,
                WindowLevelManager.Bone,
                WindowLevelManager.Vascular,
                WindowLevelManager.Mediastinum
            ];
        }

        #endregion

        #region # 属性

        #region MPR视图模型 —— MprViewModel MprViewModel
        /// <summary>
        /// MPR视图模型
        /// </summary>
        public MprViewModel MprViewModel { get; set; }
        #endregion

        #region 已选预设窗 —— WindowLevel? SelectedWindowLevel
        /// <summary>
        /// 已选预设窗
        /// </summary>
        public WindowLevel? SelectedWindowLevel
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                if (value != null)
                {
                    this.MprViewModel?.WindowWidth = value.WindowWidth;
                    this.MprViewModel?.WindowCenter = value.WindowCenter;
                }
            }
        }
        #endregion

        #region 预设窗列表 —— AvaloniaList<WindowLevel> WindowLevels
        /// <summary>
        /// 预设窗列表
        /// </summary>
        [DependencyProperty]
        public AvaloniaList<WindowLevel> WindowLevels { get; set; }
        #endregion

        #endregion

        #region # 方法

        //

        #endregion
    }
}
