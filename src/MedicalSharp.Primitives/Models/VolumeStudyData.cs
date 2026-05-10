namespace MedicalSharp.Primitives.Models
{
    /// <summary>
    /// 体积检查数据
    /// </summary>
    public class VolumeStudyData
    {
        #region 检查实例UID —— string StudyInstanceUId
        /// <summary>
        /// 检查实例UID
        /// </summary>
        public string StudyInstanceUId { get; set; }
        #endregion

        #region 检查日期 —— string StudyDate
        /// <summary>
        /// 检查日期
        /// </summary>
        /// <remarks>YYYYMMDD</remarks>
        public string StudyDate { get; set; }
        #endregion

        #region 检查时间 —— string StudyTime
        /// <summary>
        /// 检查时间
        /// </summary>
        /// <remarks>HHMMSS</remarks>
        public string StudyTime { get; set; }
        #endregion

        #region 检查描述 —— string StudyDescription
        /// <summary>
        /// 检查描述
        /// </summary>
        public string StudyDescription { get; set; }
        #endregion

        #region 检查ID —— string StudyId
        /// <summary>
        /// 检查ID
        /// </summary>
        public string StudyId { get; set; }
        #endregion

        #region 登记号 —— string AccessionNumber
        /// <summary>
        /// 登记号
        /// </summary>
        /// <remarks>Accession Number</remarks>
        public string AccessionNumber { get; set; }
        #endregion

        #region 转诊医生 —— string ReferringPhysician
        /// <summary>
        /// 转诊医生
        /// </summary>
        /// <remarks>Referring Physician Name</remarks>
        public string ReferringPhysician { get; set; }
        #endregion
    }
}
