using OpenTK.Graphics.OpenGL4;
using System;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 1D纹理
    /// </summary>
    public class Texture1D : Texture
    {
        #region # 字段及构造器

        /// <summary>
        /// 创建1D纹理构造器
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="pixelInternalFormat">像素内部格式</param>
        public Texture1D(int width, PixelInternalFormat pixelInternalFormat = PixelInternalFormat.Rgba32f)
            : base()
        {
            this.Width = width;

            //绑定纹理
            GL.BindTexture(TextureTarget.Texture1D, base.Id);

            //上传纹理至显存
            GL.TexImage1D(TextureTarget.Texture1D, 0, pixelInternalFormat, this.Width, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);

            //设置参数
            GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            //解绑
            GL.BindTexture(TextureTarget.Texture1D, 0);
        }

        #endregion

        #region # 属性

        #region 宽度 —— int Width
        /// <summary>
        /// 宽度
        /// </summary>
        public int Width { get; private set; }
        #endregion 

        #endregion

        #region # 方法

        #region 更新纹理 —— void Update(PixelFormat pixelFormat, PixelType pixelType...
        /// <summary>
        /// 更新纹理
        /// </summary>
        /// <param name="pixelFormat">像素格式</param>
        /// <param name="pixelType">像素类型</param>
        /// <param name="data">纹理数据</param>
        public void Update(PixelFormat pixelFormat, PixelType pixelType, IntPtr data)
        {
            if (data == IntPtr.Zero)
            {
                return;
            }

            GL.BindTexture(TextureTarget.Texture1D, base.Id);
            GL.TexSubImage1D(TextureTarget.Texture1D, 0, 0, this.Width, pixelFormat, pixelType, data);
            GL.BindTexture(TextureTarget.Texture1D, 0);
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
            GL.BindTexture(TextureTarget.Texture1D, base.Id);
            GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureMagFilter, (int)magFilter);
            GL.BindTexture(TextureTarget.Texture1D, 0);
        }
        #endregion

        #region 设置包裹模式 —— override void SetWrapMode(TextureWrapMode wrapMode)
        /// <summary>
        /// 设置包裹模式
        /// </summary>
        /// <param name="wrapMode">包裹模式</param>
        public override void SetWrapMode(TextureWrapMode wrapMode)
        {
            GL.BindTexture(TextureTarget.Texture1D, base.Id);
            GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureWrapS, (int)wrapMode);
            GL.BindTexture(TextureTarget.Texture1D, 0);
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
            GL.BindTexture(TextureTarget.Texture1D, base.Id);
        }
        #endregion

        #region 绑定纹理 —— override void Bind()
        /// <summary>
        /// 绑定纹理
        /// </summary>
        public override void Bind()
        {
            GL.BindTexture(TextureTarget.Texture1D, base.Id);
        }
        #endregion

        #region 解绑纹理 —— override void Unbind()
        /// <summary>
        /// 解绑纹理
        /// </summary>
        public override void Unbind()
        {
            GL.BindTexture(TextureTarget.Texture1D, 0);
        }
        #endregion

        #endregion
    }
}
