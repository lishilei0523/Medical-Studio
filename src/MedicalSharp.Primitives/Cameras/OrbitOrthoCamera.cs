using MedicalSharp.Primitives.Enums;
using OpenTK.Mathematics;

namespace MedicalSharp.Primitives.Cameras
{
    /// <summary>
    /// 轨道正交相机
    /// </summary>
    public class OrbitOrthoCamera : OrbitCamera
    {
        #region # 字段及构造器

        /// <summary>
        /// 创建轨道正交相机构造器
        /// </summary>
        /// <param name="targetPosition">目标位置</param>
        /// <param name="distance">相机到目标的距离</param>
        /// <param name="yaw">偏航角-RY（角度）</param>
        /// <param name="pitch">俯仰角-RX（角度）</param>
        /// <param name="worldUpDirection">世界坐标系上方向</param>
        /// <param name="nearPlaneDistance">近平面距离</param>
        /// <param name="farPlaneDistance">远平面距离</param>
        public OrbitOrthoCamera(Vector3 targetPosition = default, float distance = 5.0f, float yaw = 0.0f, float pitch = 0.0f, Vector3 worldUpDirection = default, float nearPlaneDistance = -100.0f, float farPlaneDistance = 100.0f)
            : base(targetPosition, distance, yaw, pitch, worldUpDirection, nearPlaneDistance, farPlaneDistance)
        {

        }

        /// <summary>
        /// 创建轨道正交相机构造器
        /// </summary>
        /// <param name="cameraPosition">相机位置</param>
        /// <param name="targetPosition">目标位置</param>
        /// <param name="worldUpDirection">世界坐标系上方向</param>
        /// <param name="nearPlaneDistance">近平面距离</param>
        /// <param name="farPlaneDistance">远平面距离</param>
        public OrbitOrthoCamera(Vector3 cameraPosition, Vector3 targetPosition, Vector3 worldUpDirection, float nearPlaneDistance = 0.125f, float farPlaneDistance = 65535)
            : base(cameraPosition, targetPosition, worldUpDirection, nearPlaneDistance, farPlaneDistance)
        {

        }

        #endregion

        #region # 属性

        #region 相机模式 —— override Vector3 CameraMode
        /// <summary>
        /// 相机模式
        /// </summary>
        public override CameraMode CameraMode
        {
            get => CameraMode.Orthographic;
        }
        #endregion

        #region 只读属性 - 投影矩阵 —— override Matrix4 ProjectionMatrix
        /// <summary>
        /// 只读属性 - 投影矩阵
        /// </summary>
        public override Matrix4 ProjectionMatrix
        {
            get
            {
                //防止视口尺寸无效
                if (this.ViewportWidth <= 0 || this.ViewportHeight <= 0)
                {
                    return Matrix4.Identity;
                }

                //TODO 完善
                //计算正交投影的边界
                //除以100.0f是为了调整缩放比例，使相机距离与实际显示比例匹配
                float left = -this.ViewportWidth / 2.0f / 100.0f;
                float right = this.ViewportWidth / 2.0f / 100.0f;
                float bottom = -this.ViewportHeight / 2.0f / 100.0f;
                float top = this.ViewportHeight / 2.0f / 100.0f;

                // 创建正交投影矩阵
                return Matrix4.CreateOrthographicOffCenter(left, right, bottom, top, this.NearPlaneDistance, this.FarPlaneDistance);
            }
        }
        #endregion

        #endregion
    }
}
