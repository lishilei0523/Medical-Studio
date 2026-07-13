using Caliburn.Micro;
using MedicalSharp.Client.ViewModels.VolumeContext;
using MedicalSharp.Controls.InputManagers;
using MedicalSharp.Controls.Visual3Ds;
using MedicalSharp.Primitives.Cameras;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Models;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using SD.IOC.Core.Mediators;

namespace MedicalSharp.Client.ViewModels.LayoutContext
{
    /// <summary>
    /// CPR布局视图模型
    /// </summary>
    public class CprLayoutViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public CprLayoutViewModel(CurveVisual3D curve)
        {
            IWindowManager windowManager = ResolveMediator.Resolve<IWindowManager>();
            IEventAggregator eventAggregator = ResolveMediator.Resolve<IEventAggregator>();
            string title = "CPR";
            CPRCamera camera = new CPRCamera();
            CPRInputManager inputManager = new CPRInputManager(camera);

            this.CprViewModel = new CprViewModel(windowManager, eventAggregator, title, camera, inputManager);
            this.CprViewModel.CurveVisual3D = curve;
            this.CprViewModel.CPRMode = CPRMode.Straightened;
            this.CprViewModel.ProjectionDirection = CPRProjectionDirection.Tangent;
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
