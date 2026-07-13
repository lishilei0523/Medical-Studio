using Caliburn.Micro;
using MedicalSharp.Client.ViewModels.VolumeContext;
using MedicalSharp.Controls.InputManagers;
using MedicalSharp.Controls.Visual3Ds;
using MedicalSharp.Primitives.Cameras;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Models;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;

namespace MedicalSharp.Client.ViewModels.LayoutContext
{
    /// <summary>
    /// CPR布局视图模型
    /// </summary>
    public class CprLayoutViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗口管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 事件聚合器
        /// </summary>
        private readonly IEventAggregator _eventAggregator;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public CprLayoutViewModel(IWindowManager windowManager, IEventAggregator eventAggregator, CurveVisual3D curve)
        {
            this._windowManager = windowManager;
            this._eventAggregator = eventAggregator;

            //默认值
            string title = "CPR";
            CPRCamera camera = new CPRCamera();
            CPRInputManager inputManager = new CPRInputManager(camera);
            this.CprViewModel = new CprViewModel(this._windowManager, this._eventAggregator, title, camera, inputManager);
            this.CprViewModel.CurveVisual3D = curve;
            this.CprViewModel.CPRMode = CPRMode.Straightened;
            this.CprViewModel.ProjectionDirection = CPRProjectionDirection.Normal;
            this.CprViewModel.StraightenDirection = CPRStraightenDirection.Horizontal;
            this.CprViewModel.RadialWidth = 1f;
        }

        #endregion

        #region # 属性

        #region CPR渲染视图模型 —— CprViewModel CprViewModel
        /// <summary>
        /// CPR渲染视图模型
        /// </summary>
        [DependencyProperty]
        public CprViewModel CprViewModel { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 设置体积数据 —— void SetVolumeData(VolumeData volumeData)
        /// <summary>
        /// 设置体积数据
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        public void SetVolumeData(VolumeData volumeData)
        {
            this.CprViewModel.VolumeData = volumeData;
        }
        #endregion

        #region 清空体积数据 —— void ClearVolumeData()
        /// <summary>
        /// 清空体积数据
        /// </summary>
        public void ClearVolumeData()
        {
            this.CprViewModel.VolumeData = null;
        }
        #endregion

        #endregion
    }
}
