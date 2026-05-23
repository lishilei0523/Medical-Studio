using MedicalSharp.Inspiration.Resources;
using Silk.NET.OpenCL;
using System;

namespace MedicalSharp.Inspiration.Algorithms
{
    /// <summary>
    /// 3D形态学算法
    /// </summary>
    /// <remarks>支持腐蚀、膨胀、开运算、闭运算、礼帽运算、黑帽运算、梯度运算</remarks>
    public sealed class Morphology3D : IDisposable
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
        /// 腐蚀内核
        /// </summary>
        private readonly ClKernel _erodeKernel;

        /// <summary>
        /// 膨胀内核
        /// </summary>
        private readonly ClKernel _dilateKernel;

        /// <summary>
        /// 图像减法内核
        /// </summary>
        private readonly ClKernel _subtractKernel;

        /// <summary>
        /// 创建3D形态学操作构造器
        /// </summary>
        /// <param name="clContext">OpenCL上下文</param>
        public Morphology3D(ClContext clContext)
        {
            this._clContext = clContext;
            this._program = ClProgram.FromFile(this._clContext, "Resources/Kernels/morphology_3d.cl");
            this._erodeKernel = this._program.CreateKernel("erode_3d");
            this._dilateKernel = this._program.CreateKernel("dilate_3d");
            this._subtractKernel = this._program.CreateKernel("subtract_3d");
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

        #region 只读属性 - 腐蚀内核 —— ClKernel ErodeKernel
        /// <summary>
        /// 只读属性 - 腐蚀内核
        /// </summary>
        public ClKernel ErodeKernel
        {
            get => this._erodeKernel;
        }
        #endregion

        #region 只读属性 - 膨胀内核 —— ClKernel DilateKernel
        /// <summary>
        /// 只读属性 - 膨胀内核
        /// </summary>
        public ClKernel DilateKernel
        {
            get => this._dilateKernel;
        }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 腐蚀 —— void Erode(ClImage3D input, ClImage3D output, int kernelSize)
        /// <summary>
        /// 腐蚀
        /// </summary>
        /// <param name="input">输入图像</param>
        /// <param name="output">输出图像</param>
        /// <param name="kernelSize">核矩阵尺寸（奇数，默认3）</param>
        /// <remarks>取邻域最小值，前景区域向内收缩，去除边缘毛刺和细小噪声</remarks>
        public void Erode(ClImage3D input, ClImage3D output, int kernelSize = 3)
        {
            this._erodeKernel.SetImageKernelArg(0, input.Handle);
            this._erodeKernel.SetImageKernelArg(1, output.Handle);
            this._erodeKernel.SetKernelArg(2, kernelSize);
            this._erodeKernel.Enqueue3D(this._clContext.CommandQueue, (uint)input.Width, (uint)input.Height, (uint)input.Depth);
        }
        #endregion

        #region 膨胀 —— void Dilate(ClImage3D input, ClImage3D output, int kernelSize)
        /// <summary>
        /// 膨胀
        /// </summary>
        /// <param name="input">输入图像</param>
        /// <param name="output">输出图像</param>
        /// <param name="kernelSize">核矩阵尺寸（奇数，默认3）</param>
        /// <remarks>取邻域最大值，前景区域向外扩张，填充内部小空洞</remarks>
        public void Dilate(ClImage3D input, ClImage3D output, int kernelSize = 3)
        {
            this._dilateKernel.SetImageKernelArg(0, input.Handle);
            this._dilateKernel.SetImageKernelArg(1, output.Handle);
            this._dilateKernel.SetKernelArg(2, kernelSize);
            this._dilateKernel.Enqueue3D(this._clContext.CommandQueue, (uint)input.Width, (uint)input.Height, (uint)input.Depth);
        }
        #endregion

        #region 开运算 —— void Open(ClImage3D input, ClImage3D output, int kernelSize)
        /// <summary>
        /// 开运算
        /// </summary>
        /// <param name="input">输入图像</param>
        /// <param name="output">输出图像</param>
        /// <param name="kernelSize">核矩阵尺寸（奇数，默认3）</param>
        /// <remarks>先腐蚀后膨胀，平滑轮廓、去除孤立噪点，保持主体大小不变</remarks>
        public void Open(ClImage3D input, ClImage3D output, int kernelSize = 3)
        {
            //创建临时图像
            using ClImage3D temp = ClImage3D.Create(this._clContext, input.Width, input.Height, input.Depth, MemFlags.ReadWrite, input.ChannelOrder, input.ChannelType);

            //先腐蚀
            this.Erode(input, temp, kernelSize);
            this._clContext.Finish();

            //后膨胀
            this.Dilate(temp, output, kernelSize);
            this._clContext.Finish();
        }
        #endregion

        #region 闭运算 —— void Close(ClImage3D input, ClImage3D output, int kernelSize)
        /// <summary>
        /// 闭运算
        /// </summary>
        /// <param name="input">输入图像</param>
        /// <param name="output">输出图像</param>
        /// <param name="kernelSize">核矩阵尺寸（奇数，默认3）</param>
        /// <remarks>先膨胀后腐蚀，填充内部小孔、连接邻近区域，保持主体大小不变</remarks>
        public void Close(ClImage3D input, ClImage3D output, int kernelSize = 3)
        {
            //创建临时图像
            using ClImage3D temp = ClImage3D.Create(this._clContext, input.Width, input.Height, input.Depth, MemFlags.ReadWrite, input.ChannelOrder, input.ChannelType);

            //先膨胀
            this.Dilate(input, temp, kernelSize);
            this._clContext.Finish();

            //后腐蚀
            this.Erode(temp, output, kernelSize);
            this._clContext.Finish();
        }
        #endregion

        #region 礼帽运算 —— void TopHat(ClImage3D input, ClImage3D output, int kernelSize)
        /// <summary>
        /// 礼帽运算
        /// </summary>
        /// <param name="input">输入图像</param>
        /// <param name="output">输出图像</param>
        /// <param name="kernelSize">核矩阵尺寸（奇数，默认3）</param>
        /// <remarks>原图减去开运算结果，提取比背景亮的细小结构（如钙化点）</remarks>
        public void TopHat(ClImage3D input, ClImage3D output, int kernelSize = 3)
        {
            // 创建临时图像
            using ClImage3D opened = ClImage3D.Create(this._clContext, input.Width, input.Height, input.Depth, MemFlags.ReadWrite, input.ChannelOrder, input.ChannelType);

            //开运算
            this.Open(input, opened, kernelSize);
            this._clContext.Finish();

            //礼帽 = 原图 - 开运算
            this.Subtract(input, opened, output);
            this._clContext.Finish();
        }
        #endregion

        #region 黑帽运算 —— void BlackHat(ClImage3D input, ClImage3D output, int kernelSize)
        /// <summary>
        /// 黑帽运算
        /// </summary>
        /// <param name="input">输入图像</param>
        /// <param name="output">输出图像</param>
        /// <param name="kernelSize">核矩阵尺寸（奇数，默认3）</param>
        /// <remarks>闭运算结果减去原图，提取比背景暗的细小结构（如微小结节）</remarks>
        public void BlackHat(ClImage3D input, ClImage3D output, int kernelSize = 3)
        {
            //创建临时图像
            using ClImage3D closed = ClImage3D.Create(this._clContext, input.Width, input.Height, input.Depth, MemFlags.ReadWrite, input.ChannelOrder, input.ChannelType);

            //闭运算
            this.Close(input, closed, kernelSize);
            this._clContext.Finish();

            //黑帽 = 闭运算 - 原图
            this.Subtract(closed, input, output);
            this._clContext.Finish();
        }
        #endregion

        #region 梯度运算 —— void Gradient(ClImage3D input, ClImage3D output, int kernelSize)
        /// <summary>
        /// 梯度运算
        /// </summary>
        /// <param name="input">输入图像</param>
        /// <param name="output">输出图像</param>
        /// <param name="kernelSize">核矩阵尺寸（奇数，默认3）</param>
        /// <remarks>膨胀结果减去腐蚀结果，直接提取边缘</remarks>
        public void Gradient(ClImage3D input, ClImage3D output, int kernelSize = 3)
        {
            //创建临时图像
            using ClImage3D eroded = ClImage3D.Create(this._clContext, input.Width, input.Height, input.Depth, MemFlags.ReadWrite, input.ChannelOrder, input.ChannelType);
            using ClImage3D dilated = ClImage3D.Create(this._clContext, input.Width, input.Height, input.Depth, MemFlags.ReadWrite, input.ChannelOrder, input.ChannelType);

            //腐蚀和膨胀
            this.Erode(input, eroded, kernelSize);
            this.Dilate(input, dilated, kernelSize);
            this._clContext.Finish();

            //梯度 = 膨胀 - 腐蚀
            this.Subtract(dilated, eroded, output);
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

            this._erodeKernel?.Dispose();
            this._dilateKernel?.Dispose();
            this._subtractKernel?.Dispose();
            this._program?.Dispose();
            this._disposed = true;
        }
        #endregion


        //Private

        #region 图像减法 —— void Subtract(ClImage3D imageA, ClImage3D imageB, ClImage3D output)
        /// <summary>
        /// 图像减法
        /// </summary>
        /// <param name="imageA">输入图像A</param>
        /// <param name="imageB">输入图像B</param>
        /// <param name="output">输出图像（A-B）</param>
        /// <remarks>逐体素相减</remarks>
        private void Subtract(ClImage3D imageA, ClImage3D imageB, ClImage3D output)
        {
            this._subtractKernel.SetImageKernelArg(0, imageA.Handle);
            this._subtractKernel.SetImageKernelArg(1, imageB.Handle);
            this._subtractKernel.SetImageKernelArg(2, output.Handle);
            this._subtractKernel.Enqueue3D(this._clContext.CommandQueue, (uint)imageA.Width, (uint)imageA.Height, (uint)imageA.Depth);
        }
        #endregion

        #endregion
    }
}
