using Avalonia.Collections;
using MedicalSharp.Presentation.Models;
using MedicalSharp.Primitives.Models;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;

namespace MedicalSharp.Client.ViewModels.AlgorithmContext
{
    /// <summary>
    /// 区域生长视图模型
    /// </summary>
    public class RegionGrowViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 默认构造器
        /// </summary>
        public RegionGrowViewModel()
        {
            base.DisplayName = "区域生长";
            this.MinHU = 400;
            this.MaxHU = 1000;
        }

        #endregion

        #region # 属性

        #region 体积数据 —— VolumeData VolumeData
        /// <summary>
        /// 体积数据
        /// </summary>
        public VolumeData VolumeData { get; set; }
        #endregion

        #region 最小HU值 —— int MinHU
        /// <summary>
        /// 最小HU值
        /// </summary>
        [DependencyProperty]
        public int MinHU { get; set; }
        #endregion

        #region 最大HU值 —— int MaxHU
        /// <summary>
        /// 最大HU值
        /// </summary>
        [DependencyProperty]
        public int MaxHU { get; set; }
        #endregion

        #region 已选组织 —— TissueInfo SelectedTissue
        /// <summary>
        /// 已选组织
        /// </summary>
        [DependencyProperty]
        public TissueInfo SelectedTissue { get; set; }
        #endregion

        #region 组织列表 —— AvaloniaList<TissueInfo> Tissues
        /// <summary>
        /// 组织列表
        /// </summary>
        [DependencyProperty]
        public AvaloniaList<TissueInfo> Tissues { get; set; }
        #endregion

        #endregion

        #region # 方法

        //

        #endregion
    }
}
