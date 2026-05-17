using MedicalSharp.Inspiration.Resources;
using Silk.NET.OpenCL;
using System;

namespace MedicalSharp.Inspiration.Algorithms
{
    /// <summary>
    /// 3D高斯滤波算法
    /// </summary>
    public sealed class GaussianBlur3D : IDisposable
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
        /// 创建3D高斯滤波算法构造器
        /// </summary>
        /// <param name="clContext">OpenCL上下文</param>
        public GaussianBlur3D(ClContext clContext)
        {
            this._clContext = clContext;
            this._program = ClProgram.FromFile(this._clContext, "Resources/Kernels/gaussian_blur_3d.cl");
            this._kernel = this._program.CreateKernel("gaussian_blur_3d");
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

        #region 执行高斯滤波 —— void Execute(ClImage3D inputImage, ClImage3D outputImage...
        /// <summary>
        /// 执行高斯滤波
        /// </summary>
        /// <param name="inputImage">输入图像</param>
        /// <param name="outputImage">输出图像</param>
        /// <param name="kernelSize">核矩阵尺寸</param>
        /// <param name="sigma">标准差</param>
        /// <remarks>核矩阵尺寸必须为奇数(3、5、7等)，标准差常用(0.5~2.5)</remarks>
        public void Execute(ClImage3D inputImage, ClImage3D outputImage, int kernelSize = 3, float sigma = 1.0f)
        {
            this._kernel.SetImageKernelArg(0, inputImage.Handle);
            this._kernel.SetImageKernelArg(1, outputImage.Handle);
            this._kernel.SetKernelArg(2, kernelSize);
            this._kernel.SetKernelArg(3, sigma);
            this._kernel.Enqueue3D(this._clContext.CommandQueue, (uint)inputImage.Width, (uint)inputImage.Height, (uint)inputImage.Depth);
        }
        #endregion

        #region 执行高斯滤波(写回) —— void ExecuteInPlace(ClImage3D image, int kernelSize...
        /// <summary>
        /// 执行高斯滤波(写回)
        /// </summary>
        /// <param name="image">输入图像</param>
        /// <param name="kernelSize">核矩阵尺寸</param>
        /// <param name="sigma">标准差</param>
        /// <remarks>核矩阵尺寸必须为奇数(3、5、7等)，标准差常用(0.5~2.5)</remarks>
        public void ExecuteInPlace(ClImage3D image, int kernelSize = 3, float sigma = 1.0f)
        {
            //创建临时输出图像
            using ClImage3D outputImage = ClImage3D.Create(this._clContext, image.Width, image.Height, image.Depth, MemFlags.ReadWrite, image.ChannelOrder, image.ChannelType);

            //执行算法
            this.Execute(image, outputImage, kernelSize, sigma);
            this._clContext.Finish();

            //写回原图像
            outputImage.CopyTo(this._clContext.CommandQueue, image);
            this._clContext.Finish();
        }
        #endregion

        #region 执行高斯滤波(GL纹理) —— void ExecuteGLTexture(ClImage3D glTexture, int kernelSize...
        /// <summary>
        /// 执行高斯滤波(GL纹理)
        /// </summary>
        /// <param name="glTexture">GL纹理图像</param>
        /// <param name="kernelSize">核矩阵尺寸</param>
        /// <param name="sigma">标准差</param>
        /// <remarks>核矩阵尺寸必须为奇数(3、5、7等)，标准差常用(0.5~2.5)</remarks>
        public void ExecuteGLTexture(ClImage3D glTexture, int kernelSize = 3, float sigma = 1.0f)
        {
            //创建临时输出图像
            using ClImage3D outputImage = ClImage3D.Create(this._clContext, glTexture.Width, glTexture.Height, glTexture.Depth, MemFlags.ReadWrite, glTexture.ChannelOrder, glTexture.ChannelType);

            //从GL接管纹理
            glTexture.AcquireForCL(this._clContext.CommandQueue);

            //执行算法
            this.Execute(glTexture, outputImage, kernelSize, sigma);
            this._clContext.Finish();

            //写回GL纹理
            outputImage.CopyTo(this._clContext.CommandQueue, glTexture);

            //归还GL纹理
            glTexture.ReleaseToGL(this._clContext.CommandQueue);
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
