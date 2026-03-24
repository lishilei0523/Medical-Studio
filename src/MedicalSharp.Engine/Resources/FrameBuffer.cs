using Microsoft.CSharp.RuntimeBinder;
using OpenTK.Graphics.OpenGL4;
using System;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 帧缓冲区
    /// </summary>
    public class FrameBuffer : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 创建帧缓冲区构造器
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        private FrameBuffer(int width, int height)
        {
            this.Id = GL.GenFramebuffer();

            #region # 验证

            if (this.Id == 0)
            {
                throw new RuntimeBinderException("创建帧缓冲区失败！");
            }

            #endregion

            this.Width = width;
            this.Height = height;
        }

        #endregion

        #region # 属性

        #region 帧缓冲区Id —— int Id
        /// <summary>
        /// 帧缓冲区Id
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

        #region 输出纹理 —— Texture2D OutputTexture
        /// <summary>
        /// 输出纹理
        /// </summary>
        public Texture2D OutputTexture { get; private set; }
        #endregion

        #region 深度缓冲区 —— RenderBuffer DepthBuffer
        /// <summary>
        /// 深度缓冲区
        /// </summary>
        public RenderBuffer DepthBuffer { get; private set; }
        #endregion

        #endregion

        #region # 方法

        //Static

        #region 创建帧缓冲区 —— static FrameBuffer Create(int width, int height...
        /// <summary>
        /// 创建帧缓冲区
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="pixelInternalFormat">像素内部格式</param>
        /// <param name="pixelFormat">像素格式</param>
        /// <param name="pixelType">像素类型</param>
        /// <returns>帧缓冲区</returns>
        public static FrameBuffer Create(int width, int height, PixelInternalFormat pixelInternalFormat = PixelInternalFormat.Rgba8, PixelFormat pixelFormat = PixelFormat.Rgba, PixelType pixelType = PixelType.UnsignedByte)
        {
            FrameBuffer frameBuffer = new FrameBuffer(width, height);
            frameBuffer.Bind();

            //创建输出纹理
            frameBuffer.OutputTexture = new Texture2D(width, height, pixelInternalFormat, pixelFormat, pixelType);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, frameBuffer.OutputTexture.Id, 0);

            frameBuffer.Unbind();
            frameBuffer.CheckFrameBufferStatus();

            return frameBuffer;
        }
        #endregion

        #region 创建帧缓冲区 —— static FrameBuffer CreateWithDepthBuffer(int width, int height...
        /// <summary>
        /// 创建帧缓冲区
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="pixelInternalFormat">像素内部格式</param>
        /// <param name="pixelFormat">像素格式</param>
        /// <param name="pixelType">像素类型</param>
        /// <returns>帧缓冲区</returns>
        /// <remarks>带深度缓冲</remarks>
        public static FrameBuffer CreateWithDepthBuffer(int width, int height, PixelInternalFormat pixelInternalFormat = PixelInternalFormat.Rgba8, PixelFormat pixelFormat = PixelFormat.Rgba, PixelType pixelType = PixelType.UnsignedByte)
        {
            FrameBuffer frameBuffer = new FrameBuffer(width, height);
            frameBuffer.Bind();

            //创建输出纹理
            frameBuffer.OutputTexture = new Texture2D(width, height, pixelInternalFormat, pixelFormat, pixelType);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, frameBuffer.OutputTexture.Id, 0);

            //创建深度缓冲
            frameBuffer.DepthBuffer = new RenderBuffer(width, height, RenderbufferStorage.DepthComponent24);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, frameBuffer.DepthBuffer.Id);

            frameBuffer.Unbind();
            frameBuffer.CheckFrameBufferStatus();

            return frameBuffer;
        }
        #endregion


        //Public

        #region 绑定帧缓冲区 —— void Bind()
        /// <summary>
        /// 绑定帧缓冲区
        /// </summary>
        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, this.Id);
        }
        #endregion

        #region 解绑帧缓冲区 —— void Unbind()
        /// <summary>
        /// 解绑帧缓冲区
        /// </summary>
        public void Unbind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
        #endregion

        #region 检查FBO完整性 —— void CheckFrameBufferStatus()
        /// <summary>
        /// 检查FBO完整性
        /// </summary>
        public void CheckFrameBufferStatus()
        {
            FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
            {
                throw new RuntimeBinderException($"FBO创建失败: {status}");
            }
        }
        #endregion

        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            GL.DeleteFramebuffer(this.Id);
            this.OutputTexture?.Dispose();
            this.DepthBuffer?.Dispose();
        }
        #endregion

        #endregion
    }
}
