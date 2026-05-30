using itk.simple;
using MedicalSharp.Insight.Models;
using MedicalSharp.Primitives.Constants;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace MedicalSharp.Insight
{
    /// <summary>
    /// SimpleITK DICOM加载器
    /// </summary>
    public class SitkDicomLoader : IDicomLoader
    {
        //Implements

        #region # 加载DICOM序列 —— VolumeData LoadSeries(string dicomFolder)
        /// <summary>
        /// 加载DICOM序列
        /// </summary>
        /// <param name="dicomFolder">DICOM文件夹</param>
        /// <returns>体积数据</returns>
        public VolumeData LoadSeries(string dicomFolder)
        {
            #region # 验证

            if (string.IsNullOrWhiteSpace(dicomFolder))
            {
                throw new ArgumentOutOfRangeException(nameof(dicomFolder), "文件夹不可为空！");
            }
            if (!Directory.Exists(dicomFolder))
            {
                throw new ArgumentOutOfRangeException(nameof(dicomFolder), "文件夹不存在！");
            }

            #endregion

            using VectorString dicomPaths = ImageSeriesReader.GetGDCMSeriesFileNames(dicomFolder);

            return this.LoadSeries(dicomPaths.ToList());
        }
        #endregion

        #region # 加载DICOM序列 —— VolumeData LoadSeries(IReadOnlyList<string> dicomPaths)
        /// <summary>
        /// 加载DICOM序列
        /// </summary>
        /// <param name="dicomPaths">DICOM文件路径列表</param>
        /// <returns>体积数据</returns>
        public VolumeData LoadSeries(IReadOnlyList<string> dicomPaths)
        {
            #region # 验证

            if (dicomPaths == null || !dicomPaths.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(dicomPaths), "文件路径不可为空！");
            }

            #endregion

            //创建图像序列读取器
            using ImageSeriesReader reader = new ImageSeriesReader();
            using VectorString dicomPathsV = new VectorString(dicomPaths);
            reader.SetFileNames(dicomPathsV);

            //读取元数据但不立即加载像素数据
            reader.LoadPrivateTagsOn();
            reader.MetaDataDictionaryArrayUpdateOn();

            //执行读取
            using Image image = reader.Execute();

            //创建体积数据
            SitkVolumeData volumeData = new SitkVolumeData();
            this.ExtractData(volumeData, image);
            this.ExtractExtraData(volumeData, dicomPaths);

            return volumeData;
        }
        #endregion

        #region # 加载NIFTI图像文件 —— VolumeData LoadNiiImage(string filePath)
        /// <summary>
        /// 加载NIFTI图像文件
        /// </summary>
        /// <param name="filePath">Nii文件路径</param>
        /// <returns>体积数据</returns>
        public VolumeData LoadNiiImage(string filePath)
        {
            #region # 验证

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentOutOfRangeException(nameof(filePath), "文件路径不可为空！");
            }
            if (!File.Exists(filePath))
            {
                throw new ArgumentOutOfRangeException(nameof(filePath), "文件不存在！");
            }

            #endregion

            //读取NIFTI图像
            using Image image = SimpleITK.ReadImage(filePath);

            //创建体积数据
            SitkVolumeData volumeData = new SitkVolumeData();
            this.ExtractData(volumeData, image);

            return volumeData;
        }
        #endregion

        #region # 加载MHD+RAW图像文件 —— VolumeData LoadRawImage(string filePath)
        /// <summary>
        /// 加载MHD+RAW图像文件
        /// </summary>
        /// <param name="filePath">MHD文件路径</param>
        /// <returns>体积数据</returns>
        public VolumeData LoadRawImage(string filePath)
        {
            #region # 验证

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentOutOfRangeException(nameof(filePath), "文件路径不可为空！");
            }
            if (!File.Exists(filePath))
            {
                throw new ArgumentOutOfRangeException(nameof(filePath), "MHD文件不存在！");
            }

            #endregion

            //读取MHD+RAW图像
            using Image image = SimpleITK.ReadImage(filePath);

            //创建体积数据
            SitkVolumeData volumeData = new SitkVolumeData();
            this.ExtractData(volumeData, image);

            return volumeData;
        }
        #endregion

        #region # 加载NIFTI预览文件 —— void LoadNiiPreview(VolumeData volumeData, string filePath)
        /// <summary>
        /// 加载NIFTI预览文件
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="filePath">Nii文件路径</param>
        public unsafe void LoadNiiPreview(VolumeData volumeData, string filePath)
        {
            #region # 验证

            if (volumeData == null)
            {
                throw new ArgumentNullException(nameof(volumeData), "体积数据不可为空！");
            }
            if (volumeData.PreviewData == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(volumeData), "预览数据指针不可为空！");
            }
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentOutOfRangeException(nameof(filePath), "文件路径不可为空！");
            }
            if (!File.Exists(filePath))
            {
                throw new ArgumentOutOfRangeException(nameof(filePath), "文件不存在！");
            }

            #endregion

            using Image previewImage = SimpleITK.ReadImage(filePath);
            PixelIDValueEnum pixelFormat = previewImage.GetPixelID();
            VectorUInt32 imageSize = previewImage.GetSize();

            #region # 验证

            if (pixelFormat != PixelIDValueEnum.sitkInt16)
            {
                throw new InvalidOperationException("预览图必须是short类型！");
            }
            if ((imageSize[0] != volumeData.Metadata.VolumeSize.X))
            {
                throw new InvalidOperationException("宽度不匹配！");
            }
            if ((imageSize[1] != volumeData.Metadata.VolumeSize.Y))
            {
                throw new InvalidOperationException("高度不匹配！");
            }
            if ((imageSize[2] != volumeData.Metadata.VolumeSize.Z))
            {
                throw new InvalidOperationException("深度不匹配！");
            }

            #endregion

            IntPtr previewPtr = previewImage.GetBufferAsInt16();
            uint bufferSize = imageSize[0] * imageSize[1] * imageSize[2] * sizeof(short);
            Buffer.MemoryCopy(previewPtr.ToPointer(), volumeData.PreviewData.ToPointer(), bufferSize, bufferSize);
        }
        #endregion

        #region # 加载MHD+RAW预览文件 —— void LoadRawPreview(VolumeData volumeData, string filePath)
        /// <summary>
        /// 加载MHD+RAW预览文件
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="filePath">MHD文件路径</param>
        public void LoadRawPreview(VolumeData volumeData, string filePath)
        {
            this.LoadNiiPreview(volumeData, filePath);
        }
        #endregion

        #region # 加载NIFTI标记文件 —— void LoadNiiMark(VolumeData volumeData, string filePath)
        /// <summary>
        /// 加载NIFTI标记文件
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="filePath">Nii文件路径</param>
        public unsafe void LoadNiiMark(VolumeData volumeData, string filePath)
        {
            #region # 验证

            if (volumeData == null)
            {
                throw new ArgumentNullException(nameof(volumeData), "体积数据不可为空！");
            }
            if (volumeData.MarkData == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(volumeData), "标记数据指针不可为空！");
            }
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentOutOfRangeException(nameof(filePath), "文件路径不可为空！");
            }
            if (!File.Exists(filePath))
            {
                throw new ArgumentOutOfRangeException(nameof(filePath), "文件不存在！");
            }

            #endregion

            using Image markImage = SimpleITK.ReadImage(filePath);
            PixelIDValueEnum pixelFormat = markImage.GetPixelID();
            VectorUInt32 imageSize = markImage.GetSize();

            #region # 验证

            if (pixelFormat != PixelIDValueEnum.sitkUInt8)
            {
                throw new InvalidOperationException("标记图必须是label类型！");
            }
            if ((imageSize[0] != volumeData.Metadata.VolumeSize.X))
            {
                throw new InvalidOperationException("宽度不匹配！");
            }
            if ((imageSize[1] != volumeData.Metadata.VolumeSize.Y))
            {
                throw new InvalidOperationException("高度不匹配！");
            }
            if ((imageSize[2] != volumeData.Metadata.VolumeSize.Z))
            {
                throw new InvalidOperationException("深度不匹配！");
            }

            #endregion

            IntPtr markPtr = markImage.GetBufferAsUInt8();
            uint bufferSize = imageSize[0] * imageSize[1] * imageSize[2] * sizeof(byte);
            Buffer.MemoryCopy(markPtr.ToPointer(), volumeData.MarkData.ToPointer(), bufferSize, bufferSize);
        }
        #endregion

        #region # 加载MHD+RAW标记文件 —— void LoadRawMark(VolumeData volumeData, string filePath)
        /// <summary>
        /// 加载MHD+RAW标记文件
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="filePath">MHD文件路径</param>
        public void LoadRawMark(VolumeData volumeData, string filePath)
        {
            this.LoadNiiMark(volumeData, filePath);
        }
        #endregion

        #region # 保存原始NIFTI图像文件 —— void SaveOriginalNiiImage(VolumeData volumeData, string filePath)
        /// <summary>
        /// 保存原始NIFTI图像文件
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="filePath">Nii文件路径</param>
        public void SaveOriginalNiiImage(VolumeData volumeData, string filePath)
        {
            #region # 验证

            if (volumeData is not SitkVolumeData sitkVolume)
            {
                throw new ArgumentOutOfRangeException(nameof(volumeData), "volumeData必须是SitkVolumeData");
            }
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentOutOfRangeException(nameof(filePath), "文件路径不可为空！");
            }

            #endregion

            SimpleITK.WriteImage(sitkVolume.SitkOriginalImage, filePath);
        }
        #endregion

        #region # 保存原始MHD+RAW图像文件 —— void SaveOriginalRawImage(VolumeData volumeData, string filePath)
        /// <summary>
        /// 保存原始MHD+RAW图像文件
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="filePath">MHD文件路径</param>
        public void SaveOriginalRawImage(VolumeData volumeData, string filePath)
        {
            #region # 验证

            if (volumeData is not SitkVolumeData sitkVolume)
            {
                throw new ArgumentOutOfRangeException(nameof(volumeData), "volumeData必须是SitkVolumeData");
            }
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentOutOfRangeException(nameof(filePath), "文件路径不可为空！");
            }

            #endregion

            SimpleITK.WriteImage(sitkVolume.SitkOriginalImage, filePath);
        }
        #endregion

        #region # 保存预览NIFTI图像文件 —— void SavePreviewNiiImage(VolumeData volumeData, string filePath)
        /// <summary>
        /// 保存预览NIFTI图像文件
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="filePath">Nii文件路径</param>
        public void SavePreviewNiiImage(VolumeData volumeData, string filePath)
        {
            #region # 验证

            if (volumeData is not SitkVolumeData sitkVolume)
            {
                throw new ArgumentOutOfRangeException(nameof(volumeData), "volumeData必须是SitkVolumeData");
            }
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentOutOfRangeException(nameof(filePath), "文件路径不可为空！");
            }

            #endregion

            SimpleITK.WriteImage(sitkVolume.SitkPreviewImage, filePath);
        }
        #endregion

        #region # 保存预览MHD+RAW图像文件 —— void SavePreviewRawImage(VolumeData volumeData, string filePath)
        /// <summary>
        /// 保存预览MHD+RAW图像文件
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="filePath">MHD文件路径</param>
        public void SavePreviewRawImage(VolumeData volumeData, string filePath)
        {
            #region # 验证

            if (volumeData is not SitkVolumeData sitkVolume)
            {
                throw new ArgumentOutOfRangeException(nameof(volumeData), "volumeData必须是SitkVolumeData");
            }
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentOutOfRangeException(nameof(filePath), "文件路径不可为空！");
            }

            #endregion

            SimpleITK.WriteImage(sitkVolume.SitkPreviewImage, filePath);
        }
        #endregion

        #region # 保存标记NIFTI图像文件 —— void SaveMarkNiiImage(VolumeData volumeData, string filePath)
        /// <summary>
        /// 保存标记NIFTI图像文件
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="filePath">Nii文件路径</param>
        public void SaveMarkNiiImage(VolumeData volumeData, string filePath)
        {
            #region # 验证

            if (volumeData is not SitkVolumeData sitkVolume)
            {
                throw new ArgumentOutOfRangeException(nameof(volumeData), "volumeData必须是SitkVolumeData");
            }
            if (sitkVolume.SitkMarkImage == null)
            {
                throw new InvalidOperationException("标记数据未分配");
            }
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentOutOfRangeException(nameof(filePath), "文件路径不可为空！");
            }

            #endregion

            SimpleITK.WriteImage(sitkVolume.SitkMarkImage, filePath);
        }
        #endregion

        #region # 保存标记MHD+RAW图像文件 —— void SaveMarkRawImage(VolumeData volumeData, string filePath)
        /// <summary>
        /// 保存标记MHD+RAW图像文件
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="filePath">MHD文件路径</param>
        public void SaveMarkRawImage(VolumeData volumeData, string filePath)
        {
            this.SaveMarkNiiImage(volumeData, filePath);
        }
        #endregion


        //Private

        #region # 提取数据 —— void ExtractData(SitkVolumeData volumeData...
        /// <summary>
        /// 提取数据
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="image">SimpleITK图像</param>
        private void ExtractData(SitkVolumeData volumeData, Image image)
        {
            #region # 验证

            if (image == null)
            {
                throw new ArgumentNullException(nameof(image), "SimpleITK图像不可为空！");
            }

            #endregion

            //获取图像尺寸
            VectorUInt32 size = image.GetSize();
            if (size.Count < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(volumeData), "Image is not 3D");
            }

            volumeData.Metadata.VolumeSize = new Vector3i((int)size[0], (int)size[1], (int)size[2]);

            //获取像素间距
            VectorDouble spacing = image.GetSpacing();
            volumeData.Metadata.Spacing = new Vector3((float)spacing[0], (float)spacing[1], (float)spacing[2]);

            //计算实际尺寸
            volumeData.Metadata.PhysicalSize = new Vector3
            (
                volumeData.Metadata.VolumeSize.X * volumeData.Metadata.Spacing.X,
                volumeData.Metadata.VolumeSize.Y * volumeData.Metadata.Spacing.Y,
                volumeData.Metadata.VolumeSize.Z * volumeData.Metadata.Spacing.Z
            );

            //计算缩放
            float maxSide = Math.Max(volumeData.Metadata.PhysicalSize.X, volumeData.Metadata.PhysicalSize.Y);
            volumeData.Metadata.VolumeScale = new Vector3
            {
                X = volumeData.Metadata.PhysicalSize.X / maxSide,
                Y = volumeData.Metadata.PhysicalSize.Y / maxSide,
                Z = volumeData.Metadata.PhysicalSize.Z / maxSide
            };

            //获取图像原点和方向
            VectorDouble origin = image.GetOrigin();
            VectorDouble direction = image.GetDirection();
            volumeData.Metadata.Origin = new Vector3((float)origin[0], (float)origin[1], (float)origin[2]);
            volumeData.Metadata.RowDirection = new Vector3((float)direction[0], (float)direction[1], (float)direction[2]);
            volumeData.Metadata.ColDirection = new Vector3((float)direction[3], (float)direction[4], (float)direction[5]);
            volumeData.Metadata.SliceDirection = new Vector3((float)direction[6], (float)direction[7], (float)direction[8]);

            //转换像素类型为short
            Image normalizedImage = image.GetPixelID() != PixelIDValueEnum.sitkInt16
                ? SimpleITK.Cast(image, PixelIDValueEnum.sitkInt16)
                : new Image(image);

            //获取体素原始数据
            volumeData.Metadata.VoxelsCount = (long)volumeData.Metadata.VolumeSize.X * volumeData.Metadata.VolumeSize.Y * volumeData.Metadata.VolumeSize.Z;
            volumeData.SitkOriginalImage = normalizedImage;
            if (volumeData.OriginalData == IntPtr.Zero)
            {
                throw new InvalidCastException("Failed to get pixel buffer");
            }

            //计算HU最小值、最大值
            CalculateMinMax(volumeData.OriginalData, volumeData.Metadata.VoxelsCount, out short minHU, out short maxHU);
            volumeData.Metadata.MinHU = minHU;
            volumeData.Metadata.MaxHU = maxHU;

            //创建预览图像
            volumeData.CreatePreview();

            //分配标记数据内存
            volumeData.AllocMarkData();
        }
        #endregion

        #region # 提取扩展数据 —— void ExtractExtraData(SitkVolumeData volumeData...
        /// <summary>
        /// 提取扩展数据
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="dicomPaths">DICOM文件路径列表</param>
        private void ExtractExtraData(SitkVolumeData volumeData, IReadOnlyList<string> dicomPaths)
        {
            Func<Image, string, string> getTagValue = (image, tag) =>
            {
                if (image.HasMetaDataKey(tag))
                {
                    return image.GetMetaData(tag)?.Trim();
                }
                return null;
            };

            //取第一张切片单独读元数据
            using Image slice = SimpleITK.ReadImage(dicomPaths[0]);

            //提取扩展元数据
            string rescaleSlope = getTagValue(slice, DicomTags.RescaleSlope);
            string rescaleIntercept = getTagValue(slice, DicomTags.RescaleIntercept);
            string windowWidth = getTagValue(slice, DicomTags.WindowWidth);
            string windowCenter = getTagValue(slice, DicomTags.WindowCenter);
            if (!string.IsNullOrWhiteSpace(rescaleSlope))
            {
                volumeData.Metadata.RescaleSlope = float.Parse(rescaleSlope);
            }
            if (!string.IsNullOrWhiteSpace(rescaleIntercept))
            {
                volumeData.Metadata.RescaleIntercept = float.Parse(rescaleIntercept);
            }
            if (!string.IsNullOrWhiteSpace(windowWidth))
            {
                if (windowWidth.Contains('\\'))
                {
                    windowWidth = windowWidth.Split('\\')[0];
                }
                volumeData.Metadata.WindowWidth = (int)MathF.Round(float.Parse(windowWidth));
            }
            if (!string.IsNullOrWhiteSpace(windowCenter))
            {
                if (windowCenter.Contains('\\'))
                {
                    windowCenter = windowCenter.Split('\\')[0];
                }
                volumeData.Metadata.WindowCenter = (int)MathF.Round(float.Parse(windowCenter));
            }

            //提取患者数据
            volumeData.PatientData.PatientId = getTagValue(slice, DicomTags.PatientID);
            volumeData.PatientData.Name = getTagValue(slice, DicomTags.PatientName);
            volumeData.PatientData.BirthDate = getTagValue(slice, DicomTags.PatientBirthDate);
            volumeData.PatientData.Sex = getTagValue(slice, DicomTags.PatientSex);
            volumeData.PatientData.Age = getTagValue(slice, DicomTags.PatientAge);
            volumeData.PatientData.Height = getTagValue(slice, DicomTags.PatientSize);
            volumeData.PatientData.Weight = getTagValue(slice, DicomTags.PatientWeight);

            //提取检查数据
            volumeData.StudyData.StudyInstanceUId = getTagValue(slice, DicomTags.StudyInstanceUID);
            volumeData.StudyData.StudyDate = getTagValue(slice, DicomTags.StudyDate);
            volumeData.StudyData.StudyTime = getTagValue(slice, DicomTags.StudyTime);
            volumeData.StudyData.StudyDescription = getTagValue(slice, DicomTags.StudyDescription);
            volumeData.StudyData.StudyId = getTagValue(slice, DicomTags.StudyID);
            volumeData.StudyData.AccessionNumber = getTagValue(slice, DicomTags.AccessionNumber);
            volumeData.StudyData.ReferringPhysician = getTagValue(slice, DicomTags.ReferringPhysicianName);
            volumeData.StudyData.InstitutionName = getTagValue(slice, DicomTags.InstitutionName);

            //提取序列数据
            volumeData.SeriesData.SeriesInstanceUId = getTagValue(slice, DicomTags.SeriesInstanceUID);
            volumeData.SeriesData.SeriesNumber = getTagValue(slice, DicomTags.SeriesNumber);
            volumeData.SeriesData.SeriesDate = getTagValue(slice, DicomTags.SeriesDate);
            volumeData.SeriesData.SeriesTime = getTagValue(slice, DicomTags.SeriesTime);
            volumeData.SeriesData.Modality = getTagValue(slice, DicomTags.Modality);
            volumeData.SeriesData.BodyPartExamined = getTagValue(slice, DicomTags.BodyPartExamined);
            volumeData.SeriesData.SliceThickness = getTagValue(slice, DicomTags.SliceThickness);
            volumeData.SeriesData.SpacingBetweenSlices = getTagValue(slice, DicomTags.SpacingBetweenSlices);
            volumeData.SeriesData.SeriesDescription = getTagValue(slice, DicomTags.SeriesDescription);

            //提取扫描数据
            volumeData.ScanData.KVP = getTagValue(slice, DicomTags.KVP);
            volumeData.ScanData.ExposureTime = getTagValue(slice, DicomTags.ExposureTime);
            volumeData.ScanData.XRayTubeCurrent = getTagValue(slice, DicomTags.XRayTubeCurrent);
            volumeData.ScanData.ConvolutionKernel = getTagValue(slice, DicomTags.ConvolutionKernel);
            volumeData.ScanData.ReconstructionDiameter = getTagValue(slice, DicomTags.ReconstructionDiameter);
            volumeData.ScanData.PitchFactor = getTagValue(slice, DicomTags.PitchFactor);
            volumeData.ScanData.ReconstructionAlgorithm = getTagValue(slice, DicomTags.ReconstructionAlgorithm);
            volumeData.ScanData.ContrastAgent = getTagValue(slice, DicomTags.ContrastAgent);
            volumeData.ScanData.ContrastDose = getTagValue(slice, DicomTags.ContrastDose);
        }
        #endregion

        #region # 计算体素HU最小最大值 —— static void CalculateMinMax(IntPtr originalData...
        /// <summary>
        /// 计算体素HU最小最大值
        /// </summary>
        /// <remarks>SIMD加速</remarks>
        private static unsafe void CalculateMinMax(IntPtr originalData, long voxelsCount, out short minHU, out short maxHU)
        {
            short* pointer = (short*)originalData.ToPointer();

            int vectorSize = Vector<short>.Count;
            Vector<short> vecMin = new Vector<short>(short.MaxValue);
            Vector<short> vecMax = new Vector<short>(short.MinValue);

            long index = 0;
            long simdEnd = voxelsCount - vectorSize;
            for (; index <= simdEnd; index += vectorSize)
            {
                Vector<short> vec = Unsafe.Read<Vector<short>>(pointer + index);
                vecMin = Vector.Min(vecMin, vec);
                vecMax = Vector.Max(vecMax, vec);
            }

            minHU = short.MaxValue;
            maxHU = short.MinValue;
            for (int j = 0; j < vectorSize; j++)
            {
                minHU = Math.Min(minHU, vecMin[j]);
                maxHU = Math.Max(maxHU, vecMax[j]);
            }
            for (; index < voxelsCount; index++)
            {
                short val = pointer[index];
                if (val < minHU)
                {
                    minHU = val;
                }
                if (val > maxHU)
                {
                    maxHU = val;
                }
            }
        }
        #endregion
    }
}
