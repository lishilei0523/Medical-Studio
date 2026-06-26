using MedicalSharp.Engine.Base;
using OpenTK.Graphics.OpenGL4;
using System;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 原子计数器缓冲区
    /// </summary>
    public class AtomicCounterBuffer : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 释放标识
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// 创建原子计数器缓冲区构造器
        /// </summary>
        /// <param name="count">计数器数量（默认1）</param>
        public AtomicCounterBuffer(int count = 1)
        {
            #region # 验证

            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "计数器数量必须大于 0！");
            }

            #endregion

            this.Count = count;
            this.BufferSize = count * sizeof(uint);
            this.Id = GL.GenBuffer();

            #region # 验证

            if (this.Id == 0)
            {
                throw new GlException("创建原子计数器缓冲区失败！");
            }

            #endregion

            this.Bind();
            GL.BufferData(BufferTarget.AtomicCounterBuffer, this.BufferSize, IntPtr.Zero, BufferUsageHint.DynamicRead);
            this.Unbind();
        }

        #endregion

        #region # 属性

        #region 原子计数器缓冲区Id —— int Id
        /// <summary>
        /// 原子计数器缓冲区Id
        /// </summary>
        public int Id { get; private set; }
        #endregion

        #region 计数器数量 —— int Count
        /// <summary>
        /// 计数器数量
        /// </summary>
        public int Count { get; private set; }
        #endregion

        #region 缓冲区尺寸 —— int BufferSize
        /// <summary>
        /// 缓冲区尺寸
        /// </summary>
        /// <remarks>单位：字节</remarks>
        public int BufferSize { get; private set; }
        #endregion

        #endregion

        #region # 方法

        #region 绑定原子计数器缓冲区 —— void Bind()
        /// <summary>
        /// 绑定原子计数器缓冲区
        /// </summary>
        public void Bind()
        {
            GL.BindBuffer(BufferTarget.AtomicCounterBuffer, this.Id);
        }
        #endregion

        #region 绑定原子计数器缓冲区 —— void Bind(int bindingPoint)
        /// <summary>
        /// 绑定原子计数器缓冲区
        /// </summary>
        /// <param name="bindingPoint">绑定点索引</param>
        /// <remarks>对应Shader中的layout(binding = N)</remarks>
        public void Bind(int bindingPoint)
        {
            GL.BindBufferBase(BufferRangeTarget.AtomicCounterBuffer, bindingPoint, this.Id);
        }
        #endregion

        #region 解绑原子计数器缓冲区 —— void Unbind()
        /// <summary>
        /// 解绑原子计数器缓冲区
        /// </summary>
        public void Unbind()
        {
            GL.BindBuffer(BufferTarget.AtomicCounterBuffer, 0);
        }
        #endregion

        #region 读取计数值 —— uint ReadValue(int index)
        /// <summary>
        /// 读取计数值
        /// </summary>
        /// <param name="index">计数器索引（默认0）</param>
        /// <returns>计数值</returns>
        public uint ReadValue(int index = 0)
        {
            #region # 验证

            if (index < 0 || index >= this.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"计数器索引超出范围 [0, {this.Count - 1}]！");
            }

            #endregion

            this.Bind();

            uint value = 0;
            GL.GetBufferSubData(BufferTarget.AtomicCounterBuffer, index * sizeof(uint), sizeof(uint), ref value);

            this.Unbind();

            return value;
        }
        #endregion

        #region 重置计数器 —— void Reset()
        /// <summary>
        /// 重置计数器
        /// </summary>
        public void Reset()
        {
            this.Bind();

            uint zero = 0;
            for (int index = 0; index < this.Count; index++)
            {
                GL.BufferSubData(BufferTarget.AtomicCounterBuffer, index * sizeof(uint), sizeof(uint), ref zero);
            }

            this.Unbind();
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
