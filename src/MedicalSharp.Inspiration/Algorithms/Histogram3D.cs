using MedicalSharp.Inspiration.Resources;
using Silk.NET.OpenCL;
using System;

namespace MedicalSharp.Inspiration.Algorithms
{
    /// <summary>
    /// 3D灰度直方图统计算法
    /// </summary>
    /// <remarks>采用全局内存分块策略，每个工作组独占一块直方图区域，CPU做最终归约</remarks>
    public sealed class Histogram3D : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 释放标识
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// OpenCL上下文
        /// </summary>
        private readonly ClContext _clContext;

        /// <summary>
        /// OpenCL程序
        /// </summary>
        private readonly ClProgram _program;

        /// <summary>
        /// OpenCL内核
        /// </summary>
        private readonly ClKernel _kernel;

        /// <summary>
        /// 创建3D灰度直方图统计算法构造器
        /// </summary>
        /// <param name="clContext">OpenCL 上下文</param>
        public Histogram3D(ClContext clContext)
        {
            this._clContext = clContext;
            this._program = ClProgram.FromFile(this._clContext, "Resources/Kernels/histogram_3d.cl");
            this._kernel = this._program.CreateKernel("histogram_3d");
        }

        #endregion

        #region # 属性

        #region 只读属性 - OpenCL程序 —— ClProgram Program
        /// <summary>
        /// 只读属性 - OpenCL程序
        /// </summary>
        public ClProgram Program
        {
            get => this._program;
        }
        #endregion

        #region 只读属性 - OpenCL内核 —— ClKernel Kernel
        /// <summary>
        /// 只读属性 - OpenCL内核
        /// </summary>
        public ClKernel Kernel
        {
            get => this._kernel;
        }
        #endregion

        #endregion

        #region # 方法

        #region 统计灰度直方图 —— uint[] Execute(ClImage3D input, int bins...
        /// <summary>
        /// 统计灰度直方图
        /// </summary>
        /// <param name="input">输入图像</param>
        /// <param name="bins">桶数量（默认4096，覆盖HU值范围 [-1024, 3071]）</param>
        /// <param name="minHU">最小HU（默认-1024）</param>
        /// <param name="maxHU">最大HU（默认3071）</param>
        /// <returns>灰度直方图数组</returns>
        /// <remarks>索引：HU值桶，值：体素数量</remarks>
        public uint[] Execute(ClImage3D input, int bins = 4096, float minHU = -1024f, float maxHU = 3071f)
        {
            #region # 验证

            if (input == null)
            {
                throw new ArgumentNullException(nameof(input), "输入图像不可为空！");
            }
            if (bins <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bins), "桶数量必须大于0！");
            }
            if (minHU >= maxHU)
            {
                throw new ArgumentOutOfRangeException(nameof(minHU), "最小HU必须小于最大HU！");
            }

            #endregion

            int width = input.Width;
            int height = input.Height;
            int depth = input.Depth;

            //计算工作组数量（和工作调度一致）
            const uint localX = 4, localY = 4, localZ = 4;
            uint groupsX = (uint)((width + localX - 1) / localX);
            uint groupsY = (uint)((height + localY - 1) / localY);
            uint groupsZ = (uint)((depth + localZ - 1) / localZ);
            int groupsCount = (int)(groupsX * groupsY * groupsZ);

            //分配全局直方图缓冲区：每个工作组独占bins个uint
            int totalHistSize = groupsCount * bins;
            using ClBuffer globalHistBuffer = ClBuffer.CreateEmpty<uint>(this._clContext, MemFlags.ReadWrite, totalHistSize);

            //初始化全局直方图为0
            uint[] zeros = new uint[totalHistSize];
            globalHistBuffer.Write(this._clContext.CommandQueue, zeros.AsSpan());
            this._clContext.Finish();

            //设置内核参数
            this._kernel.SetImageKernelArg(0, input.Handle);
            this._kernel.SetBufferKernelArg(1, globalHistBuffer);
            this._kernel.SetKernelArg(2, bins);
            this._kernel.SetKernelArg(3, minHU);
            this._kernel.SetKernelArg(4, maxHU);

            //执行（使用4×4×4工作组）
            this._kernel.Enqueue3D(this._clContext.CommandQueue, groupsX * localX, groupsY * localY, groupsZ * localZ);
            this._clContext.Finish();

            //读回所有工作组的局部直方图
            uint[] allHistograms = globalHistBuffer.Read<uint>(this._clContext.CommandQueue, totalHistSize);

            //CPU做最终归约：累加所有工作组的直方图
            uint[] result = new uint[bins];
            for (int g = 0; g < groupsCount; g++)
            {
                int offset = g * bins;
                for (int i = 0; i < bins; i++)
                {
                    result[i] += allHistograms[offset + i];
                }
            }

            return result;
        }
        #endregion

        #region 统计归一化灰度直方图 —— float[] ExecuteNormalized(ClImage3D input, int bins...
        /// <summary>
        /// 统计归一化灰度直方图
        /// </summary>
        /// <param name="input">输入图像</param>
        /// <param name="bins">桶数量（默认4096，覆盖HU值范围 [-1024, 3071]）</param>
        /// <param name="minHU">最小HU（默认-1024）</param>
        /// <param name="maxHU">最大HU（默认3071）</param>
        /// <returns>归一化灰度直方图数组</returns>
        /// <remarks>索引：HU值桶，值：体素频率，频率总和 = 1.0</remarks>
        public float[] ExecuteNormalized(ClImage3D input, int bins = 4096, float minHU = -1024f, float maxHU = 3071f)
        {
            uint[] histogram = this.Execute(input, bins, minHU, maxHU);
            float[] normalized = new float[bins];
            float total = 0;
            for (int index = 0; index < bins; index++)
            {
                total += histogram[index];
            }

            if (total > 0)
            {
                for (int index = 0; index < bins; index++)
                {
                    normalized[index] = histogram[index] / total;
                }
            }

            return normalized;
        }
        #endregion

        #region 统计灰度累积分布函数 —— float[] ExecuteCDF(ClImage3D input, int bins...
        /// <summary>
        /// 统计灰度累积分布函数
        /// </summary>
        /// <param name="input">输入图像</param>
        /// <param name="bins">桶数量（默认4096，覆盖HU值范围 [-1024, 3071]）</param>
        /// <param name="minHU">最小HU（默认-1024）</param>
        /// <param name="maxHU">最大HU（默认3071）</param>
        /// <returns>累积分布函数数组，每个元素 = 小于等于该桶的体素频率</returns>
        public float[] ExecuteCDF(ClImage3D input, int bins = 4096, float minHU = -1024f, float maxHU = 3071f)
        {
            float[] normalized = this.ExecuteNormalized(input, bins, minHU, maxHU);
            float[] cdf = new float[bins];
            cdf[0] = normalized[0];
            for (int index = 1; index < bins; index++)
            {
                cdf[index] = cdf[index - 1] + normalized[index];
            }

            return cdf;
        }
        #endregion

        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this._program?.Dispose();
            this._kernel?.Dispose();
            this._disposed = true;
        }
        #endregion

        #endregion
    }
}
