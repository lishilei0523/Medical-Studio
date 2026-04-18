using Avalonia;
using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Primitives.Cameras;

namespace MedicalSharp.Controls.Inputs
{
    /// <summary>
    /// 轨道输入管理器
    /// </summary>
    public class OrbitInputManager : InputManager
    {
        #region # 字段及构造器

        /// <summary>
        /// 轨道相机
        /// </summary>
        private readonly OrbitCamera _camera;

        /// <summary>
        /// 创建轨道输入管理器构造器
        /// </summary>
        /// <param name="camera">轨道相机</param>
        public OrbitInputManager(OrbitCamera camera)
        {
            this._camera = camera;
        }

        #endregion

        #region # 属性

        #region 只读属性 - 轨道相机 —— OrbitCamera Camera
        /// <summary>
        /// 只读属性 - 轨道相机
        /// </summary>
        public OrbitCamera Camera
        {
            get => this._camera;
        }
        #endregion

        #endregion

        #region # 方法

        #region 鼠标移动事件 —— override void OnMouseMove(OpenTKViewport viewport, PointerEventArgs eventArgs)
        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        public override void OnMouseMove(OpenTKViewport viewport, PointerEventArgs eventArgs)
        {
            Point position = eventArgs.GetPosition(viewport);
            if (this._mousePosition2D.HasValue)
            {
                float deltaX = (float)(position.X - this._mousePosition2D.Value.X);
                float deltaY = (float)(position.Y - this._mousePosition2D.Value.Y);
                if (eventArgs.Properties.IsMiddleButtonPressed)
                {
                    this._camera.Pan(deltaX / 50.0f, deltaY / 50.0f);
                    viewport.RequestNextFrameRendering();
                }
                if (eventArgs.Properties.IsRightButtonPressed)
                {
                    this._camera.Rotate(deltaX, -deltaY);
                    viewport.RequestNextFrameRendering();
                }
            }
            this._mousePosition2D = position;
        }
        #endregion

        #region 鼠标滚轮事件 —— override void OnMouseWheel(OpenTKViewport viewport, PointerWheelEventArgs eventArgs)
        /// <summary>
        /// 鼠标滚轮事件
        /// </summary>
        public override void OnMouseWheel(OpenTKViewport viewport, PointerWheelEventArgs eventArgs)
        {
            this._camera.Zoom((float)eventArgs.Delta.Y);
            viewport.RequestNextFrameRendering();
        }
        #endregion 

        #region 键盘按下事件 —— override void OnKeyDown(OpenTKViewport viewport, KeyEventArgs eventArgs)
        /// <summary>
        /// 键盘按下事件
        /// </summary>
        public override void OnKeyDown(OpenTKViewport viewport, KeyEventArgs eventArgs)
        {
            if (eventArgs.Key == Key.W)
            {
                this._camera.Zoom(0.1f);
                viewport.RequestNextFrameRendering();
            }
            if (eventArgs.Key == Key.S)
            {
                this._camera.Zoom(-0.1f);
                viewport.RequestNextFrameRendering();
            }
            if (eventArgs.Key == Key.A)
            {
                this._camera.Pan(-0.5f, 0);
                viewport.RequestNextFrameRendering();
            }
            if (eventArgs.Key == Key.D)
            {
                this._camera.Pan(0.5f, 0);
                viewport.RequestNextFrameRendering();
            }
            if (eventArgs.Key == Key.Up)
            {
                this._camera.Rotate(0, 3);
                viewport.RequestNextFrameRendering();
            }
            if (eventArgs.Key == Key.Down)
            {
                this._camera.Rotate(0, -3);
                viewport.RequestNextFrameRendering();
            }
            if (eventArgs.Key == Key.Left)
            {
                this._camera.Rotate(-3, 0);
                viewport.RequestNextFrameRendering();
            }
            if (eventArgs.Key == Key.Right)
            {
                this._camera.Rotate(3, 0);
                viewport.RequestNextFrameRendering();
            }
        }
        #endregion

        #endregion
    }
}
