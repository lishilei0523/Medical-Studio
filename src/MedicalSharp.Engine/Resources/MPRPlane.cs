using MedicalSharp.Engine.ValueTypes;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// MPR平面
    /// </summary>
    public class MPRPlane
    {
        #region # 字段及构造器

        /// <summary>
        /// 平面变化事件
        /// </summary>
        public event Action<MPRPlane> PlaneChangedEvent;

        /// <summary>
        /// 体积尺寸
        /// </summary>
        private readonly Vector3i _volumeSize;

        /// <summary>
        /// 间距
        /// </summary>
        private readonly Vector3 _spacing;

        /// <summary>
        /// 物理尺寸
        /// </summary>
        private readonly Vector3 _physicalSize;

        /// <summary>
        /// 体积缩放
        /// </summary>
        private readonly Vector3 _volumeScale;

        /// <summary>
        /// 创建MPR平面构造器
        /// </summary>
        /// <param name="volumeSize">体积尺寸</param>
        /// <param name="spacing">间距</param>
        /// <param name="physicalSize">物理尺寸</param>
        /// <param name="volumeScale">体积缩放</param>
        private MPRPlane(Vector3i volumeSize, Vector3 spacing, Vector3 physicalSize, Vector3 volumeScale)
        {
            this._volumeSize = volumeSize;
            this._spacing = spacing;
            this._physicalSize = physicalSize;
            this._volumeScale = volumeScale;
        }

        #endregion

        #region # 属性

        #region 平面中心 —— Vector3 Center
        /// <summary>
        /// 平面中心
        /// </summary>
        public Vector3 Center { get; private set; }
        #endregion

        #region U轴方向 —— Vector3 UAxis
        /// <summary>
        /// U轴方向
        /// </summary>
        /// <remarks>水平</remarks>
        public Vector3 UAxis { get; private set; }
        #endregion

        #region V轴方向 —— Vector3 VAxis
        /// <summary>
        /// V轴方向
        /// </summary>
        /// <remarks>垂直</remarks>
        public Vector3 VAxis { get; private set; }
        #endregion

        #region 法向量 —— Vector3 Normal
        /// <summary>
        /// 法向量
        /// </summary>
        public Vector3 Normal { get; private set; }
        #endregion

        #region 平面类型 —— MPRPlaneType PlaneType
        /// <summary>
        /// 平面类型
        /// </summary>
        public MPRPlaneType PlaneType { get; private set; }
        #endregion

        #region 原始平面类型 —— MPRPlaneType OriginalPlaneType
        /// <summary>
        /// 原始平面类型
        /// </summary>
        public MPRPlaneType OriginalPlaneType { get; private set; }
        #endregion

        #region 切片数量 —— int SlicesCount
        /// <summary>
        /// 切片数量
        /// </summary>
        public int SlicesCount { get; private set; }
        #endregion

        #region 切片索引 —— int SliceIndex
        /// <summary>
        /// 切片索引
        /// </summary>
        private int _sliceIndex;

        /// <summary>
        /// 切片索引
        /// </summary>
        public int SliceIndex
        {
            get => this._sliceIndex;
            set
            {
                int sliceIndex = Math.Clamp(value, 0, this.SlicesCount - 1);
                if (this._sliceIndex != sliceIndex)
                {
                    this._sliceIndex = sliceIndex;
                    this.OnChanged();
                }
            }
        }
        #endregion

        #endregion

        #region # 方法

        //Static

        #region 创建横断面 —— static MPRPlane CreateAxialPlane(Vector3i volumeSize...
        /// <summary>
        /// 创建横断面
        /// </summary>
        /// <param name="volumeSize">体积尺寸</param>
        /// <param name="spacing">间距</param>
        /// <param name="physicalSize">物理尺寸</param>
        /// <param name="volumeScale">体积缩放</param>
        /// <returns>横断面</returns>
        public static MPRPlane CreateAxialPlane(Vector3i volumeSize, Vector3 spacing, Vector3 physicalSize, Vector3 volumeScale)
        {
            MPRPlane plane = new MPRPlane(volumeSize, spacing, physicalSize, volumeScale)
            {
                Center = Vector3.Zero,
                UAxis = new Vector3(1, 0, 0),
                VAxis = new Vector3(0, 1, 0),
                Normal = new Vector3(0, 0, 1),
                PlaneType = MPRPlaneType.Axial,
                OriginalPlaneType = MPRPlaneType.Axial,
                SlicesCount = volumeSize.Z,
                SliceIndex = volumeSize.Z / 2
            };

            return plane;
        }
        #endregion

        #region 创建冠状面 —— static MPRPlane CreateCoronal(Vector3i volumeSize...
        /// <summary>
        /// 创建冠状面
        /// </summary>
        /// <param name="volumeSize">体积尺寸</param>
        /// <param name="spacing">间距</param>
        /// <param name="physicalSize">物理尺寸</param>
        /// <param name="volumeScale">体积缩放</param>
        /// <returns>冠状面</returns>
        public static MPRPlane CreateCoronal(Vector3i volumeSize, Vector3 spacing, Vector3 physicalSize, Vector3 volumeScale)
        {
            MPRPlane plane = new MPRPlane(volumeSize, spacing, physicalSize, volumeScale)
            {
                Center = Vector3.Zero,
                UAxis = new Vector3(1, 0, 0),
                VAxis = new Vector3(0, 0, 1),
                Normal = new Vector3(0, 1, 0),
                PlaneType = MPRPlaneType.Coronal,
                OriginalPlaneType = MPRPlaneType.Coronal,
                SlicesCount = volumeSize.Y,
                SliceIndex = volumeSize.Y / 2
            };

            return plane;
        }
        #endregion

        #region 创建矢状面 —— static MPRPlane CreateSagittal(Vector3i volumeSize...
        /// <summary>
        /// 创建矢状面
        /// </summary>
        /// <param name="volumeSize">体积尺寸</param>
        /// <param name="spacing">间距</param>
        /// <param name="physicalSize">物理尺寸</param>
        /// <param name="volumeScale">体积缩放</param>
        /// <returns>矢状面</returns>
        public static MPRPlane CreateSagittal(Vector3i volumeSize, Vector3 spacing, Vector3 physicalSize, Vector3 volumeScale)
        {
            MPRPlane plane = new MPRPlane(volumeSize, spacing, physicalSize, volumeScale)
            {
                Center = Vector3.Zero,
                UAxis = new Vector3(0, 1, 0),
                VAxis = new Vector3(0, 0, 1),
                Normal = new Vector3(1, 0, 0),
                PlaneType = MPRPlaneType.Sagittal,
                OriginalPlaneType = MPRPlaneType.Sagittal,
                SlicesCount = volumeSize.X,
                SliceIndex = volumeSize.X / 2
            };

            return plane;
        }
        #endregion


        //Public

        #region 获取模型矩阵 —— Matrix4 GetModelMatrix()
        /// <summary>
        /// 获取模型矩阵
        /// </summary>
        /// <returns>模型矩阵</returns>
        public Matrix4 GetModelMatrix()
        {
            const float halfSize = 0.5f;

            //切片偏移：逻辑空间 -0.5 到 0.5
            float sliceOffset = this.GetSliceOffset();

            //平面中心：逻辑空间 (0,0,0) → 世界空间 (0,0,0)
            Vector3 worldCenter = Vector3.Zero;

            //法线方向的世界偏移 = 逻辑偏移 × volumeScale
            Vector3 worldOffset = new Vector3(
                this.Normal.X * sliceOffset * this._volumeScale.X,
                this.Normal.Y * sliceOffset * this._volumeScale.Y,
                this.Normal.Z * sliceOffset * this._volumeScale.Z
            );

            //U/V轴：逻辑方向 × volumeScale，然后归一化
            Vector3 worldUAxis = new Vector3(
                this.UAxis.X * this._volumeScale.X,
                this.UAxis.Y * this._volumeScale.Y,
                this.UAxis.Z * this._volumeScale.Z
            ).Normalized();
            Vector3 worldVAxis = new Vector3(
                this.VAxis.X * this._volumeScale.X,
                this.VAxis.Y * this._volumeScale.Y,
                this.VAxis.Z * this._volumeScale.Z
            ).Normalized();

            //法线方向（重新正交化确保正确）
            Vector3 worldNormal = Vector3.Cross(worldUAxis, worldVAxis).Normalized();

            Matrix4 translation = Matrix4.CreateTranslation(worldCenter + worldOffset);
            Matrix4 basis = new Matrix4(
                new Vector4(worldUAxis * halfSize, 0),
                new Vector4(worldVAxis * halfSize, 0),
                new Vector4(worldNormal, 0),
                new Vector4(0, 0, 0, 1)
            );

            return basis * translation;
        }
        #endregion

        #region 获取平面上的点（逻辑空间） —— Vector3 GetPointOnPlane(float u, float v)
        /// <summary>
        /// 获取平面上的点（逻辑空间）
        /// </summary>
        /// <param name="u">U坐标，范围 -1 到 1</param>
        /// <param name="v">V坐标，范围 -1 到 1</param>
        /// <returns>逻辑空间中的点，范围 -0.5 到 0.5</returns>
        public Vector3 GetPointOnPlane(float u, float v)
        {
            const float halfSize = 0.5f;
            float sliceOffset = this.GetSliceOffset();

            //平面上的点 = 中心 + 法线方向偏移 + U方向偏移 + V方向偏移
            Vector3 point = this.Center + this.Normal * sliceOffset +
                            this.UAxis * u * halfSize +
                            this.VAxis * v * halfSize;

            return point;
        }
        #endregion

        #region 将点投影到平面（逻辑空间） —— Vector2 ProjectPoint(Vector3 point)
        /// <summary>
        /// 将点投影到平面（逻辑空间）
        /// </summary>
        /// <param name="point">逻辑空间中的点，范围 -0.5 到 0.5</param>
        /// <returns>平面UV坐标，范围 -1 到 1</returns>
        public Vector2 ProjectPoint(Vector3 point)
        {
            const float halfSize = 0.5f;
            float sliceOffset = this.GetSliceOffset();

            //计算相对于平面中心的偏移
            Vector3 relative = point - (this.Center + this.Normal * sliceOffset);

            //投影到U轴和V轴
            float u = Vector3.Dot(relative, this.UAxis) / halfSize;
            float v = Vector3.Dot(relative, this.VAxis) / halfSize;

            return new Vector2(u, v);
        }
        #endregion

        #region 获取平面上的体素坐标 —— Vector3i GetVoxelOnPlane(float u, float v)
        /// <summary>
        /// 获取平面上的体素坐标
        /// </summary>
        /// <param name="u">U坐标，范围 -1 到 1</param>
        /// <param name="v">V坐标，范围 -1 到 1</param>
        /// <returns>体素坐标，范围 0 到 volumeSize-1</returns>
        public Vector3i GetVoxelOnPlane(float u, float v)
        {
            //先获取逻辑空间点，范围 -0.5 到 0.5
            Vector3 localPoint = this.GetPointOnPlane(u, v);

            //逻辑空间 → 纹理坐标（0 到 1）
            Vector3 texCoord = localPoint + new Vector3(0.5f);

            //纹理坐标 → 体素坐标
            int x = (int)(texCoord.X * this._volumeSize.X);
            int y = (int)(texCoord.Y * this._volumeSize.Y);
            int z = (int)(texCoord.Z * this._volumeSize.Z);

            //边界裁剪
            x = Math.Clamp(x, 0, this._volumeSize.X - 1);
            y = Math.Clamp(y, 0, this._volumeSize.Y - 1);
            z = Math.Clamp(z, 0, this._volumeSize.Z - 1);

            return new Vector3i(x, y, z);
        }
        #endregion

        #region 将体素坐标投影到平面 —— Vector2 ProjectVoxel(Vector3i voxel)
        /// <summary>
        /// 将体素坐标投影到平面
        /// </summary>
        /// <param name="voxel">体素坐标，范围 0 到 volumeSize-1</param>
        /// <returns>平面UV坐标，范围 -1 到 1</returns>
        public Vector2 ProjectVoxel(Vector3i voxel)
        {
            // 体素坐标 → 纹理坐标（0 到 1）
            Vector3 texCoord = new Vector3(
                voxel.X / (float)(this._volumeSize.X - 1),
                voxel.Y / (float)(this._volumeSize.Y - 1),
                voxel.Z / (float)(this._volumeSize.Z - 1)
            );

            // 纹理坐标 → 逻辑空间点（-0.5 到 0.5）
            Vector3 localPoint = texCoord - new Vector3(0.5f);

            // 投影到平面得到 UV
            return this.ProjectPoint(localPoint);
        }
        #endregion


        //Private

        #region 获取切片偏移量 —— float GetSliceOffset()
        /// <summary>
        /// 获取切片偏移量
        /// </summary>
        /// <returns>切片偏移量</returns>
        private float GetSliceOffset()
        {
            //逻辑空间范围 -0.5到0.5
            float t = this.SliceIndex * 1.0f / (this.SlicesCount - 1);
            float sliceOffset = -0.5f + t;

            return sliceOffset;
        }
        #endregion

        #region 触发平面变化事件 —— void OnChanged()
        /// <summary>
        /// 触发平面变化事件
        /// </summary>
        private void OnChanged()
        {
            this.PlaneChangedEvent?.Invoke(this);
        }
        #endregion 

        #endregion
    }
}
