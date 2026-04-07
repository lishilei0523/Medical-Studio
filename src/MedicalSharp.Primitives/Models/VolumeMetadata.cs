using OpenTK.Mathematics;

namespace MedicalSharp.Primitives.Models
{
    /// <summary>
    /// 体积元数据
    /// </summary>
    public sealed class VolumeMetadata
    {
        #region 默认构造器 —— VolumeMetadata()
        /// <summary>
        /// 默认构造器
        /// </summary>
        internal VolumeMetadata()
        {
            //默认值
            this.RescaleSlope = 1.0f;
            this.RescaleIntercept = 0.0f;
            this.WindowWidth = 400;
            this.WindowCenter = 40;
        }
        #endregion

        #region 体素数量 —— long VoxelsCount
        /// <summary>
        /// 体素数量
        /// </summary>
        public long VoxelsCount { get; set; }
        #endregion

        #region 体积尺寸 —— Vector3i VolumeSize
        /// <summary>
        /// 体积尺寸
        /// </summary>
        public Vector3i VolumeSize { get; set; }
        #endregion

        #region 间距 —— Vector3 Spacing
        /// <summary>
        /// 间距
        /// </summary>
        public Vector3 Spacing { get; set; }
        #endregion

        #region 物理尺寸 —— Size3F PhysicalSize
        /// <summary>
        /// 物理尺寸
        /// </summary>
        public Vector3 PhysicalSize { get; set; }
        #endregion

        #region 体积缩放 —— Vector3 VolumeScale
        /// <summary>
        /// 体积缩放
        /// </summary>
        public Vector3 VolumeScale { get; set; }
        #endregion

        #region 斜率 —— float RescaleSlope
        /// <summary>
        /// 斜率
        /// </summary>
        public float RescaleSlope { get; set; }
        #endregion

        #region 截距 —— float RescaleIntercept
        /// <summary>
        /// 截距
        /// </summary>
        public float RescaleIntercept { get; set; }
        #endregion

        #region 图像原点 —— Vector3 Origin
        /// <summary>
        /// 图像原点
        /// </summary>
        public Vector3 Origin { get; set; }
        #endregion

        #region 行向量 —— Vector3 RowDirection
        /// <summary>
        /// 行向量
        /// </summary>
        public Vector3 RowDirection { get; set; }
        #endregion

        #region 列向量 —— Vector3 ColDirection
        /// <summary>
        /// 列向量
        /// </summary>
        public Vector3 ColDirection { get; set; }
        #endregion

        #region 切面向量 —— Vector3 SliceDirection
        /// <summary>
        /// 切面向量
        /// </summary>
        public Vector3 SliceDirection { get; set; }
        #endregion

        #region 窗宽 —— float WindowWidth
        /// <summary>
        /// 窗宽
        /// </summary>
        public float WindowWidth { get; set; }
        #endregion

        #region 窗位 —— float WindowCenter
        /// <summary>
        /// 窗位
        /// </summary>
        public float WindowCenter { get; set; }
        #endregion
    }
}
