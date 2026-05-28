using MedicalSharp.Primitives.Enums;
using OpenTK.Mathematics;
using System;
using System.Runtime.InteropServices;
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
        /// 预览同步状态
        /// </summary>
        private int _previewSyncStatus;

        /// <summary>
        /// 标记同步状态
        /// </summary>
        private int _markSyncStatus;

        /// <summary>
        /// 默认构造器
        /// </summary>
        protected VolumeData()
        {
            //默认值
            this._previewSyncStatus = (int)DataSyncStatus.Idle;
            this._markSyncStatus = (int)DataSyncStatus.Idle;
            this.Metadata = new VolumeMetadata();
            this.PatientData = new VolumePatientData();
            this.StudyData = new VolumeStudyData();
            this.SeriesData = new VolumeSeriesData();
            this.ScanData = new VolumeScanData();
        }

        #endregion

        #region # 属性

        #region 体积元数据 —— VolumeMetadata Metadata
        /// <summary>
        /// 体积元数据
        /// </summary>
        public VolumeMetadata Metadata { get; private set; }
        #endregion

        #region 患者数据 —— VolumePatientData PatientData
        /// <summary>
        /// 患者数据
        /// </summary>
        public VolumePatientData PatientData { get; private set; }
        #endregion

        #region 检查数据 —— VolumeStudyData StudyData
        /// <summary>
        /// 检查数据
        /// </summary>
        public VolumeStudyData StudyData { get; private set; }
        #endregion

        #region 序列数据 —— VolumeSeriesData SeriesData
        /// <summary>
        /// 序列数据
        /// </summary>
        public VolumeSeriesData SeriesData { get; private set; }
        #endregion

        #region 扫描数据 —— VolumeScanData ScanData
        /// <summary>
        /// 扫描数据
        /// </summary>
        public VolumeScanData ScanData { get; set; }
        #endregion

        #region 原始数据 —— abstract IntPtr OriginalData
        /// <summary>
        /// 原始数据
        /// </summary>
        public abstract IntPtr OriginalData { get; }
        #endregion

        #region 预览数据 —— abstract IntPtr PreviewData
        /// <summary>
        /// 预览数据
        /// </summary>
        public abstract IntPtr PreviewData { get; }
        #endregion

        #region 标记数据 —— abstract IntPtr MarkData
        /// <summary>
        /// 标记数据
        /// </summary>
        public abstract IntPtr MarkData { get; }
        #endregion

        #region 只读属性 - 预览同步状态 —— DataSyncStatus PreviewSyncStatus
        /// <summary>
        /// 只读属性 - 预览同步状态
        /// </summary>
        public DataSyncStatus PreviewSyncStatus
        {
            get => (DataSyncStatus)Interlocked.CompareExchange(ref this._previewSyncStatus, 0, 0);
        }
        #endregion

        #region 只读属性 - 是否可读取预览 —— bool CanReadPreviewData
        /// <summary>
        /// 只读属性 - 是否可读取预览
        /// </summary>
        public bool CanReadPreviewData
        {
            get => this.PreviewSyncStatus == DataSyncStatus.Idle;
        }

        #endregion

        #region 只读属性 - 是否可写入预览 —— bool CanWritePreviewData
        /// <summary>
        /// 只读属性 - 是否可写入预览
        /// </summary>
        public bool CanWritePreviewData
        {
            get => this.PreviewSyncStatus == DataSyncStatus.Idle;
        }
        #endregion

        #region 只读属性 - 标记同步状态 —— DataSyncStatus MarkSyncStatus
        /// <summary>
        /// 只读属性 - 标记同步状态
        /// </summary>
        public DataSyncStatus MarkSyncStatus
        {
            get => (DataSyncStatus)Interlocked.CompareExchange(ref this._markSyncStatus, 0, 0);
        }
        #endregion

        #region 只读属性 - 是否可读取标记 —— bool CanReadMarkData
        /// <summary>
        /// 只读属性 - 是否可读取标记
        /// </summary>
        public bool CanReadMarkData
        {
            get => this.MarkSyncStatus == DataSyncStatus.Idle;
        }

        #endregion

        #region 只读属性 - 是否可写入标记 —— bool CanWriteMarkData
        /// <summary>
        /// 只读属性 - 是否可写入标记
        /// </summary>
        public bool CanWriteMarkData
        {
            get => this.MarkSyncStatus == DataSyncStatus.Idle;
        }
        #endregion

        #endregion

        #region # 方法

        #region 获取原始值 —— short GetOriginalValue(Vector3i position)
        /// <summary>
        /// 获取原始值
        /// </summary>
        /// <param name="position">位置</param>
        /// <returns>体素值</returns>
        public unsafe short GetOriginalValue(Vector3i position)
        {
            if (position.X < 0 || position.X >= this.Metadata.VolumeSize.X ||
                position.Y < 0 || position.Y >= this.Metadata.VolumeSize.Y ||
                position.Z < 0 || position.Z >= this.Metadata.VolumeSize.Z)
            {
                return 0;
            }

            int index = position.Z * this.Metadata.VolumeSize.X * this.Metadata.VolumeSize.Y +
                        position.Y * this.Metadata.VolumeSize.X +
                        position.X;
            short* pointer = (short*)this.OriginalData.ToPointer();
            short voxel = pointer[index];

            return voxel;
        }
        #endregion

        #region 获取预览值 —— short GetPreviewValue(Vector3i position)
        /// <summary>
        /// 获取预览值
        /// </summary>
        /// <param name="position">位置</param>
        /// <returns>体素值</returns>
        public unsafe short GetPreviewValue(Vector3i position)
        {
            if (position.X < 0 || position.X >= this.Metadata.VolumeSize.X ||
                position.Y < 0 || position.Y >= this.Metadata.VolumeSize.Y ||
                position.Z < 0 || position.Z >= this.Metadata.VolumeSize.Z)
            {
                return 0;
            }

            int index = position.Z * this.Metadata.VolumeSize.X * this.Metadata.VolumeSize.Y +
                        position.Y * this.Metadata.VolumeSize.X +
                        position.X;
            short* pointer = (short*)this.PreviewData.ToPointer();
            short voxel = pointer[index];

            return voxel;
        }
        #endregion

        #region 获取标记值 —— byte GetMarkValue(Vector3i position)
        /// <summary>
        /// 获取标记值
        /// </summary>
        /// <param name="position">位置</param>
        /// <returns>标记值</returns>
        public unsafe byte GetMarkValue(Vector3i position)
        {
            if (position.X < 0 || position.X >= this.Metadata.VolumeSize.X ||
                position.Y < 0 || position.Y >= this.Metadata.VolumeSize.Y ||
                position.Z < 0 || position.Z >= this.Metadata.VolumeSize.Z)
            {
                return 0;
            }

            int index = position.Z * this.Metadata.VolumeSize.X * this.Metadata.VolumeSize.Y +
                        position.Y * this.Metadata.VolumeSize.X +
                        position.X;
            byte* pointer = (byte*)this.MarkData.ToPointer();
            byte markValue = pointer[index];

            return markValue;
        }
        #endregion

        #region 重置预览数据 —— void ResetPreviewData()
        /// <summary>
        /// 重置预览数据
        /// </summary>
        public unsafe void ResetPreviewData()
        {
            #region # 验证

            if (this.OriginalData == IntPtr.Zero)
            {
                throw new InvalidOperationException("原始数据未分配！");
            }
            if (this.PreviewData == IntPtr.Zero)
            {
                throw new InvalidOperationException("预览数据未分配！");
            }

            #endregion

            int size = this.Metadata.VolumeSize.X *
                           this.Metadata.VolumeSize.Y *
                           this.Metadata.VolumeSize.Z *
                           sizeof(short);

            //直接内存复制
            Buffer.MemoryCopy(this.OriginalData.ToPointer(), this.PreviewData.ToPointer(), size, size);
        }
        #endregion

        #region 重置标记数据 —— void ResetMarkData()
        /// <summary>
        /// 重置标记数据
        /// </summary>
        public unsafe void ResetMarkData()
        {
            #region # 验证

            if (this.MarkData == IntPtr.Zero)
            {
                throw new InvalidOperationException("标记数据未分配！");
            }

            #endregion

            int size = this.Metadata.VolumeSize.X *
                       this.Metadata.VolumeSize.Y *
                       this.Metadata.VolumeSize.Z *
                       sizeof(byte);

            //清空内存
            NativeMemory.Clear(this.MarkData.ToPointer(), (UIntPtr)size);
        }
        #endregion

        #region 重置标记值 —— void ResetMarkValue(byte targetMarkValue)
        /// <summary>
        /// 重置标记值
        /// </summary>
        /// <param name="targetMarkValue">目标标记值（1~255）</param>
        /// <remarks>将给定标记值重置为0</remarks>
        public unsafe void ResetMarkValue(byte targetMarkValue)
        {
            #region # 验证

            if (targetMarkValue == 0)
            {
                return;
            }

            #endregion

            int size = this.Metadata.VolumeSize.X *
                       this.Metadata.VolumeSize.Y *
                       this.Metadata.VolumeSize.Z *
                       sizeof(byte);

            //遍历重置
            byte* pointer = (byte*)this.MarkData.ToPointer();
            for (int index = 0; index < size; index++)
            {
                if (pointer[index] == targetMarkValue)
                {
                    pointer[index] = 0;
                }
            }
        }
        #endregion

        #region 尝试开始GPU->CPU同步预览 —— bool TryBeginPreviewGpuToCpu()
        /// <summary>
        /// 尝试开始GPU->CPU同步预览
        /// </summary>
        /// <returns>是否成功开始</returns>
        public bool TryBeginPreviewGpuToCpu()
        {
            int status = Interlocked.CompareExchange(ref this._previewSyncStatus, (int)DataSyncStatus.GpuToCpu, (int)DataSyncStatus.Idle);

            return status == (int)DataSyncStatus.Idle;
        }
        #endregion

        #region 尝试开始CPU->GPU同步预览 —— bool TryBeginPreviewCpuToGpu()
        /// <summary>
        /// 尝试开始CPU->GPU同步预览
        /// </summary>
        /// <returns>是否成功开始</returns>
        public bool TryBeginPreviewCpuToGpu()
        {
            int status = Interlocked.CompareExchange(ref this._previewSyncStatus, (int)DataSyncStatus.CpuToGpu, (int)DataSyncStatus.Idle);

            return status == (int)DataSyncStatus.Idle;
        }
        #endregion

        #region 结束同步预览 —— void EndPreviewSync()
        /// <summary>
        /// 结束同步预览
        /// </summary>
        /// <repreviews>无论哪种方向</repreviews>
        public void EndPreviewSync()
        {
            Interlocked.Exchange(ref this._previewSyncStatus, (int)DataSyncStatus.Idle);
        }
        #endregion

        #region 尝试开始GPU->CPU同步标记 —— bool TryBeginMarkGpuToCpu()
        /// <summary>
        /// 尝试开始GPU->CPU同步标记
        /// </summary>
        /// <returns>是否成功开始</returns>
        public bool TryBeginMarkGpuToCpu()
        {
            int status = Interlocked.CompareExchange(ref this._markSyncStatus, (int)DataSyncStatus.GpuToCpu, (int)DataSyncStatus.Idle);

            return status == (int)DataSyncStatus.Idle;
        }
        #endregion

        #region 尝试开始CPU->GPU同步标记 —— bool TryBeginMarkCpuToGpu()
        /// <summary>
        /// 尝试开始CPU->GPU同步标记
        /// </summary>
        /// <returns>是否成功开始</returns>
        public bool TryBeginMarkCpuToGpu()
        {
            int status = Interlocked.CompareExchange(ref this._markSyncStatus, (int)DataSyncStatus.CpuToGpu, (int)DataSyncStatus.Idle);

            return status == (int)DataSyncStatus.Idle;
        }
        #endregion

        #region 结束同步标记 —— void EndMarkSync()
        /// <summary>
        /// 结束同步标记
        /// </summary>
        /// <remarks>无论哪种方向</remarks>
        public void EndMarkSync()
        {
            Interlocked.Exchange(ref this._markSyncStatus, (int)DataSyncStatus.Idle);
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
