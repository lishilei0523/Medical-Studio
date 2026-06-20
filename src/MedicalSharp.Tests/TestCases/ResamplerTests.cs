using itk.simple;
using MedicalSharp.Insight;
using MedicalSharp.Insight.Operators;
using MedicalSharp.Primitives.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTK.Mathematics;
using System;
using System.Linq;

namespace MedicalSharp.Tests.TestCases
{
    /// <summary>
    /// 重采样算子测试
    /// </summary>
    [TestClass]
    public class ResamplerTests
    {
        #region # 测试提取横断面切片 —— void TestExtractAxialSlice()
        /// <summary>
        /// 测试提取横断面切片
        /// </summary>
        [TestMethod]
        public unsafe void TestExtractAxialSlice()
        {
            //准备：32×32×16，Spacing=0.7×0.7×2.0，中心切横断面
            Vector3i volumeSize = new Vector3i(32, 32, 16);
            Vector3 spacing = new Vector3(0.7f, 0.7f, 2.0f);
            SitkDicomLoader loader = new SitkDicomLoader();
            using Image originalImage = CreateTestImage(volumeSize, spacing);
            using VolumeData volumeData = loader.LoadSitkImage(originalImage);
            Resampler resampler = new Resampler(volumeData);

            //计算患者空间中心
            Vector3 patientCenter = new Vector3(
                (volumeSize.X * spacing.X * 0.5f),
                (volumeSize.Y * spacing.Y * 0.5f),
                (volumeSize.Z * spacing.Z * 0.5f)
            );

            //执行：提取横断面切片（尺寸=XY方向体素数）
            Vector2i sliceSize = new Vector2i(volumeSize.X, volumeSize.Y);
            using Image slice = resampler.ExtractSlice(patientCenter, sliceSize, Vector3.UnitX, Vector3.UnitY);

            //验证：尺寸
            VectorUInt32 resultSize = slice.GetSize();
            Assert.AreEqual((uint)volumeSize.X, resultSize[0], "X方向尺寸应等于原始X尺寸");
            Assert.AreEqual((uint)volumeSize.Y, resultSize[1], "Y方向尺寸应等于原始Y尺寸");
            Assert.AreEqual(1u, resultSize[2], "Z方向应为单层");

            //验证：Spacing应等于原始X/Y方向Spacing
            VectorDouble resultSpacing = slice.GetSpacing();
            Assert.AreEqual(spacing.X, resultSpacing[0], 0.001, "行方向Spacing应等于原始X Spacing");
            Assert.AreEqual(spacing.Y, resultSpacing[1], 0.001, "列方向Spacing应等于原始Y Spacing");

            //验证：法向量 = X × Y = Z
            VectorDouble resultDirection = slice.GetDirection();
            Assert.AreEqual(0.0, resultDirection[6], 0.001);
            Assert.AreEqual(0.0, resultDirection[7], 0.001);
            Assert.AreEqual(1.0, resultDirection[8], 0.001);

            //验证：切片数据不为空
            int voxelCount = (int)(resultSize[0] * resultSize[1]);
            short[] sliceData = new Span<short>(slice.GetBufferAsInt16().ToPointer(), voxelCount).ToArray();
            Assert.IsTrue(sliceData.Any(hu => hu == 0), "切片应包含背景值0");
        }
        #endregion

