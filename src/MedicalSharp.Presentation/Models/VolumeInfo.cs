using Caliburn.Micro;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;

namespace MedicalSharp.Presentation.Models
{
    /// <summary>
    /// 体积信息
    /// </summary>
    public class VolumeInfo : PropertyChangedBase
    {
        #region 序列实例UID —— string SeriesInstanceUId
        /// <summary>
        /// 序列实例UID
        /// </summary>
        [DependencyProperty]
        public string SeriesInstanceUId { get; set; }
        #endregion

        #region 体积尺寸 —— string VolumeSize
        /// <summary>
        /// 体积尺寸
        /// </summary>
        [DependencyProperty]
        public string VolumeSize { get; set; }
        #endregion

        #region 间距 —— string Spacing
        /// <summary>
        /// 间距
        /// </summary>
        [DependencyProperty]
        public string Spacing { get; set; }
        #endregion

        #region 物理尺寸 —— string PhysicalSize
        /// <summary>
        /// 物理尺寸
        /// </summary>
        [DependencyProperty]
        public string PhysicalSize { get; set; }
        #endregion

        #region 斜率 —— string RescaleSlope
        /// <summary>
        /// 斜率
        /// </summary>
        public string RescaleSlope { get; set; }
        #endregion

        #region 截距 —— string RescaleIntercept
        /// <summary>
        /// 截距
        /// </summary>
        public string RescaleIntercept { get; set; }
        #endregion

        #region HU范围 —— string HURange
        /// <summary>
        /// HU范围
        /// </summary>
        public string HURange { get; set; }
        #endregion

        #region 默认窗 —— string WindowLevel
        /// <summary>
        /// 默认窗
        /// </summary>
        public string WindowLevel { get; set; }
        #endregion
    }
}
