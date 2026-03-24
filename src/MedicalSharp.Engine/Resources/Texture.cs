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
        /// 创建纹理构造器
        /// </summary>
        /// <param name="pixelInternalFormat">像素内部格式</param>
        /// <param name="pixelFormat">像素格式</param>
        /// <param name="pixelType">像素类型</param>
        protected Texture(PixelInternalFormat pixelInternalFormat, PixelFormat pixelFormat, PixelType pixelType)
        {
            int textureId = GL.GenTexture();

            #region # 验证

            if (textureId == 0)
            {
                throw new RuntimeBinderException("创建纹理失败！");
            }

            #endregion

            this.Id = textureId;
            this.PixelInternalFormat = pixelInternalFormat;
            this.PixelFormat = pixelFormat;
            this.PixelType = pixelType;
        }

        #endregion

        #region # 属性

        #region 纹理Id —— int Id
        /// <summary>
        /// 纹理Id
        /// </summary>
        public int Id { get; private set; }
        #endregion

        #region 像素内部格式 —— PixelInternalFormat PixelInternalFormat
        /// <summary>
        /// 像素内部格式
        /// </summary>
        public PixelInternalFormat PixelInternalFormat { get; private set; }
        #endregion

        #region 像素格式 —— PixelFormat PixelFormat
        /// <summary>
        /// 像素格式
        /// </summary>
        public PixelFormat PixelFormat { get; private set; }
        #endregion

        #region 像素类型 —— PixelType PixelType
        /// <summary>
        /// 像素类型
        /// </summary>
        public PixelType PixelType { get; private set; }
        #endregion

        #region 最小值过滤器 —— TextureMinFilter MinFilter
        /// <summary>
        /// 最小值过滤器
        /// </summary>
        public TextureMinFilter MinFilter { get; private set; }
        #endregion

        #region 最大值过滤器 —— TextureMagFilter MagFilter
        /// <summary>
        /// 最大值过滤器
        /// </summary>
        public TextureMagFilter MagFilter { get; private set; }
        #endregion

        #region 包裹模式 —— TextureWrapMode WrapMode
        /// <summary>
        /// 包裹模式
        /// </summary>
        public TextureWrapMode WrapMode { get; private set; }
        #endregion

        #endregion

        #region # 方法

        #region 绑定纹理 —— abstract void Bind(int index)
        /// <summary>
        /// 绑定纹理
        /// </summary>
        /// <param name="index">纹理索引</param>
        public abstract void Bind(int index);
        #endregion

        #region 绑定纹理 —— abstract void Bind()
        /// <summary>
        /// 绑定纹理
        /// </summary>
        public abstract void Bind();
        #endregion

        #region 解绑纹理 —— abstract void Unbind()
        /// <summary>
        /// 解绑纹理
        /// </summary>
        public abstract void Unbind();
        #endregion

        #region 分配内存 —— abstract void AllocateMemory()
        /// <summary>
        /// 分配内存
        /// </summary>
        public abstract void AllocateMemory();
        #endregion

        #region 分配内存 —— abstract void AllocateMemory(IntPtr pixels)
        /// <summary>
        /// 分配内存
        /// </summary>
        /// <param name="pixels">像素数据</param>
        public abstract void AllocateMemory(IntPtr pixels);
        #endregion

        #region 设置过滤器 —— virtual void SetFilter(TextureMinFilter minFilter...
        /// <summary>
        /// 设置过滤器
        /// </summary>
        /// <param name="minFilter">最小值过滤器</param>
        /// <param name="magFilter">最大值过滤器</param>
        public virtual void SetFilter(TextureMinFilter minFilter, TextureMagFilter magFilter)
        {
            this.MinFilter = minFilter;
            this.MagFilter = magFilter;
        }
        #endregion

        #region 设置包裹模式 —— virtual void SetWrapMode(TextureWrapMode wrapMode)
        /// <summary>
        /// 设置包裹模式
        /// </summary>
        /// <param name="wrapMode">包裹模式</param>
        public virtual void SetWrapMode(TextureWrapMode wrapMode)
        {
            this.WrapMode = wrapMode;
        }
        #endregion

        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (this.Id != 0)
            {
                GL.DeleteTexture(this.Id);
                this.Id = 0;
            }
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
