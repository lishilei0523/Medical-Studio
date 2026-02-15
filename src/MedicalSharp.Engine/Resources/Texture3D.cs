using OpenTK.Graphics.OpenGL4;
using System;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 3D纹理
    /// </summary>
    public class Texture3D : Texture
    {
        #region # 字段及构造器

        /// <summary>
        /// 创建3D纹理构造器
        /// </summary>
        public Texture3D()
            : base()
        {

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

        #region 深度 —— int Depth
        /// <summary>
        /// 深度
        /// </summary>
        public int Depth { get; private set; }
        #endregion

        #endregion

        #region 从体数据创建纹理 —— void CreateFromVolume(int width, int height...
        /// <summary>
        /// 从体数据创建纹理
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">深度</param>
        /// <param name="originalData">原始数据</param>
        public void CreateFromVolume(int width, int height, int depth, IntPtr originalData)
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
            if (depth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(depth), "深度必须大于0！");
            }

            #endregion

            this.Width = width;
            this.Height = height;
            this.Depth = height;

            //绑定纹理
            GL.BindTexture(TextureTarget.ProxyTexture3D, base.Id);

            //设置默认纹理参数
            GL.TexParameter(TextureTarget.ProxyTexture3D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.ProxyTexture3D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.ProxyTexture3D, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.ProxyTexture3D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.ProxyTexture3D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            //分配纹理显存
            GL.TexImage3D(TextureTarget.ProxyTexture3D, 0, PixelInternalFormat.R16Snorm, this.Width, this.Height, this.Depth, 0, PixelFormat.Red, PixelType.Short, originalData);

            //检查错误
            CheckError("GL.TexImage3D");

            //解绑纹理
            GL.BindTexture(TextureTarget.ProxyTexture3D, 0);
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
            GL.BindTexture(TextureTarget.Texture3D, base.Id);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMagFilter, (int)magFilter);
            GL.BindTexture(TextureTarget.Texture3D, 0);
        }
        #endregion

        #region 设置包裹模式 —— override void SetWrapMode(TextureWrapMode wrapMode)
        /// <summary>
        /// 设置包裹模式
        /// </summary>
        /// <param name="wrapMode">包裹模式</param>
        public override void SetWrapMode(TextureWrapMode wrapMode)
        {
            GL.BindTexture(TextureTarget.Texture3D, base.Id);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapS, (int)wrapMode);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapT, (int)wrapMode);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapR, (int)wrapMode);
            GL.BindTexture(TextureTarget.Texture3D, 0);
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
            GL.BindTexture(TextureTarget.Texture3D, base.Id);
        }
        #endregion

        #region 解绑纹理 —— override void Unbind()
        /// <summary>
        /// 解绑纹理
        /// </summary>
        public override void Unbind()
        {
            GL.BindTexture(TextureTarget.Texture3D, 0);
        }
        #endregion
    }
}
