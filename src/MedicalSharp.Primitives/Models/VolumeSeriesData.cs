namespace MedicalSharp.Primitives.Models
{
    /// <summary>
    /// 体积序列数据
    /// </summary>
    public class VolumeSeriesData
    {
        #region 序列实例UID —— string SeriesInstanceUId
        /// <summary>
        /// 序列实例UID
        /// </summary>
        public string SeriesInstanceUId { get; set; }
        #endregion

        #region 序列号 —— string SeriesNumber
        /// <summary>
        /// 序列号
        /// </summary>
        /// <remarks>Se203</remarks>
        public string SeriesNumber { get; set; }
        #endregion

        #region 序列日期 —— string SeriesDate
        /// <summary>
        /// 序列日期
        /// </summary>
        /// <remarks>YYYYMMDD</remarks>
        public string SeriesDate { get; set; }
        #endregion

        #region 序列时间 —— string SeriesTime
        /// <summary>
        /// 序列时间
        /// </summary>
        /// <remarks>HHMMSS</remarks>
        public string SeriesTime { get; set; }
        #endregion

        #region 检查部位 —— string BodyPartExamined
        /// <summary>
        /// 检查部位
        /// </summary>
        public string BodyPartExamined { get; set; }
        #endregion

        #region 层厚 —— string SliceThickness
        /// <summary>
        /// 层厚
        /// </summary>
        /// <remarks>mm</remarks>
        public string SliceThickness { get; set; }
        #endregion

        #region 层间距 —— string SpacingBetweenSlices
        /// <summary>
        /// 层间距
        /// </summary>
        /// <remarks>mm</remarks>
        public string SpacingBetweenSlices { get; set; }
        #endregion

        #region 序列描述 —— string SeriesDescription
        /// <summary>
        /// 序列描述
        /// </summary>
        public string SeriesDescription { get; set; }
        #endregion
    }
}