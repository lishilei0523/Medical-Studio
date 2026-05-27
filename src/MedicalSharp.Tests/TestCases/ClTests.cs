using MedicalSharp.Inspiration.Algorithms;
using MedicalSharp.Inspiration.Resources;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTK.Mathematics;
using Silk.NET.OpenCL;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MedicalSharp.Tests.TestCases
{
    /// <summary>
    /// OpenCL测试
    /// </summary>
    [TestClass]
    public class ClTests
    {
        #region # 测试算术 —— void TestArithmetic()
        /// <summary>
        /// 测试算术
        /// </summary>
        [TestMethod]
        public void TestArithmetic()
        {
            //内核文件
            const string sourcePath = "Resources/Kernels/test_arithmetic.cl";

            //准备数据：1000个四维向量
            const int count = 1000;
            Vector4[] vectorA = new Vector4[count];
            Vector4[] vectorB = new Vector4[count];
            Vector4[] cpuResult = new Vector4[count];
            for (int index = 0; index < count; index++)
            {
                vectorA[index] = new Vector4(index, index + 2, index + 3, index + 4);
                vectorB[index] = new Vector4(index, index * 2, index * 3, index * 4);
                cpuResult[index] = vectorA[index] + vectorB[index];
            }

            //创建上下文
            using ClContext context = ClContext.Create();
            Trace.WriteLine($"设备: {context.DeviceName}");
            Trace.WriteLine($"显存: {context.GlobalMemorySize} MB");

            //创建GPU缓冲区
            using ClBuffer bufferA = ClBuffer.Create(context, MemFlags.ReadOnly, vectorA);
            using ClBuffer bufferB = ClBuffer.Create(context, MemFlags.ReadOnly, vectorB);
            using ClBuffer bufferResult = ClBuffer.CreateEmpty<Vector4>(context, MemFlags.WriteOnly, count);

            //编译内核、执行
            using ClProgram program = ClProgram.FromFile(context, sourcePath);
            using ClKernel kernel = program.CreateKernel("vector_add");
            kernel.SetBufferKernelArg(0, bufferA);
            kernel.SetBufferKernelArg(1, bufferB);
            kernel.SetBufferKernelArg(2, bufferResult);
            kernel.SetKernelArg(3, count);
            kernel.Enqueue1D(context.CommandQueue, count);
            context.Finish();

            //读回结果验证
            Vector4[] gpuResult = bufferResult.Read<Vector4>(context.CommandQueue, count);
            Assert.AreEqual(count, gpuResult.Length, "结果数组长度不匹配！");
            for (int index = 0; index < count; index++)
            {
                Assert.AreEqual(cpuResult[index], gpuResult[index], $"索引 {index} 处不匹配");
            }

            Trace.WriteLine("1000个四维向量加法全部正确！");
            Trace.WriteLine("前 5 个结果:");
            for (int index = 0; index < 5; index++)
            {
                Trace.WriteLine($"[{index}] {vectorA[index]} + {vectorB[index]} = {gpuResult[index]}");
            }
        }
        #endregion

        #region # 测试读写1D图像 —— void TestReadWriteImage1D()
        /// <summary>
        /// 测试读写1D图像
        /// </summary>
        [TestMethod]
        public void TestReadWriteImage1D()
        {
            const string sourcePath = "Resources/Kernels/test_rw_img_1D.cl";
            const int size = 256;
            const ChannelOrder channelOrder = ChannelOrder.Intensity;
            const ChannelType channelType = ChannelType.SNormInt16;

            //创建上下文
            using ClContext context = ClContext.Create();
            Trace.WriteLine($"设备: {context.DeviceName}");
            Trace.WriteLine($"OpenCL 2.0: {context.SupportsV20}");

            //创建1D图像
            using ClImage1D image = ClImage1D.Create(context, size, MemFlags.ReadWrite, channelOrder, channelType);

            //写入图像：从左到右，HU值递增-1000到+1000
            short[] inputData = new short[size];
            for (int index = 0; index < size; index++)
            {
                //线性映射：i=0 → -1000, i=size-1 → +1000
                inputData[index] = (short)(-1000 + (2000.0 * index / (size - 1)));
            }
            image.Write(context.CommandQueue, inputData);
            context.Finish();

            //验证写入：读回检查
            short[] fillData = image.Read<short>(context.CommandQueue);
            Assert.AreEqual(size, fillData.Length, "读回数组长度不匹配");
            for (int index = 0; index < size; index++)
            {
                Assert.AreEqual(inputData[index], fillData[index], 1, $"索引 {index} 不匹配");
            }
            Trace.WriteLine("写入后读回数据一致");

            //编译内核：减半所有值
            using ClProgram program = ClProgram.FromFile(context, sourcePath);
            using ClKernel kernel = program.CreateKernel("halve_1d");
            kernel.SetImageKernelArg(0, image.Handle);
            kernel.Enqueue1D(context.CommandQueue, (uint)size);
            context.Finish();

            //读回结果
            short[] resultData = image.Read<short>(context.CommandQueue);

            //验证：所有值减半
            bool passed = true;
            for (int i = 0; i < size; i++)
            {
                short expected = (short)(inputData[i] / 2);
                if (Math.Abs(resultData[i] - expected) > 2) // SNORM 量化容忍 2
                {
                    Trace.WriteLine($"索引 {i}: 期望 ~{expected}, 实际 {resultData[i]}");
                    passed = false;
                    break;
                }
            }
            Assert.IsTrue(passed, "减半结果验证失败");
            Trace.WriteLine("1D图像写入+内核减半全部正确");

            //前5个值
            Trace.WriteLine("前5个结果:");
            for (int index = 0; index < 5; index++)
            {
                Trace.WriteLine($"  [{index}] {inputData[index]} → {resultData[index]}");
            }

            //中间3个值
            Trace.WriteLine("中间3个结果:");
            for (int index = size / 2 - 1; index <= size / 2 + 1; index++)
            {
                Trace.WriteLine($"  [{index}] {inputData[index]} → {resultData[index]}");
            }
        }
        #endregion

        #region # 测试读写2D图像 —— void TestReadWriteImage2D()
        /// <summary>
        /// 测试读写2D图像
        /// </summary>
        [TestMethod]
        public void TestReadWriteImage2D()
        {
            const string sourcePath = "Resources/Kernels/test_rw_img_2D.cl";
            const int size = 64;
            const ChannelOrder channelOrder = ChannelOrder.Rgba;
            const ChannelType channelType = ChannelType.UnormInt8;

            //创建上下文
            using ClContext context = ClContext.Create();
            Trace.WriteLine($"设备: {context.DeviceName}");
            Trace.WriteLine($"OpenCL 2.0: {context.SupportsV20}");

            //创建2D图像
            using ClImage2D image = ClImage2D.Create(context, size, size, MemFlags.ReadWrite, channelOrder, channelType);

            //填充(255, 255, 255, 255) -> (1.0, 1.0, 1.0, 1.0)
            Vector4 fillColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            image.Fill(context.CommandQueue, fillColor);
            context.Finish();

            //验证填充：读回检查
            Vector4b[] fillData = image.Read<Vector4b>(context.CommandQueue);
            Assert.AreEqual(255, fillData[0].X);  //R: 255
            Assert.AreEqual(255, fillData[0].Y);  //G: 255
            Assert.AreEqual(255, fillData[0].Z);  //B: 255
            Assert.AreEqual(255, fillData[0].W);  //A: 255

            //编译内核
            using ClProgram program = ClProgram.FromFile(context, sourcePath);
            using ClKernel kernel = program.CreateKernel("test_rgba8");
            kernel.SetImageKernelArg(0, image.Handle);
            kernel.Enqueue2D(context.CommandQueue, size, size);
            context.Finish();

            //读回结果
            Vector4b[] resultData = image.Read<Vector4b>(context.CommandQueue);

            //验证：所有通道减半
            Assert.AreEqual(128, resultData[0].X);     //R: 128
            Assert.AreEqual(128, resultData[0].Y);     //G: 128
            Assert.AreEqual(128, resultData[0].Z);     //B: 128
            Assert.AreEqual(128, resultData[0].W);     //A: 128

            Trace.WriteLine("2D RGBA8 读写工作正常");
        }
        #endregion

        #region # 测试读写3D图像 —— void TestReadWriteImage3D1()
        /// <summary>
        /// 测试读写3D图像
        /// </summary>
        [TestMethod]
        public void TestReadWriteImage3D1()
        {
            const string sourcePath = "Resources/Kernels/test_rw_img_3D_1.cl";
            const int size = 16;
            const ChannelOrder channelOrder = ChannelOrder.Intensity;
            const ChannelType channelType = ChannelType.SNormInt16;

            //创建上下文
            using ClContext context = ClContext.Create();
            Trace.WriteLine($"设备: {context.DeviceName}");
            Trace.WriteLine($"显存: {context.GlobalMemorySize} MB");

            //创建3D图像
            using ClImage3D image = ClImage3D.Create(context, size, size, size, MemFlags.ReadOnly, channelOrder, channelType);

            //填充图像：全部设为1.0（SNORM最大值32767）
            image.Fill(context.CommandQueue, 1.0f);

            //验证：所有值应该是原值：32767
            short[] fillData = image.Read<short>(context.CommandQueue);
            foreach (short value in fillData)
            {
                Assert.AreEqual(value, 32767);
            }

            //编译测试内核
            using ClProgram program = ClProgram.FromFile(context, sourcePath);
            using ClKernel kernel = program.CreateKernel("test_read_write");
            kernel.SetImageKernelArg(0, image.Handle);
            kernel.Enqueue3D(context.CommandQueue, size, size, size, 4, 4, 4);
            context.Finish();

            //验证：所有值应该是原值的一半（16383左右）
            short[] resultData = image.Read<short>(context.CommandQueue);
            bool passed = true;
            for (int index = 0; index < resultData.Length; index++)
            {
                if (Math.Abs(resultData[index] - 16383) > 10) //允许少量误差
                {
                    Trace.WriteLine($"索引 {index}: 期望 ~16383, 实际 {resultData[index]}");
                    passed = false;
                    break;
                }
            }

            Assert.IsTrue(passed);
        }
        #endregion

        #region # 测试读写3D图像 —— void TestReadWriteImage3D2()
        /// <summary>
        /// 测试读写3D图像
        /// </summary>
        [TestMethod]
        public void TestReadWriteImage3D2()
        {
            const string sourcePath = "Resources/Kernels/test_rw_img_3D_2.cl";
            const int size = 16;
            const ChannelOrder channelOrder = ChannelOrder.Intensity;
            const ChannelType channelType = ChannelType.SNormInt16;

            //创建上下文
            using ClContext context = ClContext.Create();
            Trace.WriteLine($"设备: {context.DeviceName}");
            Trace.WriteLine($"显存: {context.GlobalMemorySize} MB");

            //创建3D图像
            using ClImage3D inputImage = ClImage3D.Create(context, size, size, size, MemFlags.ReadOnly, channelOrder, channelType);
            using ClImage3D outputImage = ClImage3D.Create(context, size, size, size, MemFlags.WriteOnly, channelOrder, channelType);

            //填充图像
            inputImage.Fill(context.CommandQueue, 1.0f);

            //验证写入是否成功，所有值应该是原值：32767
            short[] fillData = inputImage.Read<short>(context.CommandQueue);
            foreach (short value in fillData)
            {
                Assert.AreEqual(value, 32767);
            }

            //编译测试内核
            using ClProgram program = ClProgram.FromFile(context, sourcePath);
            using ClKernel kernel = program.CreateKernel("test_read_write");
            kernel.SetImageKernelArg(0, inputImage.Handle);
            kernel.SetImageKernelArg(1, outputImage.Handle);
            kernel.Enqueue3D(context.CommandQueue, size, size, size, 4, 4, 4);
            context.Finish();

            //验证：所有值应该是原值的一半（16383左右）
            short[] resultData = outputImage.Read<short>(context.CommandQueue);
            bool passed = true;
            for (int index = 0; index < resultData.Length; index++)
            {
                if (Math.Abs(resultData[index] - 16383) > 10) //允许少量误差
                {
                    Trace.WriteLine($"索引 {index}: 期望 ~16383, 实际 {resultData[index]}");
                    passed = false;
                    break;
                }
            }

            Assert.IsTrue(passed);
        }
        #endregion

        #region # 测试复制图像 —— void TestCopyImage()
        /// <summary>
        /// 测试复制图像
        /// </summary>
        [TestMethod]
        public void TestCopyImage()
        {
            const int size = 64;
            const ChannelOrder channelOrder = ChannelOrder.Rgba;
            const ChannelType channelType = ChannelType.SNormInt16;

            //创建上下文
            using ClContext context = ClContext.Create();
            Trace.WriteLine($"设备: {context.DeviceName}");

            //创建图像
            using ClImage3D sourceImage = ClImage3D.Create(context, size, size, size, MemFlags.ReadWrite, channelOrder, channelType);
            using ClImage3D targetImage = ClImage3D.Create(context, size, size, size, MemFlags.ReadWrite, channelOrder, channelType);

            //填充源图像
            Vector4 colorF = new Vector4(1.0f);
            Vector4s colorS = new Vector4s(short.MaxValue);
            sourceImage.Fill(context.CommandQueue, colorF);
            context.Finish();

            //验证源图像
            Vector4s[] sourceData = sourceImage.Read<Vector4s>(context.CommandQueue);
            Assert.AreEqual(size * size * size, sourceData.Length);
            Assert.AreEqual(colorS, sourceData[0], "源图像填充值不正确");

            //复制图像
            sourceImage.CopyTo(context.CommandQueue, targetImage);
            context.Finish();

            //验证目标图像
            Vector4s[] targetData = targetImage.Read<Vector4s>(context.CommandQueue);
            Assert.AreEqual(size * size * size, targetData.Length);

            bool passed = true;
            for (int index = 0; index < targetData.Length; index++)
            {
                if (Math.Abs(targetData[index].X - sourceData[index].X) > 1)
                {
                    Trace.WriteLine($"索引 {index}: 源 {sourceData[index]}, 目标 {targetData[index]}");
                    passed = false;
                    break;
                }
            }
            Assert.IsTrue(passed, "复制验证失败");

            Trace.WriteLine("复制正确");
            Trace.WriteLine($"源图像前5个R值: {string.Join(", ", sourceData.Take(5).Select(vec => vec.X))}");
            Trace.WriteLine($"目标图像前5个R值: {string.Join(", ", targetData.Take(5).Select(vec => vec.X))}");
        }
        #endregion

        #region # 测试结构体内存缓冲区 —— void TestStructBuffer()
        /// <summary>
        /// 测试结构体内存缓冲区
        /// </summary>
        [TestMethod]
        public void TestStructBuffer()
        {
            const string sourcePath = "Resources/Kernels/test_struct_buffer.cl";
            const int count = 1024;

            //创建上下文
            using ClContext context = ClContext.Create();
            Trace.WriteLine($"设备: {context.DeviceName}");

            //准备输入数据
            Voxel[] input = new Voxel[count];
            for (int index = 0; index < count; index++)
            {
                input[index] = new Voxel
                {
                    HU = index * 1.5f,
                    Label = (short)(index % 256),
                    Visited = (byte)(index % 2),
                    Padding = 0
                };
            }

            //创建GPU缓冲区
            using ClBuffer inputBuffer = ClBuffer.Create(context, MemFlags.ReadOnly, input);
            using ClBuffer outputBuffer = ClBuffer.CreateEmpty<Voxel>(context, MemFlags.WriteOnly, count);

            //编译内核
            using ClProgram program = ClProgram.FromFile(context, sourcePath);
            using ClKernel kernel = program.CreateKernel("modify_voxels");
            kernel.SetBufferKernelArg(0, inputBuffer);
            kernel.SetBufferKernelArg(1, outputBuffer);
            kernel.SetKernelArg(2, count);

            //执行
            kernel.Enqueue1D(context.CommandQueue, count);
            context.Finish();

            //读回结果
            Voxel[] output = outputBuffer.Read<Voxel>(context.CommandQueue, count);

            //验证
            Assert.AreEqual(count, output.Length, "输出数组长度不匹配");
            for (int index = 0; index < count; index++)
            {
                Assert.AreEqual(input[index].HU * 2.0f, output[index].HU, 0.001f, $"索引 {index} HU 不匹配");
                Assert.AreEqual(input[index].Label + 100, output[index].Label, $"索引 {index} Label 不匹配");
                Assert.AreEqual(input[index].Visited, output[index].Visited, $"索引 {index} Visited 不匹配");
            }

            Trace.WriteLine("结构体缓冲区测试全部正确");

            Trace.WriteLine("前5个结果:");
            for (int index = 0; index < 5; index++)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"[{index}] HU: {input[index].HU} → {output[index].HU},");
                builder.AppendLine($"[{index}] Label: {input[index].Label} → {output[index].Label}, ");
                builder.AppendLine($"[{index}] Visited: {input[index].Visited} → {output[index].Visited}");
                Trace.WriteLine(builder);
            }
        }
        #endregion

        #region # 测试内存缓冲区与图像交换 —— void TestBufferImageExchange()
        /// <summary>
        /// 测试内存缓冲区与图像交换
        /// </summary>
        [TestMethod]
        public void TestBufferImageExchange()
        {
            const int size = 64;
            const ChannelOrder channelOrder = ChannelOrder.Intensity;
            const ChannelType channelType = ChannelType.SNormInt16;

            //创建上下文
            using ClContext context = ClContext.Create();
            Trace.WriteLine($"设备: {context.DeviceName}");

            //准备数据：0~4095的斜坡值
            int count = size * size * size;
            short[] cpuArray = new short[count];
            for (int index = 0; index < count; index++)
            {
                cpuArray[index] = (short)(index % 4096); //0~4095
            }

            //创建内存缓冲区
            using ClBuffer clBuffer = ClBuffer.Create(context, MemFlags.ReadWrite, cpuArray);

            //验证读取内存缓冲区
            short[] fillBufferArray = clBuffer.Read<short>(context.CommandQueue, count);
            for (int index = 0; index < count; index++)
            {
                Assert.AreEqual(cpuArray[index], fillBufferArray[index]);
            }

            //创建图像，从内存缓冲区复制
            using ClImage3D image = ClImage3D.Create(context, size, size, size, MemFlags.ReadWrite, channelOrder, channelType);
            image.CopyFromBuffer(context.CommandQueue, clBuffer);
            context.Finish();

            //验证读取图像
            short[] imageArray = image.Read<short>(context.CommandQueue);
            for (int index = 0; index < count; index++)
            {
                Assert.AreEqual(cpuArray[index], imageArray[index]);
            }

            //创建内存缓冲区，复制图像到内存缓冲区
            using ClBuffer resultBuffer = ClBuffer.CreateEmpty<short>(context, MemFlags.ReadWrite, count);
            image.CopyToBuffer(context.CommandQueue, resultBuffer);
            context.Finish();

            //验证读取内存缓冲区
            short[] resultBufferArray = resultBuffer.Read<short>(context.CommandQueue, count);
            for (int index = 0; index < count; index++)
            {
                Assert.AreEqual(cpuArray[index], resultBufferArray[index]);
            }
        }
        #endregion

        #region # 测试统计 —— void TestStatistic()
        /// <summary>
        /// 测试统计
        /// </summary>
        [TestMethod]
        public void TestStatistic()
        {
            const string sourcePath = "Resources/Kernels/test_statistic.cl";

            //创建上下文
            using ClContext context = ClContext.Create();
            Trace.WriteLine($"设备: {context.DeviceName}");

            //准备数据
            const int count = 1000;
            float[] array = new float[count];
            Random random = new Random(42);
            for (int index = 0; index < count; index++)
            {
                array[index] = (float)(random.NextDouble() * 1000.0 - 500.0);
            }
            float cpuMin = array.Min();
            float cpuMax = array.Max();
            float cpuSum = array.Sum();

            //创建缓冲区
            using ClBuffer inputBuffer = ClBuffer.Create(context, MemFlags.ReadOnly, array);
            using ClBuffer resultBuffer = ClBuffer.CreateEmpty<Statistic>(context, MemFlags.ReadWrite, 1);

            //初始化统计结果：min=最大浮点，max=最小浮点，sum=0
            Statistic result = new Statistic
            {
                Min = float.MaxValue,
                Max = float.MinValue,
                Sum = 0
            };
            resultBuffer.Write(context.CommandQueue, [result]);

            //验证初始值
            Statistic[] initValue = resultBuffer.Read<Statistic>(context.CommandQueue, 1);
            Assert.AreEqual(initValue[0].Min, float.MaxValue, float.Epsilon);
            Assert.AreEqual(initValue[0].Max, float.MinValue, float.Epsilon);
            Assert.AreEqual(initValue[0].Sum, 0, float.Epsilon);

            //编译执行
            using ClProgram program = ClProgram.FromFile(context, sourcePath);
            using ClKernel kernel = program.CreateKernel("analyse");
            kernel.SetBufferKernelArg(0, inputBuffer);
            kernel.SetBufferKernelArg(1, resultBuffer);
            kernel.SetKernelArg(2, count);
            kernel.Enqueue1D(context.CommandQueue, (uint)count, 256);
            context.Finish();

            //验证计算结果
            result = resultBuffer.Read<Statistic>(context.CommandQueue, 1)[0];
            Assert.AreEqual(cpuMin, result.Min, 0.01f, "最小值不匹配");
            Assert.AreEqual(cpuMax, result.Max, 0.01f, "最大值不匹配");
            Assert.AreEqual(cpuSum, result.Sum, 0.1f, "求和不匹配");

            Trace.WriteLine("统计全部正确");
            Trace.WriteLine($"Min: {result.Min}, Max: {result.Max}, Sum: {result.Sum}");
        }
        #endregion

        #region # 测试3D高斯滤波算法 —— void TestGaussianBlur3D()
        /// <summary>
        /// 测试3D高斯滤波算法
        /// </summary>
        [TestMethod]
        public void TestGaussianBlur3D()
        {
            const int size = 32;

            //实例化算法
            using ClContext context = ClContext.Create();
            using GaussianBlur3D blur = new GaussianBlur3D(context);

            //定义输入、输出图像
            using ClImage3D inputImage = ClImage3D.Create(context, size, size, size, MemFlags.ReadWrite, ChannelOrder.Intensity, ChannelType.SNormInt16);
            using ClImage3D outputImage = ClImage3D.Create(context, size, size, size, MemFlags.WriteOnly, ChannelOrder.Intensity, ChannelType.SNormInt16);

            //填充数据，中心一个亮点
            inputImage.Fill(context.CommandQueue, 0.0f);
            const int center = size / 2;
            short[] spot = new short[size * size * size];
            spot[center * size * size + center * size + center] = 32767;
            inputImage.Write(context.CommandQueue, spot);
            context.Finish();

            //执行算法
            blur.Execute(inputImage, outputImage);
            context.Finish();

            //读取结果
            short[] result = outputImage.Read<short>(context.CommandQueue);

            //验证：中心点值应该变小，被模糊
            short centerValue = result[center * size * size + center * size + center];
            Assert.IsTrue(centerValue < 32000);

            //验证：相邻点应该有扩散值
            short nearValue = result[center * size * size + center * size + center - 1];
            Assert.IsTrue(nearValue > 0);
        }
        #endregion

        #region # 测试3D灰度直方图统计算法 —— void TestHistogram3D()
        /// <summary>
        /// 测试3D灰度直方图统计算法
        /// </summary>
        [TestMethod]
        public void TestHistogram3D()
        {
            const int size = 32;
            const int bins = 256;
            const float minHU = -1024f;
            const float maxHU = 3071f;

            //创建上下文
            using ClContext context = ClContext.Create();
            Trace.WriteLine($"设备: {context.DeviceName}");

            //创建3D图像并写入已知数据
            using ClImage3D image = ClImage3D.Create(context, size, size, size, MemFlags.ReadWrite, ChannelOrder.Intensity, ChannelType.SNormInt16);

            //填充统一值0.5（SNORM约16383，HU约0）
            image.Fill(context.CommandQueue, 0.0f);  //HU=0 ≈ 背景
            context.Finish();

            //在中心位置放置一个亮点（HU=500）
            short[] spotData = new short[size * size * size];
            const int centerIndex = (size / 2) * size * size + (size / 2) * size + (size / 2);
            spotData[centerIndex] = 500;  //HU=500的体素
            image.Write(context.CommandQueue, spotData);
            context.Finish();

            //执行直方图统计
            using Histogram3D histogram = new Histogram3D(context);
            uint[] result = histogram.Execute(image, bins, minHU, maxHU);

            //验证
            int totalVoxels = size * size * size;
            uint totalCount = 0;
            uint maxBinValue = 0;
            int maxBinIndex = 0;
            for (int index = 0; index < bins; index++)
            {
                totalCount += result[index];
                if (result[index] > maxBinValue)
                {
                    maxBinValue = result[index];
                    maxBinIndex = index;
                }
            }

            //总计数应等于总体素
            Assert.AreEqual(totalVoxels, (int)totalCount, "直方图总计数不匹配总体素数");

            //最大值应该在HU=0附近（大部分体素填充了0）
            float huAtMaxBin = minHU + (maxBinIndex + 0.5f) * (maxHU - minHU) / bins;
            Assert.IsTrue(Math.Abs(huAtMaxBin - 0) < 100, $"峰值HU值偏离0: {huAtMaxBin}");

            //HU=500的体素应该被统计到对应的桶
            const int spotBin = (int)((500 - minHU) / (maxHU - minHU) * bins);
            Assert.AreEqual(1u, result[spotBin], $"HU=500 的体素未被正确统计，桶 {spotBin} 的值 = {result[spotBin]}");

            Trace.WriteLine("直方图统计测试通过");
            Trace.WriteLine($"总体素: {totalVoxels}");
            Trace.WriteLine($"峰值桶: {maxBinIndex} (HU ≈ {huAtMaxBin:F0})");
            Trace.WriteLine($"HU=500 在桶: {spotBin} (值 = {result[spotBin]})");

            //测试归一化直方图
            float[] normalized = histogram.ExecuteNormalized(image, bins, minHU, maxHU);
            float normSum = 0;
            for (int i = 0; i < bins; i++)
            {
                normSum += normalized[i];
            }
            Assert.AreEqual(1.0f, normSum, 0.001f, "归一化直方图总和不等于 1.0");

            //测试CDF
            float[] cdf = histogram.ExecuteCDF(image, bins, minHU, maxHU);
            Assert.AreEqual(normalized[0], cdf[0], 0.001f, "CDF第一个值应等于归一化第一个值");
            Assert.AreEqual(1.0f, cdf[bins - 1], 0.001f, "CDF最后一个值应接近1.0");

            Trace.WriteLine("归一化直方图和CDF测试通过");
        }
        #endregion
    }
}
