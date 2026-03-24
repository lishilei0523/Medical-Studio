using OpenTK.Graphics.OpenGL4;
using System;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 2D纹理
    /// </summary>
    public class Texture2D : Texture
    {
        #region # 字段及构造器

        /// <summary>
        /// 创建2D纹理构造器
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="pixelInternalFormat">像素内部格式</param>
        /// <param name="pixelFormat">像素格式</param>
        /// <param name="pixelType">像素类型</param>
        public Texture2D(int width, int height, PixelInternalFormat pixelInternalFormat = PixelInternalFormat.Rgba8, PixelFormat pixelFormat = PixelFormat.Rgba, PixelType pixelType = PixelType.UnsignedByte)
            : base(pixelInternalFormat, pixelFormat, pixelType)
        {
            #region # 验证

            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "宽度必须大于0！");
            }
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "高度必须大于0！");
            }

            #endregion

            this.Width = width;
            this.Height = height;

            //分配显存
            this.AllocateMemory();

            //设置参数
            this.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Linear);
            this.SetWrapMode(TextureWrapMode.Repeat);
        }

        #endregion

        #region # 属性

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

        #endregion

        #region # 方法

        #region 绑定纹理 —— override void Bind(int index)
        /// <summary>
        /// 绑定纹理
        /// </summary>
        /// <param name="index">纹理索引</param>
        public override void Bind(int index)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + index);
            GL.BindTexture(TextureTarget.Texture2D, base.Id);
        }
        #endregion

        #region 绑定纹理 —— override void Bind()
        /// <summary>
        /// 绑定纹理
        /// </summary>
        public override void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, base.Id);
        }
        #endregion

        #region 解绑纹理 —— override void Unbind()
        /// <summary>
        /// 解绑纹理
        /// </summary>
        public override void Unbind()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
        #endregion 

        #region 分配内存 —— override void AllocateMemory()
        /// <summary>
        /// 分配内存
        /// </summary>
        public override void AllocateMemory()
        {
            this.Bind();

            GL.TexImage2D(TextureTarget.Texture2D, 0, this.PixelInternalFormat, this.Width, this.Height, 0, this.PixelFormat, this.PixelType, IntPtr.Zero);

            this.Unbind();
        }
        #endregion

        #region 分配内存 —— override void AllocateMemory(IntPtr pixels)
        /// <summary>
        /// 分配内存
        /// </summary>
        /// <param name="pixels">像素数据</param>
        public override void AllocateMemory(IntPtr pixels)
        {
            this.Bind();

            GL.TexImage2D(TextureTarget.Texture2D, 0, this.PixelInternalFormat, this.Width, this.Height, 0, this.PixelFormat, this.PixelType, pixels);

            this.Unbind();
        }
        #endregion

        #region 设置过滤器 —— override void SetFilter(TextureMinFilter minFilter...
        /// <summary>
        /// 设置过滤器
        /// </summary>
        /// <param name="minFilter">最小值过滤器</param>
        /// <param name="magFilter">最大值过滤器</param>
        public override void SetFilter(TextureMinFilter minFilter, TextureMagFilter magFilter)
        {
            this.Bind();

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);

            this.Unbind();
        }
        #endregion

        #region 设置包裹模式 —— override void SetWrapMode(TextureWrapMode wrapMode)
        /// <summary>
        /// 设置包裹模式
        /// </summary>
        /// <param name="wrapMode">包裹模式</param>
        public override void SetWrapMode(TextureWrapMode wrapMode)
        {
            this.Bind();

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrapMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrapMode);

            this.Unbind();
        }
        #endregion

        #endregion
    }
}
