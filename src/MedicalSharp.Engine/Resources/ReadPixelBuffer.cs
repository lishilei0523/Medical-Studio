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
        /// <param name="pixelFormat">像素格式</param>
        /// <param name="pixelType">像素类型</param>
        protected ReadPixelBuffer(int width, int height, PixelFormat pixelFormat, PixelType pixelType)
            : base(width, height, pixelFormat, pixelType)
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

        #region 获取CPU数据 —— byte[] GetCpuBufferInternal(long timeoutNanoseconds)
        /// <summary>
        /// 获取CPU数据
        /// </summary>
        /// <param name="timeoutNanoseconds">超时时间（纳秒），-1 表示无限等待</param>
        /// <remarks>会在数据传输完成时返回</remarks>
        protected byte[] GetCpuBufferInternal(long timeoutNanoseconds = -1)
        {
            base.WaitForFence(timeoutNanoseconds);
            byte[] buffer = this.ReadImmediatelyInternal();

            return buffer;
        }
        #endregion

        #region 非阻塞获取CPU数据 —— bool TryGetCpuDataInternal(out byte[] data)
        /// <summary>
        /// 非阻塞获取CPU数据
        /// </summary>
        protected bool TryGetCpuDataInternal(out byte[] data)
        {
            data = null;

            if (!base.IsDataReady)
            {
                return false;
            }

            data = this.ReadImmediatelyInternal();

            return data != null;
        }
        #endregion

        #region 立即读取数据 —— byte[] ReadImmediatelyInternal()
        /// <summary>
        /// 立即读取数据
        /// </summary>
        private byte[] ReadImmediatelyInternal()
        {
            this.Bind();

            try
            {
                IntPtr ptr = GL.MapBuffer(this.BufferTarget, BufferAccess.ReadOnly);
                if (ptr == IntPtr.Zero)
                {
                    return null;
                }

                byte[] data = new byte[this.BufferSize];
                Marshal.Copy(ptr, data, 0, this.BufferSize);

                return data;
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
