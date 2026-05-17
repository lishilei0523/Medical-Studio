using OpenTK.Mathematics;
using Silk.NET.OpenCL;
using Silk.NET.OpenCL.Extensions.KHR;
using System;

namespace MedicalSharp.Inspiration.Resources
{
    /// <summary>
    /// OpenCL图像
    /// </summary>
    /// <remarks>
    /// 对应OpenCL的image1d_t/image2d_t/image3d_t
    /// 支持独立创建或从OpenGL纹理共享（cl_khr_gl_sharing）
    /// </remarks>
    public abstract class ClImage : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 释放标识
        /// </summary>
        protected bool _disposed;

        /// <summary>
        /// OpenCL实例
        /// </summary>
        protected readonly CL _cl;

        /// <summary>
        /// OpenGL扩展实例
        /// </summary>
        /// <remarks>仅当从GL纹理创建时初始化</remarks>
        protected readonly KhrGlSharing _glSharing;

        /// <summary>
        /// 创建OpenCL图像构造器
        /// </summary>
        /// <param name="cl">OpenCL实例</param>
        /// <param name="glSharing">OpenGL扩展实例</param>
        /// <param name="handle">图像句柄</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">深度</param>
        /// <param name="memoryFlags">内存标识</param>
        /// <param name="channelOrder">通道排序</param>
        /// <param name="channelType">通道类型</param>
        /// <param name="isFromGl">是否从OpenGL纹理创建</param>
        protected ClImage(CL cl, KhrGlSharing glSharing, IntPtr handle, int width, int height, int depth, MemFlags memoryFlags, ChannelOrder channelOrder, ChannelType channelType, bool isFromGl = false)
        {
            #region # 验证

            if (isFromGl && glSharing == null)
            {
                throw new ArgumentOutOfRangeException(nameof(glSharing), "从OpenGL纹理创建图像时OpenGL扩展实例不可为空！");
            }

            #endregion

            this._cl = cl;
            this._glSharing = glSharing;
            this.Handle = handle;
            this.Width = width;
            this.Height = height;
            this.Depth = depth;
            this.MemoryFlags = memoryFlags;
            this.ChannelOrder = channelOrder;
            this.ChannelType = channelType;
            this.IsFromGl = isFromGl;
        }

        #endregion

        #region # 属性

        #region 图像句柄 —— IntPtr Handle
        /// <summary>
        /// 图像句柄
        /// </summary>
        public IntPtr Handle { get; private set; }
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
        /// <remarks>1D图像为1</remarks>
        public int Height { get; private set; }
        #endregion

        #region 深度 —— int Depth
        /// <summary>
        /// 深度
        /// </summary>
        /// <remarks>1D/2D图像为1</remarks>
        public int Depth { get; private set; }
        #endregion

        #region 内存标识 —— MemFlags MemoryFlags
        /// <summary>
        /// 内存标识
        /// </summary>
        public MemFlags MemoryFlags { get; private set; }
        #endregion

        #region 通道排序 —— ChannelOrder ChannelOrder
        /// <summary>
        /// 通道排序
        /// </summary>
        public ChannelOrder ChannelOrder { get; private set; }
        #endregion

        #region 通道类型 —— ChannelType ChannelType
        /// <summary>
        /// 通道类型
        /// </summary>
        /// <remarks>SNormInt16、Float等</remarks>
        public ChannelType ChannelType { get; private set; }
        #endregion

        #region 是否从OpenGL纹理创建 —— bool IsFromGl
        /// <summary>
        /// 是否从OpenGL纹理创建
        /// </summary>
        /// <remarks>true时需要Acquire/Release管理所有权</remarks>
        public bool IsFromGl { get; private set; }
        #endregion

        #region 只读属性 - 图像维度 —— abstract uint Dimension
        /// <summary>
        /// 只读属性 - 图像维度
        /// </summary>
        /// <remarks>1/2/3</remarks>
        public abstract uint Dimension { get; }
        #endregion

        #endregion

        #region # 方法

