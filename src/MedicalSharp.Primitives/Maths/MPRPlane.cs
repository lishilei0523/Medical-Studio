using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Managers;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalSharp.Primitives.Maths
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
        /// 原始U轴方向
        /// </summary>
        /// <remarks>水平</remarks>
        private Vector3 _originalUAxis;

        /// <summary>
        /// 原始V轴方向
        /// </summary>
        /// <remarks>垂直</remarks>
        private Vector3 _originalVAxis;

        /// <summary>
        /// 原始法向量
        /// </summary>
        private Vector3 _originalNormal;

        /// <summary>
        /// 创建MPR平面构造器
        /// </summary>
        /// <param name="volumeMetadata">体积元数据</param>
        private MPRPlane(VolumeMetadata volumeMetadata)
        {
            this.VolumeMetadata = volumeMetadata;
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

        #region 体积元数据 —— VolumeMetadata VolumeMetadata
        /// <summary>
        /// 体积元数据
        /// </summary>
        public VolumeMetadata VolumeMetadata { get; private set; }
        #endregion

        #endregion

        #region # 方法

        //Static

        #region 创建横断面 —— static MPRPlane CreateAxialPlane(VolumeMetadata volumeMetadata)
        /// <summary>
        /// 创建横断面
        /// </summary>
        /// <param name="volumeMetadata">体积元数据</param>
        /// <returns>横断面</returns>
        public static MPRPlane CreateAxialPlane(VolumeMetadata volumeMetadata)
        {
            MPRPlane plane = new MPRPlane(volumeMetadata)
            {
                _minProjection = -0.5f,
                _maxProjection = 0.5f,
                _originalUAxis = new Vector3(1, 0, 0),
                _originalVAxis = new Vector3(0, 1, 0),
                _originalNormal = new Vector3(0, 0, 1),
                Center = Vector3.Zero,
                UAxis = new Vector3(1, 0, 0),
                VAxis = new Vector3(0, 1, 0),
                Normal = new Vector3(0, 0, 1),
                PlaneType = MPRPlaneType.Axial,
                OriginalPlaneType = MPRPlaneType.Axial,
                SlicesCount = volumeMetadata.VolumeSize.Z,
                SliceIndex = volumeMetadata.VolumeSize.Z / 2,
            };

            return plane;
        }
        #endregion

        #region 创建冠状面 —— static MPRPlane CreateCoronalPlane(VolumeMetadata volumeMetadata)
        /// <summary>
        /// 创建冠状面
        /// </summary>
        /// <param name="volumeMetadata">体积元数据</param>
        /// <returns>冠状面</returns>
        public static MPRPlane CreateCoronalPlane(VolumeMetadata volumeMetadata)
        {
            MPRPlane plane = new MPRPlane(volumeMetadata)
            {
                _minProjection = -0.5f,
                _maxProjection = 0.5f,
                _originalUAxis = new Vector3(1, 0, 0),
                _originalVAxis = new Vector3(0, 0, 1),
                _originalNormal = new Vector3(0, 1, 0),
                Center = Vector3.Zero,
                UAxis = new Vector3(1, 0, 0),
                VAxis = new Vector3(0, 0, 1),
                Normal = new Vector3(0, 1, 0),
                PlaneType = MPRPlaneType.Coronal,
                OriginalPlaneType = MPRPlaneType.Coronal,
                SlicesCount = volumeMetadata.VolumeSize.Y,
                SliceIndex = volumeMetadata.VolumeSize.Y / 2
            };

            return plane;
        }
        #endregion

        #region 创建矢状面 —— static MPRPlane CreateSagittalPlane(VolumeMetadata volumeMetadata)
        /// <summary>
        /// 创建矢状面
        /// </summary>
        /// <param name="volumeMetadata">体积元数据</param>
        /// <returns>矢状面</returns>
        public static MPRPlane CreateSagittalPlane(VolumeMetadata volumeMetadata)
        {
            MPRPlane plane = new MPRPlane(volumeMetadata)
            {
                _minProjection = -0.5f,
                _maxProjection = 0.5f,
                _originalUAxis = new Vector3(0, 1, 0),
                _originalVAxis = new Vector3(0, 0, 1),
                _originalNormal = new Vector3(1, 0, 0),
                Center = Vector3.Zero,
                UAxis = new Vector3(0, 1, 0),
                VAxis = new Vector3(0, 0, 1),
                Normal = new Vector3(1, 0, 0),
                PlaneType = MPRPlaneType.Sagittal,
                OriginalPlaneType = MPRPlaneType.Sagittal,
                SlicesCount = volumeMetadata.VolumeSize.X,
                SliceIndex = volumeMetadata.VolumeSize.X / 2
            };

            return plane;
        }
        #endregion

        #region 创建斜切面 —— static MPRPlane CreateObliquePlane(MPRPlane originalPlane...
        /// <summary>
        /// 创建斜切面
        /// </summary>
        /// <param name="originalPlane">原始平面</param>
        /// <param name="rotationU">绕U轴旋转角度（上下旋转）</param>
        /// <param name="rotationV">绕V轴旋转角度（左右旋转）</param>
        /// <returns>斜切面</returns>
        public static MPRPlane CreateObliquePlane(MPRPlane originalPlane, float rotationU, float rotationV)
        {
            //基于原始轴计算旋转
            Quaternion rotU = Quaternion.FromAxisAngle(originalPlane.UAxis, MathHelper.DegreesToRadians(rotationU));
            Quaternion rotV = Quaternion.FromAxisAngle(originalPlane.VAxis, MathHelper.DegreesToRadians(rotationV));

            //组合旋转：先左右，再上下
            Quaternion total = rotU * rotV;

            //旋转原始轴
            Vector3 uAxis = Vector3.Transform(originalPlane.UAxis, total).Normalized();
            Vector3 vAxis = Vector3.Transform(originalPlane.VAxis, total).Normalized();
            Vector3 normal = Vector3.Transform(originalPlane.Normal, total).Normalized();

            //创建斜切平面
            MPRPlane plane = new MPRPlane(originalPlane.VolumeMetadata)
            {
                _originalUAxis = uAxis,
                _originalVAxis = vAxis,
                _originalNormal = normal,
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
            plane.SliceIndex = originalPlane.SliceIndex;

            return plane;
        }
        #endregion


        //Public

        #region 旋转平面 —— void Rotate(float deltaU, float deltaV)
        /// <summary>
        /// 旋转平面
        /// </summary>
        /// <param name="deltaU">绕U轴旋转角度（上下旋转）</param>
        /// <param name="deltaV">绕V轴旋转角度（左右旋转）</param>
        public void Rotate(float deltaU, float deltaV)
        {
            //绕U轴旋转（上下）
            Quaternion rotU = Quaternion.FromAxisAngle(this.UAxis, MathHelper.DegreesToRadians(deltaU));

            //绕V轴旋转（左右）
            Quaternion rotV = Quaternion.FromAxisAngle(this.VAxis, MathHelper.DegreesToRadians(deltaV));

            //组合旋转：先左右，再上下
            Quaternion total = rotU * rotV;

            //应用旋转
            this.UAxis = Vector3.Transform(this.UAxis, total).Normalized();
            this.VAxis = Vector3.Transform(this.VAxis, total).Normalized();
            this.Normal = Vector3.Transform(this.Normal, total).Normalized();

            //重新正交化
            this.Orthonormalize();

            //更新类型
            this.PlaneType = MPRPlaneType.Oblique;

            //计算投影范围
            this.CalculateProjectionRange();

            //重新计算切片数量
            this.CalculateObliqueSlicesCount();

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
                MPRPlaneType.Axial => CreateAxialPlane(this.VolumeMetadata),
                MPRPlaneType.Coronal => CreateCoronalPlane(this.VolumeMetadata),
                MPRPlaneType.Sagittal => CreateSagittalPlane(this.VolumeMetadata),
                _ => CreateAxialPlane(this.VolumeMetadata)
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
            //切片偏移：逻辑空间 -0.5到0.5
            float sliceOffset = this.GetSliceOffset();

            //平面中心：逻辑空间 (0,0,0) -> 世界空间 (0,0,0)
            Vector3 worldCenter = Vector3.Zero;

            //法线方向的世界偏移 = 逻辑偏移 * VolumeScale
            Vector3 worldOffset = new Vector3(
                this.Normal.X * sliceOffset * this.VolumeMetadata.VolumeScale.X,
                this.Normal.Y * sliceOffset * this.VolumeMetadata.VolumeScale.Y,
                this.Normal.Z * sliceOffset * this.VolumeMetadata.VolumeScale.Z
            );

            //U/V轴：逻辑方向 * VolumeScale，然后归一化
            Vector3 worldUAxis = new Vector3(
                this.UAxis.X * this.VolumeMetadata.VolumeScale.X,
                this.UAxis.Y * this.VolumeMetadata.VolumeScale.Y,
                this.UAxis.Z * this.VolumeMetadata.VolumeScale.Z
            ).Normalized();
            Vector3 worldVAxis = new Vector3(
                this.VAxis.X * this.VolumeMetadata.VolumeScale.X,
                this.VAxis.Y * this.VolumeMetadata.VolumeScale.Y,
                this.VAxis.Z * this.VolumeMetadata.VolumeScale.Z
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

            //处理缩放
            Matrix4 scale = Matrix4.CreateScale(this.VolumeMetadata.VolumeScale);

            return basis * scale * translation;
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

        #region 屏幕坐标转换平面UV坐标 —— Vector2? ScreenToPlaneUV(Vector2 mousePos2D...
        /// <summary>
        /// 屏幕坐标转换平面UV坐标
        /// </summary>
        /// <param name="mousePos2D">鼠标2D位置</param>
        /// <param name="lookDirection">视角方向</param>
        /// <param name="viewportSize">视口尺寸</param>
        /// <param name="projectionMatrix">投影矩阵</param>
        /// <param name="viewMatrix">视图矩阵</param>
        /// <returns>UV坐标，[-1, 1]，如果不在平面上则返回null</returns>
        public Vector2? ScreenToPlaneUV(Vector2 mousePos2D, Vector3 lookDirection, Vector2 viewportSize, Matrix4 projectionMatrix, Matrix4 viewMatrix)
        {
            //将屏幕坐标转换到世界空间的射线起点（近平面上的点）
            float ndcX = (2.0f * mousePos2D.X) / viewportSize.X - 1.0f;
            float ndcY = 1.0f - (2.0f * mousePos2D.Y) / viewportSize.Y;

            Vector4 rayStartNDC = new Vector4(ndcX, ndcY, -1.0f, 1.0f);
            Matrix4 invProjection = Matrix4.Invert(projectionMatrix);
            Matrix4 invView = Matrix4.Invert(viewMatrix);

            Vector4 rayStartCamera = rayStartNDC * invProjection;
            rayStartCamera /= rayStartCamera.W;
            Vector3 rayStartWorld = Vector3.TransformPosition(rayStartCamera.Xyz, invView);

            //射线方向固定为相机的前向方向（正交投影）
            Vector3 rayDirection = lookDirection;

            //创建射线
            Ray ray = new Ray(rayStartWorld, rayDirection);

            //计算平面的世界位置，平面实际位置 = Normal * sliceOffset（因为Center = (0,0,0)）
            float sliceOffset = this.GetSliceOffset();
            Vector3 worldNormal = new Vector3(
                this.Normal.X * this.VolumeMetadata.VolumeScale.X,
                this.Normal.Y * this.VolumeMetadata.VolumeScale.Y,
                this.Normal.Z * this.VolumeMetadata.VolumeScale.Z
            ).Normalized();

            float normalScale = Math.Abs(Vector3.Dot(worldNormal, this.VolumeMetadata.VolumeScale));
            float worldSliceOffset = sliceOffset * normalScale;
            Vector3 planePoint = worldNormal * worldSliceOffset;

            //射线与平面求交
            if (ray.IntersectsPlane(planePoint, worldNormal, out Vector3 hitPoint))
            {
                //转换到逻辑空间
                Vector3 localPoint = new Vector3(
                    hitPoint.X / this.VolumeMetadata.VolumeScale.X,
                    hitPoint.Y / this.VolumeMetadata.VolumeScale.Y,
                    hitPoint.Z / this.VolumeMetadata.VolumeScale.Z
                );

                //投影到平面得到UV
                Vector2 uv = this.ProjectPoint(localPoint);

                //方向修正
                if (this.PlaneType == MPRPlaneType.Axial || this.PlaneType == MPRPlaneType.Sagittal)
                {
                    uv = new Vector2(-uv.X, uv.Y);
                }

                //检查是否在平面范围内
                if (uv.X >= -1 && uv.X <= 1 && uv.Y >= -1 && uv.Y <= 1)
                {
                    return uv;
                }
            }

            return null;
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
            Vector3 point = this.Center + this.Normal * sliceOffset + this.UAxis * u * halfSize + this.VAxis * v * halfSize;

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

        #region 获取平面上的体素坐标 —— Vector3i GetVoxelPosition(float u, float v)
        /// <summary>
        /// 获取平面上的体素坐标
        /// </summary>
        /// <param name="u">U坐标，[-1, 1]</param>
        /// <param name="v">V坐标，[-1, 1]</param>
        /// <returns>体素坐标，[0, VolumeSize-1]</returns>
        public Vector3i GetVoxelPosition(float u, float v)
        {
            //先获取逻辑空间点（-0.5到0.5）
            Vector3 localPoint = this.GetPointOnPlane(u, v);

            //逻辑空间 -> 纹理坐标（0到1）
            Vector3 texCoord = localPoint + new Vector3(0.5f);

            //纹理坐标 -> 体素坐标
            int x = (int)Math.Floor(texCoord.X * this.VolumeMetadata.VolumeSize.X);
            int y = (int)Math.Floor(texCoord.Y * this.VolumeMetadata.VolumeSize.Y);
            int z = (int)Math.Floor(texCoord.Z * this.VolumeMetadata.VolumeSize.Z);

            //边界裁剪
            x = Math.Clamp(x, 0, this.VolumeMetadata.VolumeSize.X - 1);
            y = Math.Clamp(y, 0, this.VolumeMetadata.VolumeSize.Y - 1);
            z = Math.Clamp(z, 0, this.VolumeMetadata.VolumeSize.Z - 1);

            return new Vector3i(x, y, z);
        }
        #endregion

        #region 将体素坐标投影到平面 —— Vector2 ProjectVoxel(Vector3i voxelPosition)
        /// <summary>
        /// 将体素坐标投影到平面
        /// </summary>
        /// <param name="voxelPosition">体素坐标，[0, VolumeSize-1]</param>
        /// <returns>平面UV坐标，[-1, 1]</returns>
        public Vector2 ProjectVoxel(Vector3i voxelPosition)
        {
            //体素坐标 -> 纹理坐标（0到1）
            Vector3 texCoord = new Vector3(
                voxelPosition.X * 1.0f / (this.VolumeMetadata.VolumeSize.X - 1),
                voxelPosition.Y * 1.0f / (this.VolumeMetadata.VolumeSize.Y - 1),
                voxelPosition.Z * 1.0f / (this.VolumeMetadata.VolumeSize.Z - 1)
            );

            //纹理坐标 -> 逻辑空间点（-0.5到0.5）
            Vector3 localPoint = texCoord - new Vector3(0.5f);

            //投影到平面得到UV
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
                this.VolumeMetadata.VolumeSize.X * absNormal.X +
                this.VolumeMetadata.VolumeSize.Y * absNormal.Y +
                this.VolumeMetadata.VolumeSize.Z * absNormal.Z;

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
