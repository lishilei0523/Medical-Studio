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
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">深度</param>
        /// <param name="pixelInternalFormat">像素内部格式</param>
        /// <param name="pixelFormat">像素格式</param>
        /// <param name="pixelType">像素类型</param>
        public Texture3D(int width, int height, int depth, PixelInternalFormat pixelInternalFormat = PixelInternalFormat.Rgba32f, PixelFormat pixelFormat = PixelFormat.Rgba, PixelType pixelType = PixelType.Float)
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
            if (depth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(depth), "深度必须大于0！");
            }

            #endregion

            this.Width = width;
            this.Height = height;
            this.Depth = depth;
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

        #region # 方法

        //Static

        #region 从体数据创建纹理 —— static Texture3D CreateFromVolume(int width, int height...
        /// <summary>
        /// 从体数据创建纹理
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">深度</param>
        /// <param name="originalData">原始数据</param>
        public static Texture3D CreateFromVolume(int width, int height, int depth, IntPtr originalData)
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

            Texture3D texture = new Texture3D(width, height, depth, PixelInternalFormat.R16Snorm, PixelFormat.Red, PixelType.Short);

            //分配显存
            texture.AllocateMemory(originalData);

            //设置默认纹理参数
            texture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
            texture.SetWrapMode(TextureWrapMode.ClampToEdge);

            //检查错误
            CheckError("GL.TexImage3D");

            return texture;
        }
        #endregion


        //Public

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

        #region 绑定纹理 —— override void Bind()
        /// <summary>
        /// 绑定纹理
        /// </summary>
        public override void Bind()
        {
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

        #region 分配内存 —— override void AllocateMemory()
        /// <summary>
        /// 分配内存
        /// </summary>
        public override void AllocateMemory()
        {
            this.Bind();

            GL.TexImage3D(TextureTarget.Texture3D, 0, this.PixelInternalFormat, this.Width, this.Height, this.Depth, 0, this.PixelFormat, this.PixelType, IntPtr.Zero);

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

            GL.TexImage3D(TextureTarget.Texture3D, 0, this.PixelInternalFormat, this.Width, this.Height, this.Depth, 0, this.PixelFormat, this.PixelType, pixels);

            this.Unbind();
        }
        #endregion

        #region 更新纹理 —— override void Update(IntPtr pixels)
        /// <summary>
        /// 更新纹理
        /// </summary>
        /// <param name="pixels">像素数据</param>
        public override void Update(IntPtr pixels)
        {
            #region # 验证

            if (pixels == IntPtr.Zero)
            {
                return;
            }

            #endregion

            this.Bind();

            GL.TexSubImage3D(TextureTarget.Texture3D, 0, 0, 0, 0, this.Width, this.Height, this.Depth, this.PixelFormat, this.PixelType, pixels);

            this.Unbind();
        }
        #endregion

        #region 更新纹理切片 —— void UpdateSlice(int sliceIndex, IntPtr pixels)
        /// <summary>
        /// 更新纹理切片
        /// </summary>
        /// <param name="sliceIndex">切片索引</param>
        /// <param name="pixels">像素数据</param>
        public void UpdateSlice(int sliceIndex, IntPtr pixels)
        {
            #region # 验证

            if (pixels == IntPtr.Zero)
            {
                return;
            }
            if (sliceIndex < 0 || sliceIndex >= this.Depth)
            {
                throw new ArgumentOutOfRangeException(nameof(sliceIndex), $"切片索引超出范围[0,{this.Depth - 1}]！");
            }

            #endregion

            this.Bind();

            GL.TexSubImage3D(TextureTarget.Texture3D, 0, 0, 0, sliceIndex, this.Width, this.Height, 1, this.PixelFormat, this.PixelType, pixels);

            this.Unbind();
        }
        #endregion

        #region 更新纹理范围 —— void UpdateRange(int sliceIndex, int slicesCount...
        /// <summary>
        /// 更新纹理范围
        /// </summary>
        /// <param name="sliceIndex">切片索引</param>
        /// <param name="slicesCount">切片数量</param>
        /// <param name="pixels">像素数据</param>
        public void UpdateRange(int sliceIndex, int slicesCount, IntPtr pixels)
        {
            #region # 验证

            if (pixels == IntPtr.Zero)
            {
                return;
            }
            if (sliceIndex < 0 || sliceIndex + slicesCount > this.Depth)
            {
                throw new InvalidOperationException($"切片索引+切片数量超出范围[0,{this.Depth - 1}]！");
            }

            #endregion

            this.Bind();

            GL.TexSubImage3D(TextureTarget.Texture3D, 0, 0, 0, sliceIndex, this.Width, this.Height, slicesCount, this.PixelFormat, this.PixelType, pixels);

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

            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMinFilter, (int)minFilter);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMagFilter, (int)magFilter);

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

            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapS, (int)wrapMode);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapT, (int)wrapMode);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapR, (int)wrapMode);

            this.Unbind();
        }
        #endregion

        #endregion
    }
}
