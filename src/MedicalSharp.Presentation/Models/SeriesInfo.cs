using Caliburn.Micro;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;

namespace MedicalSharp.Presentation.Models
{
    /// <summary>
    /// 序列信息
    /// </summary>
    public class SeriesInfo : PropertyChangedBase
    {
        #region 序列实例UID —— string SeriesInstanceUId
        /// <summary>
        /// 序列实例UID
        /// </summary>
        [DependencyProperty]
        public string SeriesInstanceUId { get; set; }
        #endregion

        #region 序列号 —— string SeriesNumber
        /// <summary>
        /// 序列号
        /// </summary>
        /// <remarks>Se203</remarks>
        [DependencyProperty]
        public string SeriesNumber { get; set; }
        #endregion

        #region 序列日期 —— string SeriesDate
        /// <summary>
        /// 序列日期
        /// </summary>
        /// <remarks>YYYYMMDD</remarks>
        [DependencyProperty]
        public string SeriesDate { get; set; }
        #endregion

        #region 序列时间 —— string SeriesTime
        /// <summary>
        /// 序列时间
        /// </summary>
        /// <remarks>HHMMSS</remarks>
        [DependencyProperty]
        public string SeriesTime { get; set; }
        #endregion

        #region 成像设备 —— string Modality
        /// <summary>
        /// 成像设备
        /// </summary>
        /// <remarks>CT/MR/PET/CR</remarks>
        [DependencyProperty]
        public string Modality { get; set; }
        #endregion

        #region 检查部位 —— string BodyPartExamined
        /// <summary>
        /// 检查部位
        /// </summary>
        [DependencyProperty]
        public string BodyPartExamined { get; set; }
        #endregion

        #region 层厚 —— string SliceThickness
        /// <summary>
        /// 层厚
        /// </summary>
        /// <remarks>mm</remarks>
        [DependencyProperty]
        public string SliceThickness { get; set; }
        #endregion

        #region 层间距 —— string SpacingBetweenSlices
        /// <summary>
        /// 层间距
        /// </summary>
        /// <remarks>mm</remarks>
        [DependencyProperty]
        public string SpacingBetweenSlices { get; set; }
        #endregion

        #region 序列描述 —— string SeriesDescription
        /// <summary>
        /// 序列描述
        /// </summary>
        [DependencyProperty]
        public string SeriesDescription { get; set; }
        #endregion
    }
}
