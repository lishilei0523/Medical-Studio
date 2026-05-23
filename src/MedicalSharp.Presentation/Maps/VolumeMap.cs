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
        #region # 体积信息映射 —— static VolumeInfo ToVolumeInfo(this VolumeMetadata metadata)
        /// <summary>
        /// 体积信息映射
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

        #region # 患者信息映射 —— static PatientInfo ToPatientInfo(this VolumePatientData patientData)
        /// <summary>
        /// 患者信息映射
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

        #region # 检查信息映射 —— static StudyInfo ToStudyInfo(this VolumeStudyData studyData)
        /// <summary>
        /// 检查信息映射
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

        #region # 扫描信息映射 —— static ScanInfo ToScanInfo(this VolumeScanData scanData)
        /// <summary>
        /// 扫描信息映射
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

        #region # 统计信息映射 —— static StatisticInfo ToStatisticInfo(this StatisticResult result)
        /// <summary>
        /// 统计信息映射
        /// </summary>
        public static StatisticInfo ToStatisticInfo(this StatisticResult result)
        {
            StatisticInfo statisticInfo = new StatisticInfo
            {
                MinHU = result.MinHU.ToString("F0"),
                MaxHU = result.MaxHU.ToString("F0"),
                AverageHU = result.AverageHU.ToString("F0"),
                StdDevHU = result.StdDevHU.ToString("F0"),
                SurfaceArea = $"{result.SurfaceArea:F2}mm²",
                Volume = $"{result.Volume:F2}mm³",
                Sphericity = result.Sphericity.ToString("F2"),
                VoxelsCount = result.VoxelsCount.ToString()
            };

            return statisticInfo;
        }
        #endregion
    }
}
