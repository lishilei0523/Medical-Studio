using Caliburn.Micro;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;

namespace MedicalSharp.Presentation.Models
{
    /// <summary>
    /// 患者信息
    /// </summary>
    public class PatientInfo : PropertyChangedBase
    {
        #region 患者ID —— string PatientId
        /// <summary>
        /// 患者ID
        /// </summary>
        /// <remarks>病历号</remarks>
        [DependencyProperty]
        public string PatientId { get; set; }
        #endregion

        #region 患者姓名 —— string Name
        /// <summary>
        /// 患者姓名
        /// </summary>
        /// <remarks>DOE^JOHN</remarks>
        [DependencyProperty]
        public string Name { get; set; }
        #endregion

        #region 出生日期 —— string BirthDate
        /// <summary>
        /// 出生日期
        /// </summary>
        /// <remarks>YYYYMMDD</remarks>
        [DependencyProperty]
        public string BirthDate { get; set; }
        #endregion

        #region 性别 —— string Sex
        /// <summary>
        /// 性别
        /// </summary>
        /// <remarks>M/F/O</remarks>
        [DependencyProperty]
        public string Sex { get; set; }
        #endregion

        #region 年龄 —— string Age
        /// <summary>
        /// 年龄
        /// </summary>
        /// <remarks>060Y</remarks>
        [DependencyProperty]
        public string Age { get; set; }
        #endregion

        #region 身高 —— string Height
        /// <summary>
        /// 身高
        /// </summary>
        /// <remarks>cm</remarks>
        [DependencyProperty]
        public string Height { get; set; }
        #endregion

        #region 体重 —— string Weight
        /// <summary>
        /// 体重
        /// </summary>
        /// <remarks>kg</remarks>
        [DependencyProperty]
        public string Weight { get; set; }
        #endregion
    }
}
