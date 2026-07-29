using MedicalSharp.Primitives.Enums;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Primitives.Cameras
{
    /// <summary>
    /// CPR相机
    /// </summary>
    /// <remarks>
    /// 正交投影相机，正对CPR图像四边形；
    /// 相机方向固定为+Z，看向原点；
    /// 旋转曲线观察角度由Shader中的采样方向参数控制，而非相机旋转；
    /// </remarks>
    public class CPRCamera : Camera
    {
        #region # 字段及构造器

        /// <summary>
        /// 创建CPR相机构造器
        /// </summary>
        /// <param name="nearPlaneDistance">近平面距离</param>
        /// <param name="farPlaneDistance">远平面距离</param>
        public CPRCamera(float nearPlaneDistance = -2, float farPlaneDistance = 2)
            : base(nearPlaneDistance, farPlaneDistance)
        {
            this.TargetPosition = Vector3.Zero;
            this.CameraPosition = new Vector3(0, 0, -1);
            this.Distance = 1.0f;
            this.SideSize = 1.0f;
            this.ZoomFactor = 1.0f;
            this.PanOffset = Vector2.Zero;

            this.UpdateCameraVectors();
            this.UpdateViewMatrix();
            this.UpdateProjectionMatrix();
        }

        #endregion

        #region # 属性

        #region 边长 —— float SideSize
        /// <summary>
        /// 边长
        /// </summary>
        public float SideSize { get; private set; }
        #endregion

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

        #region 只读属性 - 相机模式 —— override CameraMode CameraMode
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

        #region 设置边长 —— void SetSideSize(float sideSize)
        /// <summary>
        /// 设置边长
        /// </summary>
        /// <param name="sideSize">边长</param>
        public void SetSideSize(float sideSize)
        {
            this.SideSize = sideSize;
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
            this.TargetPosition = targetPosition;

            //重新计算相机位置（保持原距离）
            this.CameraPosition = this.TargetPosition - this.LookDirection * this.Distance;

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
            this.TargetPosition = Vector3.Zero;
            this.CameraPosition = new Vector3(0, 0, -1);
            this.Distance = 1.0f;

            this.UpdateCameraVectors();
            this.UpdateViewMatrix();
            this.UpdateProjectionMatrix();
        }
        #endregion


        //Private

        #region 更新相机向量 —— override void UpdateCameraVectors()
        /// <summary>
        /// 更新相机向量
        /// </summary>
        /// <remarks>
        /// CPR相机方向固定：
        /// 看向+Z方向，上方为+Y，右侧为+X；
        /// 旋转血管观察角度由Shader中的u_RotationAngle参数控制，不在此处处理；
        /// </remarks>
        protected override void UpdateCameraVectors()
        {
            this.LookDirection = Vector3.UnitZ;
            this.UpDirection = Vector3.UnitY;
            this.RightDirection = Vector3.UnitX;
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
            float halfSideSize = this.SideSize / 2.0f / this.ZoomFactor;
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

        #endregion
    }
}
