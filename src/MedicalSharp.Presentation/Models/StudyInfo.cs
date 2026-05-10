using Caliburn.Micro;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;

namespace MedicalSharp.Presentation.Models
{
    /// <summary>
    /// 检查信息
    /// </summary>
    public class StudyInfo : PropertyChangedBase
    {
        #region 检查实例UID —— string StudyInstanceUId
        /// <summary>
        /// 检查实例UID
        /// </summary>
        [DependencyProperty]
        public string StudyInstanceUId { get; set; }
        #endregion

        #region 检查日期 —— string StudyDate
        /// <summary>
        /// 检查日期
        /// </summary>
        /// <remarks>YYYYMMDD</remarks>
        [DependencyProperty]
        public string StudyDate { get; set; }
        #endregion

        #region 检查时间 —— string StudyTime
        /// <summary>
        /// 检查时间
        /// </summary>
        /// <remarks>HHMMSS</remarks>
        [DependencyProperty]
        public string StudyTime { get; set; }
        #endregion

        #region 检查描述 —— string StudyDescription
        /// <summary>
        /// 检查描述
        /// </summary>
        [DependencyProperty]
        public string StudyDescription { get; set; }
        #endregion

        #region 检查ID —— string StudyId
        /// <summary>
        /// 检查ID
        /// </summary>
        [DependencyProperty]
        public string StudyId { get; set; }
        #endregion

        #region 登记号 —— string AccessionNumber
        /// <summary>
        /// 登记号
        /// </summary>
        /// <remarks>Accession Number</remarks>
        [DependencyProperty]
        public string AccessionNumber { get; set; }
        #endregion

        #region 转诊医生 —— string ReferringPhysician
        /// <summary>
        /// 转诊医生
        /// </summary>
        /// <remarks>Referring Physician Name</remarks>
        [DependencyProperty]
        public string ReferringPhysician { get; set; }
        #endregion
    }
}
