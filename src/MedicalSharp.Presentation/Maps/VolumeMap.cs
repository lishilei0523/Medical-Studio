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
                VoxelsCount = metadata.VoxelsCount,
                VolumeSize = $"{metadata.VolumeSize.X}×{metadata.VolumeSize.Y}×{metadata.VolumeSize.Z}",
                Spacing = $"{metadata.Spacing.X:F2}×{metadata.Spacing.Y:F2}×{metadata.Spacing.Z:F2} mm",
                PhysicalSize = $"{metadata.PhysicalSize.X:F0}×{metadata.PhysicalSize.Y:F0}×{metadata.PhysicalSize.Z:F0} mm",
                RescaleSlope = metadata.RescaleSlope.ToString("F2"),
                RescaleIntercept = metadata.RescaleIntercept.ToString("F2"),
                HURange = $"{metadata.MinHU} ~ {metadata.MaxHU}",
                Origin = $"{metadata.Origin.X}, {metadata.Origin.Y}, {metadata.Origin.Z}",
                RowDirection = $"{metadata.RowDirection.X}, {metadata.RowDirection.Y}, {metadata.RowDirection.Z}",
                ColDirection = $"{metadata.ColDirection.X}, {metadata.ColDirection.Y}, {metadata.ColDirection.Z}",
                SliceDirection = $"{metadata.SliceDirection.X}, {metadata.SliceDirection.Y}, {metadata.SliceDirection.Z}",
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
                    "Male" => "男",
                    "male" => "男",
                    "F" => "女",
                    "Female" => "女",
                    "female" => "女",
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

            string date = string.Empty;
            string time = string.Empty;
            if (!string.IsNullOrWhiteSpace(studyData.StudyDate))
            {
                try
                {
                    DateOnly studyDate = DateOnly.ParseExact(studyData.StudyDate[..8], "yyyyMMdd");
                    date = studyDate.ToString("yyyy-MM-dd");
                }
                catch
                {
                    date = studyData.StudyDate;
                }
            }
            if (!string.IsNullOrWhiteSpace(studyData.StudyTime))
            {
                try
                {
                    TimeOnly studyTime = TimeOnly.ParseExact(studyData.StudyTime[..6], "HHmmss");
                    time = studyTime.ToString("HH:mm:ss");
                }
                catch
                {
                    time = studyData.StudyTime;
                }
            }

            studyInfo.StudyDateTime = $"{date} {time}";

            return studyInfo;
        }
        #endregion

        #region # 序列信息映射 —— static SeriesInfo ToSeriesInfo(this VolumeSeriesData seriesData)
        /// <summary>
        /// 序列信息映射
        /// </summary>
        public static SeriesInfo ToSeriesInfo(this VolumeSeriesData seriesData)
        {
            #region # 验证

            if (seriesData == null)
            {
                return null;
            }

            #endregion

            SeriesInfo seriesInfo = new SeriesInfo
            {
                SeriesInstanceUId = seriesData.SeriesInstanceUId,
                SeriesNumber = seriesData.SeriesNumber,
                BodyPartExamined = seriesData.BodyPartExamined,
                SliceThickness = string.IsNullOrWhiteSpace(seriesData.SliceThickness)
                    ? null
                    : $"{seriesData.SliceThickness} mm",
                SpacingBetweenSlices = string.IsNullOrWhiteSpace(seriesData.SpacingBetweenSlices)
                    ? null
                    : $"{seriesData.SpacingBetweenSlices} mm",
                SeriesDescription = seriesData.SeriesDescription
            };

            if (!string.IsNullOrWhiteSpace(seriesData.SeriesDate))
            {
                try
                {
                    DateOnly studyDate = DateOnly.ParseExact(seriesData.SeriesDate[..8], "yyyyMMdd");
                    seriesInfo.SeriesDate = studyDate.ToString("yyyy-MM-dd");
                }
                catch
                {
                    seriesInfo.SeriesDate = seriesData.SeriesDate;
                }
            }
            if (!string.IsNullOrWhiteSpace(seriesData.SeriesTime))
            {
                try
                {
                    TimeOnly studyTime = TimeOnly.ParseExact(seriesData.SeriesTime[..6], "HHmmss");
                    seriesInfo.SeriesTime = studyTime.ToString("HH:mm:ss");
                }
                catch
                {
                    seriesInfo.SeriesTime = seriesData.SeriesTime;
                }
            }

            return seriesInfo;
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
                KVP = string.IsNullOrWhiteSpace(scanData.KVP)
                    ? null
                    : $"{scanData.KVP} kVp",
                XRayTubeCurrent = string.IsNullOrWhiteSpace(scanData.XRayTubeCurrent)
                    ? null
                    : $"{scanData.XRayTubeCurrent} mA",
                ExposureTime = string.IsNullOrWhiteSpace(scanData.ExposureTime)
                    ? null
                    : $"{scanData.ExposureTime} ms",
                ConvolutionKernel = $"{scanData.ConvolutionKernel}",
                ReconstructionDiameter = string.IsNullOrWhiteSpace(scanData.ReconstructionDiameter)
                    ? null
                    : $"{scanData.ReconstructionDiameter} mm",
                PitchFactor = scanData.PitchFactor,
                ReconstructionAlgorithm = scanData.ReconstructionAlgorithm,
                MagneticFieldStrength = string.IsNullOrWhiteSpace(scanData.MagneticFieldStrength)
                    ? null
                    : $"{scanData.MagneticFieldStrength} T",
                RepetitionTime = string.IsNullOrWhiteSpace(scanData.RepetitionTime)
                    ? null
                    : $"{scanData.RepetitionTime} ms",
                EchoTime = string.IsNullOrWhiteSpace(scanData.EchoTime)
                    ? null
                    : $"{scanData.EchoTime} ms",
                SequenceName = scanData.SequenceName,
                ContrastAgent = scanData.ContrastAgent,
                ContrastDose = string.IsNullOrWhiteSpace(scanData.ContrastDose)
                    ? null
                    : $"{scanData.ContrastDose} ml"
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
            #region # 验证

            if (result == null)
            {
                return null;
            }

            #endregion

            StatisticInfo statisticInfo = new StatisticInfo
            {
                MinHU = result.MinHU.ToString("F0"),
                MaxHU = result.MaxHU.ToString("F0"),
                AverageHU = result.AverageHU.ToString("F0"),
                StdDevHU = result.StdDevHU.ToString("F0"),
                Perimeter = $"{result.Perimeter:F2} mm",
                SurfaceArea = $"{result.SurfaceArea:F2} mm²",
                Volume = $"{result.Volume:F2} mm³",
                VoxelsCount = result.VoxelsCount.ToString()
            };

            return statisticInfo;
        }
        #endregion

        #region # 方向信息映射 —— static FourDirectionInfo ToDirectionInfo(this FourDirection fourDirection)
        /// <summary>
        /// 方向信息映射
        /// </summary>
        public static FourDirectionInfo ToDirectionInfo(this FourDirection fourDirection)
        {
            #region # 验证

            if (fourDirection == null)
            {
                return null;
            }

            #endregion

            FourDirectionInfo directionInfo = new FourDirectionInfo
            {
                Top = fourDirection.Top,
                Bottom = fourDirection.Bottom,
                Left = fourDirection.Left,
                Right = fourDirection.Right
            };

            return directionInfo;
        }
        #endregion
    }
}