        #region # 测试提取冠状面切片 —— void TestExtractCoronalSlice()
        /// <summary>
        /// 测试提取冠状面切片
        /// </summary>
        [TestMethod]
        public void TestExtractCoronalSlice()
        {
            //准备：32×32×16，Spacing=0.7×0.7×2.0
            Vector3i volumeSize = new Vector3i(32, 32, 16);
            Vector3 spacing = new Vector3(0.7f, 0.7f, 2.0f);
            SitkDicomLoader loader = new SitkDicomLoader();
            using Image originalImage = CreateTestImage(volumeSize, spacing);
            using VolumeData volumeData = loader.LoadSitkImage(originalImage);
            Resampler resampler = new Resampler(volumeData);

            Vector3 patientCenter = new Vector3(
                (volumeSize.X * spacing.X * 0.5f),
                (volumeSize.Y * spacing.Y * 0.5f),
                (volumeSize.Z * spacing.Z * 0.5f)
            );

            //执行：提取冠状面切片（尺寸=XZ方向体素数）
            Vector2i sliceSize = new Vector2i(volumeSize.X, volumeSize.Z);
            using Image slice = resampler.ExtractSlice(patientCenter, sliceSize, Vector3.UnitX, Vector3.UnitZ);

            //验证：尺寸
            VectorUInt32 resultSize = slice.GetSize();
            Assert.AreEqual((uint)volumeSize.X, resultSize[0], "X方向尺寸应等于原始X尺寸");
            Assert.AreEqual((uint)volumeSize.Z, resultSize[1], "Y方向尺寸应等于原始Z尺寸");

            //验证：Spacing应等于原始X和Z方向Spacing
            VectorDouble resultSpacing = slice.GetSpacing();
            Assert.AreEqual(spacing.X, resultSpacing[0], 0.001, "行方向Spacing应等于原始X Spacing");
            Assert.AreEqual(spacing.Z, resultSpacing[1], 0.001, "列方向Spacing应等于原始Z Spacing");
        }
        #endregion

        #region # 测试提取矢状面切片 —— void TestExtractSagittalSlice()
        /// <summary>
        /// 测试提取矢状面切片
        /// </summary>
        [TestMethod]
        public void TestExtractSagittalSlice()
        {
            Vector3i volumeSize = new Vector3i(32, 32, 16);
            Vector3 spacing = new Vector3(0.7f, 0.7f, 2.0f);
            SitkDicomLoader loader = new SitkDicomLoader();
            using Image originalImage = CreateTestImage(volumeSize, spacing);
            using VolumeData volumeData = loader.LoadSitkImage(originalImage);
            Resampler resampler = new Resampler(volumeData);

            Vector3 patientCenter = new Vector3(
                (volumeSize.X * spacing.X * 0.5f),
                (volumeSize.Y * spacing.Y * 0.5f),
                (volumeSize.Z * spacing.Z * 0.5f)
            );

            //矢状面：行=Y轴，列=Z轴
            Vector2i sliceSize = new Vector2i(volumeSize.Y, volumeSize.Z);
            using Image slice = resampler.ExtractSlice(
                patientCenter, sliceSize,
                Vector3.UnitY, Vector3.UnitZ);

            VectorUInt32 resultSize = slice.GetSize();
            Assert.AreEqual((uint)volumeSize.Y, resultSize[0]);
            Assert.AreEqual((uint)volumeSize.Z, resultSize[1]);

            VectorDouble resultSpacing = slice.GetSpacing();
            Assert.AreEqual(spacing.Y, resultSpacing[0], 0.001);
            Assert.AreEqual(spacing.Z, resultSpacing[1], 0.001);
        }
        #endregion

        #region # 测试提取斜切面切片 —— void TestExtractObliqueSlice()
        /// <summary>
        /// 测试提取斜切面切片
        /// </summary>
        /// <remarks>冠状面绕X轴旋转30°斜切</remarks>
        [TestMethod]
        public void TestExtractObliqueSlice()
        {
            Vector3i volumeSize = new Vector3i(32, 32, 16);
            Vector3 spacing = new Vector3(0.7f, 0.7f, 2.0f);
            SitkDicomLoader loader = new SitkDicomLoader();
            using Image originalImage = CreateTestImage(volumeSize, spacing);
            using VolumeData volumeData = loader.LoadSitkImage(originalImage);
            Resampler resampler = new Resampler(volumeData);

            Vector3 patientCenter = new Vector3(
                (volumeSize.X * spacing.X * 0.5f),
                (volumeSize.Y * spacing.Y * 0.5f),
                (volumeSize.Z * spacing.Z * 0.5f)
            );

            //绕X轴旋转30°：行方向=X轴，列方向在YZ平面内旋转30°
            float angle = MathF.PI / 6; //30°
            Vector3 rowDirection = Vector3.UnitX;
            Vector3 colDirection = new Vector3(0, MathF.Cos(angle), MathF.Sin(angle)).Normalized();

            //冠状面尺寸：X × Z
            Vector2i sliceSize = new Vector2i(volumeSize.X, volumeSize.Z);
            using Image slice = resampler.ExtractSlice(patientCenter, sliceSize, rowDirection, colDirection);

            //验证：切片不为空
            int voxelsCount = sliceSize.X * sliceSize.Y;
            unsafe
            {
                short* data = (short*)slice.GetBufferAsInt16().ToPointer();
                short[] sliceData = new Span<short>(data, voxelsCount).ToArray();
                Assert.IsTrue(sliceData.Any(hu => hu == 0), "斜切切片应包含背景值0");
                Assert.IsFalse(sliceData.All(hu => hu == -1024), "斜切切片不应全是填充值-1024");
            }
        }
        #endregion