        #region 填充图像 —— void Fill(IntPtr commandQueue, float value)
        /// <summary>
        /// 填充图像
        /// </summary>
        /// <param name="commandQueue">命令队列句柄</param>
        /// <param name="value">填充值</param>
        /// <remarks>浮点，自动转换到图像格式</remarks>
        public unsafe void Fill(IntPtr commandQueue, float value)
        {
            UIntPtr[] origin = [0, 0, 0];
            UIntPtr[] region = [(UIntPtr)this.Width, (UIntPtr)this.Height, (UIntPtr)this.Depth];
            fixed (UIntPtr* originPtr = origin, regionPtr = region)
            {
                int errorCode = this._cl.EnqueueFillImage(commandQueue, this.Handle, in value, originPtr, regionPtr, 0, null, null);
                ClException.ThrowOnError(errorCode, "EnqueueFillImage");
            }
        }
        #endregion

        #region 填充图像 —— void Fill(IntPtr commandQueue, Vector4 color)
        /// <summary>
        /// 填充图像
        /// </summary>
        /// <param name="commandQueue">命令队列句柄</param>
        /// <param name="color">颜色</param>
        public unsafe void Fill(IntPtr commandQueue, Vector4 color)
        {
            UIntPtr[] origin = [0, 0, 0];
            UIntPtr[] region = [(UIntPtr)this.Width, (UIntPtr)this.Height, (UIntPtr)this.Depth];
            fixed (UIntPtr* originPtr = origin, regionPtr = region)
            {
                int errorCode = this._cl.EnqueueFillImage(commandQueue, this.Handle, in color, originPtr, regionPtr, 0, null, null);
                ClException.ThrowOnError(errorCode, "EnqueueFillImage");
            }
        }
        #endregion

        #region 读取图像 —— T[] Read<T>(IntPtr commandQueue)
        /// <summary>
        /// 读取图像数据
        /// </summary>
        /// <typeparam name="T">值类型（通常为short或float）</typeparam>
        /// <param name="commandQueue">命令队列句柄</param>
        /// <returns>图像数组（一维，行主序）</returns>
        /// <remarks>到CPU内存</remarks>
        public T[] Read<T>(IntPtr commandQueue) where T : unmanaged
        {
            T[] destination = new T[this.Width * this.Height * this.Depth];
            this.Read<T>(commandQueue, destination);

            return destination;
        }
        #endregion

        #region 读取图像 —— void Read(IntPtr commandQueue, IntPtr data)
        /// <summary>
        /// 读取图像
        /// </summary>
        /// <param name="commandQueue">命令队列句柄</param>
        /// <param name="data">数据指针</param>
        public unsafe void Read(IntPtr commandQueue, IntPtr data)
        {
            UIntPtr[] origin = [0, 0, 0];
            UIntPtr[] region = [(UIntPtr)this.Width, (UIntPtr)this.Height, (UIntPtr)this.Depth];

            void* pointer = data.ToPointer();
            fixed (UIntPtr* originPtr = origin, regionPtr = region)
            {
                int errorCode = this._cl.EnqueueReadImage(commandQueue, this.Handle, true, originPtr, regionPtr, 0, 0, pointer, 0, null, null);
                ClException.ThrowOnError(errorCode, "EnqueueReadImage");
            }
        }
        #endregion

        #region 读取图像 —— void Read<T>(IntPtr commandQueue, Span<T> destination)
        /// <summary>
        /// 读取图像
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="commandQueue">命令队列句柄</param>
        /// <param name="destination">目标Span</param>
        public unsafe void Read<T>(IntPtr commandQueue, Span<T> destination) where T : unmanaged
        {
            UIntPtr[] origin = [0, 0, 0];
            UIntPtr[] region = [(UIntPtr)this.Width, (UIntPtr)this.Height, (UIntPtr)this.Depth];

            fixed (T* destinationPtr = destination)
            fixed (UIntPtr* originPtr = origin, regionPtr = region)
            {
                int errorCode = this._cl.EnqueueReadImage(commandQueue, this.Handle, true, originPtr, regionPtr, 0, 0, destinationPtr, 0, null, null);
                ClException.ThrowOnError(errorCode, "EnqueueReadImage");
            }
        }
        #endregion

