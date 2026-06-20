using itk.simple;
using MedicalSharp.Insight;
using MedicalSharp.Insight.Operators;
using MedicalSharp.Primitives.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Tests.TestCases
{
    /// <summary>
    /// 重采样算子测试
    /// </summary>
    [TestClass]
    public class ResamplerTests
    {
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
    }
}
