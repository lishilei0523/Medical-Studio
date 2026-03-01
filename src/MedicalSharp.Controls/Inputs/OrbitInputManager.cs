using Avalonia;
using Avalonia.Input;
using MedicalSharp.Engine.Cameras;

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
            get => this.Camera;
        }
        #endregion

        #endregion

        #region # 方法

        #region 鼠标移动事件 —— override void OnMouseMove(MouseButton button, Point position)
        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        /// <param name="button">鼠标按键</param>
        /// <param name="position">鼠标位置</param>
        public override void OnMouseMove(MouseButton button, Point position)
        {
            if (this._mousePosition2D.HasValue)
            {
                float deltaX = (float)(position.X - this._mousePosition2D.Value.X);
                float deltaY = (float)(position.Y - this._mousePosition2D.Value.Y);
                if (button == MouseButton.Middle)
                {
                    this._camera.Pan(deltaX / 50.0f, deltaY / 50.0f);
                }
                if (button == MouseButton.Right)
                {
                    this._camera.Rotate(deltaX, -deltaY);
                }
            }
            this._mousePosition2D = position;
        }
        #endregion

        #region 鼠标滚轮事件 —— override void OnMouseWheel(double offsetX, double offsetY)
        /// <summary>
        /// 鼠标滚轮事件
        /// </summary>
        /// <param name="offsetX">X轴偏移量</param>
        /// <param name="offsetY">Y轴偏移量</param>
        public override void OnMouseWheel(double offsetX, double offsetY)
        {
            this._camera.Zoom((float)offsetY);
        }
        #endregion 

        #region 键盘按下事件 —— override void OnKeyDown(Key key)
        /// <summary>
        /// 键盘按下事件
        /// </summary>
        /// <param name="key">键</param>
        public override void OnKeyDown(Key key)
        {
            if (key == Key.W)
            {
                this._camera.Zoom(0.1f);
            }
            if (key == Key.S)
            {
                this._camera.Zoom(-0.1f);
            }
            if (key == Key.A)
            {
                this._camera.Pan(-0.5f, 0);
            }
            if (key == Key.D)
            {
                this._camera.Pan(0.5f, 0);
            }
            if (key == Key.Up)
            {
                this._camera.Rotate(0, 3);
            }
            if (key == Key.Down)
            {
                this._camera.Rotate(0, -3);
            }
            if (key == Key.Left)
            {
                this._camera.Rotate(-3, 0);
            }
            if (key == Key.Right)
            {
                this._camera.Rotate(3, 0);
            }
        }
        #endregion

        #endregion
    }
}
