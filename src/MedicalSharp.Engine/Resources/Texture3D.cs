using MedicalSharp.Engine.Base;
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
            : base(width, height, depth, pixelInternalFormat, pixelFormat, pixelType)
        {

        }

        #endregion

        #region # 属性

        //

        #endregion

        #region # 方法

        //Static

        #region 从体积数据创建纹理 —— static Texture3D CreateFromVolume(int width, int height...
        /// <summary>
        /// 从体积数据创建纹理
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
            GlException.ThrowOnError(nameof(CreateFromVolume));

            return texture;
        }
        #endregion

        #region 从标记数据创建纹理 —— static Texture3D CreateFromMark(int width, int height...
        /// <summary>
        /// 从标记数据创建纹理
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">深度</param>
        /// <param name="markData">标记数据</param>
        public static Texture3D CreateFromMark(int width, int height, int depth, IntPtr markData)
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

            Texture3D texture = new Texture3D(width, height, depth, PixelInternalFormat.R8ui, PixelFormat.RedInteger, PixelType.UnsignedByte);

            //分配显存
            texture.AllocateMemory(markData);

            //设置默认纹理参数
            texture.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            texture.SetWrapMode(TextureWrapMode.ClampToEdge);

            //检查错误
            GlException.ThrowOnError(nameof(CreateFromMark));

            return texture;
        }
        #endregion

        #region 创建纹理副本 —— static Texture3D CreateCopy(Texture3D sourceTexture)
        /// <summary>
        /// 创建纹理副本
        /// </summary>
        /// <param name="sourceTexture">源纹理</param>
        /// <returns>副本纹理</returns>
        public static Texture3D CreateCopy(Texture3D sourceTexture)
        {
            #region # 验证

            if (sourceTexture == null)
            {
                throw new ArgumentNullException(nameof(sourceTexture), "源标记纹理不可为空！");
            }

            #endregion

            int width = sourceTexture.Width;
            int height = sourceTexture.Height;
            int depth = sourceTexture.Depth;

            //创建新纹理
            Texture3D copyTexture = new Texture3D(width, height, depth, sourceTexture.PixelInternalFormat, sourceTexture.PixelFormat, sourceTexture.PixelType);
            copyTexture.AllocateMemory(IntPtr.Zero);

            //设置默认纹理参数
            copyTexture.SetFilter(sourceTexture.MinFilter, sourceTexture.MagFilter);
            copyTexture.SetWrapMode(sourceTexture.WrapMode);

            //拷贝源纹理到新纹理
            GL.CopyImageSubData(sourceTexture.Id, ImageTarget.Texture3D, 0, 0, 0, 0, copyTexture.Id, ImageTarget.Texture3D, 0, 0, 0, 0, width, height, depth);

            //检查错误
            GlException.ThrowOnError(nameof(CreateCopy));

            return copyTexture;
        }
        #endregion

        #region 复制纹理 —— static void CopyData(Texture3D source, Texture3D target)
        /// <summary>
        /// 复制纹理
        /// </summary>
        /// <param name="source">源纹理</param>
        /// <param name="target">目标纹理</param>
        public static void Copy(Texture3D source, Texture3D target)
        {
            #region # 验证

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "源纹理不可为空！");
            }
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target), "目标纹理不可为空！");
            }
            if (source.Width != target.Width || source.Height != target.Height || source.Depth != target.Depth)
            {
                throw new ArgumentException("源纹理和目标纹理尺寸必须相同！");
            }

            #endregion

            GL.CopyImageSubData(source.Id, ImageTarget.Texture3D, 0, 0, 0, 0, target.Id, ImageTarget.Texture3D, 0, 0, 0, 0, source.Width, source.Height, source.Depth);

            //检查错误
            GlException.ThrowOnError(nameof(Copy));
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
            this.BindingIndex = index;
            GL.ActiveTexture(TextureUnit.Texture0 + this.BindingIndex);
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
            GL.ActiveTexture(TextureUnit.Texture0 + this.BindingIndex);
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

        #region 设置过滤器 —— override void SetFilter(TextureMinFilter minFilter...
        /// <summary>
        /// 设置过滤器
        /// </summary>
        /// <param name="minFilter">最小值过滤器</param>
        /// <param name="magFilter">最大值过滤器</param>
        public override void SetFilter(TextureMinFilter minFilter, TextureMagFilter magFilter)
        {
            base.SetFilter(minFilter, magFilter);

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
            base.SetWrapMode(wrapMode);

            this.Bind();

            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapS, (int)wrapMode);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapT, (int)wrapMode);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapR, (int)wrapMode);

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

        #endregion
    }
}
