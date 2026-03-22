using Microsoft.CSharp.RuntimeBinder;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.InteropServices;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 统一缓冲区
    /// </summary>
    /// <remarks>用于在着色器之间共享统一变量数据，提高性能</remarks>
    public class UniformBuffer : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 释放标志
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// 创建统一缓冲区构造器
        /// </summary>
        /// <param name="size">缓冲区大小（字节）</param>
        /// <param name="usage">缓冲区使用模式</param>
        public UniformBuffer(int size, BufferUsageHint usage = BufferUsageHint.DynamicDraw)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "缓冲区大小必须大于0");
            }

            this.Size = size;
            this.Usage = usage;

            int uniformBufferId = GL.GenBuffer();

            #region # 验证

            if (uniformBufferId == 0)
            {
                throw new RuntimeBinderException("创建统一缓冲区失败！");
            }

            #endregion

            this.Id = uniformBufferId;

            // 分配显存
            this.Bind();
            GL.BufferData(BufferTarget.UniformBuffer, this.Size, IntPtr.Zero, this.Usage);
            this.Unbind();
        }

        /// <summary>
        /// 创建统一缓冲区构造器（从现有数据初始化）
        /// </summary>
        /// <param name="data">初始化数据</param>
        /// <param name="usage">缓冲区使用模式</param>
        public UniformBuffer(byte[] data, BufferUsageHint usage = BufferUsageHint.StaticDraw)
            : this(data.Length, usage)
        {
            this.UploadData(data);
        }

        #endregion

        #region # 属性

        /// <summary>
        /// 统一缓冲区Id
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// 缓冲区大小（字节）
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// 缓冲区使用模式
        /// </summary>
        public BufferUsageHint Usage { get; private set; }

        #endregion

        #region # 绑定方法

        /// <summary>
        /// 绑定统一缓冲区
        /// </summary>
        public void Bind()
        {
            GL.BindBuffer(BufferTarget.UniformBuffer, this.Id);
        }

        /// <summary>
        /// 绑定统一缓冲区到指定绑定点
        /// </summary>
        /// <param name="bindingPoint">绑定点索引</param>
        public void BindBase(int bindingPoint)
        {
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, bindingPoint, this.Id);
        }

        /// <summary>
        /// 绑定统一缓冲区范围到指定绑定点
        /// </summary>
        /// <param name="bindingPoint">绑定点索引</param>
        /// <param name="offset">偏移量（字节）</param>
        /// <param name="size">大小（字节）</param>
        public void BindRange(int bindingPoint, int offset, int size)
        {
            GL.BindBufferRange(BufferRangeTarget.UniformBuffer, bindingPoint, this.Id,
                               (IntPtr)offset, size);
        }

        /// <summary>
        /// 解绑统一缓冲区
        /// </summary>
        public void Unbind()
        {
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }

        #endregion

        #region # 数据上传

        /// <summary>
        /// 上传数据到缓冲区（覆盖全部）
        /// </summary>
        /// <param name="data">数据数组</param>
        public void UploadData(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Length > this.Size)
            {
                throw new ArgumentException($"数据大小({data.Length})超过缓冲区大小({this.Size})");
            }

            this.Bind();

            // 使用 BufferSubData 更新数据（不重新分配）
            GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, data.Length, data);

            this.Unbind();
        }

        /// <summary>
        /// 上传数据到缓冲区（覆盖全部）
        /// </summary>
        /// <typeparam name="T">结构体类型</typeparam>
        /// <param name="data">结构体数据</param>
        public void UploadData<T>(T data) where T : unmanaged
        {
            int size = Marshal.SizeOf<T>();
            if (size > this.Size)
            {
                throw new ArgumentException($"数据大小({size})超过缓冲区大小({this.Size})");
            }

            byte[] byteArray = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            try
            {
                Marshal.StructureToPtr(data, ptr, false);
                Marshal.Copy(ptr, byteArray, 0, size);
                this.UploadData(byteArray);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        /// 上传数据到缓冲区指定位置
        /// </summary>
        /// <param name="data">数据数组</param>
        /// <param name="offset">偏移量（字节）</param>
        public void UploadData(byte[] data, int offset)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (offset + data.Length > this.Size)
            {
                throw new ArgumentException($"数据大小({data.Length})超出缓冲区范围(偏移{offset}, 总大小{this.Size})");
            }

            this.Bind();
            GL.BufferSubData(BufferTarget.UniformBuffer, (IntPtr)offset, data.Length, data);
            this.Unbind();
        }

        /// <summary>
        /// 映射缓冲区并写入数据（适合频繁更新）
        /// </summary>
        /// <typeparam name="T">结构体类型</typeparam>
        /// <param name="data">结构体数据</param>
        /// <param name="offset">偏移量（字节）</param>
        public void MapAndUpload<T>(T data, int offset = 0) where T : unmanaged
        {
            int size = Marshal.SizeOf<T>();
            if (offset + size > this.Size)
            {
                throw new ArgumentException($"数据大小({size})超出缓冲区范围(偏移{offset}, 总大小{this.Size})");
            }

            this.Bind();

            // 映射缓冲区
            IntPtr ptr = GL.MapBufferRange(BufferTarget.UniformBuffer, (IntPtr)offset, size, MapBufferAccessMask.MapInvalidateRangeBit);

            if (ptr != IntPtr.Zero)
            {
                try
                {
                    // 写入数据
                    Marshal.StructureToPtr(data, ptr, false);
                }
                finally
                {
                    // 取消映射
                    GL.UnmapBuffer(BufferTarget.UniformBuffer);
                }
            }

            this.Unbind();
        }

        /// <summary>
        /// 清空缓冲区（填充0）
        /// </summary>
        public void Clear()
        {
            this.Bind();

            // 映射并清空
            IntPtr ptr = GL.MapBufferRange(BufferTarget.UniformBuffer, IntPtr.Zero, this.Size, MapBufferAccessMask.MapInvalidateRangeBit);

            if (ptr != IntPtr.Zero)
            {
                try
                {
                    unsafe
                    {
                        byte* p = (byte*)ptr;
                        for (int i = 0; i < this.Size; i++)
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

        #region # 数据读取（调试用）

        /// <summary>
        /// 读取缓冲区数据（用于调试）
        /// </summary>
        /// <returns>缓冲区数据</returns>
        public byte[] ReadData()
        {
            this.Bind();

            byte[] data = new byte[this.Size];
            IntPtr ptr = GL.MapBuffer(BufferTarget.UniformBuffer, BufferAccess.ReadOnly);

            if (ptr != IntPtr.Zero)
            {
                try
                {
                    Marshal.Copy(ptr, data, 0, this.Size);
                }
                finally
                {
                    GL.UnmapBuffer(BufferTarget.UniformBuffer);
                }
            }

            this.Unbind();
            return data;
        }

        /// <summary>
        /// 读取缓冲区数据为结构体（用于调试）
        /// </summary>
        /// <typeparam name="T">结构体类型</typeparam>
        /// <returns>结构体数据</returns>
        public T ReadData<T>() where T : unmanaged
        {
            byte[] data = this.ReadData();
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

        #region # 释放资源

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
                return;

            if (this.Id != 0)
            {
                GL.DeleteBuffer(this.Id);
            }

            this._disposed = true;
        }

        #endregion
    }
}
