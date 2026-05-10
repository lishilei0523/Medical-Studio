using MedicalSharp.Presentation.Models;
using MedicalSharp.Primitives.Models;

namespace MedicalSharp.Presentation.Maps
{
    /// <summary>
    /// 体积相关映射
    /// </summary>
    public static class VolumeMap
    {
        #region # 体积元数据映射体积信息 —— static VolumeInfo ToVolumeInfo(this VolumeMetadata metadata)
        /// <summary>
        /// 体积元数据映射体积信息
        /// </summary>
        public static VolumeInfo ToVolumeInfo(this VolumeMetadata metadata)
        {
            #region # 验证

            if (metadata == null)
            {
                return null;
            }

            #endregion

            VolumeInfo volumeInfo = new VolumeInfo
            {
                SeriesInstanceUId = metadata.SeriesInstanceUId,
                VolumeWidth = metadata.VolumeSize.X,
                VolumeHeight = metadata.VolumeSize.Y,
                VolumeDepth = metadata.VolumeSize.Z
            };

            return volumeInfo;
        }
        #endregion

        #region # 体积患者数据映射患者信息 —— static PatientInfo ToPatientInfo(this VolumePatientData patientData)
        /// <summary>
        /// 体积患者数据映射患者信息
        /// </summary>
        public static PatientInfo ToPatientInfo(this VolumePatientData patientData)
        {
            #region # 验证

            if (patientData == null)
            {
                return null;
            }

            #endregion

            PatientInfo patientInfo = new PatientInfo
            {
                PatientId = patientData.PatientId,
                Name = patientData.Name,
                BirthDate = patientData.BirthDate,
                Sex = patientData.Sex,
                Age = patientData.Age,
                Height = patientData.Height,
                Weight = patientData.Weight
            };

            return patientInfo;
        }
        #endregion
    }
}