        #region # 测试斜切后几何信息传递 —— void TestObliqueSliceGeometryPreservation()
        /// <summary>
        /// 测试斜切后Origin和Direction正确传递
        /// </summary>
        [TestMethod]
        public void TestObliqueSliceGeometryPreservation()
        {
            Vector3i volumeSize = new Vector3i(20, 20, 20);
            Vector3d spacing = new Vector3d(0.8, 0.8, 2.0);
            SitkDicomLoader loader = new SitkDicomLoader();
            using Image originalImage = CreateTestImage(volumeSize, spacing, new Vector3d(10, -20, 30), Vector3d.UnitX, Vector3d.UnitY, Vector3d.UnitZ);
            using VolumeData volumeData = loader.LoadSitkImage(originalImage);
            Resampler resampler = new Resampler(volumeData);

            Vector3 patientCenter = new Vector3(
                10f + (float)(volumeSize.X * spacing.X * 0.5),
                -20f + (float)(volumeSize.Y * spacing.Y * 0.5),
                30f + (float)(volumeSize.Z * spacing.Z * 0.5));

            Vector2i sliceSize = new Vector2i(volumeSize.X, volumeSize.Y);
            using Image slice = resampler.ExtractSlice(
                patientCenter, sliceSize,
                Vector3.UnitX, Vector3.UnitY);

            // 验证：Origin应位于切片左上角（中心点 - 行偏移 - 列偏移）
            VectorDouble sliceOrigin = slice.GetOrigin();
            float mmHalfWidth = (float)(sliceSize.X * spacing.X * 0.5);
            float mmHalfHeight = (float)(sliceSize.Y * spacing.Y * 0.5);
            Assert.AreEqual(patientCenter.X - mmHalfWidth, sliceOrigin[0], 0.01, "Origin.X = center.X - halfWidth");
            Assert.AreEqual(patientCenter.Y - mmHalfHeight, sliceOrigin[1], 0.01, "Origin.Y = center.Y - halfHeight");

            // 验证：Direction矩阵正确（横断面 = 单位矩阵）
            VectorDouble dir = slice.GetDirection();
            Assert.AreEqual(1.0, dir[0], 0.001);
            Assert.AreEqual(1.0, dir[4], 0.001);
            Assert.AreEqual(1.0, dir[8], 0.001);
        }
        #endregion

        #region # 测试提取切片序列 —— void TestExtractSliceSeries()
        /// <summary>
        /// 测试提取切片序列
        /// </summary>
        [TestMethod]
        public void TestExtractSliceSeries()
        {
            //准备：10×10×10
            Vector3i volumeSize = new Vector3i(10, 10, 10);
            Vector3d spacing = new Vector3d(1.0, 1.0, 1.0);
            SitkDicomLoader loader = new SitkDicomLoader();
            using Image originalImage = CreateTestImage(volumeSize, spacing);
            using VolumeData volumeData = loader.LoadSitkImage(originalImage);
            Resampler resampler = new Resampler(volumeData);

            //执行：沿Z方向切5层，间距2mm
            Vector3 startOrigin = new Vector3(5, 5, 0);
            Vector3 rowDirection = Vector3.UnitX;
            Vector3 colDirection = Vector3.UnitY;
            double sliceSpacing = 2.0;
            uint slicesCount = 5;
            using Image result = resampler.ExtractSliceSeries(startOrigin, rowDirection, colDirection, sliceSpacing, slicesCount);

            //验证层数
            VectorUInt32 resultSize = result.GetSize();
            Assert.AreEqual(slicesCount, resultSize[2]);

            //验证间距
            VectorDouble resultSpacing = result.GetSpacing();
            Assert.AreEqual(sliceSpacing, resultSpacing[2], 0.001);
        }
        #endregion

