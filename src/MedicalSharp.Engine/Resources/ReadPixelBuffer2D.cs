using OpenTK.Graphics.OpenGL4;
using System;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 像素缓冲区(读)2D
    /// </summary>
    /// <remarks>GPU -> CPU</remarks>
    public class ReadPixelBuffer2D : ReadPixelBuffer
    {
        #region # 字段及构造器

        /// <summary>
        /// 创建像素缓冲区(读)2D构造器
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="pixelFormat">像素格式</param>
        /// <param name="pixelType">像素类型</param>
        public ReadPixelBuffer2D(int width, int height, PixelFormat pixelFormat = PixelFormat.Rgba, PixelType pixelType = PixelType.UnsignedByte)
            : base(width, height, pixelFormat, pixelType)
        {

        }

        #endregion

        #region # 属性

        //

        #endregion

        #region # 方法

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

        #region 获取CPU数据 —— byte[] GetCpuBuffer(long timeoutNanoseconds)
        /// <summary>
        /// 获取CPU数据
        /// </summary>
        /// <param name="timeoutNanoseconds">超时时间（纳秒），-1 表示无限等待</param>
        /// <remarks>会在数据传输完成时返回</remarks>
        public byte[] GetCpuBuffer(long timeoutNanoseconds = -1)
        {
            return base.GetCpuBufferInternal(timeoutNanoseconds);
        }
        #endregion

        #region 非阻塞获取CPU数据 —— bool TryGetCpuData(out byte[] data)
        /// <summary>
        /// 非阻塞获取CPU数据
        /// </summary>
        public bool TryGetCpuData(out byte[] data)
        {
            return base.TryGetCpuDataInternal(out data);
        }
        #endregion

        #endregion
    }
}
