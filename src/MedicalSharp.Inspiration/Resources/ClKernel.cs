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
        private readonly CL _cl;
        private bool _disposed;

        /// <summary>
        /// OpenCL 内核句柄
        /// </summary>
        public nint Handle { get; private set; }

        /// <summary>
        /// 内核函数名
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 当前已设置的参数数量（用于调试）
        /// </summary>
        public int ArgsSet { get; private set; }

        internal ClKernel(CL cl, nint handle, string name)
        {
            this._cl = cl;
            this.Handle = handle;
            this.Name = name;
            this.ArgsSet = 0;
        }

        /// <summary>
        /// 设置内核参数（Buffer）
        /// </summary>
        /// <param name="index">参数索引（从 0 开始）</param>
        /// <param name="buffer">ClBuffer 实例</param>
        public void SetArg(int index, ClBuffer buffer)
        {
            nint bufferHandle = buffer.Handle;
            int err = this._cl.SetKernelArg(this.Handle, (uint)index, (nuint)nint.Size, in bufferHandle);
            ClException.ThrowOnError(err, $"SetKernelArg({index}, Buffer) in {this.Name}");
            this.ArgsSet++;
        }

        /// <summary>
        /// 设置内核参数（值类型：int, float, Vector4 等）
        /// </summary>
        /// <typeparam name="T">值类型（必须为 unmanaged）</typeparam>
        /// <param name="index">参数索引（从 0 开始）</param>
        /// <param name="value">参数值</param>
        public unsafe void SetArg<T>(int index, T value) where T : unmanaged
        {
            int err = this._cl.SetKernelArg(this.Handle, (uint)index, (nuint)sizeof(T), in value);
            ClException.ThrowOnError(err, $"SetKernelArg({index}, {typeof(T).Name}) in {this.Name}");
            this.ArgsSet++;
        }

        /// <summary>
        /// 设置内核参数（Image / Image3D，预留）
        /// </summary>
        /// <param name="index">参数索引</param>
        /// <param name="imageHandle">OpenCL Image 句柄</param>
        internal void SetArgImage(int index, nint imageHandle)
        {
            int err = this._cl.SetKernelArg(this.Handle, (uint)index, (nuint)nint.Size, in imageHandle);
            ClException.ThrowOnError(err, $"SetKernelArg({index}, Image) in {this.Name}");
            this.ArgsSet++;
        }

        /// <summary>
        /// 执行内核（1D 工作项）
        /// </summary>
        /// <param name="commandQueue">命令队列句柄</param>
        /// <param name="globalWorkSize">全局工作项数量</param>
        /// <param name="localWorkSize">局部工作组大小（可选，默认 1）</param>
        public unsafe void Enqueue1D(nint commandQueue, nuint globalWorkSize, nuint? localWorkSize = null)
        {
            nuint local = localWorkSize ?? 1;
            int err = this._cl.EnqueueNdrangeKernel(commandQueue, this.Handle, 1, null, in globalWorkSize, in local, 0, null, null);
            ClException.ThrowOnError(err, $"EnqueueNDRangeKernel 1D ({this.Name})");
        }

        /// <summary>
        /// 执行内核（2D 工作项）
        /// </summary>
        /// <param name="commandQueue">命令队列句柄</param>
        /// <param name="globalX">全局工作项 X 方向数量</param>
        /// <param name="globalY">全局工作项 Y 方向数量</param>
        /// <param name="localX">局部工作组 X 大小（可选）</param>
        /// <param name="localY">局部工作组 Y 大小（可选）</param>
        public unsafe void Enqueue2D(nint commandQueue,
            nuint globalX, nuint globalY,
            nuint? localX = null, nuint? localY = null)
        {
            nuint[] globalWorkSize = [globalX, globalY];
            nuint[] localWorkSize = [localX ?? 1, localY ?? 1];

            fixed (nuint* globalPtr = globalWorkSize, localPtr = localWorkSize)
            {
                int err = this._cl.EnqueueNdrangeKernel(commandQueue, this.Handle, 2, null, globalPtr, localPtr, 0, null, null);
                ClException.ThrowOnError(err, $"EnqueueNDRangeKernel 2D ({this.Name})");
            }
        }

        /// <summary>
        /// 执行内核（3D 工作项，用于体积处理）
        /// </summary>
        /// <param name="commandQueue">命令队列句柄</param>
        /// <param name="globalX">全局工作项 X 方向数量</param>
        /// <param name="globalY">全局工作项 Y 方向数量</param>
        /// <param name="globalZ">全局工作项 Z 方向数量</param>
        /// <param name="localX">局部工作组 X 大小（可选）</param>
        /// <param name="localY">局部工作组 Y 大小（可选）</param>
        /// <param name="localZ">局部工作组 Z 大小（可选）</param>
        public unsafe void Enqueue3D(nint commandQueue,
            nuint globalX, nuint globalY, nuint globalZ,
            nuint? localX = null, nuint? localY = null, nuint? localZ = null)
        {
            nuint[] globalWorkSize = [globalX, globalY, globalZ];
            nuint[] localWorkSize = [localX ?? 1, localY ?? 1, localZ ?? 1];

            fixed (nuint* globalPtr = globalWorkSize, localPtr = localWorkSize)
            {
                int err = this._cl.EnqueueNdrangeKernel(commandQueue, this.Handle, 3, null, globalPtr, localPtr, 0, null, null);
                ClException.ThrowOnError(err, $"EnqueueNDRangeKernel 3D ({this.Name})");
            }
        }

        public void Dispose()
        {
            if (this._disposed) return;
            if (this.Handle != 0)
            {
                this._cl.ReleaseKernel(this.Handle);
                this.Handle = 0;
            }
            this._disposed = true;
        }
    }
}
