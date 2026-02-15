using itk.simple;
using System;
using System.Numerics;

namespace MedicalSharp.Dicoms.Models
{
    /// <summary>
    /// 体积数据
    /// </summary>
    public unsafe class VolumeData : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 无参构造器
        /// </summary>
        private VolumeData()
        {
            //默认值
            this.RescaleSlope = 1.0f;
            this.RescaleIntercept = 0.0f;
            this.WindowWidth = 400;
            this.WindowCenter = 40;
        }

        /// <summary>
        /// 创建体积数据构造器
        /// </summary>
        /// <param name="sitkImage">SimpleITK图像</param>
        public VolumeData(Image sitkImage)
            : this()
        {
            this.SitkImage = sitkImage;
        }

        /// <summary>
        /// 析构器
        /// </summary>
        ~VolumeData()
        {
            this.SitkImage?.Dispose();
        }

        #endregion

        #region # 属性

        #region 原始数据 —— short* OriginalData
        /// <summary>
        /// 原始数据
        /// </summary>
        public short* OriginalData { get; internal set; }
        #endregion

        #region 体素数量 —— long VoxelsCount
        /// <summary>
        /// 体素数量
        /// </summary>
        public long VoxelsCount { get; internal set; }
        #endregion

        #region 体积尺寸 —— Vector3 VolumeSize
        /// <summary>
        /// 体积尺寸
        /// </summary>
        public Vector3 VolumeSize { get; internal set; }
        #endregion

        #region 间距尺寸 —— Vector3 Spacing
        /// <summary>
        /// 间距尺寸
        /// </summary>
        public Vector3 Spacing { get; internal set; }
        #endregion

        #region 实际尺寸 —— Vector3 ActualSize
        /// <summary>
        /// 实际尺寸
        /// </summary>
        public Vector3 ActualSize { get; internal set; }
        #endregion

        #region 体积缩放 —— Vector3 VolumeScale
        /// <summary>
        /// 体积缩放
        /// </summary>
        public Vector3 VolumeScale { get; internal set; }
        #endregion

        #region 斜率 —— float RescaleSlope
        /// <summary>
        /// 斜率
        /// </summary>
        public float RescaleSlope { get; internal set; }
        #endregion

        #region 截距 —— float RescaleIntercept
        /// <summary>
        /// 截距
        /// </summary>
        public float RescaleIntercept { get; internal set; }
        #endregion

        #region 图像原点 —— Vector3 Origin
        /// <summary>
        /// 图像原点
        /// </summary>
        public Vector3 Origin { get; internal set; }
        #endregion

        #region 行向量 —— Vector3 RowDirection
        /// <summary>
        /// 行向量
        /// </summary>
        public Vector3 RowDirection { get; internal set; }
        #endregion

        #region 列向量 —— Vector3 ColDirection
        /// <summary>
        /// 列向量
        /// </summary>
        public Vector3 ColDirection { get; internal set; }
        #endregion

        #region 切面向量 —— Vector3 SliceDirection
        /// <summary>
        /// 切面向量
        /// </summary>
        public Vector3 SliceDirection { get; internal set; }
        #endregion

        #region 窗宽 —— float WindowWidth
        /// <summary>
        /// 窗宽
        /// </summary>
        public float WindowWidth { get; internal set; }
        #endregion

        #region 窗位 —— float WindowCenter
        /// <summary>
        /// 窗位
        /// </summary>
        public float WindowCenter { get; internal set; }
        #endregion

        #region SimpleITK图像 —— Image SitkImage
        /// <summary>
        /// SimpleITK图像
        /// </summary>
        public Image SitkImage { get; private set; }
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
        public short this[int x, int y, int z]
        {
            get
            {
                if (x < 0 || x >= this.VolumeSize.X || y < 0 || y >= this.VolumeSize.Y || z < 0 || z >= this.VolumeSize.Z)
                {
                    return 0;
                }

                int index = z * (int)this.VolumeSize.X * (int)this.VolumeSize.Y + y * (int)this.VolumeSize.X + x;
                short voxel = this.OriginalData[index];

                return voxel;
            }
        }
        #endregion

        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this.SitkImage?.Dispose();
        }
        #endregion 

        #endregion
    }
}
