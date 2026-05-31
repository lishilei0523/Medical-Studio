using MedicalSharp.Primitives.Enums;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Primitives.Models
{
    /// <summary>
    /// 渲染上下文(3D)
    /// </summary>
    public sealed class RenderContext3D
    {
        /// <summary>
        /// 创建渲染上下文(3D)构造器
        /// </summary>
        /// <param name="glContext">OpenGL上下文句柄</param>
        /// <param name="viewportWidth">视口宽度</param>
        /// <param name="viewportHeight">视口高度</param>
        /// <param name="cameraMode">相机模式</param>
        /// <param name="cameraPosition">相机位置</param>
        /// <param name="lookDirection">相机视角方向</param>
        /// <param name="upDirection">相机上方向</param>
        /// <param name="rightDirection">相机右方向</param>
        /// <param name="projectionMatrix">投影矩阵</param>
        /// <param name="viewMatrix">视图矩阵</param>
        /// <param name="fieldOfView">视野角度</param>
        /// <param name="zoomFactor">缩放因子</param>
        public RenderContext3D(IntPtr glContext, float viewportWidth, float viewportHeight, CameraMode cameraMode, Vector3 cameraPosition, Vector3 lookDirection, Vector3 upDirection, Vector3 rightDirection, Matrix4 projectionMatrix, Matrix4 viewMatrix, float fieldOfView = 30.0f, float zoomFactor = 1.0f)
        {
            this.GlContext = glContext;
            this.ViewportWidth = viewportWidth;
            this.ViewportHeight = viewportHeight;
            this.CameraMode = cameraMode;
            this.CameraPosition = cameraPosition;
            this.LookDirection = lookDirection;
            this.UpDirection = upDirection;
            this.RightDirection = rightDirection;
            this.ProjectionMatrix = projectionMatrix;
            this.ViewMatrix = viewMatrix;
            this.FieldOfView = fieldOfView;
            this.ZoomFactor = zoomFactor;
        }

        /// <summary>
        /// OpenGL上下文句柄
        /// </summary>
        public IntPtr GlContext { get; private set; }

        /// <summary>
        /// 视口宽度
        /// </summary>
        public float ViewportWidth { get; private set; }

        /// <summary>
        /// 视口高度
        /// </summary>
        public float ViewportHeight { get; private set; }

        /// <summary>
        /// 相机模式
        /// </summary>
        public CameraMode CameraMode { get; private set; }

        /// <summary>
        /// 相机位置
        /// </summary>
        public Vector3 CameraPosition { get; private set; }

        /// <summary>
        /// 视角方向
        /// </summary>
        public Vector3 LookDirection { get; private set; }

        /// <summary>
        /// 相机上方向
        /// </summary>
        public Vector3 UpDirection { get; private set; }

        /// <summary>
        /// 相机右方向
        /// </summary>
        public Vector3 RightDirection { get; private set; }

        /// <summary>
        /// 投影矩阵
        /// </summary>
        public Matrix4 ProjectionMatrix { get; private set; }

        /// <summary>
        /// 视图矩阵
        /// </summary>
        public Matrix4 ViewMatrix { get; private set; }

        /// <summary>
        /// 视野角度
        /// </summary>
        /// <remarks>透视相机适用</remarks>
        public float FieldOfView { get; private set; }

        /// <summary>
        /// 缩放因子
        /// </summary>
        /// <remarks>MPR相机适用</remarks>
        public float ZoomFactor { get; private set; }
    }
}
