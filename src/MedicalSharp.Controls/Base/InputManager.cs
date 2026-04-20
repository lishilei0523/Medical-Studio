using Avalonia.Input;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using OpenTK.Mathematics;
using System;
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
        /// 鼠标2D位置
        /// </summary>
        protected Vector2? _mousePos2D;

        /// <summary>
        /// 视口命令
        /// </summary>
        protected IViewportCommand _command;

        /// <summary>
        /// 创建输入管理器构造器
        /// </summary>
        protected InputManager()
        {
            this._mousePos2D = null;
        }

        /// <summary>
        /// 创建输入管理器构造器
        /// </summary>
        /// <param name="command">视口命令</param>
        protected InputManager(IViewportCommand command)
            : this()
        {
            #region # 验证

            if (command == null)
            {
                throw new ArgumentNullException(nameof(command), "视口命令不可为空！");
            }

            #endregion

            this._command = command;
        }

        #endregion

        #region # 属性

        #region 只读属性 - 鼠标2D位置 —— Vector2? MousePos2D
        /// <summary>
        /// 只读属性 - 鼠标2D位置
        /// </summary>
        public Vector2? MousePos2D
        {
            get => this._mousePos2D;
        }
        #endregion

        #region 只读属性 - 视口命令 —— IViewportCommand Command
        /// <summary>
        /// 视口命令
        /// </summary>
        public IViewportCommand Command
        {
            get => this._command;
        }
        #endregion

        #endregion

        #region # 方法

        #region 切换命令 —— virtual void SwitchCommand(IViewportCommand command)
        /// <summary>
        /// 切换命令
        /// </summary>
        /// <param name="command">视口命令</param>
        public virtual void SwitchCommand(IViewportCommand command)
        {
            this._command = command;
        }
        #endregion

        #region 鼠标按下事件 —— virtual void OnMouseDown(OpenTKViewport viewport, PointerPressedEventArgs eventArgs)
        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        public virtual void OnMouseDown(OpenTKViewport viewport, PointerPressedEventArgs eventArgs)
        {
            this._mousePos2D = eventArgs.GetPosition(viewport).ToVector2();
            this._command?.OnMouseDown(viewport, eventArgs);
        }
        #endregion

        #region 鼠标松开事件 —— virtual void OnMouseUp(OpenTKViewport viewport, PointerReleasedEventArgs eventArgs)
        /// <summary>
        /// 鼠标松开事件
        /// </summary>
        public virtual void OnMouseUp(OpenTKViewport viewport, PointerReleasedEventArgs eventArgs)
        {
            this._mousePos2D = null;
            this._command?.OnMouseUp(viewport, eventArgs);
        }
        #endregion

        #region 鼠标移动事件 —— virtual void OnMouseMove(OpenTKViewport viewport, PointerEventArgs eventArgs)
        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        public virtual void OnMouseMove(OpenTKViewport viewport, PointerEventArgs eventArgs)
        {
            this._command?.OnMouseMove(viewport, eventArgs);
        }
        #endregion

        #region 鼠标滚轮事件 —— virtual void OnMouseWheel(OpenTKViewport viewport, PointerWheelEventArgs eventArgs)
        /// <summary>
        /// 鼠标滚轮事件
        /// </summary>
        public virtual void OnMouseWheel(OpenTKViewport viewport, PointerWheelEventArgs eventArgs)
        {
            this._command?.OnMouseWheel(viewport, eventArgs);
        }
        #endregion

        #region 键盘按下事件 —— virtual void OnKeyDown(OpenTKViewport viewport, KeyEventArgs eventArgs)
        /// <summary>
        /// 键盘按下事件
        /// </summary>
        public virtual void OnKeyDown(OpenTKViewport viewport, KeyEventArgs eventArgs)
        {
            this._command?.OnKeyDown(viewport, eventArgs);
        }
        #endregion

        #region 键盘松开事件 —— virtual void OnKeyUp(OpenTKViewport viewport, KeyEventArgs eventArgs)
        /// <summary>
        /// 键盘松开事件
        /// </summary>
        public virtual void OnKeyUp(OpenTKViewport viewport, KeyEventArgs eventArgs)
        {
            this._command?.OnKeyUp(viewport, eventArgs);
        }
        #endregion

        #endregion
    }
}
