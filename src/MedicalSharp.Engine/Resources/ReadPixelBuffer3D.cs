using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.InteropServices;

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
            : base(width, height, pixelFormat, pixelType)
        {
            this.Depth = depth;
            this.TotalBufferSize = this.BufferSize * depth;

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

        #region 获取CPU数据 —— byte[] GetCpuBuffer(long timeoutNanoseconds)
        /// <summary>
        /// 获取CPU数据（全量）
        /// </summary>
        /// <param name="timeoutNanoseconds">超时时间（纳秒），-1 表示无限等待</param>
        /// <returns>缓冲区数据</returns>
        public byte[] GetCpuBuffer(long timeoutNanoseconds = -1)
        {
            base.WaitForFence(timeoutNanoseconds);
            byte[] data = this.ReadImmediately();

            return data;
        }
        #endregion

        #region 非阻塞获取CPU数据 —— bool TryGetCpuData(out byte[] data)
        /// <summary>
        /// 非阻塞获取CPU数据
        /// </summary>
        /// <param name="data">输出数据</param>
        /// <returns>是否成功获取数据</returns>
        public bool TryGetCpuData(out byte[] data)
        {
            data = null;
            if (!base.IsDataReady)
            {
                return false;
            }

            data = this.ReadImmediately();

            return data != null;
        }
        #endregion

        #region 立即读取数据 —— byte[] ReadImmediately()
        /// <summary>
        /// 立即读取数据
        /// </summary>
        private byte[] ReadImmediately()
        {
            this.Bind();
            try
            {
                IntPtr ptr = GL.MapBuffer(this.BufferTarget, BufferAccess.ReadOnly);
                if (ptr == IntPtr.Zero)
                {
                    return null;
                }

                //使用TotalBufferSize而不是BufferSize
                byte[] data = new byte[this.TotalBufferSize];
                Marshal.Copy(ptr, data, 0, this.TotalBufferSize);
                return data;
            }
            finally
            {
                GL.UnmapBuffer(this.BufferTarget);
                this.Unbind();
            }
        }
        #endregion

        #endregion
    }
}
