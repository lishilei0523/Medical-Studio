using MedicalSharp.Inspiration.Resources;
using Silk.NET.OpenCL;
using System;

namespace MedicalSharp.Inspiration.Operators
{
    /// <summary>
    /// 3D均值滤波算子
    /// </summary>
    public sealed class MeanBlur3D : IDisposable
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
        /// 均值滤波内核
        /// </summary>
        private readonly ClKernel _kernel;

        /// <summary>
        /// 创建3D均值滤波算子构造器
        /// </summary>
        /// <param name="clContext">OpenCL上下文</param>
        public MeanBlur3D(ClContext clContext)
        {
            this._clContext = clContext;
            this._program = ClProgram.FromFile(this._clContext, "Resources/Kernels/mean_blur_3d.cl");
            this._kernel = this._program.CreateKernel("mean_blur_3d");
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

        #region 执行均值滤波 —— void Execute(ClImage3D input, ClImage3D output...
        /// <summary>
        /// 执行均值滤波
        /// </summary>
        /// <param name="input">输入图像</param>
        /// <param name="output">输出图像</param>
        /// <param name="kernelSize">核矩阵尺寸（奇数：3、5、7，默认3）</param>
        public void Execute(ClImage3D input, ClImage3D output, int kernelSize = 3)
        {
            this._kernel.SetImageKernelArg(0, input.Handle);
            this._kernel.SetImageKernelArg(1, output.Handle);
            this._kernel.SetKernelArg(2, kernelSize);
            this._kernel.Enqueue3D(this._clContext.CommandQueue, (uint)input.Width, (uint)input.Height, (uint)input.Depth);
        }
        #endregion

        #region 执行均值滤波（写回） —— void ExecuteInPlace(ClImage3D image, int kernelSize)
        /// <summary>
        /// 执行均值滤波（写回）
        /// </summary>
        /// <param name="image">输入图像（结果写回此图像）</param>
        /// <param name="kernelSize">核矩阵尺寸（奇数，默认3）</param>
        public void ExecuteInPlace(ClImage3D image, int kernelSize = 3)
        {
            //创建临时输出图像
            using ClImage3D temp = ClImage3D.Create(this._clContext, image.Width, image.Height, image.Depth, MemFlags.ReadWrite, image.ChannelOrder, image.ChannelType);

            //执行算子
            this.Execute(image, temp, kernelSize);
            this._clContext.Finish();

            //写回原图像
            temp.CopyTo(this._clContext.CommandQueue, image);
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
