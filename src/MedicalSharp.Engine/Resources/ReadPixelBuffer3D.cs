using OpenTK.Graphics.OpenGL4;
using System;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 像素缓冲区(读)3D
    /// </summary>
    /// <remarks>GPU -> CPU</remarks>
    public class ReadPixelBuffer3D : ReadPixelBuffer
    {
        #region # 字段及构造器

        /// <summary>
        /// 创建像素缓冲区(读)3D构造器
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">深度</param>
        /// <param name="pixelFormat">像素格式</param>
        /// <param name="pixelType">像素类型</param>
        public ReadPixelBuffer3D(int width, int height, int depth, PixelFormat pixelFormat = PixelFormat.RedInteger, PixelType pixelType = PixelType.UnsignedByte)
            : base(width, height, depth, pixelFormat, pixelType)
        {

        }

        #endregion

        #region # 属性

        //

        #endregion

        #region # 方法

        #region 创建8位标记缓冲区 —— static ReadPixelBuffer3D CreateMark8(int width, int height...
        /// <summary>
        /// 创建8位标记缓冲区
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">深度</param>
        /// <returns>像素缓冲区(读)3D</returns>
        public static ReadPixelBuffer3D CreateMark8(int width, int height, int depth)
        {
            return new ReadPixelBuffer3D(width, height, depth, PixelFormat.RedInteger, PixelType.UnsignedByte);
        }
        #endregion

        #region 创建16位预览缓冲区 —— static ReadPixelBuffer3D CreatePreview16(int width, int height...
        /// <summary>
        /// 创建16位预览缓冲区
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">深度</param>
        /// <returns>像素缓冲区(读)3D</returns>
        public static ReadPixelBuffer3D CreatePreview16(int width, int height, int depth)
        {
            return new ReadPixelBuffer3D(width, height, depth, PixelFormat.Red, PixelType.Short);
        }
        #endregion

        #region 读取3D纹理 —— void ReadTexture3D(Texture3D texture, bool useFence)
        /// <summary>
        /// 读取3D纹理
        /// </summary>
        /// <param name="texture">3D纹理</param>
        /// <param name="useFence">是否使用栅栏同步</param>
        public void ReadTexture3D(Texture3D texture, bool useFence = true)
        {
            texture.Bind();
            this.Bind();

            GL.GetTexImage(TextureTarget.Texture3D, 0, this.PixelFormat, this.PixelType, IntPtr.Zero);

            //确保PBO写入完成
            GL.MemoryBarrier(MemoryBarrierFlags.PixelBufferBarrierBit);

            //创建栅栏标记读取完成
            if (useFence)
            {
                this.CreateFence();
            }

            this.Unbind();
            texture.Unbind();
        }
        #endregion

        #region 读取3D纹理 —— void ReadTexture3D(Texture3D texture, int sliceIndex, bool useFence)
        /// <summary>
        /// 读取3D纹理
        /// </summary>
        /// <param name="texture">3D纹理</param>
        /// <param name="sliceIndex">切片索引</param>
        /// <param name="useFence">是否使用栅栏同步</param>
        public void ReadTexture3D(Texture3D texture, int sliceIndex, bool useFence = true)
        {
            texture.Bind();
            this.Bind();

            GL.GetTexImage(TextureTarget.Texture3D, sliceIndex, this.PixelFormat, this.PixelType, IntPtr.Zero);

            //确保PBO写入完成
            GL.MemoryBarrier(MemoryBarrierFlags.PixelBufferBarrierBit);

            //创建栅栏标记读取完成
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
