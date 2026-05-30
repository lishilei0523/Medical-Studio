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

        #region 检查日期时间 —— string StudyDateTime
        /// <summary>
        /// 检查日期时间
        /// </summary>
        [DependencyProperty]
        public string StudyDateTime { get; set; }
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

        #region 机构名称 —— string InstitutionName
        /// <summary>
        /// 机构名称
        /// </summary>
        /// <remarks>LO</remarks>
        [DependencyProperty]
        public string InstitutionName { get; set; }
        #endregion
    }
}
