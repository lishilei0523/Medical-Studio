using Microsoft.CSharp.RuntimeBinder;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.InteropServices;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 像素缓冲区
    /// </summary>
    public class PixelBuffer
    {
        #region # 字段及构造器

        /// <summary>
        /// 创建像素缓冲区构造器
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="pixelFormat">像素格式</param>
        public PixelBuffer(int width, int height, PixelFormat pixelFormat = PixelFormat.Rgba)
        {
            this.Width = width;
            this.Height = height;
            this.PixelFormat = pixelFormat;
            int pixelBufferId = GL.GenBuffer();

            #region # 验证

            if (pixelBufferId == 0)
            {
                throw new RuntimeBinderException("创建像素缓冲区失败！");
            }

            #endregion

            this.Id = pixelBufferId;

            //分配显存
            if (pixelFormat == PixelFormat.Red)
            {
                this.BufferSize = width * height;
            }
            else if (pixelFormat == PixelFormat.Rgb)
            {
                this.BufferSize = width * height * 3;
            }
            else if (pixelFormat == PixelFormat.Rgba)
            {
                this.BufferSize = width * height * 4;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(pixelFormat), $"不支持的像素格式: {pixelFormat}");
            }

            this.Bind();
            GL.BufferData(BufferTarget.PixelPackBuffer, this.BufferSize, IntPtr.Zero, BufferUsageHint.StreamRead);
            this.Unbind();

            //设置对齐方式（重要！）
            GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
        }

        #endregion

        #region # 属性

        #region 像素缓冲区Id —— int Id
        /// <summary>
        /// 像素缓冲区Id
        /// </summary>
        public int Id { get; private set; }
        #endregion

        #region 宽度 —— int Width
        /// <summary>
        /// 宽度
        /// </summary>
        public int Width { get; private set; }
        #endregion 

        #region 高度 —— int Height
        /// <summary>
        /// 高度
        /// </summary>
        public int Height { get; private set; }
        #endregion

        #region 像素格式 —— PixelFormat PixelFormat
        /// <summary>
        /// 像素格式
        /// </summary>
        public PixelFormat PixelFormat { get; private set; }
        #endregion

        #region 缓冲区尺寸 —— int BufferSize
        /// <summary>
        /// 缓冲区尺寸
        /// </summary>
        public int BufferSize { get; private set; }
        #endregion

        #endregion

        #region # 方法

        #region 绑定像素缓冲区 —— void Bind()
        /// <summary>
        /// 绑定像素缓冲区
        /// </summary>
        public void Bind()
        {
            GL.BindBuffer(BufferTarget.PixelPackBuffer, this.Id);
        }
        #endregion

        #region 解绑像素缓冲区 —— void Unbind()
        /// <summary>
        /// 解绑像素缓冲区
        /// </summary>
        public void Unbind()
        {
            GL.BindBuffer(BufferTarget.PixelPackBuffer, 0);
        }
        #endregion

        #region 读取帧缓冲区 —— void ReadFrameBuffer(FrameBuffer frameBuffer)
        /// <summary>
        /// 读取帧缓冲区
        /// </summary>
        public void ReadFrameBuffer(FrameBuffer frameBuffer)
        {
            frameBuffer.Bind();
            this.Bind();

            GL.ReadPixels(0, 0, this.Width, this.Height, this.PixelFormat, PixelType.UnsignedByte, IntPtr.Zero);

            this.Unbind();
            frameBuffer.Unbind();
        }
        #endregion

        #region 读取2D纹理 —— void ReadTexture2D(Texture2D texture)
        /// <summary>
        /// 读取2D纹理
        /// </summary>
        public void ReadTexture2D(Texture2D texture)
        {
            texture.Bind();
            this.Bind();

            GL.GetTexImage(TextureTarget.Texture2D, 0, this.PixelFormat, PixelType.UnsignedByte, IntPtr.Zero);

            this.Unbind();
            texture.Unbind();
        }
        #endregion

        #region 获取CPU数据 —— byte[] GetCpuBuffer()
        /// <summary>
        /// 获取CPU数据
        /// </summary>
        /// <remarks>会在数据传输完成时返回</remarks>
        public byte[] GetCpuBuffer()
        {
            this.Bind();

            //映射内存（如果数据未就绪，这里会阻塞）
            IntPtr gpuPointer = GL.MapBuffer(BufferTarget.PixelPackBuffer, BufferAccess.ReadOnly);
            if (gpuPointer == IntPtr.Zero)
            {
                this.Unbind();
                return null;
            }

            //复制到托管数组
            byte[] buffer = new byte[this.BufferSize];
            Marshal.Copy(gpuPointer, buffer, 0, this.BufferSize);

            //取消映射
            GL.UnmapBuffer(BufferTarget.PixelPackBuffer);

            this.Unbind();

            return buffer;
        }
        #endregion

        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            GL.DeleteBuffer(this.Id);
        }
        #endregion 

        #endregion
    }
}
