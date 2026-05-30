using MedicalSharp.Engine.Base;
using OpenTK.Graphics.OpenGL4;
using System;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 纹理
    /// </summary>
    public abstract class Texture : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 释放标识
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// 创建纹理构造器
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">深度</param>
        /// <param name="pixelInternalFormat">像素内部格式</param>
        /// <param name="pixelFormat">像素格式</param>
        /// <param name="pixelType">像素类型</param>
        protected Texture(int width, int height, int depth, PixelInternalFormat pixelInternalFormat, PixelFormat pixelFormat, PixelType pixelType)
        {
            int textureId = GL.GenTexture();

            #region # 验证

            if (textureId == 0)
            {
                throw new GlException("创建纹理失败！");
            }
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "宽度必须大于0！");
            }
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "高度必须大于0！");
            }
            if (depth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(depth), "深度必须大于0！");
            }

            #endregion

            this.Id = textureId;
            this.BindingIndex = 0;
            this.Width = width;
            this.Height = height;
            this.Depth = depth;
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

        #region 绑定索引 —— int BindingIndex
        /// <summary>
        /// 绑定索引
        /// </summary>
        public int BindingIndex { get; protected set; }
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

        #region 深度 —— int Depth
        /// <summary>
        /// 深度
        /// </summary>
        public int Depth { get; private set; }
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

        #region 绑定图像纹理 —— void BindImageTexture(int bindingPoint...
        /// <summary>
        /// 绑定图像纹理
        /// </summary>
        /// <param name="bindingPoint">绑定点索引（0-15，与着色器中的 layout(binding = N) 对应）</param>
        /// <param name="textureAccess">访问模式</param>
        public void BindImageTexture(int bindingPoint, TextureAccess textureAccess = TextureAccess.ReadWrite)
        {
            SizedInternalFormat sizedInternalFormat = (SizedInternalFormat)this.PixelInternalFormat;
            GL.BindImageTexture(bindingPoint, this.Id, 0, true, 0, textureAccess, sizedInternalFormat);
        }
        #endregion

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

        #region 更新纹理 —— abstract void Update(IntPtr pixels)
        /// <summary>
        /// 更新纹理
        /// </summary>
        /// <param name="pixels">像素数据</param>
        public abstract void Update(IntPtr pixels);
        #endregion

        #region 清空纹理 —— virtual void Clear()
        /// <summary>
        /// 清空纹理
        /// </summary>
        /// <remarks>将纹理全部设为0</remarks>
        public virtual void Clear()
        {
            //使用glClearTexImage清除整个纹理（OpenGL 4.4+）
            GL.ClearTexImage(this.Id, 0, this.PixelFormat, this.PixelType, IntPtr.Zero);
        }
        #endregion

        #region 释放资源 —— virtual void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            if (this.Id != 0)
            {
                GL.DeleteTexture(this.Id);
                this.Id = 0;
            }

            this._disposed = true;
        }
        #endregion

        #endregion
    }
}
