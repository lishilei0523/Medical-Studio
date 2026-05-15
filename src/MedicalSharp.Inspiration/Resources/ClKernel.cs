using Silk.NET.OpenCL;
using System;

namespace MedicalSharp.Inspiration.Resources
{
    /// <summary>
    /// OpenCL内核
    /// </summary>
    /// <remarks>对应.cl文件中的一个kernel函数，负责设置参数和调度执行</remarks>
    public sealed class ClKernel : IDisposable
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
        /// 创建OpenCL内核构造器
        /// </summary>
        /// <param name="cl">OpenCL实例</param>
        /// <param name="handle">内核句柄</param>
        /// <param name="name">内核函数名</param>
        internal ClKernel(CL cl, IntPtr handle, string name)
        {
            this._cl = cl;
            this.Handle = handle;
            this.Name = name;
            this.ArgsCount = 0;
        }

        #endregion

        #region # 属性

        #region 内核句柄 —— IntPtr Handle
        /// <summary>
        /// 内核句柄
        /// </summary>
        public IntPtr Handle { get; private set; }
        #endregion

        #region 内核函数名 —— string Name
        /// <summary>
        /// 内核函数名
        /// </summary>
        public string Name { get; private set; }
        #endregion

        #region 参数数量 —— int ArgsCount
        /// <summary>
        /// 参数数量
        /// </summary>
        /// <remarks>用于调试</remarks>
        public int ArgsCount { get; private set; }
        #endregion

        #endregion

        #region # 方法

        #region 设置内核参数 —— unsafe void SetKernelArg<T>(int index, T value)
        /// <summary>
        /// 设置内核参数
        /// </summary>
        /// <typeparam name="T">值类型（必须为 unmanaged）</typeparam>
        /// <param name="index">参数索引（从 0 开始）</param>
        /// <param name="value">参数值</param>
        /// <remarks>值类型：int, float, Vector4等</remarks>
        public unsafe void SetKernelArg<T>(int index, T value) where T : unmanaged
        {
            int errorCode = this._cl.SetKernelArg(this.Handle, (uint)index, (UIntPtr)sizeof(T), in value);
            ClException.ThrowOnError(errorCode, $"SetKernelArg({index}, {typeof(T).Name}) in {this.Name}");
            this.ArgsCount++;
        }
        #endregion

        #region 设置Buffer内核参数 —— void SetBufferKernelArg(int index, ClBuffer buffer)
        /// <summary>
        /// 设置Buffer内核参数
        /// </summary>
        /// <param name="index">参数索引（从 0 开始）</param>
        /// <param name="buffer">ClBuffer 实例</param>
        public void SetBufferKernelArg(int index, ClBuffer buffer)
        {
            IntPtr bufferHandle = buffer.Handle;
            int errorCode = this._cl.SetKernelArg(this.Handle, (uint)index, (UIntPtr)IntPtr.Size, in bufferHandle);
            ClException.ThrowOnError(errorCode, $"SetKernelArg({index}, Buffer) in {this.Name}");
            this.ArgsCount++;
        }
        #endregion

        #region 设置Image内核参数 —— void SetImageKernelArg(int index, in IntPtr imageHandle)
        /// <summary>
        /// 设置Image内核参数
        /// </summary>
        /// <param name="index">参数索引</param>
        /// <param name="imageHandle">Image句柄</param>
        public void SetImageKernelArg(int index, in IntPtr imageHandle)
        {
            int errorCode = this._cl.SetKernelArg(this.Handle, (uint)index, (UIntPtr)IntPtr.Size, in imageHandle);
            ClException.ThrowOnError(errorCode, $"SetKernelArg({index}, Image) in {this.Name}");
            this.ArgsCount++;
        }
        #endregion

