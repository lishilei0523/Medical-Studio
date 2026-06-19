using MedicalSharp.Inspiration.Resources;
using Silk.NET.OpenCL;
using System;

namespace MedicalSharp.Inspiration.Operators
{
    /// <summary>
    /// 3D自适应阈值分割算子
    /// </summary>
    /// <remarks>
    /// 每个体素的阈值由其局部邻域均值动态决定
    /// 流程：均值滤波 -> 逐体素比较（体素值 > 局部均值 - 偏移量 -> 前景）
    /// 适用于光照不均、局部对比度差异大的体积数据
    /// </remarks>
    public sealed class AdaptiveThreshold3D : IDisposable
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
        /// 创建3D自适应阈值分割算子构造器
        /// </summary>
        /// <param name="clContext">OpenCL上下文</param>
        public AdaptiveThreshold3D(ClContext clContext)
        {
            this._clContext = clContext;
            this._program = ClProgram.FromFile(this._clContext, "Resources/Kernels/adaptive_threshold_3d.cl");
            this._kernel = this._program.CreateKernel("adaptive_threshold_3d");
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

        #region 执行自适应阈值分割 —— void Execute(ClImage3D input, ClImage3D output...
        /// <summary>
        /// 执行自适应阈值分割
        /// </summary>
        /// <param name="input">输入图像</param>
        /// <param name="output">输出二值图像</param>
        /// <param name="kernelSize">均值滤波核矩阵尺寸（奇数，默认5）</param>
        /// <param name="offset">偏移量（默认0.02，归一化值，约HU650）</param>
        /// <remarks>
        /// 核矩阵尺寸越大，局部均值越平滑，分割结果越粗
        /// 偏移量越大，前景越少（需要更高的体素值才能判定为前景）
        /// </remarks>
        public void Execute(ClImage3D input, ClImage3D output, int kernelSize = 5, float offset = 0.02f)
        {
            int width = input.Width;
            int height = input.Height;
            int depth = input.Depth;

            //计算局部均值
            using MeanBlur3D meanBlur = new MeanBlur3D(this._clContext);
            using ClImage3D localMean = ClImage3D.Create(this._clContext, width, height, depth, MemFlags.ReadWrite, input.ChannelOrder, input.ChannelType);
            meanBlur.Execute(input, localMean, kernelSize);
            this._clContext.Finish();

            //逐体素比较：体素值 > 局部均值 - 偏移量 -> 前景
            this._kernel.SetImageKernelArg(0, input.Handle);
            this._kernel.SetImageKernelArg(1, localMean.Handle);
            this._kernel.SetImageKernelArg(2, output.Handle);
            this._kernel.SetKernelArg(3, offset);
            this._kernel.Enqueue3D(this._clContext.CommandQueue, (uint)width, (uint)height, (uint)depth);
        }
        #endregion

        #region 执行自适应阈值分割（写回） —— void ExecuteInPlace(ClImage3D image, int kernelSize...
        /// <summary>
        /// 执行自适应阈值分割（写回）
        /// </summary>
        /// <param name="image">输入图像（结果写回此图像）</param>
        /// <param name="kernelSize">均值滤波核矩阵尺寸（奇数，默认5）</param>
        /// <param name="offset">偏移量（默认0.02，归一化值，约HU650）</param>
        public void ExecuteInPlace(ClImage3D image, int kernelSize = 5, float offset = 0.02f)
        {
            using ClImage3D output = ClImage3D.Create(this._clContext, image.Width, image.Height, image.Depth, MemFlags.ReadWrite, image.ChannelOrder, image.ChannelType);

            this.Execute(image, output, kernelSize, offset);
            this._clContext.Finish();

            output.CopyTo(this._clContext.CommandQueue, image);
            this._clContext.Finish();
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

            this._kernel?.Dispose();
            this._program?.Dispose();
            this._disposed = true;
        }
        #endregion

        #endregion
    }
}
