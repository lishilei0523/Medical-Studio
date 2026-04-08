using Avalonia;
using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Primitives.Cameras;
using System;

namespace MedicalSharp.Controls.Inputs
{
    /// <summary>
    /// MPR输入管理器
    /// </summary>
    public class MPRInputManager : InputManager
    {
        #region # 字段及构造器

        /// <summary>
        /// MPR相机
        /// </summary>
        private readonly MPRCamera _camera;

        /// <summary>
        /// 创建输入管理器构造器
        /// </summary>
        public MPRInputManager(MPRCamera camera)
        {
            this._camera = camera;
        }

        #endregion

        #region # 属性

        #region 只读属性 - MPR相机 —— MPRCamera Camera
        /// <summary>
        /// 只读属性 - MPR相机
        /// </summary>
        public MPRCamera Camera
        {
            get => this._camera;
        }
        #endregion

        #endregion

        #region # 方法

        #region 鼠标移动事件 —— override void OnMouseMove(OpenTKViewport viewport, MouseButton button...
        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        /// <param name="viewport">OpenTK视口</param>
        /// <param name="button">鼠标按键</param>
        /// <param name="position">鼠标位置</param>
        public override void OnMouseMove(OpenTKViewport viewport, MouseButton button, Point position)
        {
            if (this._mousePosition2D.HasValue)
            {
                float deltaX = (float)(position.X - this._mousePosition2D.Value.X);
                float deltaY = (float)(position.Y - this._mousePosition2D.Value.Y);
                if (button == MouseButton.Middle)
                {
                    this._camera.Pan(deltaX, deltaY);
                    viewport.RequestNextFrameRendering();
                }
                if (button == MouseButton.Right)
                {
                    this._camera.Zoom(-deltaY);
                    viewport.RequestNextFrameRendering();
                }
            }
            this._mousePosition2D = position;
        }
        #endregion

        #region 鼠标滚轮事件 —— override void OnMouseWheel(OpenTKViewport viewport, double offsetX...
        /// <summary>
        /// 鼠标滚轮事件
        /// </summary>
        /// <param name="viewport">OpenTK视口</param>
        /// <param name="offsetX">X轴偏移量</param>
        /// <param name="offsetY">Y轴偏移量</param>
        public override void OnMouseWheel(OpenTKViewport viewport, double offsetX, double offsetY)
        {
            this._camera.TargetPlane.SliceIndex += (int)Math.Ceiling(offsetY);
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 键盘按下事件 —— override void OnKeyDown(OpenTKViewport viewport, Key key)
        /// <summary>
        /// 键盘按下事件
        /// </summary>
        /// <param name="viewport">OpenTK视口</param>
        /// <param name="key">键</param>
        public override void OnKeyDown(OpenTKViewport viewport, Key key)
        {
            if (key == Key.Up)
            {
                this._camera.TargetPlane.Rotate(-3, 0);
                viewport.RequestNextFrameRendering();
            }
            if (key == Key.Down)
            {
                this._camera.TargetPlane.Rotate(3, 0);
                viewport.RequestNextFrameRendering();
            }
            if (key == Key.Left)
            {
                this._camera.TargetPlane.Rotate(0, 3);
                viewport.RequestNextFrameRendering();
            }
            if (key == Key.Right)
            {
                this._camera.TargetPlane.Rotate(0, -3);
                viewport.RequestNextFrameRendering();
            }
        }
        #endregion

        #endregion
    }
}
