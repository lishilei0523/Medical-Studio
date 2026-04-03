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
        /// 创建MPR平面构造器
        /// </summary>
        /// <param name="center">平面中心</param>
        /// <param name="uAxis">U轴方向（水平）</param>
        /// <param name="vAxis">V轴方向（垂直）</param>
        /// <param name="minSlicePosition">最小切片位置</param>
        /// <param name="maxSlicePosition">最大切片位置</param>
        /// <param name="planeType">MPR平面类型</param>
        private MPRPlane(Vector3 center, Vector3 uAxis, Vector3 vAxis, float minSlicePosition, float maxSlicePosition, MPRPlaneType planeType)
        {
            this.Center = center;
            this.UAxis = uAxis.Normalized();
            this.VAxis = vAxis.Normalized();
            this.Normal = Vector3.Cross(this.UAxis, this.VAxis).Normalized();
            this.MinSlicePosition = minSlicePosition;
            this.MaxSlicePosition = maxSlicePosition;
            this.PlaneType = planeType;

            this._slicePosition = (minSlicePosition + maxSlicePosition) / 2;
            this.SlicesCount = Math.Max(2, (int)((maxSlicePosition - minSlicePosition) / 1.0f) + 1);

            //正交化确保坐标轴正交
            this.Orthonormalize();
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

        #region 切片位置 —— float SlicePosition
        /// <summary>
        /// 切片位置
        /// </summary>
        private float _slicePosition;

        /// <summary>
        /// 切片位置
        /// </summary>
        public float SlicePosition
        {
            get => this._slicePosition;
            private set
            {
                float slicePosition = Math.Clamp(value, this.MinSlicePosition, this.MaxSlicePosition);
                if (Math.Abs(this._slicePosition - slicePosition) > 0.0001f)
                {
                    this._slicePosition = slicePosition;
                    this.OnChanged();
                }
            }
        }
        #endregion

        #region 切片索引 —— int SliceIndex
        /// <summary>
        /// 切片索引
        /// </summary>
        public int SliceIndex
        {
            get
            {
                float t = (this._slicePosition - this.MinSlicePosition) / (this.MaxSlicePosition - this.MinSlicePosition);
                return (int)Math.Round(t * (this.SlicesCount - 1));
            }
            set
            {
                int sliceIndex = Math.Clamp(value, 0, this.SlicesCount - 1);
                float t = sliceIndex * 1.0f / (this.SlicesCount - 1);
                float slicePosition = this.MinSlicePosition + t * (this.MaxSlicePosition - this.MinSlicePosition);
                this.SlicePosition = slicePosition;
            }
        }
        #endregion

        #region 切片数量 —— int SlicesCount
        /// <summary>
        /// 切片数量
        /// </summary>
        public int SlicesCount { get; private set; }
        #endregion

        #region 最小切片位置 —— float MinSlicePosition
        /// <summary>
        /// 最小切片位置
        /// </summary>
        public float MinSlicePosition { get; private set; }
        #endregion

        #region 最大切片位置 —— float MaxSlicePosition
        /// <summary>
        /// 最大切片位置
        /// </summary>
        public float MaxSlicePosition { get; private set; }
        #endregion

        #endregion

        #region # 方法

        //Static

        #region 创建横断面 —— static MPRPlane CreateAxial(BoundingBox box)
        /// <summary>
        /// 创建横断面
        /// </summary>
        /// <param name="box">包围盒</param>
        /// <returns>MPR平面</returns>
        /// <remarks>XY平面</remarks>
        public static MPRPlane CreateAxial(BoundingBox box)
        {
            Vector3 center = box.Center;
            Vector3 uAxis = new Vector3(1, 0, 0);
            Vector3 vAxis = new Vector3(0, 1, 0);
            float minSlicePosition = box.Minimum.Z;
            float maxSlicePosition = box.Maximum.Z;
            MPRPlane plane = new MPRPlane(center, uAxis, vAxis, minSlicePosition, maxSlicePosition, MPRPlaneType.Axial);

            return plane;
        }
        #endregion

        #region 创建冠状面 —— static MPRPlane CreateCoronal(BoundingBox box)
        /// <summary>
        /// 创建冠状面
        /// </summary>
        /// <param name="box">包围盒</param>
        /// <returns>MPR平面</returns>
        /// <remarks>XZ平面</remarks>
        public static MPRPlane CreateCoronal(BoundingBox box)
        {
            Vector3 center = box.Center;
            Vector3 uAxis = new Vector3(1, 0, 0);
            Vector3 vAxis = new Vector3(0, 0, 1);
            float minSlicePosition = box.Minimum.Y;
            float maxSlicePosition = box.Maximum.Y;
            MPRPlane plane = new MPRPlane(center, uAxis, vAxis, minSlicePosition, maxSlicePosition, MPRPlaneType.Coronal);

            return plane;
        }
        #endregion

        #region 创建矢状平面 —— static MPRPlane CreateSagittal(BoundingBox box)
        /// <summary>
        /// 创建矢状平面
        /// </summary>
        /// <param name="box">包围盒</param>
        /// <returns>MPR平面</returns>
        /// <remarks>YZ平面</remarks>
        public static MPRPlane CreateSagittal(BoundingBox box)
        {
            Vector3 center = box.Center;
            Vector3 uAxis = new Vector3(0, 1, 0);
            Vector3 vAxis = new Vector3(0, 0, 1);
            float minSlicePosition = box.Minimum.X;
            float maxSlicePosition = box.Maximum.X;
            MPRPlane plane = new MPRPlane(center, uAxis, vAxis, minSlicePosition, maxSlicePosition, MPRPlaneType.Sagittal);

            return plane;
        }
        #endregion

        #region 根据点和法向量创建平面 —— static MPRPlane CreateFromPointAndNormal(BoundingBox box...
        /// <summary>
        /// 根据点和法向量创建平面
        /// </summary>
        /// <param name="box">包围盒</param>
        /// <param name="point">点</param>
        /// <param name="normal">法向量</param>
        /// <param name="upHint">上方向</param>
        /// <returns>MPR平面</returns>
        public static MPRPlane CreateFromPointAndNormal(BoundingBox box, Vector3 point, Vector3 normal, Vector3? upHint = null)
        {
            normal = normal.Normalized();

            //确定上方向
            Vector3 up = upHint ?? Vector3.UnitY;
            if (Math.Abs(Vector3.Dot(normal, up)) > 0.9999f)
            {
                up = Vector3.UnitX;
            }

            //计算U轴和V轴
            Vector3 uAxis = Vector3.Cross(up, normal).Normalized();
            Vector3 vAxis = Vector3.Cross(normal, uAxis).Normalized();
            uAxis = Vector3.Cross(vAxis, normal).Normalized();

            //计算切片范围
            Vector3[] corners = box.Corners;
            float minSlicePosition = float.MaxValue;
            float maxSlicePosition = float.MinValue;
            Vector3 center = box.Center;

            foreach (Vector3 corner in corners)
            {
                float projection = Vector3.Dot(corner - center, normal);
                minSlicePosition = Math.Min(minSlicePosition, projection);
                maxSlicePosition = Math.Max(maxSlicePosition, projection);
            }

            MPRPlane plane = new MPRPlane(point, uAxis, vAxis, minSlicePosition, maxSlicePosition, MPRPlaneType.Oblique);
            float pointProjection = Vector3.Dot(point - center, normal);
            plane.SlicePosition = pointProjection;

            return plane;
        }
        #endregion

        #region 根据三个点创建平面 —— static MPRPlane CreateFromThreePoints(BoundingBox box...
        /// <summary>
        /// 根据三个点创建平面
        /// </summary>
        /// <param name="box">包围盒</param>
        /// <param name="pointA">点A</param>
        /// <param name="pointB">点B</param>
        /// <param name="pointC">点C</param>
        /// <returns>MPR平面</returns>
        public static MPRPlane CreateFromThreePoints(BoundingBox box, Vector3 pointA, Vector3 pointB, Vector3 pointC)
        {
            Vector3 normal = Vector3.Cross(pointB - pointA, pointC - pointA).Normalized();
            Vector3 center = (pointA + pointB + pointC) / 3.0f;
            Vector3 upHint = (pointB - pointA).Normalized();

            return CreateFromPointAndNormal(box, center, normal, upHint);
        }
        #endregion

        #region 从基础平面创建斜切平面 —— static MPRPlane CreateFromRotation(BoundingBox box...
        /// <summary>
        /// 从基础平面创建斜切平面
        /// </summary>
        /// <param name="box">包围盒</param>
        /// <param name="basePlane">基础平面</param>
        /// <param name="eulerAngles">欧拉角</param>
        /// <returns>MPR平面</returns>
        public static MPRPlane CreateFromRotation(BoundingBox box, MPRPlane basePlane, Vector3 eulerAngles)
        {
            //计算旋转矩阵
            float pitchRad = MathHelper.DegreesToRadians(eulerAngles.X);
            float yawRad = MathHelper.DegreesToRadians(eulerAngles.Y);
            float rollRad = MathHelper.DegreesToRadians(eulerAngles.Z);
            Matrix4 rotX = Matrix4.CreateRotationX(pitchRad);
            Matrix4 rotY = Matrix4.CreateRotationY(yawRad);
            Matrix4 rotZ = Matrix4.CreateRotationZ(rollRad);
            Matrix4 rotation = rotX * rotY * rotZ;

            //旋转U和V轴
            Vector3 newUAxis = Vector3.TransformNormal(basePlane.UAxis, rotation);
            Vector3 newVAxis = Vector3.TransformNormal(basePlane.VAxis, rotation);

            return CreateFromPointAndNormal(box, basePlane.Center, Vector3.Cross(newUAxis, newVAxis).Normalized(), newVAxis);
        }
        #endregion


        //Public

        #region 获取平面模型矩阵 —— Matrix4 GetModelMatrix(Vector3 volumeScale)
        /// <summary>
        /// 获取平面模型矩阵
        /// </summary>
        /// <param name="volumeScale">体积缩放</param>
        /// <returns>模型矩阵</returns>
        public Matrix4 GetModelMatrix(Vector3 volumeScale)
        {
            //单位平面的半长 = 0.5（因为范围是 -0.5 到 0.5）
            const float halfSize = 0.5f;

            //应用体积缩放到中心点
            Vector3 worldCenter = new Vector3(
                this.Center.X * volumeScale.X,
                this.Center.Y * volumeScale.Y,
                this.Center.Z * volumeScale.Z
            );

            //应用体积缩放到轴向
            Vector3 worldUAxis = new Vector3(
                this.UAxis.X * volumeScale.X,
                this.UAxis.Y * volumeScale.Y,
                this.UAxis.Z * volumeScale.Z
            ).Normalized();
            Vector3 worldVAxis = new Vector3(
                this.VAxis.X * volumeScale.X,
                this.VAxis.Y * volumeScale.Y,
                this.VAxis.Z * volumeScale.Z
            ).Normalized();

            //重新计算法向量并正交化
            Vector3 worldNormal = Vector3.Cross(worldUAxis, worldVAxis).Normalized();
            worldVAxis = Vector3.Cross(worldNormal, worldUAxis).Normalized();
            worldUAxis = Vector3.Cross(worldVAxis, worldNormal).Normalized();

            //计算切片位置的世界偏移
            float normalScale = Math.Abs(Vector3.Dot(worldNormal, volumeScale));
            float worldSliceOffset = this._slicePosition * normalScale;

            //构建变换矩阵
            Matrix4 translation = Matrix4.CreateTranslation(worldCenter + worldNormal * worldSliceOffset);

            Matrix4 basis = new Matrix4(
                new Vector4(worldUAxis * halfSize, 0),
                new Vector4(worldVAxis * halfSize, 0),
                new Vector4(worldNormal, 0),
                new Vector4(0, 0, 0, 1)
            );

            return basis * translation;
        }
        #endregion

        #region 获取平面模型逆矩阵 —— Matrix4 GetInverseModelMatrix(Vector3 volumeScale)
        /// <summary>
        /// 获取平面模型逆矩阵
        /// </summary>
        /// <param name="volumeScale">体积缩放</param>
        /// <returns>模型逆矩阵</returns>
        public Matrix4 GetInverseModelMatrix(Vector3 volumeScale)
        {
            //先获取正向矩阵
            Matrix4 modelMatrix = this.GetModelMatrix(volumeScale);

            //返回逆矩阵
            return modelMatrix.Inverted();
        }
        #endregion

        #region 旋转平面 —— void Rotate(float pitch, float yaw, float roll)
        /// <summary>
        /// 旋转平面
        /// </summary>
        /// <param name="pitch">俯仰角</param>
        /// <param name="yaw">偏航角</param>
        /// <param name="roll">翻滚角</param>
        public void Rotate(float pitch, float yaw, float roll = 0)
        {
            float pitchRad = MathHelper.DegreesToRadians(pitch);
            float yawRad = MathHelper.DegreesToRadians(yaw);
            float rollRad = MathHelper.DegreesToRadians(roll);

            Matrix4 rotX = Matrix4.CreateRotationX(pitchRad);
            Matrix4 rotY = Matrix4.CreateRotationY(yawRad);
            Matrix4 rotZ = Matrix4.CreateRotationZ(rollRad);
            Matrix4 rotation = rotX * rotY * rotZ;

            this.UAxis = Vector3.TransformNormal(this.UAxis, rotation).Normalized();
            this.VAxis = Vector3.TransformNormal(this.VAxis, rotation).Normalized();
            this.Normal = Vector3.Cross(this.UAxis, this.VAxis).Normalized();

            this.Orthonormalize();
            this.PlaneType = MPRPlaneType.Oblique;
            this.OnChanged();
        }
        #endregion

        #region 平移平面 —— void Translate(Vector3 offset)
        /// <summary>
        /// 平移平面
        /// </summary>
        public void Translate(Vector3 offset)
        {
            this.Center += offset;
            this.OnChanged();
        }
        #endregion

        #region 重置为标准平面 —— void ResetToStandard(BoundingBox box)
        /// <summary>
        /// 重置为标准平面
        /// </summary>
        public void ResetToStandard(BoundingBox box)
        {
            MPRPlane standardPlane = this.PlaneType switch
            {
                MPRPlaneType.Axial => CreateAxial(box),
                MPRPlaneType.Coronal => CreateCoronal(box),
                MPRPlaneType.Sagittal => CreateSagittal(box),
                _ => CreateAxial(box)
            };

            this.Center = standardPlane.Center;
            this.UAxis = standardPlane.UAxis;
            this.VAxis = standardPlane.VAxis;
            this.Normal = standardPlane.Normal;
            this.MinSlicePosition = standardPlane.MinSlicePosition;
            this.MaxSlicePosition = standardPlane.MaxSlicePosition;
            this.SlicesCount = standardPlane.SlicesCount;
            this._slicePosition = standardPlane.SlicePosition;

            this.OnChanged();
        }
        #endregion

        #region 获取平面上点在世界坐标位置 —— Vector3 GetPointOnPlane(float u, float v, Vector3? volumeScale)
        /// <summary>
        /// 获取平面上点在世界坐标位置
        /// </summary>
        /// <param name="u">U坐标，范围[-1, 1]</param>
        /// <param name="v">V坐标，范围[-1, 1]</param>
        /// <param name="volumeScale">体积缩放（可选，用于精确计算）</param>
        /// <returns>世界坐标位置</returns>
        public Vector3 GetPointOnPlane(float u, float v, Vector3? volumeScale = null)
        {
            //单位平面半长固定为 0.5
            const float halfSize = 0.5f;

            //逻辑空间中的点
            Vector3 localPoint = this.Center + this.Normal * this._slicePosition +
                                 this.UAxis * u * halfSize +
                                 this.VAxis * v * halfSize;

            // 如果提供了体积缩放，转换到世界空间
            if (volumeScale.HasValue)
            {
                localPoint = new Vector3(
                    localPoint.X * volumeScale.Value.X,
                    localPoint.Y * volumeScale.Value.Y,
                    localPoint.Z * volumeScale.Value.Z
                );
            }

            return localPoint;
        }
        #endregion

        #region 将世界坐标点投影到平面 —— Vector2 ProjectPoint(Vector3 point, Vector3? volumeScale)
        /// <summary>
        /// 将世界坐标点投影到平面
        /// </summary>
        /// <param name="point">世界坐标点</param>
        /// <param name="volumeScale">体积缩放（可选，用于精确计算）</param>
        /// <returns>平面UV坐标</returns>
        public Vector2 ProjectPoint(Vector3 point, Vector3? volumeScale = null)
        {
            const float halfSize = 0.5f;

            //如果提供了体积缩放，先转换到逻辑空间
            Vector3 localPoint = point;
            if (volumeScale.HasValue)
            {
                localPoint = new Vector3(
                    point.X / volumeScale.Value.X,
                    point.Y / volumeScale.Value.Y,
                    point.Z / volumeScale.Value.Z
                );
            }

            //计算相对于平面中心的偏移
            Vector3 relative = localPoint - (this.Center + this.Normal * this._slicePosition);

            //投影到U轴和V轴
            float u = Vector3.Dot(relative, this.UAxis) / halfSize;
            float v = Vector3.Dot(relative, this.VAxis) / halfSize;

            return new Vector2(u, v);
        }
        #endregion


        //Private

        #region 正交化坐标轴 —— void Orthonormalize()
        /// <summary>
        /// 正交化坐标轴
        /// </summary>
        private void Orthonormalize()
        {
            //先归一化U轴
            this.UAxis = this.UAxis.Normalized();

            //基于U轴和原始Normal计算V轴
            this.VAxis = Vector3.Cross(this.Normal, this.UAxis).Normalized();

            //基于U和V重新计算Normal
            this.Normal = Vector3.Cross(this.UAxis, this.VAxis).Normalized();

            //最后再微调V轴确保完全正交
            this.VAxis = Vector3.Cross(this.Normal, this.UAxis).Normalized();
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
