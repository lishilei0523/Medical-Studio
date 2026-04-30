using Microsoft.CSharp.RuntimeBinder;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.InteropServices;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 着色器存储缓冲区
    /// </summary>
    public class ShaderStorageBuffer : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 释放标识
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// 创建着色器存储缓冲区构造器
        /// </summary>
        /// <param name="bufferSize">缓冲区尺寸（字节）</param>
        /// <param name="usage">缓冲区用途提示（影响驱动优化策略）</param>
        /// <exception cref="ArgumentOutOfRangeException">缓冲区尺寸必须大于0</exception>
        /// <exception cref="RuntimeBinderException">创建SSBO失败</exception>
        public ShaderStorageBuffer(int bufferSize, BufferUsageHint usage = BufferUsageHint.StaticRead)
        {
            #region # 验证

            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize), "缓冲区尺寸必须大于0");
            }

            #endregion

            //生成缓冲区对象
            this.Id = GL.GenBuffer();

            #region # 验证

            if (this.Id == 0)
            {
                throw new RuntimeBinderException("创建着色器存储缓冲区失败！");
            }

            #endregion

            this.BufferSize = bufferSize;
            this.Usage = usage;

            //分配显存
            this.AllocateMemory();
        }

        /// <summary>
        /// 创建着色器存储缓冲区构造器
        /// </summary>
        /// <param name="data">初始化数据</param>
        /// <param name="usage">缓冲区用途提示</param>
        /// <exception cref="ArgumentNullException">数据不可为空</exception>
        /// <remarks>从现有数据初始化</remarks>
        public ShaderStorageBuffer(byte[] data, BufferUsageHint usage = BufferUsageHint.StaticRead)
            : this(data.Length, usage)
        {
            this.Update(data);
        }

        #endregion

        #region # 属性

        #region 着色器存储缓冲区Id —— int Id
        /// <summary>
        /// 着色器存储缓冲区Id
        /// </summary>
        public int Id { get; private set; }
        #endregion

        #region 缓冲区尺寸 —— int BufferSize
        /// <summary>
        /// 缓冲区尺寸
        /// </summary>
        public int BufferSize { get; private set; }
        #endregion

        #region 缓冲区用途 —— BufferUsageHint Usage
        /// <summary>
        /// 缓冲区用途
        /// </summary>
        /// <remarks>
        /// 提示驱动缓冲区使用模式，便于驱动优化：
        /// - StaticRead: 数据写入一次，多次读取
        /// - DynamicDraw: 数据频繁更新，多次绘制
        /// - StreamDraw: 数据每帧更新，绘制一次
        /// </remarks>
        public BufferUsageHint Usage { get; private set; }
        #endregion

        #endregion

        #region # 方法

        #region 绑定着色器存储缓冲区 —— void Bind()
        /// <summary>
        /// 绑定着色器存储缓冲区
        /// </summary>
        /// <remarks>
        /// 仅激活缓冲区，不绑定到特定索引。
        /// 通常用于后续的BufferData、BufferSubData等操作。
        /// </remarks>
        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, this.Id);
        }
        #endregion

        #region 绑定着色器存储缓冲区 —— void Bind(int bindingPoint)
        /// <summary>
        /// 绑定着色器存储缓冲区
        /// </summary>
        /// <param name="bindingPoint">绑定点索引（0-15，与着色器中的 layout(binding = N) 对应）</param>
        /// <remarks>
        /// 将缓冲区绑定到指定的索引点，着色器通过相同的 binding 值访问该缓冲区。
        /// 示例：
        ///   C# 端：ssbo.Bind(2);
        ///   GLSL 端：layout(binding = 2, std430) buffer StatsBuffer { ... };
        /// </remarks>
        public void Bind(int bindingPoint)
        {
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, bindingPoint, this.Id);
        }
        #endregion

        #region 绑定着色器存储缓冲区 —— void Bind(int bindingPoint, int offset, int size)
        /// <summary>
        /// 绑定着色器存储缓冲区
        /// </summary>
        /// <param name="bindingPoint">绑定点索引</param>
        /// <param name="offset">偏移量（字节）</param>
        /// <param name="size">绑定区域尺寸（字节）</param>
        /// <remarks>
        /// 绑定缓冲区的子区域
        /// 适用于多块数据存储在同一缓冲区，但不同着色器只需访问其中一部分的场景。
        /// 可以减少缓冲区对象的数量，优化显存使用。
        /// </remarks>
        public void Bind(int bindingPoint, int offset, int size)
        {
            GL.BindBufferRange(BufferRangeTarget.ShaderStorageBuffer, bindingPoint, this.Id, offset, size);
        }
        #endregion

        #region 解绑着色器存储缓冲区 —— void Unbind()
        /// <summary>
        /// 解绑着色器存储缓冲区
        /// </summary>
        /// <remarks>
        /// 将当前绑定的 SSBO 解除绑定，恢复默认状态。
        /// 通常在绑定其他缓冲区前解绑，避免误操作。
        /// </remarks>
        public void Unbind()
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
        }
        #endregion

        #region 分配内存 —— void AllocateMemory()
        /// <summary>
        /// 分配显存
        /// </summary>
        /// <remarks>
        /// 为缓冲区分配指定大小的显存，初始内容未定义。
        /// 如果缓冲区已存在数据，旧数据会被丢弃。
        /// 通常在构造器中自动调用，也可用于重新分配更大尺寸。
        /// </remarks>
        public void AllocateMemory()
        {
            this.Bind();
            GL.BufferData(BufferTarget.ShaderStorageBuffer, this.BufferSize, IntPtr.Zero, this.Usage);
            this.Unbind();
        }
        #endregion

        #region 更新数据 —— void Update(byte[] data)
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="data">数据数组</param>
        /// <exception cref="ArgumentNullException">数据不可为空</exception>
        /// <exception cref="ArgumentException">数据尺寸超过缓冲区尺寸</exception>
        /// <remarks>
        /// 字节数组全量
        /// 从 CPU 上传数据到 GPU 缓冲区。
        /// 如果数据尺寸小于缓冲区大小，只更新前 data.Length 字节。
        /// </remarks>
        public void Update(byte[] data)
        {
            #region # 验证

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (data.Length > this.BufferSize)
            {
                throw new ArgumentException($"数据尺寸 {data.Length} 超过缓冲区尺寸 {this.BufferSize}");
            }

            #endregion

            this.Bind();
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, data.Length, data);
            this.Unbind();
        }
        #endregion

        #region 部分更新数据 —— void UpdateSub(byte[] data, int offset)
        /// <summary>
        /// 部分更新数据
        /// </summary>
        /// <param name="data">数据数组</param>
        /// <param name="offset">偏移量（字节）</param>
        /// <remarks>不更新整个缓冲区，只更新从偏移量开始的部分区域。</remarks>
        public void UpdateSub(byte[] data, int offset)
        {
            #region # 验证

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (offset + data.Length > this.BufferSize)
            {
                throw new ArgumentException("数据超出缓冲区范围");
            }

            #endregion

            this.Bind();
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, offset, data.Length, data);
            this.Unbind();
        }
        #endregion

        #region 更新类型数据 —— void Update<T>(T instance)
        /// <summary>
        /// 更新类型数据
        /// </summary>
        /// <typeparam name="T">结构体类型</typeparam>
        /// <param name="instance">结构体实例</param>
        /// <remarks>
        /// 单个结构体
        /// 适用于缓冲区只存储一个结构体的场景。
        /// </remarks>
        public unsafe void Update<T>(T instance) where T : unmanaged
        {
            int bufferSize = Marshal.SizeOf<T>();

            #region # 验证

            if (bufferSize > this.BufferSize)
            {
                throw new ArgumentException($"数据尺寸 {bufferSize} 超过缓冲区尺寸 {this.BufferSize}");
            }

            #endregion

            this.Bind();
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, bufferSize, new IntPtr(&instance));
            this.Unbind();
        }
        #endregion

        #region 批量更新类型数据 —— void UpdateRange<T>(T[] array)
        /// <summary>
        /// 批量更新类型数据
        /// </summary>
        /// <typeparam name="T">结构体类型</typeparam>
        /// <param name="array">结构体列表</param>
        /// <remarks>
        /// 泛型数组全量
        /// 自动计算结构体大小并转换字节数组上传。
        /// T 必须是 blittable 类型（结构体只包含值类型，无引用）。
        /// </remarks>
        public unsafe void UpdateRange<T>(T[] array) where T : unmanaged
        {
            #region # 验证

            if (array == null || array.Length == 0)
            {
                return;
            }

            #endregion

            int bufferSize = array.Length * sizeof(T);

            #region # 验证

            if (bufferSize > this.BufferSize)
            {
                throw new ArgumentException($"数据尺寸 {bufferSize} 超过缓冲区尺寸 {this.BufferSize}");
            }

            #endregion

            this.Bind();
            fixed (T* pointer = array)
            {
                GL.BufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, bufferSize, new IntPtr(pointer));
            }
            this.Unbind();
        }
        #endregion

        #region 读取数据 —— byte[] Read()
        /// <summary>
        /// 读取数据
        /// </summary>
        /// <returns>缓冲区数据</returns>
        /// <remarks>
        /// 从 GPU 缓冲区读回 CPU 内存，会阻塞当前线程直到读取完成。
        /// 适用于读取统计数据、计算结果的典型场景。
        /// </remarks>
        public byte[] Read()
        {
            this.Bind();

            byte[] data = new byte[this.BufferSize];
            IntPtr ptr = GL.MapBuffer(BufferTarget.ShaderStorageBuffer, BufferAccess.ReadOnly);
            if (ptr != IntPtr.Zero)
            {
                try
                {
                    Marshal.Copy(ptr, data, 0, this.BufferSize);
                }
                finally
                {
                    GL.UnmapBuffer(BufferTarget.ShaderStorageBuffer);
                }
            }

            this.Unbind();

            return data;
        }
        #endregion

        #region 读取类型数据 —— T Read<T>()
        /// <summary>
        /// 读取类型数据
        /// </summary>
        /// <typeparam name="T">结构体类型</typeparam>
        /// <returns>结构体实例</returns>
        /// <remarks>适用于缓冲区只存储一个结构体的情况。</remarks>
        public T Read<T>() where T : unmanaged
        {
            byte[] data = this.Read();
            int size = Marshal.SizeOf<T>();

            #region # 验证

            if (data.Length < size)
            {
                throw new InvalidOperationException($"缓冲区尺寸 {data.Length} 小于结构体尺寸 {size}");
            }

            #endregion

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

        #region 批量读取类型数据 —— T[] ReadRange<T>()
        /// <summary>
        /// 批量读取类型数据
        /// </summary>
        /// <typeparam name="T">结构体类型</typeparam>
        /// <returns>结构体列表</returns>
        /// <remarks>
        /// 自动解析字节数组为结构体数组。
        /// T 必须是 blittable 类型，且缓冲区大小必须与 T[] 对齐。
        /// </remarks>
        public unsafe T[] ReadRange<T>() where T : unmanaged
        {
            byte[] buffer = this.Read();
            int elementSize = Marshal.SizeOf<T>();
            int elementsCount = buffer.Length / elementSize;

            T[] result = new T[elementsCount];
            fixed (byte* bufferPtr = buffer)
            fixed (T* resultPtr = result)
            {
                System.Buffer.MemoryCopy(bufferPtr, resultPtr, buffer.Length, buffer.Length);
            }

            return result;
        }
        #endregion

        #region 清空缓冲区 —— void Clear()
        /// <summary>
        /// 清空缓冲区
        /// </summary>
        /// <remarks>将整个缓冲区填充为0</remarks>
        public void Clear()
        {
            this.Bind();
            GL.BufferData(BufferTarget.ShaderStorageBuffer, this.BufferSize, IntPtr.Zero, this.Usage);
            this.Unbind();
        }
        #endregion

        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        /// <remarks>
        /// 删除 OpenGL 缓冲区对象，释放显存。
        /// 调用后 SSBO 不可再使用。
        /// </remarks>
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
