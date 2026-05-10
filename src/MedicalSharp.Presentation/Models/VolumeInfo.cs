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

        #region 窗宽 —— string WindowWidth
        /// <summary>
        /// 窗宽
        /// </summary>
        public string WindowWidth { get; set; }
        #endregion

        #region 窗位 —— string WindowCenter
        /// <summary>
        /// 窗位
        /// </summary>
        public string WindowCenter { get; set; }
        #endregion
    }
}
