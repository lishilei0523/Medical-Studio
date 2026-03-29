using MedicalSharp.Client.ViewModels.VolumeContext;
using MedicalSharp.Dicoms.Models;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using SD.IOC.Core.Mediators;

namespace MedicalSharp.Client.ViewModels.LayoutContext
{
    /// <summary>
    /// 布局视图模型
    /// </summary>
    public abstract class LayoutViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        protected LayoutViewModel()
        {
            this.VolumeViewModel = ResolveMediator.Resolve<VolumeViewModel>();
            this.MprAxialViewModel = ResolveMediator.Resolve<MprAxialViewModel>();
            this.MprCoronalViewModel = ResolveMediator.Resolve<MprCoronalViewModel>();
            this.MprSagittalViewModel = ResolveMediator.Resolve<MprSagittalViewModel>();
        }

        #endregion

        #region # 属性

        #region 体积渲染视图模型 —— VolumeViewModel VolumeViewModel
        /// <summary>
        /// 体积渲染视图模型
        /// </summary>
        [DependencyProperty]
        public VolumeViewModel VolumeViewModel { get; set; }
        #endregion

        #region MPR横断位视图模型 —— MprAxialViewModel MprAxialViewModel
        /// <summary>
        /// MPR横断位视图模型
        /// </summary>
        [DependencyProperty]
        public MprAxialViewModel MprAxialViewModel { get; set; }
        #endregion

        #region MPR冠状位视图模型 —— MprCoronalViewModel MprCoronalViewModel
        /// <summary>
        /// MPR冠状位视图模型
        /// </summary>
        [DependencyProperty]
        public MprCoronalViewModel MprCoronalViewModel { get; set; }
        #endregion

        #region MPR矢状位视图模型 —— MprSagittalViewModel MprSagittalViewModel
        /// <summary>
        /// MPR矢状位视图模型
        /// </summary>
        [DependencyProperty]
        public MprSagittalViewModel MprSagittalViewModel { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 设置体积数据 —— virtual void SetVolumeData(VolumeData volumeData)
        /// <summary>
        /// 设置体积数据
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        public virtual void SetVolumeData(VolumeData volumeData)
        {
            this.VolumeViewModel.VolumeData = volumeData;
            this.MprAxialViewModel.VolumeData = volumeData;
            this.MprCoronalViewModel.VolumeData = volumeData;
            this.MprSagittalViewModel.VolumeData = volumeData;
        }
        #endregion

        #region 清空体积数据 —— virtual void ClearVolumeData()
        /// <summary>
        /// 清空体积数据
        /// </summary>
        public virtual void ClearVolumeData()
        {
            this.VolumeViewModel.VolumeData = null;
            this.MprAxialViewModel.VolumeData = null;
            this.MprCoronalViewModel.VolumeData = null;
            this.MprSagittalViewModel.VolumeData = null;
        }
        #endregion

        #endregion
    }
}
