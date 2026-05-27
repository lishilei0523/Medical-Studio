using MedicalSharp.Inspiration.Resources;
using Silk.NET.OpenCL;
using System;

namespace MedicalSharp.Inspiration.Algorithms
{
    /// <summary>
    /// 3D Canny边缘检测算法
    /// </summary>
    /// <remarks>
    /// 使用形态学膨胀近似滞后跟踪，GPU友好，对应OpenCV的Cv2.Canny
    /// 流程：高斯滤波 -> Sobel梯度 -> 双阈值 + 形态学滞后跟踪
    /// </remarks>
    public sealed class Canny3D : IDisposable
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
        /// 创建3D Canny边缘检测算法构造器
        /// </summary>
        /// <param name="clContext">OpenCL 上下文</param>
        public Canny3D(ClContext clContext)
        {
            this._clContext = clContext;
            this._program = ClProgram.FromFile(clContext, "Resources/Kernels/canny_3d.cl");
            this._kernel = this._program.CreateKernel("canny_3d");
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

        #region 执行Canny边缘检测 —— void Execute(ClImage3D input, ClImage3D output, float lowerThreshold...
        /// <summary>
        /// 执行Canny边缘检测
        /// </summary>
        /// <param name="input">输入图像</param>
        /// <param name="output">输出图像</param>
        /// <param name="lowerThreshold">低阈值（弱边缘，默认10）</param>
        /// <param name="upperThreshold">高阈值（强边缘，默认30）</param>
        /// <param name="sigma">高斯滤波标准差（默认1.0）</param>
        /// <param name="dilateRadius">滞后跟踪膨胀半径（默认1）</param>
        public void Execute(ClImage3D input, ClImage3D output, float lowerThreshold = 10.0f, float upperThreshold = 30.0f, float sigma = 1.0f, int dilateRadius = 1)
        {
            int width = input.Width;
            int height = input.Height;
            int depth = input.Depth;

            //高斯滤波
            using GaussianBlur3D gaussianBlur = new GaussianBlur3D(this._clContext);
            using ClImage3D smoothed = ClImage3D.Create(this._clContext, width, height, depth, MemFlags.ReadWrite, input.ChannelOrder, input.ChannelType);
            gaussianBlur.Execute(input, smoothed, 3, sigma);
            this._clContext.Finish();

            //Sobel梯度计算
            using Sobel3D sobel = new Sobel3D(this._clContext);
            using ClImage3D gradient = ClImage3D.Create(this._clContext, width, height, depth, MemFlags.ReadWrite, input.ChannelOrder, input.ChannelType);
            sobel.Execute(smoothed, gradient, 1.0f, 1.0f, 0.0f);
            this._clContext.Finish();

            //双阈值 + 形态学滞后跟踪
            this._kernel.SetImageKernelArg(0, gradient.Handle);
            this._kernel.SetImageKernelArg(1, output.Handle);
            this._kernel.SetKernelArg(2, lowerThreshold);
            this._kernel.SetKernelArg(3, upperThreshold);
            this._kernel.SetKernelArg(4, dilateRadius);
            this._kernel.Enqueue3D(this._clContext.CommandQueue, (uint)width, (uint)height, (uint)depth);
        }
        #endregion

        #region 执行Canny边缘检测（写回） —— void ExecuteInPlace(ClImage3D image, float lowerThreshold...
        /// <summary>
        /// 执行Canny边缘检测（写回）
        /// </summary>
        /// <param name="image">输入图像（结果写回此图像）</param>
        /// <param name="lowerThreshold">低阈值（弱边缘，默认10）</param>
        /// <param name="upperThreshold">高阈值（强边缘，默认30）</param>
        /// <param name="sigma">高斯滤波标准差（默认1.0）</param>
        /// <param name="dilateRadius">滞后跟踪膨胀半径（默认1）</param>
        public void ExecuteInPlace(ClImage3D image, float lowerThreshold = 10.0f, float upperThreshold = 30.0f, float sigma = 1.0f, int dilateRadius = 1)
        {
            using ClImage3D output = ClImage3D.Create(this._clContext, image.Width, image.Height, image.Depth, MemFlags.ReadWrite, image.ChannelOrder, image.ChannelType);

            this.Execute(image, output, lowerThreshold, upperThreshold, sigma, dilateRadius);
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
