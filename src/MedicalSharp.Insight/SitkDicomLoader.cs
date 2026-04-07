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

        #region # 加载DICOM序列 —— VolumeData LoadSeries(ICollection<string> dicomPaths)
        /// <summary>
        /// 加载DICOM序列
        /// </summary>
        /// <param name="dicomPaths">DICOM文件路径集</param>
        /// <returns>体积数据</returns>
        public VolumeData LoadSeries(ICollection<string> dicomPaths)
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

            return volumeData;
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

            volumeData.VolumeSize = new Vector3i((int)size[0], (int)size[1], (int)size[2]);

            //获取像素间距
            VectorDouble spacing = image.GetSpacing();
            volumeData.Spacing = new Vector3((float)spacing[0], (float)spacing[1], (float)spacing[2]);

            //计算实际尺寸
            volumeData.PhysicalSize = new Vector3
            (
                volumeData.VolumeSize.X * volumeData.Spacing.X,
                volumeData.VolumeSize.Y * volumeData.Spacing.Y,
                volumeData.VolumeSize.Z * volumeData.Spacing.Z
            );

            //计算缩放
            float maxSide = Math.Max(volumeData.PhysicalSize.X, volumeData.PhysicalSize.Y);
            volumeData.VolumeScale = new Vector3
            {
                X = volumeData.PhysicalSize.X / maxSide,
                Y = volumeData.PhysicalSize.Y / maxSide,
                Z = volumeData.PhysicalSize.Z / maxSide
            };

            //获取斜率和截距
            if (image.HasMetaDataKey(DicomTags.RescaleSlope))
            {
                volumeData.RescaleSlope = float.Parse(image.GetMetaData(DicomTags.RescaleSlope));
            }
            if (image.HasMetaDataKey(DicomTags.RescaleIntercept))
            {
                volumeData.RescaleIntercept = float.Parse(image.GetMetaData(DicomTags.RescaleIntercept));
            }

            //获取图像原点和方向
            VectorDouble origin = image.GetOrigin();
            VectorDouble direction = image.GetDirection();
            volumeData.Origin = new Vector3((float)origin[0], (float)origin[1], (float)origin[2]);
            volumeData.RowDirection = new Vector3((float)direction[0], (float)direction[1], (float)direction[2]);
            volumeData.ColDirection = new Vector3((float)direction[3], (float)direction[4], (float)direction[5]);
            volumeData.SliceDirection = new Vector3((float)direction[6], (float)direction[7], (float)direction[8]);

            //获取窗宽窗位
            if (image.HasMetaDataKey(DicomTags.WindowWidth))
            {
                volumeData.WindowWidth = float.Parse(image.GetMetaData(DicomTags.WindowWidth));
            }
            if (image.HasMetaDataKey(DicomTags.WindowCenter))
            {
                volumeData.WindowCenter = float.Parse(image.GetMetaData(DicomTags.WindowCenter));
            }

            //转换像素类型为short
            Image normalizedImage = image.GetPixelID() != PixelIDValueEnum.sitkInt16
                ? SimpleITK.Cast(image, PixelIDValueEnum.sitkInt16)
                : new Image(image);

            //获取体素原始数据
            volumeData.VoxelsCount = (long)volumeData.VolumeSize.X * volumeData.VolumeSize.Y * volumeData.VolumeSize.Z;
            volumeData.SitkImage = normalizedImage;
            if (volumeData.OriginalData == IntPtr.Zero)
            {
                throw new InvalidCastException("Failed to get pixel buffer");
            }
#if DEBUG
            unsafe
            {
                //Span<short> span = new Span<short>(volumeData.OriginalData.ToPointer(), (int)volumeData.VoxelsCount);
                //short[] voxels = span.ToArray();
                //short minVal = voxels.Min();
                //short maxVal = voxels.Max();
                //System.Diagnostics.Trace.WriteLine($"Loaded {volumeData.VoxelsCount} voxels, min={minVal}, max={maxVal}");
            }
#endif
        }
        #endregion 
    }
}
