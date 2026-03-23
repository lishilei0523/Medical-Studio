using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.InteropServices;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 像素缓冲区(读)
    /// </summary>
    /// <remarks>GPU -> CPU</remarks>
    public class ReadPixelBuffer : PixelBuffer
    {
        #region # 字段及构造器

        /// <summary>
        /// 创建像素缓冲区(读)构造器
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="pixelFormat">像素格式</param>
        public ReadPixelBuffer(int width, int height, PixelFormat pixelFormat = PixelFormat.Rgba)
            : base(width, height, pixelFormat)
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

        //Public

        #region 读取帧缓冲区 —— void ReadFrameBuffer(FrameBuffer frameBuffer...
        /// <summary>
        /// 读取帧缓冲区
        /// </summary>
        /// <param name="frameBuffer">帧缓冲区(null表示读取默认帧缓冲)</param>
        /// <param name="useFence">是否使用栅栏同步</param>
        public void ReadFrameBuffer(FrameBuffer frameBuffer, bool useFence = true)
        {
            //绑定帧缓冲区
            if (frameBuffer != null)
            {
                frameBuffer.Bind();
            }
            else
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }

            this.Bind();

            GL.ReadPixels(0, 0, this.Width, this.Height, this.PixelFormat, PixelType.UnsignedByte, IntPtr.Zero);

            //确保PBO写入完成（可选）
            GL.MemoryBarrier(MemoryBarrierFlags.PixelBufferBarrierBit);

            //创建栅栏标记读取完成（推荐，用于非阻塞检查）
            if (useFence)
            {
                base.CreateFence();
            }

            this.Unbind();

            frameBuffer?.Unbind();
        }
        #endregion

        #region 读取2D纹理 —— void ReadTexture2D(Texture2D texture)
        /// <summary>
        /// 读取2D纹理
        /// </summary>
        /// <param name="texture">2D纹理</param>
        /// <param name="useFence">是否使用栅栏同步</param>
        public void ReadTexture2D(Texture2D texture, bool useFence = true)
        {
            texture.Bind();
            this.Bind();

            GL.GetTexImage(TextureTarget.Texture2D, 0, this.PixelFormat, PixelType.UnsignedByte, IntPtr.Zero);

            //确保PBO写入完成
            GL.MemoryBarrier(MemoryBarrierFlags.PixelBufferBarrierBit);

            //创建栅栏标记读取完成
            if (useFence)
            {
                base.CreateFence();
            }

            this.Unbind();
            texture.Unbind();
        }
        #endregion

        #region 读取3D纹理 —— void ReadTexture3D(Texture3D texture, int level...
        /// <summary>
        /// 读取3D纹理
        /// </summary>
        public void ReadTexture3D(Texture3D texture, int level = 0, bool useFence = true)
        {
            texture.Bind();
            this.Bind();

            GL.GetTexImage(TextureTarget.Texture3D, level, this.PixelFormat, PixelType.UnsignedByte, IntPtr.Zero);

            //确保PBO写入完成
            GL.MemoryBarrier(MemoryBarrierFlags.PixelBufferBarrierBit);

            //创建栅栏标记读取完成
            if (useFence)
            {
                this.CreateFence();
            }

            this.Unbind();
            texture.Unbind();
        }
        #endregion

        #region 获取CPU数据 —— byte[] GetCpuBuffer(long timeoutNanoseconds)
        /// <summary>
        /// 获取CPU数据
        /// </summary>
        /// <param name="timeoutNanoseconds">超时时间（纳秒），-1 表示无限等待</param>
        /// <remarks>会在数据传输完成时返回</remarks>
        public byte[] GetCpuBuffer(long timeoutNanoseconds = -1)
        {
            base.WaitForFence(timeoutNanoseconds);
            byte[] buffer = this.ReadImmediately();

            return buffer;
        }
        #endregion

        #region 非阻塞获取CPU数据 —— bool TryGetData(out byte[] data)
        /// <summary>
        /// 非阻塞获取CPU数据
        /// </summary>
        public bool TryGetData(out byte[] data)
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


        //Private

        #region 立即读取数据 —— byte[] ReadImmediately()
        /// <summary>
        /// 立即读取数据
        /// </summary>
        private byte[] ReadImmediately()
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

        #region 等待数据就绪 —— bool WaitForData(long timeoutNanoseconds)
        /// <summary>
        /// 等待数据就绪
        /// </summary>
        /// <param name="timeoutNanoseconds">超时时间（纳秒），-1 表示无限等待</param>
        public bool WaitForData(long timeoutNanoseconds = -1)
        {
            try
            {
                this.WaitForFence(timeoutNanoseconds);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #endregion
    }
}
