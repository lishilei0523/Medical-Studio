using Microsoft.CSharp.RuntimeBinder;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.InteropServices;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 像素缓冲区(读)
    /// </summary>
    /// <remarks>GPU -> CPU</remarks>
    public abstract class ReadPixelBuffer : PixelBuffer
    {
        #region # 字段及构造器

        /// <summary>
        /// 创建像素缓冲区(读)构造器
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">深度</param>
        /// <param name="pixelFormat">像素格式</param>
        /// <param name="pixelType">像素类型</param>
        protected ReadPixelBuffer(int width, int height, int depth, PixelFormat pixelFormat, PixelType pixelType)
            : base(width, height, depth, pixelFormat, pixelType)
        {
            base.CreateBuffer();
        }

        #endregion

        #region # 属性

        #region 只读属性 - 缓冲区目标 —— override BufferTarget BufferTarget
        /// <summary>
        /// 只读属性 - 缓冲区目标
        /// </summary>
        protected override BufferTarget BufferTarget
        {
            get => BufferTarget.PixelPackBuffer;
        }
        #endregion

        #region 只读属性 - 缓冲区用途 —— override BufferUsageHint BufferUsage
        /// <summary>
        /// 只读属性 - 缓冲区用途
        /// </summary>
        protected override BufferUsageHint BufferUsage
        {
            get => BufferUsageHint.StreamRead;
        }
        #endregion

        #endregion

        #region # 方法

        #region 绑定像素缓冲区 —— override void Bind()
        /// <summary>
        /// 绑定像素缓冲区
        /// </summary>
        public override void Bind()
        {
            GL.BindBuffer(BufferTarget.PixelPackBuffer, this.Id);
        }
        #endregion

        #region 解绑像素缓冲区 —— override void Unbind()
        /// <summary>
        /// 解绑像素缓冲区
        /// </summary>
        public override void Unbind()
        {
            GL.BindBuffer(BufferTarget.PixelPackBuffer, 0);
        }
        #endregion

        #region 获取CPU数据 —— byte[] GetCpuBuffer(long timeoutNanoseconds)
        /// <summary>
        /// 获取CPU数据
        /// </summary>
        /// <param name="timeoutNanoseconds">超时时间（纳秒），-1 表示无限等待</param>
        /// <returns>缓冲区数据</returns>
        /// <remarks>会在数据传输完成时返回</remarks>
        public byte[] GetCpuBuffer(long timeoutNanoseconds = -1)
        {
            base.WaitForFence(timeoutNanoseconds);
            byte[] buffer = this.ReadImmediately();

            return buffer;
        }
        #endregion

        #region 获取CPU数据 —— void GetCpuBuffer(IntPtr data, long timeoutNanoseconds)
        /// <summary>
        /// 获取CPU数据
        /// </summary>
        /// <param name="data">数据指针</param>
        /// <param name="timeoutNanoseconds">超时时间（纳秒），-1 表示无限等待</param>
        /// <remarks>会在数据传输完成时返回</remarks>
        public void GetCpuBuffer(IntPtr data, long timeoutNanoseconds = -1)
        {
            base.WaitForFence(timeoutNanoseconds);
            this.ReadImmediately(data);
        }
        #endregion

        #region 非阻塞获取CPU数据 —— bool TryGetCpuData(out byte[] data)
        /// <summary>
        /// 非阻塞获取CPU数据
        /// </summary>
        public bool TryGetCpuData(out byte[] data)
        {
            data = null;

            if (!base.IsDataReady)
            {
                return false;
            }

            data = this.ReadImmediately();

            return data != null;
        }
        #endregion

        #region 非阻塞获取CPU数据 —— bool TryGetCpuData(IntPtr data)
        /// <summary>
        /// 非阻塞获取CPU数据
        /// </summary>
        /// <param name="data">数据指针</param>
        /// <returns>是否成功获取数据</returns>
        public bool TryGetCpuData(IntPtr data)
        {
            if (!base.IsDataReady)
            {
                return false;
            }

            this.ReadImmediately(data);

            return true;
        }
        #endregion

        #region 立即读取数据 —— byte[] ReadImmediately()
        /// <summary>
        /// 立即读取数据
        /// </summary>
        private byte[] ReadImmediately()
        {
            this.Bind();

            try
            {
                IntPtr gpuPtr = GL.MapBuffer(this.BufferTarget, BufferAccess.ReadOnly);
                if (gpuPtr == IntPtr.Zero)
                {
                    throw new GlException("GL.MapBuffer失败！");
                }

                byte[] data = new byte[this.BufferSize];
                Marshal.Copy(gpuPtr, data, 0, this.BufferSize);

                return data;
            }
            finally
            {
                GL.UnmapBuffer(this.BufferTarget);
                this.Unbind();
            }
        }
        #endregion

        #region 立即读取数据 —— void ReadImmediately(IntPtr data)
        /// <summary>
        /// 立即读取数据
        /// </summary>
        /// <param name="data">数据指针</param>
        private unsafe void ReadImmediately(IntPtr data)
        {
            this.Bind();

            try
            {
                IntPtr gpuPtr = GL.MapBuffer(this.BufferTarget, BufferAccess.ReadOnly);
                if (gpuPtr == IntPtr.Zero)
                {
                    throw new GlException("GL.MapBuffer失败！");
                }

                NativeMemory.Copy(gpuPtr.ToPointer(), data.ToPointer(), (UIntPtr)this.BufferSize);
            }
            finally
            {
                GL.UnmapBuffer(this.BufferTarget);
                this.Unbind();
            }
        }
        #endregion

        #endregion
    }
}