        #region # 测试等体素重采样 —— void TestExecuteIsotropic()
        /// <summary>
        /// 测试等体素重采样
        /// </summary>
        [TestMethod]
        public void TestExecuteIsotropic()
        {
            //准备：10×10×5, Spacing=0.6×0.6×1.2
            Vector3i volumeSize = new Vector3i(10, 10, 5);
            Vector3d spacing = new Vector3d(0.6, 0.6, 1.2);
            SitkDicomLoader loader = new SitkDicomLoader();
            using Image originalImage = CreateTestImage(volumeSize, spacing);
            using VolumeData volumeData = loader.LoadSitkImage(originalImage);
            Resampler resampler = new Resampler(volumeData);

            //执行
            float unifiedSpacing = 0.6f;
            using Image result = resampler.ExecuteIsotropic(unifiedSpacing);

            //验证：Spacing 三个方向相等
            VectorDouble resultSpacing = result.GetSpacing();
            Assert.AreEqual(unifiedSpacing, resultSpacing[0], 0.001);
            Assert.AreEqual(unifiedSpacing, resultSpacing[1], 0.001);
            Assert.AreEqual(unifiedSpacing, resultSpacing[2], 0.001);

            //验证：物理范围不变 -> Z方向体素数翻倍
            VectorUInt32 resultSize = result.GetSize();
            Assert.AreEqual(10u, resultSize[0]);
            Assert.AreEqual(10u, resultSize[1]);
            Assert.AreEqual(10u, resultSize[2]); //5 × 1.2 / 0.6 = 10
        }
        #endregion

        #region # 测试指定尺寸重采样 —— void TestExecuteToSize()
        /// <summary>
        /// 测试指定尺寸重采样
        /// </summary>
        [TestMethod]
        public void TestExecuteToSize()
        {
            //准备：10×10×10, Spacing=1×1×1
            Vector3i volumeSize = new Vector3i(10, 10, 10);
            Vector3d spacing = new Vector3d(1.0, 1.0, 1.0);
            SitkDicomLoader loader = new SitkDicomLoader();
            using Image originalImage = CreateTestImage(volumeSize, spacing);
            using VolumeData volumeData = loader.LoadSitkImage(originalImage);
            Resampler resampler = new Resampler(volumeData);

            //执行：降采样到5×5×5
            Vector3i newVolumeSize = new Vector3i(5, 5, 5);
            using Image result = resampler.ExecuteToSize(newVolumeSize);

            //验证：尺寸正确
            VectorUInt32 resultSize = result.GetSize();
            Assert.AreEqual((uint)newVolumeSize[0], resultSize[0]);
            Assert.AreEqual((uint)newVolumeSize[1], resultSize[1]);
            Assert.AreEqual((uint)newVolumeSize[2], resultSize[2]);

            //验证：间距翻倍以保持物理范围
            VectorDouble resultSpacing = result.GetSpacing();
            Assert.AreEqual(2.0, resultSpacing[0], 0.001);
            Assert.AreEqual(2.0, resultSpacing[1], 0.001);
            Assert.AreEqual(2.0, resultSpacing[2], 0.001);
        }
        #endregion

        #region # 测试指定间距重采样 —— void TestExecuteToSpacing()
        /// <summary>
        /// 测试指定间距重采样
        /// </summary>
        [TestMethod]
        public void TestExecuteToSpacing()
        {
            //准备：10×10×10, Spacing = 1×1×1
            Vector3i volumeSize = new Vector3i(10, 10, 10);
            Vector3d spacing = new Vector3d(1.0, 1.0, 1.0);
            SitkDicomLoader loader = new SitkDicomLoader();
            using Image originalImage = CreateTestImage(volumeSize, spacing);
            using VolumeData volumeData = loader.LoadSitkImage(originalImage);
            Resampler resampler = new Resampler(volumeData);

            //执行：间距改为2×2×2
            Vector3 newSpacing = new Vector3(2, 2, 2);
            using Image result = resampler.ExecuteToSpacing(newSpacing);

            //验证：尺寸减半
            VectorUInt32 resultSize = result.GetSize();
            Assert.AreEqual(5u, resultSize[0]);
            Assert.AreEqual(5u, resultSize[1]);
            Assert.AreEqual(5u, resultSize[2]);

            //验证：间距正确
            VectorDouble resultSpacing = result.GetSpacing();
            Assert.AreEqual(newSpacing[0], resultSpacing[0], 0.001);
            Assert.AreEqual(newSpacing[1], resultSpacing[1], 0.001);
            Assert.AreEqual(newSpacing[2], resultSpacing[2], 0.001);
        }
        #endregion

