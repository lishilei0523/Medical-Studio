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
        /// 无参构造器
        /// </summary>
        private FrameBuffer()
        {
            int frameBufferId = GL.GenFramebuffer();

            #region # 验证

            if (frameBufferId == 0)
            {
                throw new RuntimeBinderException("创建帧缓冲区失败！");
            }

            #endregion

            this.Id = frameBufferId;
        }

        #endregion

        #region # 属性

        #region 帧缓冲区Id —— int Id
        /// <summary>
        /// 帧缓冲区Id
        /// </summary>
        public int Id { get; private set; }
        #endregion

        #region 输出纹理 —— Texture2D OutputTexture
        /// <summary>
        /// 输出纹理
        /// </summary>
        public Texture2D OutputTexture { get; private set; }
        #endregion

        #region 深度缓冲区 —— RenderBuffer RenderBuffer
        /// <summary>
        /// 深度缓冲区
        /// </summary>
        public RenderBuffer RenderBuffer { get; private set; }
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
        /// <param name="textureFormat">纹理格式</param>
        /// <returns>帧缓冲区</returns>
        public static FrameBuffer Create(int width, int height, PixelInternalFormat textureFormat = PixelInternalFormat.Rgba32f)
        {
            FrameBuffer frameBuffer = new FrameBuffer();
            frameBuffer.Bind();

            //创建输出纹理
            frameBuffer.OutputTexture = new Texture2D(width, height, textureFormat);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, frameBuffer.OutputTexture.Id, 0);

            frameBuffer.Unbind();
            frameBuffer.CheckFrameBufferStatus();

            return frameBuffer;
        }
        #endregion

        #region 创建帧缓冲区 —— static FrameBuffer CreateWithRenderBuffer(int width, int height...
        /// <summary>
        /// 创建帧缓冲区
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="textureFormat">内部像素格式</param>
        /// <param name="renderBufferFormat">深度缓冲格式</param>
        /// <returns>帧缓冲区</returns>
        /// <remarks>带深度缓冲</remarks>
        public static FrameBuffer CreateWithRenderBuffer(int width, int height, PixelInternalFormat textureFormat = PixelInternalFormat.Rgba32f, RenderbufferStorage renderBufferFormat = RenderbufferStorage.DepthComponent24)
        {
            FrameBuffer frameBuffer = new FrameBuffer();
            frameBuffer.Bind();

            //创建输出纹理
            frameBuffer.OutputTexture = new Texture2D(width, height, textureFormat);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, frameBuffer.OutputTexture.Id, 0);

            //创建深度缓冲
            frameBuffer.RenderBuffer = new RenderBuffer(width, height, renderBufferFormat);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, frameBuffer.RenderBuffer.Id);

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
            this.RenderBuffer?.Dispose();
        }
        #endregion

        #endregion
    }
}
