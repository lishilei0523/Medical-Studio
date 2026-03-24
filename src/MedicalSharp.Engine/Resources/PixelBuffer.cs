using Microsoft.CSharp.RuntimeBinder;
using OpenTK.Graphics.OpenGL4;
using System;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 像素缓冲区
    /// </summary>
    public abstract class PixelBuffer : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 是否已释放
        /// </summary>
        protected bool _disposed;

        /// <summary>
        /// 栅栏同步对象
        /// </summary>
        protected IntPtr _fenceSync;

        /// <summary>
        /// 创建像素缓冲区构造器
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="pixelFormat">像素格式</param>
        /// <param name="pixelType">像素类型</param>
        protected PixelBuffer(int width, int height, PixelFormat pixelFormat, PixelType pixelType)
        {
            this.Width = width;
            this.Height = height;
            this.PixelFormat = pixelFormat;
            this.PixelType = pixelType;
            this.BytesPerPixel = this.CalculateBytesPerPixel(pixelFormat, pixelType);
            this.BufferSize = width * height * this.BytesPerPixel;

            //设置对齐方式（重要！）
            GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
        }

        #endregion

        #region # 属性

        #region 像素缓冲区Id —— int Id
        /// <summary>
        /// 像素缓冲区Id
        /// </summary>
        public int Id { get; protected set; }
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

        #region 像素类型 —— PixelType PixelType
        /// <summary>
        /// 像素类型
        /// </summary>
        public PixelType PixelType { get; private set; }
        #endregion

        #region 像素字节数 —— int BytesPerPixel
        /// <summary>
        /// 像素字节数
        /// </summary>
        public int BytesPerPixel { get; private set; }
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

        #region 只读属性 - 缓冲区目标 —— abstract BufferTarget BufferTarget
        /// <summary>
        /// 只读属性 - 缓冲区目标
        /// </summary>
        protected abstract BufferTarget BufferTarget { get; }
        #endregion

        #region 只读属性 - 缓冲区用途 —— abstract BufferUsageHint BufferUsage
        /// <summary>
        /// 只读属性 - 缓冲区用途
        /// </summary>
        protected abstract BufferUsageHint BufferUsage { get; }
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

        #region 释放资源 —— virtual void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            if (this.Id != 0)
            {
                GL.DeleteBuffer(this.Id);
                this.Id = 0;
            }
            if (this._fenceSync != IntPtr.Zero)
            {
                GL.DeleteSync(this._fenceSync);
                this._fenceSync = IntPtr.Zero;
            }

            this._disposed = true;
        }
        #endregion


        //protected

        #region 创建缓冲区 —— void CreateBuffer()
        /// <summary>
        /// 创建缓冲区
        /// </summary>
        protected void CreateBuffer()
        {
            this.Id = GL.GenBuffer();

            #region # 验证

            if (this.Id == 0)
            {
                throw new RuntimeBinderException($"创建像素缓冲区失败！");
            }

            #endregion

            this.Bind();
            GL.BufferData(this.BufferTarget, this.BufferSize, IntPtr.Zero, this.BufferUsage);
            this.Unbind();
        }
        #endregion

        #region 创建栅栏 —— void CreateFence()
        /// <summary>
        /// 创建栅栏
        /// </summary>
        protected void CreateFence()
        {
            if (this._fenceSync != IntPtr.Zero)
            {
                GL.DeleteSync(this._fenceSync);
            }

            this._fenceSync = GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, WaitSyncFlags.None);
        }
        #endregion

        #region 等待栅栏 —— void WaitForFence(long timeoutNanoseconds)
        /// <summary>
        /// 等待栅栏
        /// </summary>
        /// <param name="timeoutNanoseconds">超时时间(纳秒)</param>
        protected void WaitForFence(long timeoutNanoseconds)
        {
            #region # 验证

            if (this._fenceSync == IntPtr.Zero)
            {
                return;
            }

            #endregion

            ClientWaitSyncFlags flags = ClientWaitSyncFlags.SyncFlushCommandsBit;
            ulong timeout = timeoutNanoseconds < 0 ? ulong.MaxValue : (ulong)timeoutNanoseconds;

            WaitSyncStatus status = GL.ClientWaitSync(this._fenceSync, flags, timeout);
            if (status == WaitSyncStatus.TimeoutExpired)
            {
                throw new TimeoutException("操作超时");
            }
            if (status != WaitSyncStatus.AlreadySignaled && status != WaitSyncStatus.ConditionSatisfied)
            {
                throw new InvalidOperationException("同步错误");
            }

            GL.DeleteSync(this._fenceSync);
            this._fenceSync = IntPtr.Zero;
        }
        #endregion

        #region 计算像素字节数 —— int CalculateBytesPerPixel(PixelFormat pixelFormat...
        /// <summary>
        /// 计算像素字节数
        /// </summary>
        /// <param name="pixelFormat">像素格式</param>
        /// <param name="pixelType">像素类型</param>
        /// <returns>像素字节数</returns>
        protected int CalculateBytesPerPixel(PixelFormat pixelFormat, PixelType pixelType)
        {
            int components = pixelFormat switch
            {
                PixelFormat.Red => 1,
                PixelFormat.Rgb => 3,
                PixelFormat.Rgba => 4,
                _ => 4
            };

            int typeSize = pixelType switch
            {
                PixelType.UnsignedByte => 1,
                PixelType.Byte => 1,
                PixelType.UnsignedShort => 2,
                PixelType.Short => 2,
                PixelType.UnsignedInt => 4,
                PixelType.Int => 4,
                PixelType.Float => 4,
                PixelType.HalfFloat => 2,
                _ => 4
            };

            return components * typeSize;
        }
        #endregion

        #endregion
    }
}