        #region # 测试几何信息传递 —— void TestGeometryPreservation()
        /// <summary>
        /// 测试重采样后几何信息传递
        /// </summary>
        [TestMethod]
        public void TestGeometryPreservation()
        {
            //准备：非零Origin
            Vector3i volumeSize = new Vector3i(10, 10, 10);
            Vector3d spacing = new Vector3d(0.8, 0.8, 2.0);
            Vector3d origin = new Vector3d(10.0, -20.0, 30.0);
            Vector3d rowDirection = new Vector3d(1, 0, 0);
            Vector3d colDirection = new Vector3d(0, 1, 0);
            Vector3d sliceDirection = new Vector3d(0, 0, 1);
            SitkDicomLoader loader = new SitkDicomLoader();
            using Image originalImage = CreateTestImage(volumeSize, spacing, origin, rowDirection, colDirection, sliceDirection);
            using VolumeData volumeData = loader.LoadSitkImage(originalImage);
            Resampler resampler = new Resampler(volumeData);

            //执行
            using Image result = resampler.ExecuteIsotropic(1.0f);

            //验证Origin
            VectorDouble resultOrigin = result.GetOrigin();
            Assert.AreEqual(origin.X, resultOrigin[0], 0.001);
            Assert.AreEqual(origin.Y, resultOrigin[1], 0.001);
            Assert.AreEqual(origin.Z, resultOrigin[2], 0.001);

            //验证Direction
            VectorDouble resultDirection = result.GetDirection();
            Assert.AreEqual(rowDirection.X, resultDirection[0], 0.001);
            Assert.AreEqual(rowDirection.Y, resultDirection[1], 0.001);
            Assert.AreEqual(rowDirection.Z, resultDirection[2], 0.001);
            Assert.AreEqual(colDirection.X, resultDirection[3], 0.001);
            Assert.AreEqual(colDirection.Y, resultDirection[4], 0.001);
            Assert.AreEqual(colDirection.Z, resultDirection[5], 0.001);
            Assert.AreEqual(sliceDirection.X, resultDirection[6], 0.001);
            Assert.AreEqual(sliceDirection.Y, resultDirection[7], 0.001);
            Assert.AreEqual(sliceDirection.Z, resultDirection[8], 0.001);
        }
        #endregion