        #region 写入图像 —— void Write(IntPtr commandQueue, IntPtr data)
        /// <summary>
        /// 写入图像
        /// </summary>
        /// <param name="commandQueue">命令队列句柄</param>
        /// <param name="data">数据指针</param>
        public unsafe void Write(IntPtr commandQueue, IntPtr data)
        {
            UIntPtr[] origin = [0, 0, 0];
            UIntPtr[] region = [(UIntPtr)this.Width, (UIntPtr)this.Height, (UIntPtr)this.Depth];

            void* pointer = data.ToPointer();
            fixed (UIntPtr* originPtr = origin, regionPtr = region)
            {
                int errorCode = this._cl.EnqueueWriteImage(commandQueue, this.Handle, true, originPtr, regionPtr, 0, 0, pointer, 0, null, null);
                ClException.ThrowOnError(errorCode, "EnqueueWriteImage");
            }
        }
        #endregion

        #region 写入图像 —— void Write<T>(IntPtr commandQueue, ReadOnlySpan<T> data)
        /// <summary>
        /// 写入图像
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="commandQueue">命令队列句柄</param>
        /// <param name="data">图像数据</param>
        public unsafe void Write<T>(IntPtr commandQueue, ReadOnlySpan<T> data) where T : unmanaged
        {
            UIntPtr[] origin = [0, 0, 0];
            UIntPtr[] region = [(UIntPtr)this.Width, (UIntPtr)this.Height, (UIntPtr)this.Depth];

            fixed (void* pointer = data)
            fixed (UIntPtr* originPtr = origin, regionPtr = region)
            {
                int errorCode = this._cl.EnqueueWriteImage(commandQueue, this.Handle, true, originPtr, regionPtr, 0, 0, pointer, 0, null, null);
                ClException.ThrowOnError(errorCode, "EnqueueWriteImage");
            }
        }
        #endregion

        #region 复制图像 —— void CopyTo(IntPtr commandQueue, ClImage targetImage)
        /// <summary>
        /// 复制图像
        /// </summary>
        /// <param name="commandQueue">命令队列句柄</param>
        /// <param name="targetImage">目标图像</param>
        /// <remarks>尺寸需相同，底层调用clEnqueueCopyImage</remarks>
        public unsafe void CopyTo(IntPtr commandQueue, ClImage targetImage)
        {
            #region # 验证

            if (this.Width != targetImage.Width)
            {
                throw new ArgumentOutOfRangeException(nameof(targetImage), "宽度不一致！");
            }
            if (this.Height != targetImage.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(targetImage), "高度不一致！");
            }
            if (this.Depth != targetImage.Depth)
            {
                throw new ArgumentOutOfRangeException(nameof(targetImage), "深度不一致！");
            }

            #endregion

