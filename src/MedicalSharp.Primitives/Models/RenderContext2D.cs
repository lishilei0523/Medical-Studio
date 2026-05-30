using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Primitives.Models
{
    /// <summary>
    /// 渲染上下文(2D)
    /// </summary>
    /// <remarks>用于Overlay系统，屏幕空间渲染</remarks>
    public sealed class RenderContext2D
    {
        /// <summary>
        /// 创建渲染上下文(2D)构造器
        /// </summary>
        /// <param name="glContext">OpenGL上下文句柄</param>
        /// <param name="viewportWidth">视口宽度</param>
        /// <param name="viewportHeight">视口高度</param>
        /// <param name="mainCameraRotation">主相机旋转矩阵（用于同步方向）</param>
        public RenderContext2D(IntPtr glContext, int viewportWidth, int viewportHeight, Matrix4 mainCameraRotation)
        {
            this.GlContext = glContext;
            this.ViewportWidth = viewportWidth;
            this.ViewportHeight = viewportHeight;
            this.MainCameraRotation = mainCameraRotation;
            this.OrthoMatrix = Matrix4.CreateOrthographicOffCenter(0, viewportWidth, viewportHeight, 0, -1, 1);
        }

        /// <summary>
        /// OpenGL上下文句柄
        /// </summary>
        public IntPtr GlContext { get; private set; }

        /// <summary>
        /// 视口宽度
        /// </summary>
        public int ViewportWidth { get; private set; }

        /// <summary>
        /// 视口高度
        /// </summary>
        public int ViewportHeight { get; private set; }

        /// <summary>
        /// 主相机旋转矩阵（用于同步方向）
        /// </summary>
        /// <remarks>坐标轴等元素需要跟随相机旋转时使用</remarks>
        public Matrix4 MainCameraRotation { get; private set; }

        /// <summary>
        /// 正交投影矩阵
        /// </summary>
        /// <remarks>固定正交投影，屏幕左上角为原点(0,0)，右下角为(Width, Height)</remarks>
        public Matrix4 OrthoMatrix { get; private set; }
    }
}