        #region # 测试非法输入 —— void TestInvalidInputs()
        /// <summary>
        /// 测试非法输入
        /// </summary>
        [TestMethod]
        public void TestInvalidInputs()
        {
            Vector3i volumeSize = new Vector3i(10, 10, 10);
            Vector3d spacing = new Vector3d(1.0, 1.0, 1.0);
            SitkDicomLoader loader = new SitkDicomLoader();
            using Image originalImage = CreateTestImage(volumeSize, spacing);
            using VolumeData volumeData = loader.LoadSitkImage(originalImage);
            Resampler resampler = new Resampler(volumeData);

            //等体素间距必须大于0
            Assert.Throws<ArgumentOutOfRangeException>(() => resampler.ExecuteIsotropic(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => resampler.ExecuteIsotropic(-1));

            //指定尺寸各分量必须大于0
            Assert.Throws<ArgumentOutOfRangeException>(() => resampler.ExecuteToSize(Vector3i.Zero));

            //指定间距各分量必须大于0
            Assert.Throws<ArgumentOutOfRangeException>(() => resampler.ExecuteToSpacing(Vector3.Zero));

            //行/列方向不能为零向量
            Vector3 center = new Vector3(5, 5, 5);
            Vector2i size = new Vector2i(10, 10);
            Assert.Throws<ArgumentOutOfRangeException>(() => resampler.ExtractSlice(center, size, Vector3.Zero, Vector3.UnitY));
            Assert.Throws<ArgumentOutOfRangeException>(() => resampler.ExtractSlice(center, size, Vector3.UnitX, Vector3.Zero));

            //层间距必须大于0
            Assert.Throws<ArgumentOutOfRangeException>(() => resampler.ExtractSliceSeries(center, Vector3.UnitX, Vector3.UnitY, 0, 5));

            //层数必须大于0
            Assert.Throws<ArgumentOutOfRangeException>(() => resampler.ExtractSliceSeries(center, Vector3.UnitX, Vector3.UnitY, 1.0, 0));
        }
        #endregion

        #region # 测试斜切序列数据连续性 —— void TestObliqueSeriesDataContinuity()
        /// <summary>
        /// 测试斜切序列数据连续性
        /// </summary>
        /// <remarks>
        /// 创建Z方向有梯度的测试数据，沿Z方向提取斜切序列，
        /// 验证序列各层数据因梯度变化而不完全相同。
        /// </remarks>
        [TestMethod]
        public unsafe void TestObliqueSeriesDataContinuity()
        {
            //准备：16×16×8测试数据，Z方向梯度0~7
            Vector3i volumeSize = new Vector3i(16, 16, 8);
            Vector3d spacing = new Vector3d(1.0, 1.0, 1.0);
            SitkDicomLoader loader = new SitkDicomLoader();
            using Image originalImage = CreateTestImageWithZGradient(volumeSize, spacing);
            using VolumeData volumeData = loader.LoadSitkImage(originalImage);
            Resampler resampler = new Resampler(volumeData);

            //执行：沿Z方向提取4层斜切序列，起始于第2层，间距2mm
            using Image series = resampler.ExtractSliceSeries(new Vector3(8, 8, 2), Vector3.UnitX, Vector3.UnitY, 2.0, 4);

            //验证1：序列尺寸
            VectorUInt32 seriesSize = series.GetSize();
            Assert.AreEqual(16u, seriesSize[0], "X方向尺寸应为16");
            Assert.AreEqual(16u, seriesSize[1], "Y方向尺寸应为16");
            Assert.AreEqual(4u, seriesSize[2], "Z方向层数应为4");

            //验证2：逐层数据不完全相同（因梯度变化）
            int length = volumeSize.X * volumeSize.Y * volumeSize.Z;
            short[] seriesData = new Span<short>(series.GetBufferAsInt16().ToPointer(), length).ToArray();
            int voxelsPerSlice = 16 * 16;
            double[] layerMeans = new double[4];
            for (int z = 0; z < 4; z++)
            {
                ReadOnlySpan<short> sliceSpan = new ReadOnlySpan<short>(seriesData, z * voxelsPerSlice, voxelsPerSlice);
                layerMeans[z] = sliceSpan.ToArray().Select(hu => (double)hu).Average();
            }

            //验证各层均值不同（至少有两层差异大于0.5）
            double maxDiff = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = i + 1; j < 4; j++)
                {
                    maxDiff = Math.Max(maxDiff, Math.Abs(layerMeans[i] - layerMeans[j]));
                }
            }
            Assert.IsTrue(maxDiff > 0.5, $"各层均值应有明显差异，实际最大差异 {maxDiff:F2}");

            //验证3：序列间距正确
            VectorDouble seriesSpacing = series.GetSpacing();
            Assert.AreEqual(2.0, seriesSpacing[2], 0.001, "层间距应为2mm");
        }
        #endregion


        //Private

        #region # 创建测试图像 —— static Image CreateTestImage(Vector3i volumeSize, Vector3d spacing)
        /// <summary>
        /// 创建测试图像
        /// </summary>
        /// <remarks>原点为0，方向为单位矩阵</remarks>
        private static Image CreateTestImage(Vector3i volumeSize, Vector3d spacing)
        {
            using VectorUInt32 imageSize = new VectorUInt32
            {
                (uint)volumeSize.X, (uint)volumeSize.Y, (uint)volumeSize.Z
            };
            using VectorDouble imageSpacing = new VectorDouble
            {
                spacing.X, spacing.Y, spacing.Z
            };
            using VectorDouble imageOrigin = new VectorDouble
            {
                0, 0, 0
            };
            using VectorDouble imageDirection = new VectorDouble
            {
                1, 0, 0,
                0, 1, 0,
                0, 0, 1
            };

            Image image = new Image(imageSize, PixelIDValueEnum.sitkInt16);
            image.SetSpacing(imageSpacing);
            image.SetOrigin(imageOrigin);
            image.SetDirection(imageDirection);

            return image;
        }
        #endregion

