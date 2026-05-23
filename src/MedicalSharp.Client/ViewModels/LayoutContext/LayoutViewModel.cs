using MedicalSharp.Client.ViewModels.VolumeContext;
using MedicalSharp.Primitives.Models;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;

namespace MedicalSharp.Client.ViewModels.LayoutContext
{
    /// <summary>
    /// 布局视图模型
    /// </summary>
    public class LayoutViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public LayoutViewModel(VolumeViewModel volumeViewModel, MprAxialViewModel mprAxialViewModel, MprCoronalViewModel mprCoronalViewModel, MprSagittalViewModel mprSagittalViewModel)
        {
            this.VolumeViewModel = volumeViewModel;
            this.MprAxialViewModel = mprAxialViewModel;
            this.MprCoronalViewModel = mprCoronalViewModel;
            this.MprSagittalViewModel = mprSagittalViewModel;
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

        #region 设置体积数据 —— void SetVolumeData(VolumeData volumeData)
        /// <summary>
        /// 设置体积数据
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        public void SetVolumeData(VolumeData volumeData)
        {
            this.VolumeViewModel.VolumeData = volumeData;
            this.MprAxialViewModel.VolumeData = volumeData;
            this.MprCoronalViewModel.VolumeData = volumeData;
            this.MprSagittalViewModel.VolumeData = volumeData;
        }
        #endregion

        #region 清空体积数据 —— void ClearVolumeData()
        /// <summary>
        /// 清空体积数据
        /// </summary>
        public void ClearVolumeData()
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
