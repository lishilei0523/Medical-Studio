using Microsoft.CSharp.RuntimeBinder;
using OpenTK.Graphics.OpenGL4;
using System;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 渲染缓冲区
    /// </summary>
    public class RenderBuffer : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 创建渲染缓冲区构造器
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="renderBufferStorage">深度/模板格式</param>
        public RenderBuffer(int width, int height, RenderbufferStorage renderBufferStorage = RenderbufferStorage.DepthComponent24)
        {
            this.Id = GL.GenRenderbuffer();

            #region # 验证

            if (this.Id == 0)
            {
                throw new RuntimeBinderException("创建深度缓冲区失败！");
            }

            #endregion

            this.Width = width;
            this.Height = height;
            this.RenderBufferStorage = renderBufferStorage;

            //分配显存
            this.AllocateMemory();
        }

        #endregion

        #region # 属性

        #region 渲染缓冲区Id —— int Id
        /// <summary>
        /// 渲染缓冲区Id
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

        #region 深度/模板格式 —— RenderbufferStorage RenderBufferStorage
        /// <summary>
        /// 深度/模板格式
        /// </summary>
        public RenderbufferStorage RenderBufferStorage { get; private set; }
        #endregion

        #endregion

        #region # 方法

        #region 绑定渲染缓冲区 —— void Bind()
        /// <summary>
        /// 绑定渲染缓冲区
        /// </summary>
        public void Bind()
        {
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, this.Id);
        }
        #endregion

        #region 解绑渲染缓冲区 —— void Unbind()
        /// <summary>
        /// 解绑渲染缓冲区
        /// </summary>
        public void Unbind()
        {
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
        }
        #endregion

        #region 分配内存 —— void AllocateMemory()
        /// <summary>
        /// 分配内存
        /// </summary>
        public void AllocateMemory()
        {
            this.Bind();

            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, this.RenderBufferStorage, this.Width, this.Height);

            this.Unbind();
        }
        #endregion

        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            GL.DeleteRenderbuffer(this.Id);
        }
        #endregion 

        #endregion
    }
}
