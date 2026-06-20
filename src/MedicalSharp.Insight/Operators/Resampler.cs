using itk.simple;
using MedicalSharp.Insight.Models;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Insight.Operators
{
    /// <summary>
    /// 重采样算子
    /// </summary>
    public sealed class Resampler
    {
        #region # 字段及构造器

        /// <summary>
        /// 体积数据
        /// </summary>
        private readonly SitkVolumeData _volumeData;

        /// <summary>
        /// 创建重采样算子构造器
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        public Resampler(VolumeData volumeData)
        {
            #region # 验证

            if (volumeData == null)
            {
                throw new ArgumentNullException(nameof(volumeData), "体积数据不可为空！");
            }
            if (volumeData is not SitkVolumeData sitkVolumeData)
            {
                throw new ArgumentNullException(nameof(volumeData), "volumeData必须是SitkVolumeData！");
            }

            #endregion

            this._volumeData = sitkVolumeData;
        }

        #endregion

        #region # 属性

        #region 只读属性 - SimpleITK图像 —— Image SitkImage
        /// <summary>
        /// 只读属性 - SimpleITK图像
        /// </summary>
        public Image SitkImage
        {
            get => this._volumeData.SitkPreviewImage;
        }
        #endregion

        #endregion

        #region # 方法

        #region 执行等体素重采样 —— Image ExecuteIsotropic(float spacing)
        /// <summary>
        /// 执行等体素重采样
        /// </summary>
        /// <param name="spacing">目标体素间距（毫米，默认1.0）</param>
        /// <returns>等体素重采样后的3D图像</returns>
        /// <remarks>
        /// 将图像重采样为X/Y/Z方向间距一致的体数据（正方体体素）。 
        /// 临床场景：
        /// - 深度学习模型（TotalSegmentator、nnU-Net等）要求等体素输入；
        /// - 三维可视化需要各向同性的体素，避免Z方向拉伸；
        /// - 形态学操作在非等体素数据上结果会变形 ；
        /// 保持物理范围不变，自动计算新的体素数量。
        /// 示例：512×512×200 体素，间距 0.6×0.6×1.2mm -> ExecuteIsotropic(0.6) -> 512×512×400 体素
        /// </remarks>
        public Image ExecuteIsotropic(float spacing = 1.0f)
        {
            #region # 验证

            if (spacing <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(spacing), "体素间距必须大于0！");
            }

            #endregion

            Image originalImage = this.SitkImage;
            VectorUInt32 originalSize = originalImage.GetSize();
            VectorDouble originalSpacing = originalImage.GetSpacing();
            VectorDouble originalOrigin = originalImage.GetOrigin();
            VectorDouble originalDirection = originalImage.GetDirection();

            //保持物理范围不变，根据新的间距计算新的体素数量
            using VectorUInt32 newSize = new VectorUInt32
            {
                (uint)Math.Ceiling(originalSize[0] * originalSpacing[0] / spacing),
                (uint)Math.Ceiling(originalSize[1] * originalSpacing[1] / spacing),
                (uint)Math.Ceiling(originalSize[2] * originalSpacing[2] / spacing)
            };

            //间距
            using VectorDouble newSpacing = new VectorDouble { spacing, spacing, spacing };

            //单位矩阵变换
            using Transform transform = new Transform(3, TransformEnum.sitkIdentity);

            using ResampleImageFilter resampler = new ResampleImageFilter();
            resampler.SetSize(newSize);
            resampler.SetOutputSpacing(newSpacing);
            resampler.SetOutputOrigin(originalOrigin);
            resampler.SetOutputDirection(originalDirection);
            resampler.SetTransform(transform);
            resampler.SetInterpolator(InterpolatorEnum.sitkLinear);
            resampler.SetDefaultPixelValue(-1024);

            Image resampledImage = resampler.Execute(originalImage);

            return resampledImage;
        }
        #endregion

        #region 执行指定尺寸重采样 —— Image ExecuteToSize(Vector3i newVolumeSize)
        /// <summary>
        /// 执行指定尺寸重采样
        /// </summary>
        /// <param name="newVolumeSize">目标体积尺寸（体素数）</param>
        /// <returns>重采样后的3D图像</returns>
        /// <remarks>
        /// 指定目标体素数，自动计算间距以保持物理范围不变。 
        /// 临床场景：
        /// - 快速原型验证：512³降采样到256³，滤波/分割速度快8倍
        /// - 显存受限：低分辨率数据送GPU推理，避免OOM
        /// - 多序列对齐：不同尺寸的数据统一到相同体素数 
        /// 示例：512³ -> ExecuteToSize(256³)，间距自动翻倍
        /// </remarks>
        public Image ExecuteToSize(Vector3i newVolumeSize)
        {
            #region # 验证

            if (newVolumeSize.X == 0 || newVolumeSize.Y == 0 || newVolumeSize.Z == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newVolumeSize), "目标尺寸各分量必须大于0！");
            }

            #endregion

            Image originalImage = this.SitkImage;
            VectorUInt32 originalSize = originalImage.GetSize();
            VectorDouble originalSpacing = originalImage.GetSpacing();
            VectorDouble originalOrigin = originalImage.GetOrigin();
            VectorDouble originalDirection = originalImage.GetDirection();

            //保持物理范围不变，反算新的间距
            using VectorUInt32 size = new VectorUInt32
            {
                (uint)newVolumeSize.X, (uint)newVolumeSize.Y, (uint)newVolumeSize.Z
            };
            using VectorDouble spacing = new VectorDouble
            {
                originalSize[0] * originalSpacing[0] / newVolumeSize.X,
                originalSize[1] * originalSpacing[1] / newVolumeSize.Y,
                originalSize[2] * originalSpacing[2] / newVolumeSize.Z
            };

            //单位矩阵变换
            using Transform transform = new Transform(3, TransformEnum.sitkIdentity);

            using ResampleImageFilter resampler = new ResampleImageFilter();
            resampler.SetSize(size);
            resampler.SetOutputSpacing(spacing);
            resampler.SetOutputOrigin(originalOrigin);
            resampler.SetOutputDirection(originalDirection);
            resampler.SetTransform(transform);
            resampler.SetInterpolator(InterpolatorEnum.sitkLinear);
            resampler.SetDefaultPixelValue(-1024);

            Image resampledImage = resampler.Execute(originalImage);

            return resampledImage;
        }
        #endregion

        #region 执行指定间距重采样 —— Image ExecuteToSpacing(Vector3 newSpacing)
        /// <summary>
        /// 执行指定间距重采样
        /// </summary>
        /// <param name="newSpacing">目标体素间距（毫米），可X/Y/Z方向不一致</param>
        /// <returns>重采样后的3D图像</returns>
        /// <remarks>
        /// 指定目标物理间距，自动计算体素数以保持物理范围不变。 
        /// 临床场景：
        /// - 多序列标准化：A序列层厚5mm，B序列层厚1.25mm，统一到相同间距后逐体素比较；
        /// - 层厚规范化：原始0.625mm薄层重采样到5mm厚层，模拟常规阅片条件；
        /// - 跨设备数据对齐：不同CT设备的层厚不同，统一间距后配准融合；
        /// 和ExecuteIsotropic的区别：允许X/Y/Z方向间距不同。
        /// 多序列比较时，间距一致比体素正方更重要。
        /// </remarks>
        public Image ExecuteToSpacing(Vector3 newSpacing)
        {
            #region # 验证

            if (newSpacing.X <= 0 || newSpacing.Y <= 0 || newSpacing.Z <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newSpacing), "目标间距各分量必须大于0！");
            }

            #endregion

            Image originalImage = this.SitkImage;
            VectorUInt32 originalSize = originalImage.GetSize();
            VectorDouble originalSpacing = originalImage.GetSpacing();
            VectorDouble originalOrigin = originalImage.GetOrigin();
            VectorDouble originalDirection = originalImage.GetDirection();

            using VectorDouble spacing = new VectorDouble
            {
                newSpacing.X, newSpacing.Y, newSpacing.Z
            };

            //保持物理范围不变，根据新的间距计算新的体素数量
            using VectorUInt32 size = new VectorUInt32
            {
                (uint)Math.Ceiling(originalSize[0] * originalSpacing[0] / newSpacing.X),
                (uint)Math.Ceiling(originalSize[1] * originalSpacing[1] / newSpacing.Y),
                (uint)Math.Ceiling(originalSize[2] * originalSpacing[2] / newSpacing.Z)
            };


            //单位矩阵变换
            using Transform transform = new Transform(3, TransformEnum.sitkIdentity);

            using ResampleImageFilter resampler = new ResampleImageFilter();
            resampler.SetSize(size);
            resampler.SetOutputSpacing(spacing);
            resampler.SetOutputOrigin(originalOrigin);
            resampler.SetOutputDirection(originalDirection);
            resampler.SetTransform(transform);
            resampler.SetInterpolator(InterpolatorEnum.sitkLinear);
            resampler.SetDefaultPixelValue(-1024);

            Image resampledImage = resampler.Execute(originalImage);

            return resampledImage;
        }
        #endregion

        #endregion
    }
}
