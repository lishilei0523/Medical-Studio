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

        #region 体积宽度 —— int VolumeWidth
        /// <summary>
        /// 体积宽度
        /// </summary>
        [DependencyProperty]
        public int VolumeWidth { get; set; }
        #endregion

        #region 体积高度 —— int VolumeHeight
        /// <summary>
        /// 体积高度
        /// </summary>
        [DependencyProperty]
        public int VolumeHeight { get; set; }
        #endregion

        #region 体积深度 —— int VolumeDepth
        /// <summary>
        /// 体积深度
        /// </summary>
        [DependencyProperty]
        public int VolumeDepth { get; set; }
        #endregion
    }
}
