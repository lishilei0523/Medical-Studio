using Silk.NET.OpenCL;
using System;

namespace MedicalSharp.Inspiration.Resources
{
    /// <summary>
    /// OpenCL内存缓冲区
    /// </summary>
    public sealed class ClBuffer : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 释放标识
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// OpenCL实例
        /// </summary>
        private readonly CL _cl;

        /// <summary>
        /// 创建OpenCL内存缓冲区构造器
        /// </summary>
        /// <param name="cl">OpenCL实例</param>
        /// <param name="handle">内存缓冲区句柄</param>
        /// <param name="bufferSize">内存缓冲区尺寸</param>
        private ClBuffer(CL cl, IntPtr handle, UIntPtr bufferSize)
        {
            this._cl = cl;
            this.Handle = handle;
            this.BufferSize = bufferSize;
        }

        #endregion

        #region # 属性

        #region 内存缓冲区句柄 —— IntPtr Handle
        /// <summary>
        /// 内存缓冲区句柄
        /// </summary>
        public IntPtr Handle { get; private set; }
        #endregion

        #region 内存缓冲区尺寸 —— UIntPtr BufferSize
        /// <summary>
        /// 内存缓冲区尺寸
        /// </summary>
        /// <remarks>单位：字节</remarks>
        public UIntPtr BufferSize { get; private set; }
        #endregion

        #endregion

        #region # 方法

        #region 创建OpenCL内存缓冲区 —— static unsafe ClBuffer Create(ClContext clContext...
        /// <summary>
        /// 创建OpenCL内存缓冲区
        /// </summary>
        /// <param name="clContext">OpenCL上下文</param>
        /// <param name="flags">内存标识</param>
        /// <param name="elementSize">每个元素的字节数</param>
        /// <param name="data">CPU端数据（byte数组）</param>
        /// <returns>OpenCL内存缓冲区实例</returns>
        public static unsafe ClBuffer Create(ClContext clContext, MemFlags flags, int elementSize, byte[] data)
        {
            CL cl = CL.GetApi();
            UIntPtr size = (UIntPtr)data.Length;
            fixed (byte* pointer = data)
            {
                IntPtr handle = cl.CreateBuffer(clContext.Handle, flags | MemFlags.CopyHostPtr, size, pointer, out int err);
                ClException.ThrowOnError(err, "CreateBuffer");
                ClBuffer clBuffer = new ClBuffer(cl, handle, size);

                return clBuffer;
            }
        }
        #endregion

        #region 创建OpenCL内存缓冲区 —— static unsafe ClBuffer Create<T>(ClContext clContext...
        /// <summary>
        /// 创建OpenCL内存缓冲区
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="clContext">OpenCL上下文</param>
        /// <param name="flags">内存标识</param>
        /// <param name="data">CPU端数据</param>
        /// <returns>OpenCL内存缓冲区实例</returns>
        public static unsafe ClBuffer Create<T>(ClContext clContext, MemFlags flags, ReadOnlySpan<T> data) where T : unmanaged
        {
            CL cl = CL.GetApi();
            UIntPtr bufferSize = (UIntPtr)(data.Length * sizeof(T));
            IntPtr handle;
            fixed (void* pointer = data)
            {
                handle = cl.CreateBuffer(clContext.Handle, flags | MemFlags.CopyHostPtr, bufferSize, pointer, out int err);
                ClException.ThrowOnError(err, "CreateBuffer");
            }
            if (handle == IntPtr.Zero)
            {
                throw new ClException("CreateBuffer 返回空句柄！");
            }

            ClBuffer clBuffer = new ClBuffer(cl, handle, bufferSize);

            return clBuffer;
        }
        #endregion

        #region 创建空OpenCL内存缓冲区 —— static unsafe ClBuffer CreateEmpty(ClContext clContext...
        /// <summary>
        /// 创建空OpenCL内存缓冲区
        /// </summary>
        /// <param name="clContext">OpenCL上下文</param>
        /// <param name="flags">内存标识</param>
        /// <param name="bufferSize">缓冲区尺寸</param>
        /// <returns>OpenCL内存缓冲区实例</returns>
        public static unsafe ClBuffer CreateEmpty(ClContext clContext, MemFlags flags, int bufferSize)
        {
            CL cl = CL.GetApi();
            UIntPtr size = (UIntPtr)bufferSize;
            IntPtr handle = cl.CreateBuffer(clContext.Handle, flags, size, null, out int err);
            ClException.ThrowOnError(err, "CreateBuffer (empty)");
            ClBuffer clBuffer = new ClBuffer(cl, handle, size);

            return clBuffer;
        }
        #endregion

