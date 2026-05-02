using MedicalSharp.Primitives.Enums;
using System;
using System.Threading;

namespace MedicalSharp.Primitives.Models
{
    /// <summary>
    /// 体积数据
    /// </summary>
    public abstract class VolumeData : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 释放标识
        /// </summary>
        protected bool _disposed;

        /// <summary>
        /// 标记同步状态
        /// </summary>
        private int _syncStatus;

        /// <summary>
        /// 默认构造器
        /// </summary>
        protected VolumeData()
        {
            //默认值
            this._syncStatus = (int)MarkSyncStatus.Idle;
            this.Metadata = new VolumeMetadata();
        }

        #endregion

        #region # 属性

        #region 体积信息 —— VolumeInfo Metadata
        /// <summary>
        /// 体积信息
        /// </summary>
        public VolumeMetadata Metadata { get; private set; }
        #endregion

        #region 原始数据 —— abstract IntPtr OriginalData
        /// <summary>
        /// 原始数据
        /// </summary>
        public abstract IntPtr OriginalData { get; }
        #endregion

        #region 标记数据 —— abstract IntPtr MarkData
        /// <summary>
        /// 标记数据
        /// </summary>
        public abstract IntPtr MarkData { get; }
        #endregion

        #region 只读属性 - 标记同步状态 —— MarkSyncStatus MarkSyncStatus
        /// <summary>
        /// 只读属性 - 标记同步状态
        /// </summary>
        public MarkSyncStatus MarkSyncStatus
        {
            get => (MarkSyncStatus)Interlocked.CompareExchange(ref this._syncStatus, 0, 0);
        }
        #endregion

        #region 只读属性 - 是否可读取标记 —— bool CanReadMarkData
        /// <summary>
        /// 只读属性 - 是否可读取标记
        /// </summary>
        public bool CanReadMarkData
        {
            get => this.MarkSyncStatus == MarkSyncStatus.Idle;
        }

        #endregion

        #region 只读属性 - 是否可写入标记 —— bool CanWriteMarkData
        /// <summary>
        /// 只读属性 - 是否可写入标记
        /// </summary>
        public bool CanWriteMarkData
        {
            get => this.MarkSyncStatus == MarkSyncStatus.Idle;
        }
        #endregion

        #endregion

        #region # 方法

        #region 获取体素值 —— short this[int x, int y, int z]
        /// <summary>
        /// 获取体素值
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="z">Z坐标</param>
        /// <returns>体素值</returns>
        public unsafe short this[int x, int y, int z]
        {
            get
            {
                if (x < 0 || x >= this.Metadata.VolumeSize.X || y < 0 || y >= this.Metadata.VolumeSize.Y || z < 0 || z >= this.Metadata.VolumeSize.Z)
                {
                    return 0;
                }

                int index = z * this.Metadata.VolumeSize.X * this.Metadata.VolumeSize.Y + y * this.Metadata.VolumeSize.X + x;
                short* pointer = (short*)this.OriginalData.ToPointer();
                short voxel = pointer[index];

                return voxel;
            }
        }
        #endregion

        #region 获取标记值 —— byte GetMarkValue(int x, int y, int z)
        /// <summary>
        /// 获取标记值
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="z">Z坐标</param>
        /// <returns>标记值</returns>
        public unsafe byte GetMarkValue(int x, int y, int z)
        {
            if (x < 0 || x >= this.Metadata.VolumeSize.X || y < 0 || y >= this.Metadata.VolumeSize.Y || z < 0 || z >= this.Metadata.VolumeSize.Z)
            {
                return 0;
            }

            int index = z * this.Metadata.VolumeSize.X * this.Metadata.VolumeSize.Y + y * this.Metadata.VolumeSize.X + x;
            byte* pointer = (byte*)this.MarkData.ToPointer();
            byte markValue = pointer[index];

            return markValue;
        }
        #endregion

        #region 开始GPU->CPU同步 —— void BeginGpuToCpu()
        /// <summary>
        /// 开始GPU->CPU同步
        /// </summary>
        /// <remarks>调用前应确保UI层已防止重复操作</remarks>
        public void BeginGpuToCpu()
        {
            int previous = Interlocked.Exchange(ref this._syncStatus, (int)MarkSyncStatus.GpuToCpu);
#if DEBUG
            //调试模式下检查状态（Release时可移除）
            if (previous != (int)MarkSyncStatus.Idle)
            {
                string message = $"同步状态异常：当前状态为 {this.MarkSyncStatus}，无法开始GPU->CPU同步。这通常是由于UI层未正确防止重复操作所致。";
                throw new InvalidOperationException(message);
            }
#endif
        }
        #endregion

        #region 开始CPU->GPU同步 —— void BeginCpuToGpu()
        /// <summary>
        /// 开始CPU->GPU同步
        /// </summary>
        /// <remarks>调用前应确保UI层已防止重复操作</remarks>
        public void BeginCpuToGpu()
        {
            int previous = Interlocked.Exchange(ref this._syncStatus, (int)MarkSyncStatus.CpuToGpu);
#if DEBUG
            //调试模式下检查状态（Release 时可移除）
            if (previous != (int)MarkSyncStatus.Idle)
            {
                string message = $"同步状态异常：当前状态为 {this.MarkSyncStatus}，无法开始CPU->GPU同步。这通常是由于UI层未正确防止重复操作所致。";
                throw new InvalidOperationException(message);
            }
#endif
        }
        #endregion

        #region 尝试开始GPU->CPU同步 —— bool TryBeginGpuToCpu()
        /// <summary>
        /// 尝试开始GPU->CPU同步
        /// </summary>
        /// <returns>是否成功开始</returns>
        public bool TryBeginGpuToCpu()
        {
            int status = Interlocked.CompareExchange(ref this._syncStatus, (int)MarkSyncStatus.GpuToCpu, (int)MarkSyncStatus.Idle);

            return status == (int)MarkSyncStatus.Idle;
        }
        #endregion

        #region 尝试开始CPU->GPU同步 —— bool TryBeginCpuToGpu()
        /// <summary>
        /// 尝试开始CPU->GPU同步
        /// </summary>
        /// <returns>是否成功开始</returns>
        public bool TryBeginCpuToGpu()
        {
            int status = Interlocked.CompareExchange(ref this._syncStatus, (int)MarkSyncStatus.CpuToGpu, (int)MarkSyncStatus.Idle);

            return status == (int)MarkSyncStatus.Idle;
        }
        #endregion

        #region 结束同步 —— void EndSync()
        /// <summary>
        /// 结束同步（无论哪种方向）
        /// </summary>
        public void EndSync()
        {
            Interlocked.Exchange(ref this._syncStatus, (int)MarkSyncStatus.Idle);
        }
        #endregion

        #region 释放资源 —— virtual void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {

        }
        #endregion

        #endregion
    }
}
