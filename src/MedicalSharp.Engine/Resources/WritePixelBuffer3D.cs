using Microsoft.CSharp.RuntimeBinder;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.InteropServices;

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
            : base(width, height, pixelFormat, pixelType)
        {
            this.Depth = depth;
            this.TotalBufferSize = this.BufferSize * this.Depth;
            this.Id = GL.GenBuffer();

            #region # 验证

            if (this.Id == 0)
            {
                throw new RuntimeBinderException("创建像素缓冲区失败！");
            }

            #endregion

            //分配3D大小的缓冲区
            this.Bind();
            GL.BufferData(base.BufferTarget, this.TotalBufferSize, IntPtr.Zero, base.BufferUsage);
            this.Unbind();
        }

        #endregion

        #region # 属性

        #region 深度 —— int Depth
        /// <summary>
        /// 深度
        /// </summary>
        public int Depth { get; private set; }
        #endregion

        #region 总缓冲区尺寸 —— int TotalBufferSize
        /// <summary>
        /// 总缓冲区尺寸
        /// </summary>
        public int TotalBufferSize { get; private set; }
        #endregion

        #endregion

        #region # 方法

        //Static

        #region 创建8位灰度缓冲区 —— static WritePixelBuffer3D CreateGray8(int width, int height...
        /// <summary>
        /// 创建8位灰度缓冲区
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">深度</param>
        /// <returns>像素缓冲区(写)3D</returns>
        public static WritePixelBuffer3D CreateGray8(int width, int height, int depth)
        {
            return new WritePixelBuffer3D(width, height, depth, PixelFormat.Red, PixelType.UnsignedByte);
        }
        #endregion

        #region 创建16位灰度缓冲区 —— static WritePixelBuffer3D CreateGray16(int width, int height...
        /// <summary>
        /// 创建16位灰度缓冲区
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">深度</param>
        /// <returns>像素缓冲区(写)3D</returns>
        /// <remarks>CT/MRI 常用</remarks>
        public static WritePixelBuffer3D CreateGray16(int width, int height, int depth)
        {
            return new WritePixelBuffer3D(width, height, depth, PixelFormat.Red, PixelType.UnsignedShort);
        }
        #endregion

        #region 创建32位浮点灰度缓冲区 —— static WritePixelBuffer3D CreateGray32F(int width, int height...
        /// <summary>
        /// 创建32位浮点灰度缓冲区
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">深度</param>
        /// <returns>像素缓冲区(写)3D</returns>
        public static WritePixelBuffer3D CreateGray32F(int width, int height, int depth)
        {
            return new WritePixelBuffer3D(width, height, depth, PixelFormat.Red, PixelType.Float);
        }
        #endregion


        //Public

        #region 上传byte数组 —— override void UploadData(byte[] data)
        /// <summary>
        /// 上传byte数组
        /// </summary>
        public override void UploadData(byte[] data)
        {
            #region # 验证

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "数据不可为空！");
            }
            if (data.Length != this.TotalBufferSize)
            {
                throw new ArgumentOutOfRangeException($"数据尺寸\"{data.Length}\"与缓冲区尺寸\"{this.TotalBufferSize}\"不匹配");
            }

            #endregion


            this.Bind();

            IntPtr ptr = GL.MapBuffer(this.BufferTarget, BufferAccess.WriteOnly);
            if (ptr != IntPtr.Zero)
            {
                Marshal.Copy(data, 0, ptr, data.Length);
                GL.UnmapBuffer(this.BufferTarget);
            }

            this.Unbind();
        }
        #endregion

        #region 上传short数组 —— override void UploadData(short[] data)
        /// <summary>
        /// 上传short数组
        /// </summary>
        public override void UploadData(short[] data)
        {
            #region # 验证

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "数据不可为空！");
            }

            int byteSize = data.Length * sizeof(short);
            if (byteSize != this.TotalBufferSize)
            {
                throw new ArgumentOutOfRangeException($"数据尺寸\"{byteSize}\"与缓冲区尺寸\"{this.TotalBufferSize}\"不匹配");
            }

            #endregion

            this.Bind();

            IntPtr ptr = GL.MapBuffer(this.BufferTarget, BufferAccess.WriteOnly);
            if (ptr != IntPtr.Zero)
            {
                Marshal.Copy(data, 0, ptr, data.Length);
                GL.UnmapBuffer(this.BufferTarget);
            }

            this.Unbind();
        }
        #endregion

        #region 上传int数组 —— override void UploadData(int[] data)
        /// <summary>
        /// 上传int数组
        /// </summary>
        public override void UploadData(int[] data)
        {
            #region # 验证

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "数据不可为空！");
            }

            int byteSize = data.Length * sizeof(int);
            if (byteSize != this.TotalBufferSize)
            {
                throw new ArgumentOutOfRangeException($"数据尺寸\"{byteSize}\"与缓冲区尺寸\"{this.TotalBufferSize}\"不匹配");
            }

            #endregion

            this.Bind();

            IntPtr ptr = GL.MapBuffer(this.BufferTarget, BufferAccess.WriteOnly);
            if (ptr != IntPtr.Zero)
            {
                Marshal.Copy(data, 0, ptr, data.Length);
                GL.UnmapBuffer(this.BufferTarget);
            }

            this.Unbind();
        }
        #endregion

        #region 上传float数组 —— override void UploadData(float[] data)
        /// <summary>
        /// 上传float数组
        /// </summary>
        public override void UploadData(float[] data)
        {
            #region # 验证

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "数据不可为空！");
            }

            int byteSize = data.Length * sizeof(float);
            if (byteSize != this.TotalBufferSize)
            {
                throw new ArgumentOutOfRangeException($"数据尺寸\"{byteSize}\"与缓冲区尺寸\"{this.TotalBufferSize}\"不匹配");
            }

            #endregion

            this.Bind();

            IntPtr ptr = GL.MapBuffer(this.BufferTarget, BufferAccess.WriteOnly);
            if (ptr != IntPtr.Zero)
            {
                Marshal.Copy(data, 0, ptr, data.Length);
                GL.UnmapBuffer(this.BufferTarget);
            }

            this.Unbind();
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

            GL.TexSubImage3D(TextureTarget.Texture3D, 0, 0, 0, sliceIndex, this.Width, this.Height, slicesCount, this.PixelFormat, this.PixelType, (IntPtr)(sliceIndex * this.BufferSize));

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