            UIntPtr[] origin = [0, 0, 0];
            UIntPtr[] region = [(UIntPtr)this.Width, (UIntPtr)this.Height, (UIntPtr)this.Depth];
            fixed (UIntPtr* originPtr = origin, regionPtr = region)
            {
                int errorCode = this._cl.EnqueueCopyImage(commandQueue, this.Handle, targetImage.Handle, originPtr, originPtr, regionPtr, 0, null, null);
                ClException.ThrowOnError(errorCode, "EnqueueCopyImage");
            }
        }
        #endregion

        #region 复制图像到内存缓冲区 —— void CopyToBuffer(IntPtr commandQueue, ClBuffer clBuffer)
        /// <summary>
        /// 复制图像到内存缓冲区
        /// </summary>
        /// <param name="commandQueue">命令队列句柄</param>
        /// <param name="clBuffer">内存缓冲区</param>
        /// <remarks>底层调用clEnqueueCopyImageToBuffer</remarks>
        public unsafe void CopyToBuffer(IntPtr commandQueue, ClBuffer clBuffer)
        {
            UIntPtr[] origin = [0, 0, 0];
            UIntPtr[] region = [(UIntPtr)this.Width, (UIntPtr)this.Height, (UIntPtr)this.Depth];
            fixed (UIntPtr* originPtr = origin, regionPtr = region)
            {
                int errorCode = this._cl.EnqueueCopyImageToBuffer(commandQueue, this.Handle, clBuffer.Handle, originPtr, regionPtr, 0, 0, null, null);
                ClException.ThrowOnError(errorCode, "EnqueueCopyImageToBuffer");
            }
        }
        #endregion

        #region 从内存缓冲区复制图像 —— void CopyFromBuffer(IntPtr commandQueue, ClBuffer clBuffer)
        /// <summary>
        /// 从内存缓冲区复制图像
        /// </summary>
        /// <param name="commandQueue">命令队列句柄</param>
        /// <param name="clBuffer">内存缓冲区</param>
        /// <remarks>底层调用clEnqueueCopyBufferToImage</remarks>
        public unsafe void CopyFromBuffer(IntPtr commandQueue, ClBuffer clBuffer)
        {
            UIntPtr[] origin = [0, 0, 0];
            UIntPtr[] region = [(UIntPtr)this.Width, (UIntPtr)this.Height, (UIntPtr)this.Depth];
            fixed (UIntPtr* originPtr = origin, regionPtr = region)
            {
                int errorCode = this._cl.EnqueueCopyBufferToImage(commandQueue, clBuffer.Handle, this.Handle, 0, originPtr, regionPtr, 0, null, null);
                ClException.ThrowOnError(errorCode, "EnqueueCopyBufferToImage");
            }
        }
        #endregion

        #region 接管OpenGL图像所有权 —— void AcquireForCL(IntPtr commandQueue)
        /// <summary>
        /// 接管OpenGL图像所有权
        /// </summary>
        /// <param name="commandQueue">命令队列句柄</param>
        /// <remarks>
        /// 调用后可以安全地用OpenCL读写此图像
        /// 仅对从OpenGL纹理创建的图像有效
        /// 必须与<see cref="ReleaseToGL"/>成对使用
        /// Warning: Acquire之后、Release之前，OpenGL不能操作此纹理
        /// </remarks>
        public unsafe void AcquireForCL(IntPtr commandQueue)
        {
            #region # 验证

            if (!this.IsFromGl || this._glSharing == null)
            {
                return;
            }

            #endregion

            IntPtr handle = this.Handle;
            int errorCode = this._glSharing.EnqueueAcquireGlobjects(commandQueue, 1, &handle, 0, null, null);
            ClException.ThrowOnError(errorCode, "EnqueueAcquireGLObjects");
        }
        #endregion

        #region 归还OpenGL图像所有权 —— void ReleaseToGL(IntPtr commandQueue)
        /// <summary>
        /// 归还OpenGL图像所有权
        /// </summary>
        /// <param name="commandQueue">命令队列句柄</param>
        /// <remarks>
        /// 调用后OpenGL可以继续使用此纹理进行渲染
        /// 仅对从OpenGL纹理创建的图像有效
        /// 必须与<see cref="AcquireForCL"/>成对使用
        /// Release之后可以立即在OpenGL侧使用此纹理
        /// </remarks>
        public unsafe void ReleaseToGL(IntPtr commandQueue)
        {
            #region # 验证

            if (!this.IsFromGl || this._glSharing == null)
            {
                return;
            }

            #endregion

            IntPtr handle = this.Handle;
            int errorCode = this._glSharing.EnqueueReleaseGlobjects(commandQueue, 1, &handle, 0, null, null);
            ClException.ThrowOnError(errorCode, "EnqueueReleaseGLObjects");
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

            if (this.Handle != IntPtr.Zero)
            {
                this._glSharing?.Dispose();
                this._cl.ReleaseMemObject(this.Handle);
                this.Handle = IntPtr.Zero;
            }

            this._disposed = true;
        }
        #endregion

        #endregion
    }
}
