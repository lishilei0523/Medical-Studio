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
        public Texture2D(int width, int height, PixelInternalFormat pixelInternalFormat = PixelInternalFormat.Rgba32f)
            : base()
        {
            this.Width = width;
            this.Height = height;

            //绑定纹理
            GL.BindTexture(TextureTarget.Texture2D, base.Id);

            //上传纹理至显存
            GL.TexImage2D(TextureTarget.Texture2D, 0, pixelInternalFormat, this.Width, this.Height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

            //设置参数
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            //解绑
            GL.BindTexture(TextureTarget.Texture2D, 0);
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

        #region 设置过滤器 —— override void SetFilter(TextureMinFilter minFilter...
        /// <summary>
        /// 设置过滤器
        /// </summary>
        /// <param name="minFilter">最小值过滤器</param>
        /// <param name="magFilter">最大值过滤器</param>
        public override void SetFilter(TextureMinFilter minFilter, TextureMagFilter magFilter)
        {
            GL.BindTexture(TextureTarget.Texture2D, base.Id);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
        #endregion

        #region 设置包裹模式 —— override void SetWrapMode(TextureWrapMode wrapMode)
        /// <summary>
        /// 设置包裹模式
        /// </summary>
        /// <param name="wrapMode">包裹模式</param>
        public override void SetWrapMode(TextureWrapMode wrapMode)
        {
            GL.BindTexture(TextureTarget.Texture2D, base.Id);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrapMode);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
        #endregion

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

        #endregion
    }
}
