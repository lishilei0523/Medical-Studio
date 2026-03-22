using Microsoft.CSharp.RuntimeBinder;
using OpenTK.Graphics.OpenGL4;
using System;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 深度缓冲区
    /// </summary>
    public class RenderBuffer : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 创建深度缓冲区构造器
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="internalFormat">内部格式</param>
        public RenderBuffer(int width, int height, RenderbufferStorage internalFormat = RenderbufferStorage.DepthComponent24)
        {
            int renderBufferId = GL.GenRenderbuffer();

            #region # 验证

            if (renderBufferId == 0)
            {
                throw new RuntimeBinderException("创建深度缓冲区失败！");
            }

            #endregion

            this.Id = renderBufferId;

            //分配显存
            this.Bind();
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, internalFormat, width, height);
            this.Unbind();
        }

        #endregion

        #region # 属性

        #region 深度缓冲区Id —— int Id
        /// <summary>
        /// 深度缓冲区Id
        /// </summary>
        public int Id { get; private set; }
        #endregion

        #endregion

        #region # 方法

        #region 绑定深度缓冲区 —— void Bind()
        /// <summary>
        /// 绑定深度缓冲区
        /// </summary>
        public void Bind()
        {
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, this.Id);
        }
        #endregion

        #region 解绑深度缓冲区 —— void Unbind()
        /// <summary>
        /// 解绑深度缓冲区
        /// </summary>
        public void Unbind()
        {
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
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
