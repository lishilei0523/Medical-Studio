using MedicalSharp.Presentation.Models;
using MedicalSharp.Primitives.Models;
using SD.Toolkits.Mapper;
using System;

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
                VolumeSize = $"{metadata.VolumeSize.X}×{metadata.VolumeSize.Y}×{metadata.VolumeSize.Z}",
                Spacing = $"{metadata.Spacing.X:F2}×{metadata.Spacing.Y:F2}×{metadata.Spacing.Z:F2}",
                PhysicalSize = $"{metadata.PhysicalSize.X:F0}×{metadata.PhysicalSize.Y:F0}×{metadata.PhysicalSize.Z:F0}",
                RescaleSlope = metadata.RescaleSlope.ToString("F2"),
                RescaleIntercept = metadata.RescaleIntercept.ToString("F2"),
                HURange = $"{metadata.MinHU} ~ {metadata.MaxHU}",
                WindowLevel = $"{metadata.WindowWidth}, {metadata.WindowCenter}"
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

            PatientInfo patientInfo = patientData.Map<VolumePatientData, PatientInfo>();

            return patientInfo;
        }
        #endregion

        #region # 体积检查数据映射检查信息 —— static StudyInfo ToStudyInfo(this VolumeStudyData studyData)
        /// <summary>
        /// 体积检查数据映射检查信息
        /// </summary>
        public static StudyInfo ToStudyInfo(this VolumeStudyData studyData)
        {
            #region # 验证

            if (studyData == null)
            {
                return null;
            }

            #endregion

            StudyInfo studyInfo = studyData.Map<VolumeStudyData, StudyInfo>();
            if (!string.IsNullOrWhiteSpace(studyData.StudyDate))
            {
                DateOnly studyDate = DateOnly.ParseExact(studyData.StudyDate, "yyyyMMdd");
                studyInfo.StudyDate = studyDate.ToString("yyyy-MM-dd");
            }
            if (!string.IsNullOrWhiteSpace(studyData.StudyTime))
            {
                TimeOnly studyTime = TimeOnly.ParseExact(studyData.StudyTime, "HHmmss");
                studyInfo.StudyTime = studyTime.ToString("HH:mm:ss");
            }

            return studyInfo;
        }
        #endregion

        #region # 体积扫描数据映射扫描信息 —— static ScanInfo ToScanInfo(this VolumeScanData scanData)
        /// <summary>
        /// 体积扫描数据映射扫描信息
        /// </summary>
        public static ScanInfo ToScanInfo(this VolumeScanData scanData)
        {
            #region # 验证

            if (scanData == null)
            {
                return null;
            }

            #endregion

            ScanInfo scanInfo = scanData.Map<VolumeScanData, ScanInfo>();

            return scanInfo;
        }
        #endregion
    }
}
