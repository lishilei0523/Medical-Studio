using MedicalSharp.Engine.Resources;
using MedicalSharp.Engine.ValueTypes;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;

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
        /// <param name="volumeTexture">体积纹理</param>
        /// <param name="volumeSize">体素尺寸</param>
        /// <param name="spacing">间距</param>
        /// <param name="physicalSize">实际尺寸</param>
        /// <param name="volumeScale">体积缩放</param>
        /// <param name="rescaleSlope">斜率</param>
        /// <param name="rescaleIntercept">截距</param>
        /// <param name="origin">图像原点</param>
        /// <param name="rowDirection">行向量</param>
        /// <param name="colDirection">列向量</param>
        /// <param name="sliceDirection">切面向量</param>
        public VolumeRenderable(Texture3D volumeTexture, Vector3 volumeSize, Vector3 spacing, Vector3 physicalSize, Vector3 volumeScale, float rescaleSlope, float rescaleIntercept, Vector3 origin, Vector3 rowDirection, Vector3 colDirection, Vector3 sliceDirection)
        {
            this.VolumeTexture = volumeTexture;
            this.VolumeSize = volumeSize;
            this.Spacing = spacing;
            this.PhysicalSize = physicalSize;
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

        #region 体积纹理 —— Texture3D VolumeTexture
        /// <summary>
        /// 体积纹理
        /// </summary>
        public Texture3D VolumeTexture { get; private set; }
        #endregion

        #region 体积尺寸 —— Vector3 VolumeSize
        /// <summary>
        /// 体积尺寸
        /// </summary>
        public Vector3 VolumeSize { get; private set; }
        #endregion

        #region 间距 —— Vector3 Spacing
        /// <summary>
        /// 间距
        /// </summary>
        public Vector3 Spacing { get; internal set; }
        #endregion

        #region 物理尺寸 —— Vector3 PhysicalSize
        /// <summary>
        /// 物理尺寸
        /// </summary>
        public Vector3 PhysicalSize { get; private set; }
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

        #region # 方法

        #region 计算包围盒 —— override BoundingBox CalculateBoundingBox()
        /// <summary>
        /// 计算包围盒
        /// </summary>
        protected override BoundingBox CalculateBoundingBox()
        {
            Matrix4 scaleMatrix = Matrix4.CreateScale(this.VolumeScale);
            IEnumerable<Vector3> originalPositions = ResourceManager.UnitCube.Vertices.Select(x => x.Position);
            IEnumerable<Vector3> localPositions = originalPositions.Select(position => Vector3.TransformPosition(position, scaleMatrix));
            BoundingBox boundingBox = BoundingBox.FromPoints([.. localPositions]);

            return boundingBox;
        }
        #endregion

        #endregion
    }
}
