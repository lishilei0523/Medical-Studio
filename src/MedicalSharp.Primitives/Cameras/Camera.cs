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
        /// 视口宽度
        /// </summary>
        protected float _viewportWidth;

        /// <summary>
        /// 视口高度
        /// </summary>
        protected float _viewportHeight;

        /// <summary>
        /// 近平面距离
        /// </summary>
        protected float _nearPlaneDistance;

        /// <summary>
        /// 远平面距离
        /// </summary>
        protected float _farPlaneDistance;

        /// <summary>
        /// 创建相机构造器
        /// </summary>
        /// <param name="nearPlaneDistance">近平面距离</param>
        /// <param name="farPlaneDistance">远平面距离</param>
        protected Camera(float nearPlaneDistance = -1.0f, float farPlaneDistance = 1.0f)
        {
            this._viewportWidth = 0;
            this._viewportHeight = 0;
            this._nearPlaneDistance = nearPlaneDistance;
            this._farPlaneDistance = farPlaneDistance;
        }

        #endregion

        #region # 属性

        #region 相机位置 —— abstract Vector3 CameraPosition
        /// <summary>
        /// 相机位置
        /// </summary>
        public abstract Vector3 CameraPosition { get; }
        #endregion

        #region 视角方向 —— abstract Vector3 LookDirection 
        /// <summary>
        /// 视角方向
        /// </summary>
        public abstract Vector3 LookDirection { get; }
        #endregion

        #region 相机上方向 —— abstract Vector3 UpDirection
        /// <summary>
        /// 相机上方向
        /// </summary>
        public abstract Vector3 UpDirection { get; }
        #endregion

        #region 相机右方向 —— abstract Vector3 RightDirection
        /// <summary>
        /// 相机右方向
        /// </summary>
        public abstract Vector3 RightDirection { get; }
        #endregion

        #region 投影矩阵 —— abstract Matrix4 ProjectionMatrix
        /// <summary>
        /// 投影矩阵
        /// </summary>
        public abstract Matrix4 ProjectionMatrix { get; }
        #endregion

        #region 视图矩阵 —— abstract Matrix4 ViewMatrix
        /// <summary>
        /// 视图矩阵
        /// </summary>
        public abstract Matrix4 ViewMatrix { get; }
        #endregion

        #region 只读属性 - 视口宽度 —— float ViewportWidth
        /// <summary>
        /// 只读属性 - 视口宽度
        /// </summary>
        public float ViewportWidth
        {
            get => this._viewportWidth;
        }
        #endregion

        #region 只读属性 - 视口高度 —— float ViewportHeight
        /// <summary>
        /// 只读属性 - 视口高度
        /// </summary>
        public float ViewportHeight
        {
            get => this._viewportHeight;
        }
        #endregion

        #endregion

        #region # 方法

        #region 设置视口尺寸 —— virtual void SetViewportSize(float width, float height)
        /// <summary>
        /// 设置视口尺寸
        /// </summary>
        /// <param name="width">视口宽度</param>
        /// <param name="height">视口高度</param>
        public virtual void SetViewportSize(float width, float height)
        {
            this._viewportWidth = width;
            this._viewportHeight = height;
        }
        #endregion

        #region 看向指定位置 —— abstract void LookAt(Vector3 targetPosition)
        /// <summary>
        /// 看向指定位置
        /// </summary>
        /// <param name="targetPosition">目标位置（世界坐标）</param>
        public abstract void LookAt(Vector3 targetPosition);
        #endregion

        #endregion
    }
}
