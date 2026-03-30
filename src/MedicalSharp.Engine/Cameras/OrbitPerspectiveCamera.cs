using OpenTK.Mathematics;

namespace MedicalSharp.Engine.Cameras
{
    /// <summary>
    /// 轨道透视相机
    /// </summary>
    public class OrbitPerspectiveCamera : OrbitCamera
    {
        #region # 字段及构造器

        /// <summary>
        /// 创建轨道透视相机构造器
        /// </summary>
        /// <param name="targetPosition">目标位置</param>
        /// <param name="distance">相机到目标的距离</param>
        /// <param name="yaw">偏航角-RY（角度）</param>
        /// <param name="pitch">俯仰角-RX（角度）</param>
        /// <param name="worldUpDirection">世界坐标系上方向</param>
        /// <param name="nearPlaneDistance">近平面距离</param>
        /// <param name="farPlaneDistance">远平面距离</param>
        /// <param name="fieldOfView">视野角度（度）</param>
        public OrbitPerspectiveCamera(Vector3 targetPosition = default, float distance = 5.0f, float yaw = 0.0f, float pitch = 0.0f, Vector3 worldUpDirection = default, float nearPlaneDistance = 0.125f, float farPlaneDistance = 65535.0f, float fieldOfView = 30.0f)
            : base(targetPosition, distance, yaw, pitch, worldUpDirection, nearPlaneDistance, farPlaneDistance)
        {
            this.FieldOfView = fieldOfView;
        }

        /// <summary>
        /// 创建轨道透视相机构造器
        /// </summary>
        /// <param name="cameraPosition">相机位置</param>
        /// <param name="targetPosition">目标位置</param>
        /// <param name="worldUpDirection">世界坐标系上方向</param>
        /// <param name="nearPlaneDistance">近平面距离</param>
        /// <param name="farPlaneDistance">远平面距离</param>
        /// <param name="fieldOfView">视野角度（度）</param>
        public OrbitPerspectiveCamera(Vector3 cameraPosition, Vector3 targetPosition, Vector3 worldUpDirection, float nearPlaneDistance = 0.125f, float farPlaneDistance = 65535.0f, float fieldOfView = 30.0f)
            : base(cameraPosition, targetPosition, worldUpDirection, nearPlaneDistance, farPlaneDistance)
        {
            this.FieldOfView = fieldOfView;
        }

        #endregion

        #region # 属性

        #region 视野角度 —— float FieldOfView
        /// <summary>
        /// 视野角度（度）
        /// </summary>
        public float FieldOfView { get; private set; }
        #endregion

        #region 只读属性 - 投影矩阵 —— override Matrix4 ProjectionMatrix
        /// <summary>
        /// 只读属性 - 投影矩阵
        /// </summary>
        public override Matrix4 ProjectionMatrix
        {
            get
            {
                //防止除以零
                if (this._viewportWidth <= 0 || this._viewportHeight <= 0)
                {
                    return Matrix4.Identity;
                }

                float aspectRatio = this._viewportWidth / this._viewportHeight;

                return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(this.FieldOfView), aspectRatio, this._nearPlaneDistance, this._farPlaneDistance);
            }
        }
        #endregion

        #endregion

        #region # 方法

        #region 设置视野角度 —— void SetFieldOfView(float fieldOfView)
        /// <summary>
        /// 设置视野角度
        /// </summary>
        /// <param name="fieldOfView">视野角度</param>
        public void SetFieldOfView(float fieldOfView)
        {
            this.FieldOfView = fieldOfView;
        }
        #endregion

        #endregion
    }
}
