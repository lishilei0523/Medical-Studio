using MedicalSharp.Engine.Resources;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Engine.Cameras
{
    /// <summary>
    /// MPR相机
    /// </summary>
    public class MPRCamera2 : Camera
    {
        #region # 字段及构造器

        /// <summary>
        /// 目标平面
        /// </summary>
        private MPRPlane _targetPlane;

        /// <summary>
        /// 目标距离
        /// </summary>
        private float _distance;

        /// <summary>
        /// 相机位置
        /// </summary>
        private Vector3 _cameraPosition;

        /// <summary>
        /// 相机上方向
        /// </summary>
        private Vector3 _upDirection;

        /// <summary>
        /// 投影矩阵
        /// </summary>
        private Matrix4 _projectionMatrix;

        /// <summary>
        /// 视图矩阵
        /// </summary>
        private Matrix4 _viewMatrix;

        /// <summary>
        /// 创建MPR相机构造器
        /// </summary>
        /// <param name="nearPlaneDistance">近平面距离</param>
        /// <param name="farPlaneDistance">远平面距离</param>
        public MPRCamera2(float nearPlaneDistance = -1000, float farPlaneDistance = 1000)
            : base(nearPlaneDistance, farPlaneDistance)
        {
            this._distance = 2.0f;
            this._panOffset = Vector2.Zero;
            this._targetPosition = Vector3.Zero;
            this._zoomFactor = 1.0f;
            this.UpdateViewMatrix();
            this.UpdateProjectionMatrix();
        }

        #endregion

        #region # 属性

        #region 相机位置 —— override Vector3 CameraPosition
        /// <summary>
        /// 相机位置
        /// </summary>
        public override Vector3 CameraPosition => this._cameraPosition;
        #endregion

        #region 视角方向 —— override Vector3 LookDirection
        /// <summary>
        /// 视角方向
        /// </summary>
        public override Vector3 LookDirection => (this._targetPosition - this._cameraPosition).Normalized();
        #endregion

        #region 相机上方向 —— override Vector3 UpDirection
        /// <summary>
        /// 相机上方向
        /// </summary>
        public override Vector3 UpDirection => this._upDirection;
        #endregion

        #region 相机右方向 —— override Vector3 RightDirection
        /// <summary>
        /// 相机右方向
        /// </summary>
        public override Vector3 RightDirection => Vector3.Cross(this.LookDirection, this.UpDirection).Normalized();
        #endregion

        #region 投影矩阵 —— override Matrix4 ProjectionMatrix
        /// <summary>
        /// 投影矩阵
        /// </summary>
        public override Matrix4 ProjectionMatrix => this._projectionMatrix;
        #endregion

        #region 视图矩阵 —— override Matrix4 ViewMatrix
        /// <summary>
        /// 视图矩阵
        /// </summary>
        public override Matrix4 ViewMatrix => this._viewMatrix;
        #endregion

        #region 目标位置 —— Vector3 TargetPosition
        /// <summary>
        /// 目标位置
        /// </summary>
        private Vector3 _targetPosition;

        /// <summary>
        /// 目标位置
        /// </summary>
        public Vector3 TargetPosition
        {
            get => this._targetPosition;
        }
        #endregion

        #region 缩放因子 —— float ZoomFactor
        /// <summary>
        /// 缩放因子
        /// </summary>
        private float _zoomFactor;

        /// <summary>
        /// 缩放因子
        /// </summary>
        public float ZoomFactor
        {
            get => this._zoomFactor;
            set
            {
                if (value > 0 && !this._zoomFactor.Equals(value))
                {
                    this._zoomFactor = value;
                    this.UpdateProjectionMatrix();
                }
            }
        }
        #endregion

        #region 平移偏移量 —— Vector2 PanOffset
        /// <summary>
        /// 平移偏移量
        /// </summary>
        private Vector2 _panOffset;

        /// <summary>
        /// 平移偏移量
        /// </summary>
        public Vector2 PanOffset
        {
            get => this._panOffset;
            set
            {
                if (this._panOffset != value)
                {
                    this._panOffset = value;
                    this.UpdateProjectionMatrix();
                }
            }
        }
        #endregion

        #region 只读属性 - 目标平面 —— MPRPlane TargetPlane
        /// <summary>
        /// 只读属性 - 目标平面
        /// </summary>
        public MPRPlane TargetPlane
        {
            get => this._targetPlane;
        }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 设置视口尺寸 —— override void SetViewportSize(float width, float height)
        /// <summary>
        /// 设置视口尺寸
        /// </summary>
        /// <param name="width">视口宽度</param>
        /// <param name="height">视口高度</param>
        public override void SetViewportSize(float width, float height)
        {
            base.SetViewportSize(width, height);
            this.UpdateProjectionMatrix();
        }
        #endregion

        #region 绑定MPR平面 —— void BindPlane(MPRPlane plane, float distance)
        /// <summary>
        /// 绑定MPR平面
        /// </summary>
        /// <param name="plane">MPR平面</param>
        /// <param name="distance">目标距离</param>
        public void BindPlane(MPRPlane plane, float distance = 2.0f)
        {
            if (this._targetPlane != null)
            {
                this._targetPlane.PlaneChangedEvent -= this.OnPlaneChanged;
            }

            this._targetPlane = plane;
            this._distance = distance;

            if (this._targetPlane != null)
            {
                this._targetPlane.PlaneChangedEvent += this.OnPlaneChanged;
                this.UpdateCameraVectors();
            }
        }
        #endregion

        #region 缩放相机 —— void Zoom(float delta)
        /// <summary>
        /// 缩放相机
        /// </summary>
        /// <param name="delta">缩放增量</param>
        /// <remarks>正数放大，负数缩小</remarks>
        public void Zoom(float delta)
        {
            this._zoomFactor *= (1.0f + delta * 0.1f);
            this._zoomFactor = Math.Clamp(this._zoomFactor, 0.1f, 10.0f);
            this.UpdateProjectionMatrix();
        }
        #endregion

        #region 平移相机 —— void Pan(float deltaX, float deltaY)
        /// <summary>
        /// 平移相机
        /// </summary>
        /// <param name="deltaX">水平平移量</param>
        /// <param name="deltaY">垂直平移量</param>
        public void Pan(float deltaX, float deltaY)
        {
            #region # 验证

            if (this._viewportWidth <= 0 || this._viewportHeight <= 0)
            {
                return;
            }

            #endregion

            //将鼠标移动归一化到[-1, 1]范围
            float normalizedDeltaX = deltaX / this._viewportWidth * 2.0f;
            float normalizedDeltaY = deltaY / this._viewportHeight * 2.0f;

            //考虑缩放因子
            float panSpeed = 1.0f / this._zoomFactor;

            this._panOffset.X -= normalizedDeltaX * panSpeed;
            this._panOffset.Y += normalizedDeltaY * panSpeed;

            this.UpdateProjectionMatrix();
        }
        #endregion

        #region 重置相机 —— void Reset()
        /// <summary>
        /// 重置相机
        /// </summary>
        public void Reset()
        {
            this._zoomFactor = 1.0f;
            this._panOffset = Vector2.Zero;
            this.UpdateProjectionMatrix();
            this.UpdateCameraVectors();
        }
        #endregion

        #region 看向体素 —— void LookAtVoxel(Vector3i voxelPosition)
        /// <summary>
        /// 看向体素
        /// </summary>
        /// <param name="voxelPosition">体素位置</param>
        public void LookAtVoxel(Vector3i voxelPosition)
        {
            #region # 验证

            if (this._targetPlane == null)
            {
                return;
            }

            #endregion

            Vector3 texCoord = new Vector3(
                voxelPosition.X * 1.0f / (this._targetPlane.VolumeSize.X - 1),
                voxelPosition.Y * 1.0f / (this._targetPlane.VolumeSize.Y - 1),
                voxelPosition.Z * 1.0f / (this._targetPlane.VolumeSize.Z - 1)
            );
            Vector3 localPoint = texCoord - new Vector3(0.5f);

            Vector3 worldPoint = new Vector3(
                localPoint.X * this._targetPlane.VolumeScale.X,
                localPoint.Y * this._targetPlane.VolumeScale.Y,
                localPoint.Z * this._targetPlane.VolumeScale.Z
            );

            this._targetPosition = worldPoint;

            Vector3 worldNormal = new Vector3(this._targetPlane.Normal.X * this._targetPlane.VolumeScale.X, this._targetPlane.Normal.Y * this._targetPlane.VolumeScale.Y, this._targetPlane.Normal.Z * this._targetPlane.VolumeScale.Z
            ).Normalized();

            this._cameraPosition = this._targetPosition - worldNormal * this._distance;
            this.UpdateViewMatrix();
        }
        #endregion

        #region 看向体素 —— void LookAtVoxel(Vector3i voxelPosition, Vector2 offsetUV)
        /// <summary>
        /// 看向体素
        /// </summary>
        /// <param name="voxelPosition">体素坐标</param>
        /// <param name="offsetUV">UV偏移量</param>
        public void LookAtVoxel(Vector3i voxelPosition, Vector2 offsetUV)
        {
            #region # 验证

            if (this._targetPlane == null)
            {
                return;
            }

            #endregion

            Vector3 texCoord = new Vector3(
                voxelPosition.X * 1.0f / (this._targetPlane.VolumeSize.X - 1),
                voxelPosition.Y * 1.0f / (this._targetPlane.VolumeSize.Y - 1),
                voxelPosition.Z * 1.0f / (this._targetPlane.VolumeSize.Z - 1)
            );
            Vector3 localPoint = texCoord - new Vector3(0.5f);

            const float halfSize = 0.5f;
            Vector3 offsetLocal = this._targetPlane.UAxis * offsetUV.X * halfSize + this._targetPlane.VAxis * offsetUV.Y * halfSize;
            localPoint += offsetLocal;

            Vector3 worldPoint = new Vector3(
                localPoint.X * this._targetPlane.VolumeScale.X,
                localPoint.Y * this._targetPlane.VolumeScale.Y,
                localPoint.Z * this._targetPlane.VolumeScale.Z
            );

            this._targetPosition = worldPoint;

            Vector3 worldNormal = new Vector3(this._targetPlane.Normal.X * this._targetPlane.VolumeScale.X, this._targetPlane.Normal.Y * this._targetPlane.VolumeScale.Y, this._targetPlane.Normal.Z * this._targetPlane.VolumeScale.Z
            ).Normalized();

            this._cameraPosition = this._targetPosition - worldNormal * this._distance;
            this.UpdateViewMatrix();
        }
        #endregion


        //Private

        #region 更新相机向量 —— void UpdateCameraVectors()
        /// <summary>
        /// 更新相机向量
        /// </summary>
        private void UpdateCameraVectors()
        {
            #region # 验证

            if (this._targetPlane == null)
            {
                return;
            }

            #endregion

            float sliceOffset = this._targetPlane.GetSliceOffset();

            Vector3 worldNormal = new Vector3(this._targetPlane.Normal.X * this._targetPlane.VolumeScale.X, this._targetPlane.Normal.Y * this._targetPlane.VolumeScale.Y, this._targetPlane.Normal.Z * this._targetPlane.VolumeScale.Z
            ).Normalized();

            Vector3 worldVAxis = new Vector3(this._targetPlane.VAxis.X * this._targetPlane.VolumeScale.X, this._targetPlane.VAxis.Y * this._targetPlane.VolumeScale.Y, this._targetPlane.VAxis.Z * this._targetPlane.VolumeScale.Z
            ).Normalized();

            float normalScale = Math.Abs(Vector3.Dot(worldNormal, this._targetPlane.VolumeScale));
            float worldSliceOffset = sliceOffset * normalScale;

            Vector3 planePosition = worldNormal * worldSliceOffset;

            this._cameraPosition = planePosition - worldNormal * this._distance;
            this._targetPosition = planePosition;
            this._upDirection = worldVAxis;

            this.UpdateViewMatrix();
        }
        #endregion

        #region 更新投影矩阵 —— void UpdateProjectionMatrix()
        /// <summary>
        /// 更新投影矩阵
        /// </summary>
        private void UpdateProjectionMatrix()
        {
            #region # 验证

            if (this._viewportWidth <= 0 || this._viewportHeight <= 0)
            {
                this._projectionMatrix = Matrix4.Identity;
                return;
            }

            #endregion

            float aspect = this._viewportWidth / this._viewportHeight;
            float halfSideSize = 0.5f / this._zoomFactor;
            float left, right, bottom, top;

            if (aspect >= 1.0f)
            {
                left = -halfSideSize * aspect + this._panOffset.X;
                right = halfSideSize * aspect + this._panOffset.X;
                bottom = -halfSideSize + this._panOffset.Y;
                top = halfSideSize + this._panOffset.Y;
            }
            else
            {
                left = -halfSideSize + this._panOffset.X;
                right = halfSideSize + this._panOffset.X;
                bottom = -halfSideSize / aspect + this._panOffset.Y;
                top = halfSideSize / aspect + this._panOffset.Y;
            }

            this._projectionMatrix = Matrix4.CreateOrthographicOffCenter(left, right, bottom, top, this._nearPlaneDistance, this._farPlaneDistance);
        }
        #endregion

        #region 更新视图矩阵 —— void UpdateViewMatrix()
        /// <summary>
        /// 更新视图矩阵
        /// </summary>
        private void UpdateViewMatrix()
        {
            this._viewMatrix = Matrix4.LookAt(this.CameraPosition, this.TargetPosition, this.UpDirection);
        }
        #endregion

        #region MPR平面变化事件 —— void OnPlaneChanged(MPRPlane plane)
        /// <summary>
        /// MPR平面变化事件
        /// </summary>
        /// <param name="plane">MPR平面</param>
        private void OnPlaneChanged(MPRPlane plane)
        {
            this.UpdateCameraVectors();
        }
        #endregion

        #endregion
    }
}
