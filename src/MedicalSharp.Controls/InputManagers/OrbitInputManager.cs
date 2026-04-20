using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Commands;
using MedicalSharp.Primitives.Cameras;

namespace MedicalSharp.Controls.InputManagers
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
            this._command = new OrbitCameraCommand(camera);
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
            base.OnMouseMove(viewport, eventArgs);
        }
        #endregion

        #region 鼠标滚轮事件 —— override void OnMouseWheel(OpenTKViewport viewport, PointerWheelEventArgs eventArgs)
        /// <summary>
        /// 鼠标滚轮事件
        /// </summary>
        public override void OnMouseWheel(OpenTKViewport viewport, PointerWheelEventArgs eventArgs)
        {
            base.OnMouseWheel(viewport, eventArgs);
        }
        #endregion 

        #region 键盘按下事件 —— override void OnKeyDown(OpenTKViewport viewport, KeyEventArgs eventArgs)
        /// <summary>
        /// 键盘按下事件
        /// </summary>
        public override void OnKeyDown(OpenTKViewport viewport, KeyEventArgs eventArgs)
        {
            base.OnKeyDown(viewport, eventArgs);
        }
        #endregion

        #endregion
    }
}
