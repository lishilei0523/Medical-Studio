using MedicalSharp.Primitives.Enums;
using OpenTK.Mathematics;

namespace MedicalSharp.Primitives.Cameras
{
    /// <summary>
    /// 相机
    /// </summary>
    public abstract class Camera
    {
        #region # 字段及构造器

        /// <summary>
        /// 创建相机构造器
        /// </summary>
        /// <param name="nearPlaneDistance">近平面距离</param>
        /// <param name="farPlaneDistance">远平面距离</param>
        protected Camera(float nearPlaneDistance = -1.0f, float farPlaneDistance = 1.0f)
        {
            this.ViewportWidth = 0;
            this.ViewportHeight = 0;
            this.NearPlaneDistance = nearPlaneDistance;
            this.FarPlaneDistance = farPlaneDistance;
        }

        #endregion

        #region # 属性

        #region 相机位置 —— Vector3 CameraPosition
        /// <summary>
        /// 相机位置
        /// </summary>
        public Vector3 CameraPosition { get; protected set; }
        #endregion

        #region 目标位置 —— Vector3 TargetPosition
        /// <summary>
        /// 目标位置
        /// </summary>
        public Vector3 TargetPosition { get; protected set; }
        #endregion

        #region 相机到目标距离 —— float Distance
        /// <summary>
        /// 相机到目标距离
        /// </summary>
        public float Distance { get; protected set; }
        #endregion

        #region 视角方向 —— Vector3 LookDirection 
        /// <summary>
        /// 视角方向
        /// </summary>
        public Vector3 LookDirection { get; protected set; }
        #endregion

        #region 相机上方向 —— Vector3 UpDirection
        /// <summary>
        /// 相机上方向
        /// </summary>
        public Vector3 UpDirection { get; protected set; }
        #endregion

        #region 相机右方向 —— Vector3 RightDirection
        /// <summary>
        /// 相机右方向
        /// </summary>
        public Vector3 RightDirection { get; protected set; }
        #endregion

        #region 投影矩阵 —— Matrix4 ProjectionMatrix
        /// <summary>
        /// 投影矩阵
        /// </summary>
        public Matrix4 ProjectionMatrix { get; protected set; }
        #endregion

        #region 视图矩阵 —— Matrix4 ViewMatrix
        /// <summary>
        /// 视图矩阵
        /// </summary>
        public Matrix4 ViewMatrix { get; protected set; }
        #endregion

        #region 近平面距离 —— float NearPlaneDistance
        /// <summary>
        /// 近平面距离
        /// </summary>
        public float NearPlaneDistance { get; protected set; }
        #endregion

        #region 远平面距离 —— float FarPlaneDistance
        /// <summary>
        /// 远平面距离
        /// </summary>
        public float FarPlaneDistance { get; protected set; }
        #endregion

        #region 视口宽度 —— float ViewportWidth
        /// <summary>
        /// 视口宽度
        /// </summary>
        public float ViewportWidth { get; protected set; }
        #endregion

        #region 视口高度 —— float ViewportHeight
        /// <summary>
        /// 视口高度
        /// </summary>
        public float ViewportHeight { get; protected set; }
        #endregion

        #region 只读属性 - 相机模式 —— abstract Vector3 CameraMode
        /// <summary>
        /// 只读属性 - 相机模式
        /// </summary>
        public abstract CameraMode CameraMode { get; }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 设置视口尺寸 —— virtual void SetViewportSize(float width, float height)
        /// <summary>
        /// 设置视口尺寸
        /// </summary>
        /// <param name="width">视口宽度</param>
        /// <param name="height">视口高度</param>
        public virtual void SetViewportSize(float width, float height)
        {
            this.ViewportWidth = width;
            this.ViewportHeight = height;
            this.UpdateProjectionMatrix();
        }
        #endregion

        #region 看向指定位置 —— abstract void LookAt(Vector3 targetPosition)
        /// <summary>
        /// 看向指定位置
        /// </summary>
        /// <param name="targetPosition">目标位置（世界坐标）</param>
        public abstract void LookAt(Vector3 targetPosition);
        #endregion

        #region 缩放相机 —— abstract void Zoom(float delta)
        /// <summary>
        /// 缩放相机
        /// </summary>
        /// <param name="delta">缩放增量</param>
        /// <remarks>正数放大，负数缩小</remarks>
        public abstract void Zoom(float delta);
        #endregion

        #region 平移相机 —— abstract void Pan(float deltaX, float deltaY)
        /// <summary>
        /// 平移相机
        /// </summary>
        /// <param name="deltaX">水平平移量</param>
        /// <param name="deltaY">垂直平移量</param>
        public abstract void Pan(float deltaX, float deltaY);
        #endregion

        #region 重置相机 —— abstract void Reset()
        /// <summary>
        /// 重置相机
        /// </summary>
        public abstract void Reset();
        #endregion


        //Protected

        #region 更新相机向量 —— abstract void UpdateCameraVectors()
        /// <summary>
        /// 更新相机向量
        /// </summary>
        protected abstract void UpdateCameraVectors();
        #endregion

        #region 更新投影矩阵 —— abstract void UpdateProjectionMatrix()
        /// <summary>
        /// 更新投影矩阵
        /// </summary>
        protected abstract void UpdateProjectionMatrix();
        #endregion

        #region 更新视图矩阵 —— virtual void UpdateViewMatrix()
        /// <summary>
        /// 更新视图矩阵
        /// </summary>
        protected virtual void UpdateViewMatrix()
        {
            this.ViewMatrix = Matrix4.LookAt(this.CameraPosition, this.TargetPosition, this.UpDirection);
        }
        #endregion

        #endregion
    }
}
