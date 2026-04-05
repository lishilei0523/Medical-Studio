using MedicalSharp.Engine.ValueTypes;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

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
        /// 最小投影（斜切平面使用）
        /// </summary>
        private float _minProjection;

        /// <summary>
        /// 最大投影（斜切平面使用）
        /// </summary>
        private float _maxProjection;

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

        #region 只读属性 - 体积尺寸 —— Vector3i VolumeSize
        /// <summary>
        /// 只读属性 - 体积尺寸
        /// </summary>
        public Vector3i VolumeSize
        {
            get => this._volumeSize;
        }
        #endregion

        #region 只读属性 - 间距 —— Vector3 Spacing
        /// <summary>
        /// 只读属性 - 间距
        /// </summary>
        public Vector3 Spacing
        {
            get => this._spacing;
        }
        #endregion

        #region 只读属性 - 物理尺寸 —— Vector3 PhysicalSize
        /// <summary>
        /// 只读属性 - 物理尺寸
        /// </summary>
        public Vector3 PhysicalSize
        {
            get => this._physicalSize;
        }
        #endregion

        #region 只读属性 - 体积缩放 —— Vector3 VolumeScale
        /// <summary>
        /// 只读属性 - 体积缩放
        /// </summary>
        public Vector3 VolumeScale
        {
            get => this._volumeScale;
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
                _minProjection = -0.5f,
                _maxProjection = 0.5f,
                Center = Vector3.Zero,
                UAxis = new Vector3(1, 0, 0),
                VAxis = new Vector3(0, -1, 0),
                Normal = new Vector3(0, 0, 1),
                PlaneType = MPRPlaneType.Axial,
                OriginalPlaneType = MPRPlaneType.Axial,
                SlicesCount = volumeSize.Z,
                SliceIndex = volumeSize.Z / 2
            };

            return plane;
        }
        #endregion

        #region 创建冠状面 —— static MPRPlane CreateCoronalPlane(Vector3i volumeSize...
        /// <summary>
        /// 创建冠状面
        /// </summary>
        /// <param name="volumeSize">体积尺寸</param>
        /// <param name="spacing">间距</param>
        /// <param name="physicalSize">物理尺寸</param>
        /// <param name="volumeScale">体积缩放</param>
        /// <returns>冠状面</returns>
        public static MPRPlane CreateCoronalPlane(Vector3i volumeSize, Vector3 spacing, Vector3 physicalSize, Vector3 volumeScale)
        {
            MPRPlane plane = new MPRPlane(volumeSize, spacing, physicalSize, volumeScale)
            {
                _minProjection = -0.5f,
                _maxProjection = 0.5f,
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

        #region 创建矢状面 —— static MPRPlane CreateSagittalPlane(Vector3i volumeSize...
        /// <summary>
        /// 创建矢状面
        /// </summary>
        /// <param name="volumeSize">体积尺寸</param>
        /// <param name="spacing">间距</param>
        /// <param name="physicalSize">物理尺寸</param>
        /// <param name="volumeScale">体积缩放</param>
        /// <returns>矢状面</returns>
        public static MPRPlane CreateSagittalPlane(Vector3i volumeSize, Vector3 spacing, Vector3 physicalSize, Vector3 volumeScale)
        {
            MPRPlane plane = new MPRPlane(volumeSize, spacing, physicalSize, volumeScale)
            {
                _minProjection = -0.5f,
                _maxProjection = 0.5f,
                Center = Vector3.Zero,
                UAxis = new Vector3(0, 1, 0),
                VAxis = new Vector3(0, 0, 1),
                Normal = new Vector3(-1, 0, 0),
                PlaneType = MPRPlaneType.Sagittal,
                OriginalPlaneType = MPRPlaneType.Sagittal,
                SlicesCount = volumeSize.X,
                SliceIndex = volumeSize.X / 2
            };

            return plane;
        }
        #endregion

        #region 创建斜切面 —— static MPRPlane CreateObliquePlane(MPRPlane originalPlane...
        /// <summary>
        /// 创建斜切面
        /// </summary>
        /// <param name="originalPlane">原始平面</param>
        /// <param name="eulerAngles">欧拉角</param>
        /// <returns>斜切面</returns>
        public static MPRPlane CreateObliquePlane(MPRPlane originalPlane, Vector3 eulerAngles)
        {
            //计算旋转矩阵
            float pitchRad = MathHelper.DegreesToRadians(eulerAngles.X);
            float yawRad = MathHelper.DegreesToRadians(eulerAngles.Y);
            float rollRad = MathHelper.DegreesToRadians(eulerAngles.Z);

            Matrix4 rotX = Matrix4.CreateRotationX(pitchRad);
            Matrix4 rotY = Matrix4.CreateRotationY(yawRad);
            Matrix4 rotZ = Matrix4.CreateRotationZ(rollRad);
            Matrix4 rotation = rotX * rotY * rotZ;

            //旋转轴方向
            Vector3 uAxis = Vector3.TransformNormal(originalPlane.UAxis, rotation).Normalized();
            Vector3 vAxis = Vector3.TransformNormal(originalPlane.VAxis, rotation).Normalized();
            Vector3 normal = Vector3.TransformNormal(originalPlane.Normal, rotation).Normalized();

            //创建斜切平面
            MPRPlane plane = new MPRPlane(originalPlane._volumeSize, originalPlane._spacing, originalPlane._physicalSize, originalPlane._volumeScale)
            {
                Center = originalPlane.Center,
                UAxis = uAxis,
                VAxis = vAxis,
                Normal = normal,
                PlaneType = MPRPlaneType.Oblique,
                OriginalPlaneType = originalPlane.OriginalPlaneType
            };

            //重新正交化
            plane.Orthonormalize();

            //计算投影范围
            plane.CalculateProjectionRange();

            //计算斜切平面的切片数量（体积在法线方向上的投影）
            plane.CalculateObliqueSlicesCount();

            //保持切片索引比例
            float t = originalPlane.SliceIndex / (float)(originalPlane.SlicesCount - 1);
            plane.SliceIndex = Math.Clamp((int)(t * (plane.SlicesCount - 1)), 0, plane.SlicesCount - 1);

            return plane;
        }
        #endregion


        //Public

        #region 旋转平面 —— void Rotate(Vector3 eulerAngles)
        /// <summary>
        /// 旋转平面
        /// </summary>
        /// <param name="eulerAngles">欧拉角</param>
        public void Rotate(Vector3 eulerAngles)
        {
            float pitchRad = MathHelper.DegreesToRadians(eulerAngles.X);
            float yawRad = MathHelper.DegreesToRadians(eulerAngles.Y);
            float rollRad = MathHelper.DegreesToRadians(eulerAngles.Z);

            Matrix4 rotX = Matrix4.CreateRotationX(pitchRad);
            Matrix4 rotY = Matrix4.CreateRotationY(yawRad);
            Matrix4 rotZ = Matrix4.CreateRotationZ(rollRad);
            Matrix4 rotation = rotX * rotY * rotZ;

            this.UAxis = Vector3.TransformNormal(this.UAxis, rotation).Normalized();
            this.VAxis = Vector3.TransformNormal(this.VAxis, rotation).Normalized();
            this.Normal = Vector3.Cross(this.UAxis, this.VAxis).Normalized();

            this.Orthonormalize();
            this.PlaneType = MPRPlaneType.Oblique;

            this.CalculateProjectionRange();

            //保存当前切片比例
            float t = this._sliceIndex / (float)(this.SlicesCount - 1);

            //重新计算切片数量
            this.CalculateObliqueSlicesCount();

            //恢复切片比例
            this._sliceIndex = Math.Clamp((int)(t * (this.SlicesCount - 1)), 0, this.SlicesCount - 1);

            this.OnChanged();
        }
        #endregion

        #region 重置为标准平面 —— void ResetToStandard()
        /// <summary>
        /// 重置为标准平面
        /// </summary>
        public void ResetToStandard()
        {
            MPRPlane standardPlane = this.OriginalPlaneType switch
            {
                MPRPlaneType.Axial => CreateAxialPlane(this._volumeSize, this._spacing, this._physicalSize, this._volumeScale),
                MPRPlaneType.Coronal => CreateCoronalPlane(this._volumeSize, this._spacing, this._physicalSize, this._volumeScale),
                MPRPlaneType.Sagittal => CreateSagittalPlane(this._volumeSize, this._spacing, this._physicalSize, this._volumeScale),
                _ => CreateAxialPlane(this._volumeSize, this._spacing, this._physicalSize, this._volumeScale)
            };

            this.Center = standardPlane.Center;
            this.UAxis = standardPlane.UAxis;
            this.VAxis = standardPlane.VAxis;
            this.Normal = standardPlane.Normal;
            this.PlaneType = standardPlane.PlaneType;
            this.SlicesCount = standardPlane.SlicesCount;
            this._sliceIndex = standardPlane._sliceIndex;
            this._minProjection = standardPlane._minProjection;
            this._maxProjection = standardPlane._maxProjection;

            this.OnChanged();
        }
        #endregion

        #region 获取模型矩阵 —— Matrix4 GetModelMatrix()
        /// <summary>
        /// 获取模型矩阵
        /// </summary>
        /// <returns>模型矩阵</returns>
        public Matrix4 GetModelMatrix()
        {
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
                new Vector4(worldUAxis, 0),
                new Vector4(worldVAxis, 0),
                new Vector4(worldNormal, 0),
                new Vector4(0, 0, 0, 1)
            );

            return basis * translation;
        }
        #endregion

        #region 获取切片偏移量 —— float GetSliceOffset()
        /// <summary>
        /// 获取切片偏移量
        /// </summary>
        /// <returns>切片偏移量</returns>
        public float GetSliceOffset()
        {
            #region # 验证

            if (this.SlicesCount <= 1)
            {
                return 0;
            }

            #endregion

            float sliceOffset;
            if (this.PlaneType == MPRPlaneType.Oblique)
            {
                //斜切平面：根据投影范围映射
                float t = this._sliceIndex * 1.0f / (this.SlicesCount - 1);
                sliceOffset = this._minProjection + t * (this._maxProjection - this._minProjection);
            }
            else
            {
                //标准平面：逻辑空间范围 -0.5到0.5
                float t = this.SliceIndex * 1.0f / (this.SlicesCount - 1);
                sliceOffset = -0.5f + t;
            }

            return sliceOffset;
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

        #region 正交化坐标轴 —— void Orthonormalize()
        /// <summary>
        /// 正交化坐标轴
        /// </summary>
        private void Orthonormalize()
        {
            this.UAxis = this.UAxis.Normalized();
            this.VAxis = Vector3.Cross(this.Normal, this.UAxis).Normalized();
            this.Normal = Vector3.Cross(this.UAxis, this.VAxis).Normalized();
            this.VAxis = Vector3.Cross(this.Normal, this.UAxis).Normalized();
        }
        #endregion

        #region 计算投影范围 —— void CalculateProjectionRange()
        /// <summary>
        /// 计算投影范围
        /// </summary>
        private void CalculateProjectionRange()
        {
            this._minProjection = float.MaxValue;
            this._maxProjection = float.MinValue;

            IEnumerable<Vector3> corners = ResourceManager.UnitCube.Vertices.Select(vertex => vertex.Position);
            foreach (Vector3 corner in corners)
            {
                float projection = Vector3.Dot(corner, this.Normal);
                this._minProjection = Math.Min(this._minProjection, projection);
                this._maxProjection = Math.Max(this._maxProjection, projection);
            }
        }
        #endregion

        #region 计算斜切面切片数量 —— void CalculateObliqueSlicesCount()
        /// <summary>
        /// 计算斜切面切片数量
        /// </summary>
        private void CalculateObliqueSlicesCount()
        {
            Vector3 absNormal = new Vector3(
                Math.Abs(this.Normal.X),
                Math.Abs(this.Normal.Y),
                Math.Abs(this.Normal.Z)
            );
            float projection =
                this._volumeSize.X * absNormal.X +
                this._volumeSize.Y * absNormal.Y +
                this._volumeSize.Z * absNormal.Z;

            this.SlicesCount = (int)Math.Floor(Math.Max(projection, 2));
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