        #region 创建空OpenCL内存缓冲区 —— static unsafe ClBuffer CreateEmpty<T>(ClContext clContext...
        /// <summary>
        /// 创建空OpenCL内存缓冲区
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="clContext">OpenCL上下文</param>
        /// <param name="flags">内存标识</param>
        /// <param name="elementsCount">元素数量</param>
        /// <returns>OpenCL内存缓冲区实例</returns>
        public static unsafe ClBuffer CreateEmpty<T>(ClContext clContext, MemFlags flags, int elementsCount) where T : unmanaged
        {
            CL cl = CL.GetApi();
            UIntPtr size = (UIntPtr)(elementsCount * sizeof(T));
            IntPtr handle = cl.CreateBuffer(clContext.Handle, flags, size, null, out int err);
            ClException.ThrowOnError(err, "CreateBuffer (empty)");
            if (handle == IntPtr.Zero)
            {
                throw new ClException("CreateBuffer 返回空句柄");
            }

            ClBuffer clBuffer = new ClBuffer(cl, handle, size);

            return clBuffer;
        }
        #endregion

        #region 读取数据到CPU —— unsafe byte[] Read(IntPtr commandQueue, uint offset...
        /// <summary>
        /// 读取数据到CPU
        /// </summary>
        /// <param name="commandQueue">命令队列句柄</param>
        /// <param name="offset">偏移量</param>
        /// <param name="size">尺寸</param>
        /// <returns>CPU端数据</returns>
        public unsafe byte[] Read(IntPtr commandQueue, uint offset, uint size)
        {
            byte[] result = new byte[size];
            fixed (byte* pointer = result)
            {
                int errorCode = this._cl.EnqueueReadBuffer(commandQueue, this.Handle, true, offset, size, pointer, 0, null, null);
                ClException.ThrowOnError(errorCode, "EnqueueReadBuffer");
            }

            return result;
        }
        #endregion

        #region 读取数据到CPU —— unsafe T[] Read<T>(IntPtr commandQueue, int elementsCount)
        /// <summary>
        /// 读取数据到CPU
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="commandQueue">命令队列句柄</param>
        /// <param name="elementsCount">元素数量</param>
        /// <returns>CPU端数据</returns>
        public unsafe T[] Read<T>(IntPtr commandQueue, int elementsCount) where T : unmanaged
        {
            T[] result = new T[elementsCount];
            fixed (void* pointer = result)
            {
                int errorCode = this._cl.EnqueueReadBuffer(commandQueue, this.Handle, true, 0, (UIntPtr)(elementsCount * sizeof(T)), pointer, 0, null, null);
                ClException.ThrowOnError(errorCode, "EnqueueReadBuffer");
            }

            return result;
        }
        #endregion

        #region 读取数据到CPU —— unsafe void Read<T>(IntPtr commandQueue, Span<T> destination)
        /// <summary>
        /// 读取数据到CPU
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="commandQueue">命令队列句柄</param>
        /// <param name="destination">CPU端Span</param>
        public unsafe void Read<T>(IntPtr commandQueue, Span<T> destination) where T : unmanaged
        {
            fixed (void* ptr = destination)
            {
                int errorCode = this._cl.EnqueueReadBuffer(commandQueue, this.Handle, true, 0, (UIntPtr)(destination.Length * sizeof(T)), ptr, 0, null, null);
                ClException.ThrowOnError(errorCode, "EnqueueReadBuffer");
            }
        }
        #endregion

        #region 写入数据到设备 —— unsafe void Write(IntPtr commandQueue, byte[] data)
        /// <summary>
        /// 写入数据到设备
        /// </summary>
        /// <param name="commandQueue">命令队列句柄</param>
        /// <param name="data">CPU端数据</param>
        public unsafe void Write(IntPtr commandQueue, byte[] data)
        {
            fixed (byte* pointer = data)
            {
                int errorCode = this._cl.EnqueueWriteBuffer(commandQueue, this.Handle, true, 0, (UIntPtr)data.Length, pointer, 0, null, null);
                ClException.ThrowOnError(errorCode, "EnqueueWriteBuffer");
            }
        }
        #endregion

        #region 写入数据到设备 —— unsafe void Write<T>(IntPtr commandQueue, ReadOnlySpan<T> data)
        /// <summary>
        /// 写入数据到设备
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="commandQueue">命令队列句柄</param>
        /// <param name="data">CPU端数据</param>
        public unsafe void Write<T>(IntPtr commandQueue, ReadOnlySpan<T> data) where T : unmanaged
        {
            fixed (void* pointer = data)
            {
                int errorCode = this._cl.EnqueueWriteBuffer(commandQueue, this.Handle, true, 0, (UIntPtr)(data.Length * sizeof(T)), pointer, 0, null, null);
                ClException.ThrowOnError(errorCode, "EnqueueWriteBuffer");
            }
        }
        #endregion

        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }
            if (this.Handle != IntPtr.Zero)
            {
                this._cl.ReleaseMemObject(this.Handle);
                this.Handle = IntPtr.Zero;
            }
            this._disposed = true;
        }
        #endregion 

        #endregion
    }
}
