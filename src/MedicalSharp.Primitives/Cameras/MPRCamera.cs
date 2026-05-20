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
        /// 创建MPR相机构造器
        /// </summary>
        /// <param name="nearPlaneDistance">近平面距离</param>
        /// <param name="farPlaneDistance">远平面距离</param>
        public MPRCamera(float nearPlaneDistance = -5, float farPlaneDistance = 5)
            : base(nearPlaneDistance, farPlaneDistance)
        {
            this.TargetPosition = Vector3.Zero;
            this.Distance = 4.0f;
            this.ZoomFactor = 1.0f;
            this.PanOffset = Vector2.Zero;
            this.UpdateViewMatrix();
            this.UpdateProjectionMatrix();
        }

        #endregion

        #region # 属性

        #region 缩放因子 —— float ZoomFactor
        /// <summary>
        /// 缩放因子
        /// </summary>
        public float ZoomFactor { get; private set; }
        #endregion

        #region 平移偏移量 —— Vector2 PanOffset
        /// <summary>
        /// 平移偏移量
        /// </summary>
        public Vector2 PanOffset { get; private set; }
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

        #region 看向指定位置 —— override void LookAt(Vector3 targetPosition)
        /// <summary>
        /// 看向指定位置
        /// </summary>
        /// <param name="targetPosition">目标位置（世界坐标）</param>
        public override void LookAt(Vector3 targetPosition)
        {
            //更新目标位置
            this.TargetPosition = targetPosition;

            //重新计算相机位置（保持原距离）
            if (this.TargetPlane != null)
            {
                this.CameraPosition = this.TargetPosition - this.TargetPlane.WorldNormal * this.Distance;
            }

            this.UpdateViewMatrix();
        }
        #endregion

        #region 缩放相机 —— override void Zoom(float delta)
        /// <summary>
        /// 缩放相机
        /// </summary>
        /// <param name="delta">缩放增量</param>
        /// <remarks>正数放大，负数缩小</remarks>
        public override void Zoom(float delta)
        {
            this.ZoomFactor *= (1.0f + delta * 0.1f);
            this.ZoomFactor = Math.Clamp(this.ZoomFactor, 0.1f, 10.0f);
            this.UpdateProjectionMatrix();
        }
        #endregion

        #region 平移相机 —— override void Pan(float deltaX, float deltaY)
        /// <summary>
        /// 平移相机
        /// </summary>
        /// <param name="deltaX">水平平移量</param>
        /// <param name="deltaY">垂直平移量</param>
        public override void Pan(float deltaX, float deltaY)
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
            float panSpeed = 1.0f / this.ZoomFactor;

            Vector2 panOffset = this.PanOffset;
            panOffset.X -= normalizedDeltaX * panSpeed;
            panOffset.Y += normalizedDeltaY * panSpeed;
            this.PanOffset = panOffset;

            this.UpdateProjectionMatrix();
        }
        #endregion

        #region 重置相机 —— override void Reset()
        /// <summary>
        /// 重置相机
        /// </summary>
        public override void Reset()
        {
            this.ZoomFactor = 1.0f;
            this.PanOffset = Vector2.Zero;
            this.UpdateCameraVectors();
            this.UpdateProjectionMatrix();
        }
        #endregion


        //Private

        #region 更新相机向量 —— override void UpdateCameraVectors()
        /// <summary>
        /// 更新相机向量
        /// </summary>
        protected override void UpdateCameraVectors()
        {
            #region # 验证

            if (this.TargetPlane == null)
            {
                return;
            }

            #endregion

            Vector3 worldCenter = this.TargetPlane.WorldCenter;
            Vector3 worldUAxis = this.TargetPlane.WorldUAxis.Normalized();
            Vector3 worldVAxis = this.TargetPlane.WorldVAxis.Normalized();
            Vector3 worldNormal = this.TargetPlane.WorldNormal.Normalized();

            this.CameraPosition = worldCenter - worldNormal * this.Distance;
            this.TargetPosition = worldCenter;
            this.LookDirection = worldNormal;
            this.UpDirection = worldVAxis;
            this.RightDirection = worldUAxis;
            this.UpdateViewMatrix();
        }
        #endregion

        #region 更新投影矩阵 —— override void UpdateProjectionMatrix()
        /// <summary>
        /// 更新投影矩阵
        /// </summary>
        protected override void UpdateProjectionMatrix()
        {
            #region # 验证

            if (this.ViewportWidth <= 0 || this.ViewportHeight <= 0)
            {
                this.ProjectionMatrix = Matrix4.Identity;
                return;
            }

            #endregion

            float aspect = this.ViewportWidth / this.ViewportHeight;
            float halfSideSize = 0.5f / this.ZoomFactor;
            float left, right, bottom, top;
            if (aspect >= 1.0f)
            {
                left = -halfSideSize * aspect + this.PanOffset.X;
                right = halfSideSize * aspect + this.PanOffset.X;
                bottom = -halfSideSize + this.PanOffset.Y;
                top = halfSideSize + this.PanOffset.Y;
            }
            else
            {
                left = -halfSideSize + this.PanOffset.X;
                right = halfSideSize + this.PanOffset.X;
                bottom = -halfSideSize / aspect + this.PanOffset.Y;
                top = halfSideSize / aspect + this.PanOffset.Y;
            }

            this.ProjectionMatrix = Matrix4.CreateOrthographicOffCenter(left, right, bottom, top, this.NearPlaneDistance, this.FarPlaneDistance);
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
