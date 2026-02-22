using MedicalSharp.Engine.Resources;
using OpenTK.Mathematics;

namespace MedicalSharp.Engine.Renderables
{
    /// <summary>
    /// 体积渲染对象
    /// </summary>
    public class VolumeRenderable : Renderable
    {
        #region # 字段及构造器

        /// <summary>
        /// 创建体积渲染对象构造器
        /// </summary>
        /// <param name="texture3D">3D纹理</param>
        /// <param name="volumeScale">体积缩放</param>
        /// <param name="rescaleSlope">斜率</param>
        /// <param name="rescaleIntercept">截距</param>
        /// <param name="origin">图像原点</param>
        /// <param name="rowDirection">行向量</param>
        /// <param name="colDirection">列向量</param>
        /// <param name="sliceDirection">切面向量</param>
        public VolumeRenderable(Texture3D texture3D, Vector3 volumeScale, float rescaleSlope, float rescaleIntercept, Vector3 origin, Vector3 rowDirection, Vector3 colDirection, Vector3 sliceDirection)
        {
            this.Texture3D = texture3D;
            this.VolumeScale = volumeScale;
            this.RescaleSlope = rescaleSlope;
            this.RescaleIntercept = rescaleIntercept;
            this.Origin = origin;
            this.RowDirection = rowDirection;
            this.ColDirection = colDirection;
            this.SliceDirection = sliceDirection;
        }

        #endregion

        #region # 属性

        #region 3D纹理 —— Texture3D Texture3D
        /// <summary>
        /// 3D纹理
        /// </summary>
        public Texture3D Texture3D { get; private set; }
        #endregion

        #region 体积缩放 —— Vector3 VolumeScale
        /// <summary>
        /// 体积缩放
        /// </summary>
        public Vector3 VolumeScale { get; private set; }
        #endregion

        #region 斜率 —— float RescaleSlope
        /// <summary>
        /// 斜率
        /// </summary>
        public float RescaleSlope { get; private set; }
        #endregion

        #region 截距 —— float RescaleIntercept
        /// <summary>
        /// 截距
        /// </summary>
        public float RescaleIntercept { get; private set; }
        #endregion

        #region 图像原点 —— Vector3 Origin
        /// <summary>
        /// 图像原点
        /// </summary>
        public Vector3 Origin { get; private set; }
        #endregion

        #region 行向量 —— Vector3 RowDirection
        /// <summary>
        /// 行向量
        /// </summary>
        public Vector3 RowDirection { get; private set; }
        #endregion

        #region 列向量 —— Vector3 ColDirection
        /// <summary>
        /// 列向量
        /// </summary>
        public Vector3 ColDirection { get; private set; }
        #endregion

        #region 切面向量 —— Vector3 SliceDirection
        /// <summary>
        /// 切面向量
        /// </summary>
        public Vector3 SliceDirection { get; private set; }
        #endregion 

        #endregion
    }
}
