using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.InteropServices;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 统一缓冲区
    /// </summary>
    /// <typeparam name="T">结构体类型</typeparam>
    public class UniformBuffer<T> : IDisposable where T : unmanaged
    {
        #region # 字段及构造器

        private readonly UniformBuffer _buffer;
        private bool _disposed;

        /// <summary>
        /// 创建泛型统一缓冲区构造器
        /// </summary>
        /// <param name="usage">缓冲区使用模式</param>
        public UniformBuffer(BufferUsageHint usage = BufferUsageHint.DynamicDraw)
        {
            int size = Marshal.SizeOf<T>();
            this._buffer = new UniformBuffer(size, usage);
        }

        /// <summary>
        /// 创建泛型统一缓冲区构造器（带初始数据）
        /// </summary>
        /// <param name="data">初始数据</param>
        /// <param name="usage">缓冲区使用模式</param>
        public UniformBuffer(T data, BufferUsageHint usage = BufferUsageHint.StaticDraw)
        {
            int size = Marshal.SizeOf<T>();
            this._buffer = new UniformBuffer(size, usage);
            this.UploadData(data);
        }

        #endregion

        #region # 属性

        /// <summary>
        /// 统一缓冲区Id
        /// </summary>
        public int Id => this._buffer.Id;

        /// <summary>
        /// 缓冲区大小（字节）
        /// </summary>
        public int Size => this._buffer.Size;

        /// <summary>
        /// 缓冲区使用模式
        /// </summary>
        public BufferUsageHint Usage => this._buffer.Usage;

        #endregion

        #region # 绑定方法

        /// <summary>
        /// 绑定统一缓冲区
        /// </summary>
        public void Bind()
        {
            this._buffer.Bind();
        }

        /// <summary>
        /// 绑定统一缓冲区到指定绑定点
        /// </summary>
        /// <param name="bindingPoint">绑定点索引</param>
        public void BindBase(int bindingPoint)
        {
            this._buffer.BindBase(bindingPoint);
        }

        /// <summary>
        /// 绑定统一缓冲区范围到指定绑定点
        /// </summary>
        /// <param name="bindingPoint">绑定点索引</param>
        /// <param name="offset">偏移量（字节）</param>
        public void BindRange(int bindingPoint, int offset)
        {
            this._buffer.BindRange(bindingPoint, offset, Marshal.SizeOf<T>());
        }

        /// <summary>
        /// 解绑统一缓冲区
        /// </summary>
        public void Unbind()
        {
            this._buffer.Unbind();
        }

        #endregion

        #region # 数据上传

        /// <summary>
        /// 上传数据到缓冲区
        /// </summary>
        /// <param name="data">结构体数据</param>
        public void UploadData(T data)
        {
            this._buffer.UploadData(data);
        }

        /// <summary>
        /// 映射并上传数据（适合频繁更新）
        /// </summary>
        /// <param name="data">结构体数据</param>
        /// <param name="offset">偏移量（字节）</param>
        public void MapAndUpload(T data, int offset = 0)
        {
            this._buffer.MapAndUpload(data, offset);
        }

        #endregion

        #region # 数据读取

        /// <summary>
        /// 读取缓冲区数据
        /// </summary>
        public T ReadData()
        {
            return this._buffer.ReadData<T>();
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

            this._buffer?.Dispose();
            this._disposed = true;
        }

        #endregion
    }
}
