using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models.Arguments;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Primitives.Cameras
{
    /// <summary>
    /// MPR相机
    /// </summary>
    public class MPRCamera : Camera
    {
        #region # 字段及构造器

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
        public MPRCamera(float nearPlaneDistance = -1000, float farPlaneDistance = 1000)
            : base(nearPlaneDistance, farPlaneDistance)
        {
            this._distance = 4.0f;
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

        #region 目标平面 —— MPRPlane TargetPlane
        /// <summary>
        /// 目标平面
        /// </summary>
        public MPRPlane TargetPlane { get; private set; }
        #endregion

        #region 只读属性 - 相机模式 —— override Vector3 CameraMode
        /// <summary>
        /// 只读属性 - 相机模式
        /// </summary>
        public override CameraMode CameraMode
        {
            get => CameraMode.Orthographic;
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

        #region 看向指定位置 —— override void LookAt(Vector3 targetPosition)
        /// <summary>
        /// 看向指定位置
        /// </summary>
        /// <param name="targetPosition">目标位置（世界坐标）</param>
        public override void LookAt(Vector3 targetPosition)
        {
            //更新目标位置
            this._targetPosition = targetPosition;

            //重新计算相机位置（保持原距离）
            if (this.TargetPlane != null)
            {
                this._cameraPosition = this._targetPosition - this.TargetPlane.WorldNormal * this._distance;
            }

            this.UpdateViewMatrix();
        }
        #endregion

        #region 绑定MPR平面 —— void BindPlane(MPRPlane plane)
        /// <summary>
        /// 绑定MPR平面
        /// </summary>
        /// <param name="plane">MPR平面</param>
        public void BindPlane(MPRPlane plane)
        {
            #region # 验证

            if (plane == null)
            {
                throw new ArgumentNullException(nameof(plane), "MPR平面不可为空！");
            }

            #endregion

            //卸载旧实例事件
            if (this.TargetPlane != null)
            {
                this.TargetPlane.PlaneChangedEvent -= this.OnPlaneChanged;
            }

            this.TargetPlane = plane;
            this.TargetPlane.PlaneChangedEvent += this.OnPlaneChanged;
            this.UpdateCameraVectors();
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

            if (this.ViewportWidth <= 0 || this.ViewportHeight <= 0)
            {
                return;
            }

            #endregion

            //将鼠标移动归一化到[-1, 1]范围
            float normalizedDeltaX = deltaX / this.ViewportWidth * 2.0f;
            float normalizedDeltaY = deltaY / this.ViewportHeight * 2.0f;

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


        //Private

        #region 更新相机向量 —— void UpdateCameraVectors()
        /// <summary>
        /// 更新相机向量
        /// </summary>
        private void UpdateCameraVectors()
        {
            #region # 验证

            if (this.TargetPlane == null)
            {
                return;
            }

            #endregion

            Vector3 worldCenter = this.TargetPlane.WorldCenter;
            Vector3 worldNormal = this.TargetPlane.WorldNormal;
            Vector3 worldUpDirection = this.TargetPlane.WorldVAxis;
            this._cameraPosition = worldCenter - worldNormal * this._distance;
            this._targetPosition = worldCenter;
            this._upDirection = worldUpDirection;

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

            if (this.ViewportWidth <= 0 || this.ViewportHeight <= 0)
            {
                this._projectionMatrix = Matrix4.Identity;
                return;
            }

            #endregion

            float aspect = this.ViewportWidth / this.ViewportHeight;
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

            this._projectionMatrix = Matrix4.CreateOrthographicOffCenter(left, right, bottom, top, this.NearPlaneDistance, this.FarPlaneDistance);
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

        #region MPR平面变化事件 —— void OnPlaneChanged(object sender...
        /// <summary>
        /// MPR平面变化事件
        /// </summary>
        private void OnPlaneChanged(object sender, MPRPlaneChangedEventArgs eventArgs)
        {
            this.UpdateCameraVectors();
        }
        #endregion

        #endregion
    }
}
