using System;

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
        /// 默认构造器
        /// </summary>
        protected VolumeData()
        {
            //默认值
            this.Id = Guid.NewGuid().ToString();
            this.Metadata = new VolumeMetadata();
        }

        #endregion

        #region # 属性

        #region 标识Id —— string Id
        /// <summary>
        /// 标识Id
        /// </summary>
        public string Id { get; protected set; }
        #endregion

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

        #region 释放资源 —— abstract void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public abstract void Dispose();
        #endregion

        #endregion
    }
}
