using MedicalSharp.Inspiration.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTK.Mathematics;
using Silk.NET.OpenCL;
using System;
using System.Diagnostics;

namespace MedicalSharp.Tests.TestCases
{
    /// <summary>
    /// OpenCL测试
    /// </summary>
    [TestClass]
    public class ClTests
    {
        /// <summary>
        /// 测试向量加法
        /// </summary>
        [TestMethod]
        public void TestVectorAdd()
        {
            //内核文件
            const string sourcePath = "Kernels/vector_add.cl";

            //准备数据：1000个四位向量
            const int vectorsCount = 1000;
            Vector4[] vectorA = new Vector4[vectorsCount];
            Vector4[] vectorB = new Vector4[vectorsCount];
            Vector4[] cpuResult = new Vector4[vectorsCount];
            for (int index = 0; index < vectorsCount; index++)
            {
                vectorA[index] = new Vector4(index, index + 2, index + 3, index + 4);
                vectorB[index] = new Vector4(index, index * 2, index * 3, index * 4);
                cpuResult[index] = vectorA[index] + vectorB[index];
            }

            //创建上下文
            using ClContext context = ClContext.Create();
            Trace.WriteLine($"设备: {context.DeviceName}");
            Trace.WriteLine($"显存: {context.GlobalMemorySize} MB");

            //编译内核
            using ClProgram program = ClProgram.FromFile(context, sourcePath);
            using ClKernel kernel = program.CreateKernel("vector_add");

            //创建GPU缓冲区
            using ClBuffer bufferA = ClBuffer.Create(context, MemFlags.ReadOnly, vectorA.AsSpan());
            using ClBuffer bufferB = ClBuffer.Create(context, MemFlags.ReadOnly, vectorB.AsSpan());
            using ClBuffer bufferResult = ClBuffer.CreateEmpty<Vector4>(context, MemFlags.WriteOnly, vectorsCount);

            //设置参数
            kernel.SetBufferKernelArg(0, bufferA);
            kernel.SetBufferKernelArg(1, bufferB);
            kernel.SetBufferKernelArg(2, bufferResult);
            kernel.SetKernelArg(3, vectorsCount);

            //执行
            kernel.Enqueue1D(context.CommandQueue, vectorsCount);

            //等待完成
            context.Finish();

            //读回结果
            Vector4[] gpuResult = bufferResult.Read<Vector4>(context.CommandQueue, vectorsCount);

            //验证
            Assert.AreEqual(vectorsCount, gpuResult.Length, "结果数组长度不匹配！");

            for (int index = 0; index < vectorsCount; index++)
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
    }
}
