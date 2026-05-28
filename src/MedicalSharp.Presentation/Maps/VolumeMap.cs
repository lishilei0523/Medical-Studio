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
            if (!string.IsNullOrWhiteSpace(patientData.Sex))
            {
                patientInfo.Sex = patientData.Sex switch
                {
                    "M" => "男",
                    "F" => "女",
                    _ => patientData.Sex
                };
            }
            if (!string.IsNullOrWhiteSpace(patientData.Age))
            {
                patientInfo.Age = int.TryParse(patientData.Age.Replace("Y", string.Empty), out int age)
                    ? $"{age}岁"
                    : patientData.Age;
            }
            if (!string.IsNullOrWhiteSpace(patientData.BirthDate))
            {
                try
                {
                    DateOnly birthDate = DateOnly.ParseExact(patientData.BirthDate[..8], "yyyyMMdd");
                    patientInfo.BirthDate = birthDate.ToString("yyyy-MM-dd");
                }
                catch
                {
                    patientInfo.BirthDate = patientData.BirthDate;
                }
            }

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
                try
                {
                    DateOnly studyDate = DateOnly.ParseExact(studyData.StudyDate[..8], "yyyyMMdd");
                    studyInfo.StudyDate = studyDate.ToString("yyyy-MM-dd");
                }
                catch
                {
                    studyInfo.StudyDate = studyData.StudyDate;
                }
            }
            if (!string.IsNullOrWhiteSpace(studyData.StudyTime))
            {
                try
                {
                    TimeOnly studyTime = TimeOnly.ParseExact(studyData.StudyTime[..6], "HHmmss");
                    studyInfo.StudyTime = studyTime.ToString("HH:mm:ss");
                }
                catch
                {
                    studyInfo.StudyTime = studyData.StudyTime;
                }
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

            ScanInfo scanInfo = new ScanInfo
            {
                Modality = scanData.Modality,
                KVP = $"{scanData.KVP} kVp",
                XRayTubeCurrent = $"{scanData.XRayTubeCurrent} mA",
                ExposureTime = $"{scanData.ExposureTime} ms",
                ConvolutionKernel = $"{scanData.ConvolutionKernel}",
                ReconstructionDiameter = $"{scanData.ReconstructionDiameter} mm",
                SliceThickness = $"{scanData.SliceThickness} mm"
            };

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
                VoxelsCount = result.VoxelsCount.ToString()
            };

            return statisticInfo;
        }
        #endregion
    }
}