        #region # 创建测试图像 —— static Image CreateTestImage(Vector3i volumeSize, Vector3d spacing...
        /// <summary>
        /// 创建测试图像
        /// </summary>
        /// <remarks>带几何信息</remarks>
        private static Image CreateTestImage(Vector3i volumeSize, Vector3d spacing, Vector3d origin, Vector3d rowDirection, Vector3d colDirection, Vector3d sliceDirection)
        {
            using VectorUInt32 imageSize = new VectorUInt32
            {
                (uint)volumeSize.X, (uint)volumeSize.Y, (uint)volumeSize.Z
            };
            using VectorDouble imageSpacing = new VectorDouble
            {
                spacing.X, spacing.Y, spacing.Z
            };
            using VectorDouble imageOrigin = new VectorDouble
            {
                origin.X, origin.Y, origin.Z
            };
            using VectorDouble imageDirection = new VectorDouble
            {
                rowDirection.X, rowDirection.Y, rowDirection.Z,
                colDirection.X, colDirection.Y, colDirection.Z,
                sliceDirection.X, sliceDirection.Y, sliceDirection.Z
            };

            Image image = new Image(imageSize, PixelIDValueEnum.sitkInt16);
            image.SetSpacing(imageSpacing);
            image.SetOrigin(imageOrigin);
            image.SetDirection(imageDirection);

            return image;
        }
        #endregion

        #region # 创建中心带标记的测试图像 —— static Image CreateTestImageWithCenterMark(...
        /// <summary>
        /// 创建中心带标记的测试图像
        /// </summary>
        /// <remarks>
        /// 背景值为0，中心指定区域填充为指定标记值。
        /// 用于验证斜切时线性插值在边界处产生的过渡值。
        /// </remarks>
        private static Image CreateTestImageWithCenterMark(Vector3i volumeSize, Vector3d spacing, int centerSize, short markValue)
        {
            Image image = CreateTestImage(volumeSize, spacing);
            int totalVoxels = volumeSize.X * volumeSize.Y * volumeSize.Z;
            short[] data = new short[totalVoxels]; //默认全0

            //计算中心区域范围
            int startX = (volumeSize.X - centerSize) / 2;
            int startY = (volumeSize.Y - centerSize) / 2;
            int endX = startX + centerSize;
            int endY = startY + centerSize;

            //填充中心区域
            for (int z = 0; z < volumeSize.Z; z++)
            {
                for (int y = startY; y < endY; y++)
                {
                    for (int x = startX; x < endX; x++)
                    {
                        int index = z * volumeSize.Y * volumeSize.X + y * volumeSize.X + x;
                        data[index] = markValue;
                    }
                }
            }

            unsafe
            {
                IntPtr buffer = image.GetBufferAsInt16();
                fixed (short* pointer = data)
                {
                    Buffer.MemoryCopy(pointer, buffer.ToPointer(), totalVoxels * sizeof(short), totalVoxels * sizeof(short));
                }
            }
            return image;
        }
        #endregion

        #region # 创建Z方向梯度的测试图像 —— static Image CreateTestImageWithZGradient(...
        /// <summary>
        /// 创建Z方向梯度的测试图像
        /// </summary>
        /// <remarks>
        /// 每层填充该层的Z索引值，用于验证斜切序列各层数据的连续变化
        /// </remarks>
        private static Image CreateTestImageWithZGradient(Vector3i volumeSize, Vector3d spacing)
        {
            Image image = CreateTestImage(volumeSize, spacing);
            int totalVoxels = volumeSize.X * volumeSize.Y * volumeSize.Z;
            short[] data = new short[totalVoxels];
            for (int z = 0; z < volumeSize.Z; z++)
            {
                short layerValue = (short)z; //Z方向梯度 0, 1, 2, ...
                for (int y = 0; y < volumeSize.Y; y++)
                {
                    for (int x = 0; x < volumeSize.X; x++)
                    {
                        int index = z * volumeSize.Y * volumeSize.X + y * volumeSize.X + x;
                        data[index] = layerValue;
                    }
                }
            }

            unsafe
            {
                IntPtr buffer = image.GetBufferAsInt16();
                fixed (short* pointer = data)
                {
                    Buffer.MemoryCopy(pointer, buffer.ToPointer(), totalVoxels * sizeof(short), totalVoxels * sizeof(short));
                }
            }
            return image;
        }
        #endregion
    }
}
