using Microsoft.CSharp.RuntimeBinder;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.InteropServices;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 统一缓冲区
    /// </summary>
    public class UniformBuffer : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 释放标识
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// 创建统一缓冲区构造器
        /// </summary>
        /// <param name="bufferSize">缓冲区尺寸</param>
        /// <param name="usage">缓冲区模式</param>
        public UniformBuffer(int bufferSize, BufferUsageHint usage = BufferUsageHint.DynamicDraw)
        {
            #region # 验证

            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize), "缓冲区尺寸必须大于0");
            }

            #endregion

            this.BufferSize = bufferSize;
            this.Usage = usage;

            int uniformBufferId = GL.GenBuffer();

            #region # 验证

            if (uniformBufferId == 0)
            {
                throw new RuntimeBinderException("创建统一缓冲区失败！");
            }

            #endregion

            this.Id = uniformBufferId;

            //分配显存
            this.Bind();
            GL.BufferData(BufferTarget.UniformBuffer, this.BufferSize, IntPtr.Zero, this.Usage);
            this.Unbind();
        }

        /// <summary>
        /// 创建统一缓冲区构造器
        /// </summary>
        /// <param name="data">初始化数据</param>
        /// <param name="usage">缓冲区模式</param>
        /// <remarks>从现有数据初始化</remarks>
        public UniformBuffer(byte[] data, BufferUsageHint usage = BufferUsageHint.StaticDraw)
            : this(data.Length, usage)
        {
            this.Update(data);
        }

        #endregion

        #region # 属性

        #region 统一缓冲区Id —— int Id
        /// <summary>
        /// 统一缓冲区Id
        /// </summary>
        public int Id { get; private set; }
        #endregion

        #region 统一缓冲区尺寸 —— int BufferSize
        /// <summary>
        /// 统一缓冲区尺寸
        /// </summary>
        public int BufferSize { get; private set; }
        #endregion

        #region 统一缓冲区模式 —— BufferUsageHint Usage
        /// <summary>
        /// 统一缓冲区模式
        /// </summary>
        public BufferUsageHint Usage { get; private set; }
        #endregion

        #endregion

        #region # 方法

        #region 绑定统一缓冲区 —— void Bind()
        /// <summary>
        /// 绑定统一缓冲区
        /// </summary>
        public void Bind()
        {
            GL.BindBuffer(BufferTarget.UniformBuffer, this.Id);
        }
        #endregion

        #region 绑定统一缓冲区 —— void Bind(int bindingPoint)
        /// <summary>
        /// 绑定统一缓冲区
        /// </summary>
        /// <param name="bindingPoint">绑定点索引</param>
        public void Bind(int bindingPoint)
        {
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, bindingPoint, this.Id);
        }
        #endregion

        #region 绑定统一缓冲区 —— void Bind(int bindingPoint, int offset, int size)
        /// <summary>
        /// 绑定统一缓冲区
        /// </summary>
        /// <param name="bindingPoint">绑定点索引</param>
        /// <param name="offset">偏移量（字节）</param>
        /// <param name="size">尺寸（字节）</param>
        public void Bind(int bindingPoint, int offset, int size)
        {
            GL.BindBufferRange(BufferRangeTarget.UniformBuffer, bindingPoint, this.Id, (IntPtr)offset, size);
        }
        #endregion

        #region 解绑统一缓冲区 —— void Unbind()
        /// <summary>
        /// 解绑统一缓冲区
        /// </summary>
        public void Unbind()
        {
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }
        #endregion

        #region 更新统一缓冲区 —— void Update(byte[] data)
        /// <summary>
        /// 更新统一缓冲区
        /// </summary>
        /// <param name="data">数据数组</param>
        public void Update(byte[] data)
        {
            #region # 验证

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (data.Length > this.BufferSize)
            {
                throw new ArgumentException($"数据尺寸\"{data.Length}\"超过缓冲区尺寸\"{this.BufferSize}\"！");
            }

            #endregion

            this.Bind();

            //使用BufferSubData更新数据（不重新分配）
            GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, data.Length, data);

            this.Unbind();
        }
        #endregion

        #region 更新统一缓冲区 —— void Update<T>(T data)
        /// <summary>
        /// 更新统一缓冲区
        /// </summary>
        /// <typeparam name="T">结构体类型</typeparam>
        /// <param name="data">结构体数据</param>
        public void Update<T>(T data) where T : unmanaged
        {
            int size = Marshal.SizeOf<T>();

            #region # 验证

            if (size > this.BufferSize)
            {
                throw new ArgumentException($"数据尺寸\"{size}\"超过缓冲区尺寸\"{this.BufferSize}\"！");
            }

            #endregion

            byte[] bytes = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(data, ptr, false);
                Marshal.Copy(ptr, bytes, 0, size);
                this.Update(bytes);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
        #endregion

        #region 部分更新统一缓冲区 —— void UploadPartially(byte[] data, int offset)
        /// <summary>
        /// 部分更新统一缓冲区
        /// </summary>
        /// <param name="data">数据数组</param>
        /// <param name="offset">偏移量（字节）</param>
        public void UploadPartially(byte[] data, int offset)
        {
            #region # 验证

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "数据不可为空！");
            }
            if (offset + data.Length > this.BufferSize)
            {
                throw new ArgumentException($"数据尺寸\"{data.Length}\"超出缓冲区范围！(偏移: {offset}, 缓冲区尺寸: {this.BufferSize})");
            }

            #endregion

            this.Bind();

            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)offset, data.Length, data);

            this.Unbind();
        }
        #endregion

        #region 映射更新统一缓冲区 —— void MapAndUpdate<T>(T data, int offset = 0)
        /// <summary>
        /// 映射更新统一缓冲区
        /// </summary>
        /// <typeparam name="T">结构体类型</typeparam>
        /// <param name="data">结构体数据</param>
        /// <param name="offset">偏移量（字节）</param>
        /// <remarks>适合频繁更新</remarks>
        public void MapAndUpdate<T>(T data, int offset = 0) where T : unmanaged
        {
            int size = Marshal.SizeOf<T>();

            #region # 验证

            if (offset + size > this.BufferSize)
            {
                throw new ArgumentException($"数据尺寸\"{size}\"超出缓冲区范围！(偏移: {offset}, 总尺寸: {this.BufferSize})");
            }

            #endregion

            this.Bind();

            //映射缓冲区
            IntPtr ptr = GL.MapBufferRange(BufferTarget.UniformBuffer, (IntPtr)offset, size, MapBufferAccessMask.MapInvalidateRangeBit);
            if (ptr != IntPtr.Zero)
            {
                try
                {
                    //写入数据
                    Marshal.StructureToPtr(data, ptr, false);
                }
                finally
                {
                    //取消映射
                    GL.UnmapBuffer(BufferTarget.UniformBuffer);
                }
            }

            this.Unbind();
        }
        #endregion

        #region 清空统一缓冲区 —— void Clear()
        /// <summary>
        /// 清空统一缓冲区
        /// </summary>
        /// <remarks>填充0</remarks>
        public void Clear()
        {
            this.Bind();

            //映射并清空
            IntPtr ptr = GL.MapBufferRange(BufferTarget.UniformBuffer, IntPtr.Zero, this.BufferSize, MapBufferAccessMask.MapInvalidateRangeBit);
            if (ptr != IntPtr.Zero)
            {
                try
                {
                    unsafe
                    {
                        byte* p = (byte*)ptr.ToPointer();
                        for (int i = 0; i < this.BufferSize; i++)
                        {
                            p[i] = 0;
                        }
                    }
                }
                finally
                {
                    GL.UnmapBuffer(BufferTarget.UniformBuffer);
                }
            }

            this.Unbind();
        }
        #endregion

        #region 读取统一缓冲区 —— byte[] Read()
        /// <summary>
        /// 读取统一缓冲区
        /// </summary>
        /// <returns>缓冲区数据</returns>
        /// <remarks>仅用于调试</remarks>
        public byte[] Read()
        {
            this.Bind();

            byte[] data = new byte[this.BufferSize];
            IntPtr ptr = GL.MapBuffer(BufferTarget.UniformBuffer, BufferAccess.ReadOnly);
            if (ptr != IntPtr.Zero)
            {
                try
                {
                    Marshal.Copy(ptr, data, 0, this.BufferSize);
                }
                finally
                {
                    GL.UnmapBuffer(BufferTarget.UniformBuffer);
                }
            }

            this.Unbind();

            return data;
        }
        #endregion

        #region 读取统一缓冲区 —— T Read<T>()
        /// <summary>
        /// 读取统一缓冲区
        /// </summary>
        /// <typeparam name="T">结构体类型</typeparam>
        /// <returns>结构体数据</returns>
        /// <remarks>仅用于调试</remarks>
        public T Read<T>() where T : unmanaged
        {
            byte[] data = this.Read();
            int size = Marshal.SizeOf<T>();

            if (data.Length < size)
            {
                throw new InvalidOperationException($"缓冲区大小({data.Length})小于结构体大小({size})");
            }

            IntPtr ptr = Marshal.AllocHGlobal(size);

            try
            {
                Marshal.Copy(data, 0, ptr, size);
                return Marshal.PtrToStructure<T>(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
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

            GL.DeleteBuffer(this.Id);
            this._disposed = true;
        }
        #endregion

        #endregion
    }
}
