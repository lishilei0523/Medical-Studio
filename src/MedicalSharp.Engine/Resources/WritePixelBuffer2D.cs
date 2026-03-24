using OpenTK.Graphics.OpenGL4;
using System;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 像素缓冲区(写)2D
    /// </summary>
    public class WritePixelBuffer2D : WritePixelBuffer
    {
        #region # 字段及构造器

        /// <summary>
        /// 创建像素缓冲区(写)2D构造器
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="pixelFormat">像素格式</param>
        /// <param name="pixelType">像素类型</param>
        public WritePixelBuffer2D(int width, int height, PixelFormat pixelFormat = PixelFormat.Rgba, PixelType pixelType = PixelType.UnsignedByte)
            : base(width, height, pixelFormat, pixelType)
        {
            this.CreateBuffer();
        }

        #endregion

        #region # 方法

        //Static

        #region 创建8位灰度缓冲区 —— static WritePixelBuffer2D CreateGray8(int width, int height)
        /// <summary>
        /// 创建8位灰度缓冲区
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <returns>像素缓冲区(写)2D</returns>
        public static WritePixelBuffer2D CreateGray8(int width, int height)
        {
            return new WritePixelBuffer2D(width, height, PixelFormat.Red, PixelType.UnsignedByte);
        }
        #endregion

        #region 创建16位灰度缓冲区 —— static WritePixelBuffer2D CreateGray16(int width, int height)
        /// <summary>
        /// 创建16位灰度缓冲区
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <returns>像素缓冲区(写)2D</returns>
        /// <remarks>医学影像常用</remarks>
        public static WritePixelBuffer2D CreateGray16(int width, int height)
        {
            return new WritePixelBuffer2D(width, height, PixelFormat.Red, PixelType.UnsignedShort);
        }
        #endregion

        #region 创建32位浮点灰度缓冲区 —— static WritePixelBuffer2D CreateGray32F(int width, int height)
        /// <summary>
        /// 创建32位浮点灰度缓冲区
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <returns>像素缓冲区(写)2D</returns>
        public static WritePixelBuffer2D CreateGray32F(int width, int height)
        {
            return new WritePixelBuffer2D(width, height, PixelFormat.Red, PixelType.Float);
        }
        #endregion

        #region 创建8位RGBA缓冲区 —— static WritePixelBuffer2D CreateRgba8(int width, int height)
        /// <summary>
        /// 创建8位RGBA缓冲区
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <returns>像素缓冲区(写)2D</returns>
        public static WritePixelBuffer2D CreateRgba8(int width, int height)
        {
            return new WritePixelBuffer2D(width, height, PixelFormat.Rgba, PixelType.UnsignedByte);
        }
        #endregion


        //Public

        #region 上传到2D纹理 —— void UploadToTexture(Texture2D texture...
        /// <summary>
        /// 上传到2D纹理
        /// </summary>
        /// <param name="texture">目标纹理</param>
        /// <param name="useFence">是否使用栅栏</param>
        public void UploadToTexture(Texture2D texture, bool useFence = true)
        {
            texture.Bind();
            this.Bind();

            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, this.Width, this.Height, this.PixelFormat, this.PixelType, IntPtr.Zero);

            if (useFence)
            {
                this.CreateFence();
            }

            this.Unbind();
            texture.Unbind();
        }
        #endregion

        #region 上传到3D纹理 —— void UploadToTextureSlice(Texture3D texture, int sliceIndex...
        /// <summary>
        /// 上传到3D纹理
        /// </summary>
        /// <param name="texture">目标纹理</param>
        /// <param name="sliceIndex">切片索引</param>
        /// <param name="useFence">是否使用栅栏</param>
        public void UploadToTextureSlice(Texture3D texture, int sliceIndex, bool useFence = true)
        {
            texture.Bind();
            this.Bind();

            GL.TexSubImage3D(TextureTarget.Texture3D, 0, 0, 0, sliceIndex, this.Width, this.Height, 1, this.PixelFormat, this.PixelType, IntPtr.Zero);

            if (useFence)
            {
                this.CreateFence();
            }

            this.Unbind();
            texture.Unbind();
        }
        #endregion

        #endregion
    }
}
