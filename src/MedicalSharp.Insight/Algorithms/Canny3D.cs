using itk.simple;
using MedicalSharp.Insight.Models;
using MedicalSharp.Primitives.Models;
using System;

namespace MedicalSharp.Insight.Algorithms
{
    /// <summary>
    /// Canny边缘检测算法
    /// </summary>
    public class Canny3D
    {
        #region # 字段及构造器

        /// <summary>
        /// 体积数据
        /// </summary>
        private readonly VolumeData _volumeData;

        /// <summary>
        /// 创建Canny边缘检测算法构造器
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        public Canny3D(VolumeData volumeData)
        {
            #region # 验证

            if (volumeData == null)
            {
                throw new ArgumentNullException(nameof(volumeData), "体积数据不可为空！");
            }
            if (volumeData is not SitkVolumeData)
            {
                throw new ArgumentNullException(nameof(volumeData), "volumeData必须是SitkVolumeData！");
            }

            #endregion

            this._volumeData = volumeData;
        }

        #endregion

        #region # 属性

        #region 只读属性 - SimpleITK图像 —— Image SitkImage
        /// <summary>
        /// 只读属性 - SimpleITK图像
        /// </summary>
        public Image SitkImage
        {
            get
            {
                SitkVolumeData volumeData = (SitkVolumeData)this._volumeData;
                Image sitkImage = volumeData.SitkPreviewImage;

                return sitkImage;
            }
        }
        #endregion

        #endregion

        #region # 方法

        #region 执行Canny边缘检测算法 —— void Execute(double lowerThreshold...
        /// <summary>
        /// 执行Canny边缘检测算法
        /// </summary>
        /// <param name="lowerThreshold">低阈值（弱边缘，默认10）</param>
        /// <param name="upperThreshold">高阈值（强边缘，默认30）</param>
        /// <param name="sigma">高斯滤波标准差（默认1.0）</param>
        public unsafe void Execute(double lowerThreshold = 10.0, double upperThreshold = 30.0, double sigma = 1.0)
        {
            #region # 验证

            if (lowerThreshold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lowerThreshold), "低阈值必须为非负数！");
            }
            if (upperThreshold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(upperThreshold), "高阈值必须为非负数！");
            }
            if (lowerThreshold >= upperThreshold)
            {
                throw new ArgumentOutOfRangeException(nameof(lowerThreshold), "低阈值必须小于高阈值！");
            }
            if (sigma <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sigma), "高斯滤波标准差必须大于0！");
            }

            #endregion

            Image sitkImage = this.SitkImage;

            //高斯滤波（对应Cv2.Canny的第一步：平滑去噪）
            using Image smoothed = SimpleITK.SmoothingRecursiveGaussian(sitkImage, sigma);

            //计算梯度强度（对应Cv2.Sobel + 梯度强度计算）
            using Image gradientMagnitude = SimpleITK.GradientMagnitudeRecursiveGaussian(smoothed, sigma);

            //双阈值检测
            using Image strongEdges = SimpleITK.BinaryThreshold(gradientMagnitude, upperThreshold, double.MaxValue);
            using Image weakEdges = SimpleITK.BinaryThreshold(gradientMagnitude, lowerThreshold, upperThreshold - 1e-10);

            //滞后跟踪：弱边缘如果连接到强边缘就保留
            using Image weakDilated = SimpleITK.BinaryDilate(weakEdges, new VectorUInt32([3, 3, 3]));
            using Image connected = SimpleITK.And(weakDilated, strongEdges);

            //最终的边缘是强边缘 + 连通的弱边缘
            using Image result = SimpleITK.Or(strongEdges, connected);
            using Image targetImage = SimpleITK.Cast(result, PixelIDValueEnum.sitkInt16);

            //原地写回
            IntPtr targetPtr = targetImage.GetBufferAsInt16();
            IntPtr sourcePtr = sitkImage.GetBufferAsInt16();
            VectorUInt32 imageSize = sitkImage.GetSize();
            uint bufferSize = imageSize[0] * imageSize[1] * imageSize[2] * sizeof(short);
            Buffer.MemoryCopy(targetPtr.ToPointer(), sourcePtr.ToPointer(), bufferSize, bufferSize);
        }
        #endregion

        #endregion
    }
}
