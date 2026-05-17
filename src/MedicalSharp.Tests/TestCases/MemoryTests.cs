using MedicalSharp.Primitives.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Buffers;
using System.Runtime.InteropServices;

namespace MedicalSharp.Tests.TestCases
{
    /// <summary>
    /// 内存测试
    /// </summary>
    [TestClass]
    public class MemoryTests
    {
        /// <summary>
        /// 测试本地内存管理器
        /// </summary>
        [TestMethod]
        public unsafe void TestNativeMemoryManager()
        {
            //定义长度
            const int count = 1024;

            //分配内存
            float* buffer = (float*)NativeMemory.Alloc((UIntPtr)count * sizeof(float));

            //填充数据
            for (int index = 0; index < count; index++)
            {
                buffer[index] = index;
            }

            //转换为Memory<T>
            NativeMemoryManager<float> memoryManager = new NativeMemoryManager<float>(buffer, count);
            Memory<float> memory = memoryManager.Memory;

            //验证长度
            Assert.AreEqual(memory.Length, count);

            //验证数据
            using MemoryHandle memoryHandle = memory.Pin();
            float* memoryPointer = (float*)memoryHandle.Pointer;
            for (int index = 0; index < count; index++)
            {
                float sourceValue = buffer[index];
                float targetValue = memoryPointer[index];

                Assert.AreEqual(sourceValue, targetValue, 1e-6f);
            }
        }
    }
}
