using Avalonia;
using Avalonia.Input;
using MedicalSharp.Controls.Base;

namespace MedicalSharp.Controls.Inputs
{
    /// <summary>
    /// 输入管理器
    /// </summary>
    public abstract class InputManager
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

        #region 鼠标按下事件 —— virtual void OnMouseDown(OpenTKViewport viewport, MouseButton button...
        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        /// <param name="viewport">OpenTK视口</param>
        /// <param name="button">鼠标按键</param>
        /// <param name="position">鼠标位置</param>
        public virtual void OnMouseDown(OpenTKViewport viewport, MouseButton button, Point position)
        {
            this._mousePosition2D = position;
        }
        #endregion

        #region 鼠标松开事件 —— virtual void OnMouseUp(OpenTKViewport viewport, MouseButton button...
        /// <summary>
        /// 鼠标松开事件
        /// </summary>
        /// <param name="viewport">OpenTK视口</param>
        /// <param name="button">鼠标按键</param>
        /// <param name="position">鼠标位置</param>
        public virtual void OnMouseUp(OpenTKViewport viewport, MouseButton button, Point position)
        {
            this._mousePosition2D = null;
        }
        #endregion

        #region 鼠标移动事件 —— virtual void OnMouseMove(OpenTKViewport viewport, MouseButton button...
        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        /// <param name="viewport">OpenTK视口</param>
        /// <param name="button">鼠标按键</param>
        /// <param name="position">鼠标位置</param>
        public virtual void OnMouseMove(OpenTKViewport viewport, MouseButton button, Point position)
        {

        }
        #endregion

        #region 鼠标滚轮事件 —— virtual void OnMouseWheel(OpenTKViewport viewport, double offsetX...
        /// <summary>
        /// 鼠标滚轮事件
        /// </summary>
        /// <param name="viewport">OpenTK视口</param>
        /// <param name="offsetX">X轴偏移量</param>
        /// <param name="offsetY">Y轴偏移量</param>
        public virtual void OnMouseWheel(OpenTKViewport viewport, double offsetX, double offsetY)
        {

        }
        #endregion

        #region 键盘按下事件 —— virtual void OnKeyDown(OpenTKViewport viewport, Key key)
        /// <summary>
        /// 键盘按下事件
        /// </summary>
        /// <param name="viewport">OpenTK视口</param>
        /// <param name="key">键</param>
        public virtual void OnKeyDown(OpenTKViewport viewport, Key key)
        {

        }
        #endregion

        #region 键盘松开事件 —— virtual void OnKeyUp(OpenTKViewport viewport, Key key)
        /// <summary>
        /// 键盘松开事件
        /// </summary>
        /// <param name="viewport">OpenTK视口</param>
        /// <param name="key">键</param>
        public virtual void OnKeyUp(OpenTKViewport viewport, Key key)
        {

        }
        #endregion

        #endregion
    }
}
