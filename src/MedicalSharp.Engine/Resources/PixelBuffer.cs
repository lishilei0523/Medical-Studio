using Microsoft.CSharp.RuntimeBinder;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.InteropServices;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 像素缓冲区
    /// </summary>
    public class PixelBuffer : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 栅栏同步对象
        /// </summary>
        private IntPtr _fenceSync;

        /// <summary>
        /// 创建像素缓冲区构造器
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="pixelFormat">像素格式</param>
        public PixelBuffer(int width, int height, PixelFormat pixelFormat = PixelFormat.Rgba)
        {
            this.Width = width;
            this.Height = height;
            this.PixelFormat = pixelFormat;
            this.BufferSize = this.CalculateBufferSize(width, height, pixelFormat);

            int pixelBufferId = GL.GenBuffer();

            #region # 验证

            if (pixelBufferId == 0)
            {
                throw new RuntimeBinderException("创建像素缓冲区失败！");
            }

            #endregion

            this.Id = pixelBufferId;

            //分配显存
            this.Bind();
            GL.BufferData(BufferTarget.PixelPackBuffer, this.BufferSize, IntPtr.Zero, BufferUsageHint.StreamRead);
            this.Unbind();

            //设置对齐方式（重要！）
            GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
        }

        #endregion

        #region # 属性

        #region 像素缓冲区Id —— int Id
        /// <summary>
        /// 像素缓冲区Id
        /// </summary>
        public int Id { get; private set; }
        #endregion

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

        #region 像素格式 —— PixelFormat PixelFormat
        /// <summary>
        /// 像素格式
        /// </summary>
        public PixelFormat PixelFormat { get; private set; }
        #endregion

        #region 缓冲区尺寸 —— int BufferSize
        /// <summary>
        /// 缓冲区尺寸
        /// </summary>
        /// <remarks>单位: 字节</remarks>
        public int BufferSize { get; private set; }
        #endregion

        #region 只读属性 - 数据是否就绪 —— bool IsDataReady
        /// <summary>
        /// 只读属性 - 数据是否就绪
        /// </summary>
        public bool IsDataReady
        {
            get
            {
                if (this._fenceSync == IntPtr.Zero)
                {
                    return false;
                }

                WaitSyncStatus status = GL.ClientWaitSync(this._fenceSync, ClientWaitSyncFlags.None, 0);

                return status == WaitSyncStatus.AlreadySignaled || status == WaitSyncStatus.ConditionSatisfied;
            }
        }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 绑定像素缓冲区 —— void Bind()
        /// <summary>
        /// 绑定像素缓冲区
        /// </summary>
        public void Bind()
        {
            GL.BindBuffer(BufferTarget.PixelPackBuffer, this.Id);
        }
        #endregion

        #region 解绑像素缓冲区 —— void Unbind()
        /// <summary>
        /// 解绑像素缓冲区
        /// </summary>
        public void Unbind()
        {
            GL.BindBuffer(BufferTarget.PixelPackBuffer, 0);
        }
        #endregion

        #region 读取帧缓冲区 —— void ReadFrameBuffer(FrameBuffer frameBuffer...
        /// <summary>
        /// 读取帧缓冲区
        /// </summary>
        /// <param name="frameBuffer">帧缓冲区</param>
        /// <param name="useFence">是否使用栅栏同步</param>
        public void ReadFrameBuffer(FrameBuffer frameBuffer, bool useFence = true)
        {
            frameBuffer.Bind();

            this.Bind();

            GL.ReadPixels(0, 0, this.Width, this.Height, this.PixelFormat, PixelType.UnsignedByte, IntPtr.Zero);

            //确保PBO写入完成（可选）
            GL.MemoryBarrier(MemoryBarrierFlags.PixelBufferBarrierBit);

            //创建栅栏标记读取完成（推荐，用于非阻塞检查）
            if (useFence)
            {
                if (this._fenceSync != IntPtr.Zero)
                {
                    GL.DeleteSync(this._fenceSync);
                }

                this._fenceSync = GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, WaitSyncFlags.None);
            }

            this.Unbind();

            frameBuffer.Unbind();
        }
        #endregion

        #region 读取2D纹理 —— void ReadTexture2D(Texture2D texture)
        /// <summary>
        /// 读取2D纹理
        /// </summary>
        /// <param name="texture">2D纹理</param>
        /// <param name="useFence">是否使用栅栏同步</param>
        public void ReadTexture2D(Texture2D texture, bool useFence = true)
        {
            texture.Bind();
            this.Bind();

            GL.GetTexImage(TextureTarget.Texture2D, 0, this.PixelFormat, PixelType.UnsignedByte, IntPtr.Zero);

            //确保PBO写入完成
            GL.MemoryBarrier(MemoryBarrierFlags.PixelBufferBarrierBit);

            //创建栅栏标记读取完成
            if (useFence)
            {
                if (this._fenceSync != IntPtr.Zero)
                {
                    GL.DeleteSync(this._fenceSync);
                }

                this._fenceSync = GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, WaitSyncFlags.None);
            }

            this.Unbind();
            texture.Unbind();
        }
        #endregion

        #region 获取CPU数据 —— byte[] GetCpuBuffer()
        /// <summary>
        /// 获取CPU数据
        /// </summary>
        /// <param name="timeoutNanoseconds">超时时间（纳秒），-1 表示无限等待</param>
        /// <remarks>会在数据传输完成时返回</remarks>
        public byte[] GetCpuBuffer(long timeoutNanoseconds = -1)
        {
            //如果有栅栏，先等待数据就绪
            if (this._fenceSync != 0)
            {
                ClientWaitSyncFlags flags = ClientWaitSyncFlags.SyncFlushCommandsBit;
                ulong timeout = timeoutNanoseconds < 0 ? ulong.MaxValue : (ulong)timeoutNanoseconds;

                WaitSyncStatus status = GL.ClientWaitSync(this._fenceSync, flags, timeout);
                if (status == WaitSyncStatus.TimeoutExpired)
                {
                    return null;  //超时
                }
                if (status != WaitSyncStatus.AlreadySignaled &&
                    status != WaitSyncStatus.ConditionSatisfied)
                {
                    return null;  //错误
                }

                //清理栅栏
                GL.DeleteSync(this._fenceSync);
                this._fenceSync = IntPtr.Zero;
            }

            this.Bind();

            byte[] buffer;
            try
            {
                //映射内存（如果数据未就绪，这里会阻塞）
                IntPtr gpuPointer = GL.MapBuffer(BufferTarget.PixelPackBuffer, BufferAccess.ReadOnly);
                if (gpuPointer == IntPtr.Zero)
                {
                    this.Unbind();
                    return null;
                }

                //复制到托管数组
                buffer = new byte[this.BufferSize];
                Marshal.Copy(gpuPointer, buffer, 0, this.BufferSize);
            }
            finally
            {
                //取消映射
                GL.UnmapBuffer(BufferTarget.PixelPackBuffer);

                this.Unbind();
            }

            return buffer;
        }
        #endregion

        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            GL.DeleteBuffer(this.Id);
            if (this._fenceSync != IntPtr.Zero)
            {
                GL.DeleteSync(this._fenceSync);
                this._fenceSync = IntPtr.Zero;
            }
        }
        #endregion


        //Private

        #region 计算缓冲区尺寸 —— int CalculateBufferSize(int width, int height...
        /// <summary>
        /// 计算缓冲区尺寸
        /// </summary>
        private int CalculateBufferSize(int width, int height, PixelFormat pixelFormat)
        {
            return pixelFormat switch
            {
                PixelFormat.Red => width * height,
                PixelFormat.Rgb => width * height * 3,
                PixelFormat.Rgba => width * height * 4,
                _ => throw new ArgumentOutOfRangeException(nameof(pixelFormat), $"不支持的像素格式: {pixelFormat}")
            };
        }
        #endregion

        #endregion
    }
}
