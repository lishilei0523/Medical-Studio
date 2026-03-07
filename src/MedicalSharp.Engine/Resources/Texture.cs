using Microsoft.CSharp.RuntimeBinder;
using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel.Design;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 纹理
    /// </summary>
    public abstract class Texture : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 无参构造器
        /// </summary>
        protected Texture()
        {
            int textureId = GL.GenTexture();

            #region # 验证

            if (textureId == 0)
            {
                throw new RuntimeBinderException("创建纹理失败！");
            }

            #endregion

            this.Id = textureId;
        }

        #endregion

        #region # 属性

        #region 纹理Id —— int Id
        /// <summary>
        /// 纹理Id
        /// </summary>
        public int Id { get; private set; }
        #endregion

        #endregion

        #region # 方法

        #region 设置过滤器 —— abstract void SetFilter(TextureMinFilter minFilter...
        /// <summary>
        /// 设置过滤器
        /// </summary>
        /// <param name="minFilter">最小值过滤器</param>
        /// <param name="magFilter">最大值过滤器</param>
        public abstract void SetFilter(TextureMinFilter minFilter, TextureMagFilter magFilter);
        #endregion

        #region 设置包裹模式 —— abstract void SetWrapMode(TextureWrapMode wrapMode)
        /// <summary>
        /// 设置包裹模式
        /// </summary>
        /// <param name="wrapMode">包裹模式</param>
        public abstract void SetWrapMode(TextureWrapMode wrapMode);
        #endregion

        #region 绑定纹理 —— abstract void Bind(int index)
        /// <summary>
        /// 绑定纹理
        /// </summary>
        /// <param name="index">纹理索引</param>
        public abstract void Bind(int index);
        #endregion

        #region 解绑纹理 —— abstract void Unbind()
        /// <summary>
        /// 解绑纹理
        /// </summary>
        public abstract void Unbind();
        #endregion

        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            GL.DeleteTexture(this.Id);
        }
        #endregion

        #region 检查错误 —— static void CheckError(string operation)
        /// <summary>
        /// 检查错误
        /// </summary>
        /// <param name="operation">操作</param>
        protected static void CheckError(string operation)
        {
            ErrorCode errorCode = GL.GetError();
            if (errorCode != ErrorCode.NoError)
            {
                throw new CheckoutException($"OpenGL Error in {operation}: {errorCode}");
            }
        }
        #endregion

        #endregion
    }
}
