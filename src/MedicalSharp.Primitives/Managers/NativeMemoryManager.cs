using System;
using System.Buffers;

namespace MedicalSharp.Primitives.Managers
{
    /// <summary>
    /// 本地内存管理器
    /// </summary>
    public unsafe class NativeMemoryManager<T> : MemoryManager<T> where T : unmanaged
    {
        #region # 字段及构造器

        /// <summary>
        /// 原始类型指针
        /// </summary>
        private readonly T* _originalPointer;

        /// <summary>
        /// 数据长度
        /// </summary>
        private readonly int _length;

        /// <summary>
        /// 创建本地内存管理器构造器
        /// </summary>
        /// <param name="originalPointer">原始数据指针</param>
        /// <param name="length">元素个数</param>
        public NativeMemoryManager(T* originalPointer, int length)
        {
            this._originalPointer = originalPointer;
            this._length = length;
        }

        /// <summary>
        /// 创建本地内存管理器构造器
        /// </summary>
        /// <param name="originalPointer">原始数据指针</param>
        /// <param name="length">元素个数</param>
        public NativeMemoryManager(IntPtr originalPointer, int length)
        {
            this._originalPointer = (T*)originalPointer.ToPointer();
            this._length = length;
        }

        #endregion

        #region # 属性

        #region 只读属性 - 原始类型指针 —— T* OriginalPointer
        /// <summary>
        /// 只读属性 - 原始类型指针
        /// </summary>
        public T* OriginalPointer
        {
            get => this._originalPointer;
        }
        #endregion

        #region 只读属性 - 数据长度 —— int Length
        /// <summary>
        /// 只读属性 - 数据长度
        /// </summary>
        public int Length
        {
            get => this._length;
        }
        #endregion

        #endregion

        #region # 方法

        #region 获取Span —— override Span<T> GetSpan()
        /// <summary>
        /// 获取Span
        /// </summary>
        /// <returns>Span实例</returns>
        public override Span<T> GetSpan()
        {
            Span<T> span = new Span<T>(this._originalPointer, this._length);

            return span;
        }
        #endregion

        #region 固定内存 —— override MemoryHandle Pin(int elementIndex)
        /// <summary>
        /// 固定内存
        /// </summary>
        /// <param name="elementIndex">元素索引</param>
        /// <returns>内存句柄</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public override MemoryHandle Pin(int elementIndex = 0)
        {
            #region # 验证

            if (elementIndex < 0 || elementIndex >= this._length)
            {
                throw new ArgumentOutOfRangeException(nameof(elementIndex), "元素索引已越界！");
            }

            #endregion

            MemoryHandle handle = new MemoryHandle(this._originalPointer + elementIndex);

            return handle;
        }
        #endregion

        #region 取消固定 —— override void Unpin()
        /// <summary>
        /// 取消固定
        /// </summary>
        public override void Unpin()
        {
            //非托管内存不需要操作
        }
        #endregion

        #region 释放资源 —— override void Dispose(bool disposing)
        /// <summary>
        /// 释放资源
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            //不释放本地内存，由调用方管理生命周期
        }
        #endregion

        #endregion
    }
}
