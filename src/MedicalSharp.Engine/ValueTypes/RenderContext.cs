using OpenTK.Mathematics;

namespace MedicalSharp.Engine.ValueTypes
{
    /// <summary>
    /// 渲染上下文
    /// </summary>
    public sealed class RenderContext
    {
        /// <summary>
        /// 创建渲染上下文构造器
        /// </summary>
        /// <param name="viewportWidth">视口宽度</param>
        /// <param name="viewportHeight">视口高度</param>
        /// <param name="cameraPosition">相机位置</param>
        /// <param name="lookDirection">视角方向</param>
        /// <param name="projectionMatrix">投影矩阵</param>
        /// <param name="viewMatrix">视图矩阵</param>
        public RenderContext(float viewportWidth, float viewportHeight, Vector3 cameraPosition, Vector3 lookDirection, Matrix4 projectionMatrix, Matrix4 viewMatrix)
        {
            this.ViewportWidth = viewportWidth;
            this.ViewportHeight = viewportHeight;
            this.CameraPosition = cameraPosition;
            this.LookDirection = lookDirection;
            this.ProjectionMatrix = projectionMatrix;
            this.ViewMatrix = viewMatrix;
        }

        /// <summary>
        /// 视口宽度
        /// </summary>
        public float ViewportWidth { get; private set; }

        /// <summary>
        /// 视口高度
        /// </summary>
        public float ViewportHeight { get; private set; }

        /// <summary>
        /// 相机位置
        /// </summary>
        public Vector3 CameraPosition { get; private set; }

        /// <summary>
        /// 视角方向
        /// </summary>
        public Vector3 LookDirection { get; private set; }

        /// <summary>
        /// 投影矩阵
        /// </summary>
        public Matrix4 ProjectionMatrix { get; private set; }

        /// <summary>
        /// 视图矩阵
        /// </summary>
        public Matrix4 ViewMatrix { get; private set; }
    }
}
