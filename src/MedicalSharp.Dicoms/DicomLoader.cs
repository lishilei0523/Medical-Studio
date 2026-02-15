using itk.simple;
using MedicalSharp.Dicoms.Constants;
using MedicalSharp.Dicoms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MedicalSharp.Dicoms
{
    /// <summary>
    /// DICOM加载器
    /// </summary>
    public static class DicomLoader
    {
        //Public

        #region # 加载DICOM序列 —— static Volume LoadSeries(string dicomFolder)
        /// <summary>
        /// 加载DICOM序列
        /// </summary>
        /// <param name="dicomFolder">DICOM文件夹</param>
        /// <returns>体积数据</returns>
        public static Volume LoadSeries(string dicomFolder)
        {
            VectorString filePaths = ImageSeriesReader.GetGDCMSeriesFileNames(dicomFolder);

            return LoadSeries(filePaths);
        }
        #endregion

        #region # 加载DICOM序列 —— static Volume LoadSeries(ICollection<string> dicomPaths)
        /// <summary>
        /// 加载DICOM序列
        /// </summary>
        /// <param name="dicomPaths">DICOM文件路径集</param>
        /// <returns>体积数据</returns>
        public static Volume LoadSeries(ICollection<string> dicomPaths)
        {
            #region # 验证

            if (!dicomPaths.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(dicomPaths), "文件路径不可为空！");
            }

            #endregion

            //创建图像系列读取器
            using ImageSeriesReader reader = new ImageSeriesReader();
            reader.SetFileNames(new VectorString(dicomPaths));

            //读取元数据但不立即加载像素数据
            reader.LoadPrivateTagsOn();
            reader.MetaDataDictionaryArrayUpdateOn();

            //执行读取
            Image image = reader.Execute();

            //创建体积数据
            Volume volume = new Volume(image);
            ExtractData(volume);

            return volume;
        }
        #endregion


        //Private

        #region # 提取数据 —— static unsafe void ExtractData(Volume volume)
        /// <summary>
        /// 提取数据
        /// </summary>
        /// <param name="volume">体积数据</param>
        private static unsafe void ExtractData(Volume volume)
        {
            #region # 验证

            if (!volume.SitkImage)
            {
                throw new ArgumentOutOfRangeException(nameof(volume), "SimpleITK图像不可为空！");
            }

            #endregion

            Image image = volume.SitkImage;

            //获取图像尺寸
            VectorUInt32 size = image.GetSize();
            if (size.Count < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(volume), "Image is not 3D");
            }

            volume.VolumeSize = new Vector3(size[0], size[1], size[2]);

            //获取像素间距
            VectorDouble spacing = image.GetSpacing();
            volume.Spacing = new Vector3((float)spacing[0], (float)spacing[1], (float)spacing[2]);

            //计算实际尺寸
            volume.ActualSize = new Vector3
            {
                X = volume.VolumeSize.X * volume.Spacing.X,
                Y = volume.VolumeSize.Y * volume.Spacing.Y,
                Z = volume.VolumeSize.Z * volume.Spacing.Z
            };

            //计算缩放
            float maxSide = Math.Max(volume.ActualSize.X, Math.Max(volume.ActualSize.Y, volume.ActualSize.Z));
            volume.VolumeScale = new Vector3
            {
                X = volume.ActualSize.X / maxSide,
                Y = volume.ActualSize.Y / maxSide,
                Z = volume.ActualSize.Z / maxSide
            };

            //获取斜率和截距
            if (image.HasMetaDataKey(DicomTags.RescaleSlope))
            {
                volume.RescaleSlope = float.Parse(image.GetMetaData(DicomTags.RescaleSlope));
            }
            if (image.HasMetaDataKey(DicomTags.RescaleIntercept))
            {
                volume.RescaleIntercept = float.Parse(image.GetMetaData(DicomTags.RescaleIntercept));
            }

            //获取图像原点和方向
            VectorDouble origin = image.GetOrigin();
            VectorDouble direction = image.GetDirection();
            volume.Origin = new Vector3((float)origin[0], (float)origin[1], (float)origin[2]);
            volume.RowDirection = new Vector3((float)direction[0], (float)direction[1], (float)direction[2]);
            volume.ColDirection = new Vector3((float)direction[3], (float)direction[4], (float)direction[5]);
            volume.SliceDirection = new Vector3((float)direction[6], (float)direction[7], (float)direction[8]);

            //获取窗宽窗位
            if (image.HasMetaDataKey(DicomTags.WindowWidth))
            {
                volume.WindowWidth = float.Parse(image.GetMetaData(DicomTags.WindowWidth));
            }
            if (image.HasMetaDataKey(DicomTags.WindowCenter))
            {
                volume.WindowCenter = float.Parse(image.GetMetaData(DicomTags.WindowCenter));
            }

            //转换像素类型为short
            if (image.GetPixelID() != PixelIDValueEnum.sitkInt16)
            {
                image = SimpleITK.Cast(image, PixelIDValueEnum.sitkInt16);
            }

            //获取体素原始数据
            volume.VoxelsCount = (long)volume.VolumeSize.X * (long)volume.VolumeSize.Y * (long)volume.VolumeSize.Z;
            volume.OriginalData = (short*)image.GetBufferAsVoid().ToPointer();
            if (volume.OriginalData == null)
            {
                throw new InvalidCastException("Failed to get pixel buffer");
            }
#if DEBUG
            Span<short> span = new Span<short>(volume.OriginalData, (int)volume.VoxelsCount);
            short[] voxels = span.ToArray();
            short minVal = voxels.Min();
            short maxVal = voxels.Max();
            Console.WriteLine($"Loaded {volume.VoxelsCount} voxels, min={minVal}, max={maxVal}");
#endif
        }
        #endregion

    }
}
