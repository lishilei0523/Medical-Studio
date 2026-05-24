using OpenTK.Graphics.OpenGL4;
using System;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 像素缓冲区(写)3D
    /// </summary>
    public class WritePixelBuffer3D : WritePixelBuffer
    {
        #region # 字段及构造器

        /// <summary>
        /// 创建像素缓冲区(写)3D构造器
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">深度</param>
        /// <param name="pixelFormat">像素格式</param>
        /// <param name="pixelType">像素类型</param>
        public WritePixelBuffer3D(int width, int height, int depth, PixelFormat pixelFormat = PixelFormat.Red, PixelType pixelType = PixelType.UnsignedByte)
            : base(width, height, depth, pixelFormat, pixelType)
        {

        }

        #endregion

        #region # 属性

        //

        #endregion

        #region # 方法

        #region 创建8位标记缓冲区 —— static WritePixelBuffer3D CreateMark8(int width, int height...
        /// <summary>
        /// 创建8位标记缓冲区
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">深度</param>
        /// <returns>像素缓冲区(写)3D</returns>
        public static WritePixelBuffer3D CreateMark8(int width, int height, int depth)
        {
            return new WritePixelBuffer3D(width, height, depth, PixelFormat.RedInteger, PixelType.UnsignedByte);
        }
        #endregion

        #region 创建16位预览缓冲区 —— static WritePixelBuffer3D CreatePreview16(int width, int height...
        /// <summary>
        /// 创建16位预览缓冲区
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">深度</param>
        /// <returns>像素缓冲区(写)3D</returns>
        public static WritePixelBuffer3D CreatePreview16(int width, int height, int depth)
        {
            return new WritePixelBuffer3D(width, height, depth, PixelFormat.Red, PixelType.Short);
        }
        #endregion

        #region 上传单个切片 —— void UploadSlice(byte[] sliceData, int sliceIndex...
        /// <summary>
        /// 上传单个切片
        /// </summary>
        /// <param name="sliceData">切片数据</param>
        /// <param name="sliceIndex">切片索引</param>
        /// <param name="useFence">是否使用栅栏</param>
        public void UploadSlice(byte[] sliceData, int sliceIndex, bool useFence = true)
        {
            #region # 验证

            if (sliceData == null)
            {
                throw new ArgumentNullException(nameof(sliceData), "切片数据不可为空！");
            }
            if (sliceData.Length != this.BufferSize)
            {
                throw new ArgumentException($"切片数据尺寸不匹配: 期望 {this.BufferSize}, 实际 {sliceData.Length}");
            }
            if (sliceIndex < 0 || sliceIndex >= this.Depth)
            {
                throw new ArgumentOutOfRangeException(nameof(sliceIndex), "切片索引超出范围！");
            }

            #endregion

            int offset = sliceIndex * this.BufferSize;

            this.Bind();

            //使用BufferSubData 更新指定范围
            GL.BufferSubData(this.BufferTarget, (IntPtr)offset, this.BufferSize, sliceData);

            if (useFence)
            {
                this.CreateFence();
            }

            this.Unbind();
        }
        #endregion

        #region 上传单个切片 —— void UploadSlice(short[] sliceData, int sliceIndex...
        /// <summary>
        /// 上传单个切片
        /// </summary>
        /// <param name="sliceData">切片数据</param>
        /// <param name="sliceIndex">切片索引</param>
        /// <param name="useFence">是否使用栅栏</param>
        public void UploadSlice(short[] sliceData, int sliceIndex, bool useFence = true)
        {
            #region # 验证

            if (sliceData == null)
            {
                throw new ArgumentNullException(nameof(sliceData), "切片数据不可为空！");
            }

            int byteSize = sliceData.Length * sizeof(short);
            if (byteSize != this.BufferSize)
            {
                throw new ArgumentException($"切片数据尺寸不匹配: 期望 {this.BufferSize}, 实际 {byteSize}");
            }
            if (sliceIndex < 0 || sliceIndex >= this.Depth)
            {
                throw new ArgumentOutOfRangeException(nameof(sliceIndex), "切片索引超出范围！");
            }

            #endregion

            int offset = sliceIndex * this.BufferSize;

            this.Bind();

            GL.BufferSubData(this.BufferTarget, (IntPtr)offset, this.BufferSize, sliceData);

            if (useFence)
            {
                this.CreateFence();
            }

            this.Unbind();
        }
        #endregion

        #region 上传单个切片 —— void UploadSlice(int[] sliceData, int sliceIndex...
        /// <summary>
        /// 上传单个切片
        /// </summary>
        /// <param name="sliceData">切片数据</param>
        /// <param name="sliceIndex">切片索引</param>
        /// <param name="useFence">是否使用栅栏</param>
        public void UploadSlice(int[] sliceData, int sliceIndex, bool useFence = true)
        {
            #region # 验证

            if (sliceData == null)
            {
                throw new ArgumentNullException(nameof(sliceData), "切片数据不可为空！");
            }

            int byteSize = sliceData.Length * sizeof(int);
            if (byteSize != this.BufferSize)
            {
                throw new ArgumentException($"切片数据尺寸不匹配: 期望 {this.BufferSize}, 实际 {byteSize}");
            }
            if (sliceIndex < 0 || sliceIndex >= this.Depth)
            {
                throw new ArgumentOutOfRangeException(nameof(sliceIndex), "切片索引超出范围！");
            }

            #endregion

            int offset = sliceIndex * this.BufferSize;

            this.Bind();

            GL.BufferSubData(this.BufferTarget, (IntPtr)offset, this.BufferSize, sliceData);

            if (useFence)
            {
                this.CreateFence();
            }

            this.Unbind();
        }
        #endregion

        #region 上传单个切片 —— void UploadSlice(float[] sliceData, int sliceIndex...
        /// <summary>
        /// 上传单个切片
        /// </summary>
        /// <param name="sliceData">切片数据</param>
        /// <param name="sliceIndex">切片索引</param>
        /// <param name="useFence">是否使用栅栏</param>
        public void UploadSlice(float[] sliceData, int sliceIndex, bool useFence = true)
        {
            #region # 验证

            if (sliceData == null)
            {
                throw new ArgumentNullException(nameof(sliceData), "切片数据不可为空！");
            }

            int byteSize = sliceData.Length * sizeof(float);
            if (byteSize != this.BufferSize)
            {
                throw new ArgumentException($"切片数据尺寸不匹配: 期望 {this.BufferSize}, 实际 {byteSize}");
            }
            if (sliceIndex < 0 || sliceIndex >= this.Depth)
            {
                throw new ArgumentOutOfRangeException(nameof(sliceIndex), "切片索引超出范围！");
            }

            #endregion

            int offset = sliceIndex * this.BufferSize;

            this.Bind();

            GL.BufferSubData(this.BufferTarget, (IntPtr)offset, this.BufferSize, sliceData);

            if (useFence)
            {
                this.CreateFence();
            }

            this.Unbind();
        }
        #endregion

        #region 上传到3D纹理 —— void UploadToTexture(Texture3D texture, bool useFence)
        /// <summary>
        /// 上传到3D纹理
        /// </summary>
        /// <param name="texture">目标纹理</param>
        /// <param name="useFence">是否使用栅栏</param>
        public void UploadToTexture(Texture3D texture, bool useFence = true)
        {
            texture.Bind();
            this.Bind();

            GL.TexSubImage3D(TextureTarget.Texture3D, 0, 0, 0, 0, this.Width, this.Height, this.Depth, this.PixelFormat, this.PixelType, IntPtr.Zero);

            if (useFence)
            {
                this.CreateFence();
            }

            this.Unbind();
            texture.Unbind();
        }
        #endregion

        #region 上传指定范围到3D纹理 —— void UploadToTextureRange(Texture3D texture, int sliceIndex...
        /// <summary>
        /// 上传指定范围到3D纹理
        /// </summary>
        /// <param name="texture">目标纹理</param>
        /// <param name="sliceIndex">切片索引</param>
        /// <param name="slicesCount">切片数量</param>
        /// <param name="useFence">是否使用栅栏</param>
        public void UploadToTextureRange(Texture3D texture, int sliceIndex, int slicesCount, bool useFence = true)
        {
            texture.Bind();
            this.Bind();

            int offset = sliceIndex * this.Width * this.Height * this.BytesPerPixel;
            GL.TexSubImage3D(TextureTarget.Texture3D, 0, 0, 0, sliceIndex, this.Width, this.Height, slicesCount, this.PixelFormat, this.PixelType, offset);

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