        #region 执行1D工作项内核 —— unsafe void Enqueue1D(IntPtr commandQueue, uint globalSize...
        /// <summary>
        /// 执行1D工作项内核
        /// </summary>
        /// <param name="commandQueue">命令队列句柄</param>
        /// <param name="globalSize">全局工作项尺寸</param>
        /// <param name="localSize">局部工作组大小</param>
        public unsafe void Enqueue1D(IntPtr commandQueue, uint globalSize, uint localSize = 256)
        {
            UIntPtr globalWorkSize = ((globalSize + localSize - 1) / localSize) * localSize;
            UIntPtr localWorkSize = localSize;
            int errorCode = this._cl.EnqueueNdrangeKernel(commandQueue, this.Handle, 1, null, in globalWorkSize, in localWorkSize, 0, null, null);
            ClException.ThrowOnError(errorCode, $"EnqueueNDRangeKernel 1D ({this.Name})");
        }
        #endregion

        #region 执行2D工作项内核 —— unsafe void Enqueue2D(IntPtr commandQueue, uint globalSizeX...
        /// <summary>
        /// 执行2D工作项内核
        /// </summary>
        /// <param name="commandQueue">命令队列句柄</param>
        /// <param name="globalSizeX">全局工作项X方向尺寸</param>
        /// <param name="globalSizeY">全局工作项Y方向尺寸</param>
        /// <param name="localSizeX">局部工作组X方向尺寸</param>
        /// <param name="localSizeY">局部工作组Y方向尺寸</param>
        public unsafe void Enqueue2D(IntPtr commandQueue, uint globalSizeX, uint globalSizeY, uint localSizeX = 16, uint localSizeY = 16)
        {
            UIntPtr[] globalWorkSizes =
            [
                ((globalSizeX + localSizeX - 1) / localSizeX) * localSizeX,
                ((globalSizeY + localSizeY - 1) / localSizeY) * localSizeY
            ];
            UIntPtr[] localWorkSizes = [localSizeX, localSizeY];
            fixed (UIntPtr* globalPtr = globalWorkSizes, localPtr = localWorkSizes)
            {
                int errorCode = this._cl.EnqueueNdrangeKernel(commandQueue, this.Handle, 2, null, globalPtr, localPtr, 0, null, null);
                ClException.ThrowOnError(errorCode, $"EnqueueNDRangeKernel 2D ({this.Name})");
            }
        }
        #endregion

        #region 执行3D工作项内核 —— unsafe void Enqueue3D(IntPtr commandQueue, uint globalSizeX...
        /// <summary>
        /// 执行3D工作项内核
        /// </summary>
        /// <param name="commandQueue">命令队列句柄</param>
        /// <param name="globalSizeX">全局工作项X方向尺寸</param>
        /// <param name="globalSizeY">全局工作项Y方向尺寸</param>
        /// <param name="globalSizeZ">全局工作项Z方向尺寸</param>
        /// <param name="localSizeX">局部工作组X尺寸</param>
        /// <param name="localSizeY">局部工作组Y尺寸</param>
        /// <param name="localSizeZ">局部工作组Z尺寸</param>
        public unsafe void Enqueue3D(IntPtr commandQueue, uint globalSizeX, uint globalSizeY, uint globalSizeZ, uint localSizeX = 8, uint localSizeY = 8, uint localSizeZ = 8)
        {
            UIntPtr[] globalWorkSizes =
            [
                ((globalSizeX + localSizeX - 1) / localSizeX) * localSizeX,
                ((globalSizeY + localSizeY - 1) / localSizeY) * localSizeY,
                ((globalSizeZ + localSizeZ - 1) / localSizeZ) * localSizeZ
            ];
            UIntPtr[] localWorkSizes = [localSizeX, localSizeY, localSizeZ];
            fixed (UIntPtr* globalPtr = globalWorkSizes, localPtr = localWorkSizes)
            {
                int errorCode = this._cl.EnqueueNdrangeKernel(commandQueue, this.Handle, 3, null, globalPtr, localPtr, 0, null, null);
                ClException.ThrowOnError(errorCode, $"EnqueueNDRangeKernel 3D ({this.Name})");
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
                this._cl.ReleaseKernel(this.Handle);
                this.Handle = IntPtr.Zero;
            }
            this._disposed = true;
        }
        #endregion 

        #endregion
    }
}
