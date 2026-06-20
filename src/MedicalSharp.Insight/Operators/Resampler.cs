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

        #region 提取单张切片 —— Image ExtractSlice(Vector3 sliceCenter...
        /// <summary>
        /// 提取单张切片
        /// </summary>
        /// <param name="sliceCenter">切面中心（毫米空间）</param>
        /// <param name="sliceSize">切片尺寸（体素空间）</param>
        /// <param name="rowDirection">行方向单位向量（毫米空间，决定切面的水平方向）</param>
        /// <param name="colDirection">列方向单位向量（毫米空间，决定切面的垂直方向）</param>
        /// <returns>2D切片图像（Z方向尺寸为1）</returns>
        /// <remarks>
        /// 沿任意方向切出一张2D切片，用于：
        /// - 任意角度MPR截图；
        /// - 曲面重建的初始切面；
        /// - 导出单张斜切面供其他工具使用；
        /// 输出Z方向尺寸为1（单层），X/Y尺寸与原始数据一致
        /// </remarks>
        public Image ExtractSlice(Vector3 sliceCenter, Vector2i sliceSize, Vector3 rowDirection, Vector3 colDirection)
        {
            #region # 验证

            if (rowDirection == Vector3.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(rowDirection), "行方向向量不能为零向量！");
            }
            if (colDirection == Vector3.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(colDirection), "列方向向量不能为零向量！");
            }

            #endregion

            Image originalImage = this.SitkImage;
            VectorDouble originalSpacing = originalImage.GetSpacing();

            //切片尺寸
            using VectorUInt32 sliceUSize = new VectorUInt32
            {
                (uint)sliceSize.X, (uint)sliceSize.Y, 1
            };

            //切片间距
            double rowSpacing = ComputeProjectedSpacing(rowDirection, originalSpacing);
            double colSpacing = ComputeProjectedSpacing(colDirection, originalSpacing);
            using VectorDouble sliceSpacing = new VectorDouble
            {
                rowSpacing, colSpacing, 1.0
            };

            //切片原点（左上角） = 中心点 - 行方向偏移 - 列方向偏移
            float mmHalfWidth = (float)(sliceSize.X * rowSpacing * 0.5f);
            float mmHalfHeight = (float)(sliceSize.Y * colSpacing * 0.5f);
            Vector3 topLeft = sliceCenter - rowDirection * mmHalfWidth - colDirection * mmHalfHeight;
            using VectorDouble sliceOrigin = new VectorDouble
            {
                topLeft.X, topLeft.Y, topLeft.Z
            };

            //切片方向：行方向、列方向、法向量
            Vector3 normal = Vector3.Cross(rowDirection, colDirection).Normalized();
            using VectorDouble sliceDirection = new VectorDouble
            {
                rowDirection.X, rowDirection.Y, rowDirection.Z,
                colDirection.X, colDirection.Y, colDirection.Z,
                normal.X, normal.Y, normal.Z
            };

            //单位矩阵变换
            using Transform transform = new Transform(3, TransformEnum.sitkIdentity);

            using ResampleImageFilter resampler = new ResampleImageFilter();
            resampler.SetSize(sliceUSize);
            resampler.SetOutputSpacing(sliceSpacing);
            resampler.SetOutputOrigin(sliceOrigin);
            resampler.SetOutputDirection(sliceDirection);
            resampler.SetTransform(transform);
            resampler.SetInterpolator(InterpolatorEnum.sitkLinear); //线性插值保证图像质量
            resampler.SetDefaultPixelValue(-1024); //CT空气HU值
            Image resampledImage = resampler.Execute(originalImage);

            return resampledImage;
        }
        #endregion

        #region 提取切片序列 —— Image ExtractSliceSeries(Vector3 startOrigin...
        /// <summary>
        /// 提取切片序列
        /// </summary>
        /// <param name="startOrigin">序列起始位置（毫米空间，第一层的中心点）</param>
        /// <param name="rowDirection">行方向单位向量（毫米空间，决定切面的水平方向）</param>
        /// <param name="colDirection">列方向单位向量（毫米空间，决定切面的垂直方向）</param>
        /// <param name="sliceSpacing">层间距（毫米）</param>
        /// <param name="slicesCount">层数</param>
        /// <returns>切片序列3D图像</returns>
        /// <remarks>
        /// 从指定的起始位置开始，沿法向量方向逐层生成斜切序列。 
        /// 临床场景：
        /// - 用户确定起始解剖位置（如主动脉弓起始部），向指定方向切N层；
        /// - 沿血管走行方向，从近端到远端逐层重建；
        /// 层方向：从起始位置沿法向量方向推进，每层间距为 sliceSpacing 毫米；
        /// </remarks>
        public Image ExtractSliceSeries(Vector3 startOrigin, Vector3 rowDirection, Vector3 colDirection, double sliceSpacing, uint slicesCount)
        {
            #region # 验证

            if (rowDirection == Vector3.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(rowDirection), "行方向向量不能为零向量！");
            }
            if (colDirection == Vector3.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(colDirection), "列方向向量不能为零向量！");
            }
            if (sliceSpacing <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sliceSpacing), "层间距必须大于0！");
            }
            if (slicesCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(slicesCount), "层数必须大于0！");
            }

            #endregion

            Image originalImage = this.SitkImage;
            VectorUInt32 originalSize = originalImage.GetSize();
            VectorDouble originalSpacing = originalImage.GetSpacing();

            //法向量 = 行方向 × 列方向（右手定则）
            Vector3 normal = Vector3.Cross(rowDirection, colDirection).Normalized();

            //组装方向矩阵
            using VectorDouble obliqueDirection = new VectorDouble
            {
                rowDirection.X, rowDirection.Y, rowDirection.Z,
                colDirection.X, colDirection.Y, colDirection.Z,
                normal.X, normal.Y, normal.Z
            };

            //序列尺寸
            using VectorUInt32 seriesSize = new VectorUInt32
            {
                originalSize[0],
                originalSize[1],
                slicesCount
            };

            //序列间距
            using VectorDouble seriesSpacing = new VectorDouble
            {
                originalSpacing[0],
                originalSpacing[1],
                sliceSpacing
            };

            //序列原点
            using VectorDouble seriesOrigin = new VectorDouble
            {
                startOrigin.X, startOrigin.Y, startOrigin.Z
            };

            //单位矩阵变换
            using Transform transform = new Transform(3, TransformEnum.sitkIdentity);

            using ResampleImageFilter resampler = new ResampleImageFilter();
            resampler.SetSize(seriesSize);
            resampler.SetOutputSpacing(seriesSpacing);
            resampler.SetOutputOrigin(seriesOrigin);
            resampler.SetOutputDirection(obliqueDirection);
            resampler.SetTransform(transform);
            resampler.SetInterpolator(InterpolatorEnum.sitkLinear);
            resampler.SetDefaultPixelValue(-1024);

            Image resampledImage = resampler.Execute(originalImage);

            return resampledImage;
        }
        #endregion

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


        //Private

        #region 计算原始Spacing在指定方向上的投影长度 —— static double ComputeProjectedSpacing(...
        /// <summary>
        /// 计算原始Spacing在指定方向上的投影长度
        /// </summary>
        private static double ComputeProjectedSpacing(Vector3 direction, VectorDouble originalSpacing)
        {
            //方向向量在各轴上的分量平方，加权原始Spacing
            double dx = direction.X * direction.X * originalSpacing[0] * originalSpacing[0];
            double dy = direction.Y * direction.Y * originalSpacing[1] * originalSpacing[1];
            double dz = direction.Z * direction.Z * originalSpacing[2] * originalSpacing[2];
            double projectionLength = Math.Sqrt(dx + dy + dz);

            return projectionLength;
        }
        #endregion

        #endregion
    }
}
