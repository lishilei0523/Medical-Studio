using itk.simple;
using MedicalSharp.Dicoms.Constants;
using MedicalSharp.Dicoms.Models;
using MedicalSharp.Dicoms.ValueTypes;
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

        #region # 加载DICOM序列 —— static VolumeData LoadSeries(string dicomFolder)
        /// <summary>
        /// 加载DICOM序列
        /// </summary>
        /// <param name="dicomFolder">DICOM文件夹</param>
        /// <returns>体积数据</returns>
        public static VolumeData LoadSeries(string dicomFolder)
        {
            using VectorString dicomPaths = ImageSeriesReader.GetGDCMSeriesFileNames(dicomFolder);

            return LoadSeries(dicomPaths.ToList());
        }
        #endregion

        #region # 加载DICOM序列 —— static VolumeData LoadSeries(ICollection<string> dicomPaths)
        /// <summary>
        /// 加载DICOM序列
        /// </summary>
        /// <param name="dicomPaths">DICOM文件路径集</param>
        /// <returns>体积数据</returns>
        public static VolumeData LoadSeries(ICollection<string> dicomPaths)
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
            VolumeData volumeData = new VolumeData(image);
            ExtractData(volumeData);

            return volumeData;
        }
        #endregion


        //Private

        #region # 提取数据 —— static void ExtractData(VolumeData volumeData)
        /// <summary>
        /// 提取数据
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        private static void ExtractData(VolumeData volumeData)
        {
            #region # 验证

            if (volumeData.SitkImage == null)
            {
                throw new ArgumentOutOfRangeException(nameof(volumeData), "SimpleITK图像不可为空！");
            }

            #endregion

            Image image = volumeData.SitkImage;

            //获取图像尺寸
            VectorUInt32 size = image.GetSize();
            if (size.Count < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(volumeData), "Image is not 3D");
            }

            volumeData.VoxelSize = new Size3I((int)size[0], (int)size[1], (int)size[2]);

            //获取像素间距
            VectorDouble spacing = image.GetSpacing();
            volumeData.Spacing = new Size3F((float)spacing[0], (float)spacing[1], (float)spacing[2]);

            //计算实际尺寸
            volumeData.ActualSize = new Size3F
            (
                volumeData.VoxelSize.Width * volumeData.Spacing.Width,
                volumeData.VoxelSize.Height * volumeData.Spacing.Height,
                volumeData.VoxelSize.Depth * volumeData.Spacing.Depth
            );

            //计算缩放
            float maxSide = Math.Max(volumeData.ActualSize.Width, Math.Max(volumeData.ActualSize.Height, volumeData.ActualSize.Depth));
            volumeData.VolumeScale = new Vector3
            {
                X = volumeData.ActualSize.Width / maxSide,
                Y = volumeData.ActualSize.Height / maxSide,
                Z = volumeData.ActualSize.Depth / maxSide
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
            if (image.GetPixelID() != PixelIDValueEnum.sitkInt16)
            {
                image = SimpleITK.Cast(image, PixelIDValueEnum.sitkInt16);
            }

            //获取体素原始数据
            volumeData.VoxelsCount = (long)volumeData.VoxelSize.Width * volumeData.VoxelSize.Height * volumeData.VoxelSize.Depth;
            volumeData.OriginalData = image.GetBufferAsInt16();
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
