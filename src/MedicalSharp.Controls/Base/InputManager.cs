using Avalonia;
using Avalonia.Input;
using IInputManager = MedicalSharp.Controls.Interfaces.IInputManager;

namespace MedicalSharp.Controls.Base
{
    /// <summary>
    /// 输入管理器
    /// </summary>
    public abstract class InputManager : IInputManager
    {
        #region # 字段及构造器

        /// <summary>
        /// 鼠标位置
        /// </summary>
        protected Point? _mousePosition2D;

        /// <summary>
        /// 创建输入管理器构造器
        /// </summary>
        protected InputManager()
        {
            this._mousePosition2D = null;
        }

        #endregion

        #region # 属性

        #region 只读属性 - 鼠标位置 —— Point? MousePosition2D
        /// <summary>
        /// 只读属性 - 鼠标位置
        /// </summary>
        public Point? MousePosition2D
        {
            get => this._mousePosition2D;
        }
        #endregion

        #endregion

        #region # 方法

        #region 鼠标按下事件 —— virtual void OnMouseDown(OpenTKViewport viewport, PointerPressedEventArgs eventArgs)
        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        public virtual void OnMouseDown(OpenTKViewport viewport, PointerPressedEventArgs eventArgs)
        {
            this._mousePosition2D = eventArgs.GetPosition(viewport);
        }
        #endregion

        #region 鼠标松开事件 —— virtual void OnMouseUp(OpenTKViewport viewport, PointerReleasedEventArgs eventArgs)
        /// <summary>
        /// 鼠标松开事件
        /// </summary>
        public virtual void OnMouseUp(OpenTKViewport viewport, PointerReleasedEventArgs eventArgs)
        {
            this._mousePosition2D = null;
        }
        #endregion

        #region 鼠标移动事件 —— virtual void OnMouseMove(OpenTKViewport viewport, PointerEventArgs eventArgs)
        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        public virtual void OnMouseMove(OpenTKViewport viewport, PointerEventArgs eventArgs)
        {

        }
        #endregion

        #region 鼠标滚轮事件 —— virtual void OnMouseWheel(OpenTKViewport viewport, PointerWheelEventArgs eventArgs)
        /// <summary>
        /// 鼠标滚轮事件
        /// </summary>
        public virtual void OnMouseWheel(OpenTKViewport viewport, PointerWheelEventArgs eventArgs)
        {

        }
        #endregion

        #region 键盘按下事件 —— virtual void OnKeyDown(OpenTKViewport viewport, KeyEventArgs eventArgs)
        /// <summary>
        /// 键盘按下事件
        /// </summary>
        public virtual void OnKeyDown(OpenTKViewport viewport, KeyEventArgs eventArgs)
        {

        }
        #endregion

        #region 键盘松开事件 —— virtual void OnKeyUp(OpenTKViewport viewport, KeyEventArgs eventArgs)
        /// <summary>
        /// 键盘松开事件
        /// </summary>
        public virtual void OnKeyUp(OpenTKViewport viewport, KeyEventArgs eventArgs)
        {

        }
        #endregion

        #endregion
    }
}
