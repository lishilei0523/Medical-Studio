using MedicalSharp.Inspiration.Resources;
using Silk.NET.OpenCL;
using System;

namespace MedicalSharp.Inspiration.Algorithms
{
    /// <summary>
    /// Scharr边缘检测算法
    /// </summary>
    public sealed class Scharr3D : IDisposable
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
        /// 创建Scharr边缘检测算法构造器
        /// </summary>
        /// <param name="clContext">OpenCL上下文</param>
        public Scharr3D(ClContext clContext)
        {
            this._clContext = clContext;
            this._program = ClProgram.FromFile(this._clContext, "Resources/Kernels/scharr_3d.cl");
            this._kernel = this._program.CreateKernel("scharr_3d");
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

        #region 执行Scharr边缘检测 —— void Execute(ClImage3D input, ClImage3D output...
        /// <summary>
        /// 执行Scharr边缘检测
        /// </summary>
        /// <param name="input">输入图像</param>
        /// <param name="output">输出图像</param>
        /// <param name="alpha">X方向权重（0.0~1.0，默认0.5）</param>
        /// <param name="beta">Y方向权重（0.0~1.0，默认0.5）</param>
        /// <param name="gamma">偏移量（加到最终结果，默认0）</param>
        public void Execute(ClImage3D input, ClImage3D output, float alpha = 0.5f, float beta = 0.5f, float gamma = 0.0f)
        {
            this._kernel.SetImageKernelArg(0, input.Handle);
            this._kernel.SetImageKernelArg(1, output.Handle);
            this._kernel.SetKernelArg(2, alpha);
            this._kernel.SetKernelArg(3, beta);
            this._kernel.SetKernelArg(4, gamma);
            this._kernel.Enqueue3D(this._clContext.CommandQueue, (uint)input.Width, (uint)input.Height, (uint)input.Depth);
        }
        #endregion

        #region 执行Scharr边缘检测（写回） —— void ExecuteInPlace(ClImage3D image...
        /// <summary>
        /// 执行Scharr边缘检测（写回）
        /// </summary>
        /// <param name="image">输入图像（结果写回此图像）</param>
        /// <param name="alpha">X方向权重（0.0~1.0，默认0.5）</param>
        /// <param name="beta">Y方向权重（0.0~1.0，默认0.5）</param>
        /// <param name="gamma">偏移量（加到最终结果，默认0）</param>
        public void ExecuteInPlace(ClImage3D image, float alpha = 0.5f, float beta = 0.5f, float gamma = 0.0f)
        {
            using ClImage3D output = ClImage3D.Create(this._clContext, image.Width, image.Height, image.Depth, MemFlags.ReadWrite, image.ChannelOrder, image.ChannelType);

            this.Execute(image, output, alpha, beta, gamma);
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
