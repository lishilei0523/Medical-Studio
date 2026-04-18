using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Interfaces;
using System;

namespace MedicalSharp.Controls.InputManagers
{
    /// <summary>
    /// 命令输入管理器
    /// </summary>
    public class CommandInputManager : InputManager
    {
        #region # 字段及构造器

        /// <summary>
        /// 视口命令
        /// </summary>
        private IViewportCommand _command;

        /// <summary>
        /// 创建命令输入管理器构造器
        /// </summary>
        /// <param name="command">视口命令</param>
        public CommandInputManager(IViewportCommand command)
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

        #region 切换命令 —— void SwitchCommand(IViewportCommand command)
        /// <summary>
        /// 切换命令
        /// </summary>
        /// <param name="command">视口命令</param>
        public void SwitchCommand(IViewportCommand command)
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

        #region 鼠标按下事件 —— override void OnMouseDown(OpenTKViewport viewport, PointerPressedEventArgs eventArgs)
        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        public override void OnMouseDown(OpenTKViewport viewport, PointerPressedEventArgs eventArgs)
        {
            base.OnMouseDown(viewport, eventArgs);
            this._command.OnMouseDown(viewport, eventArgs);
        }
        #endregion

        #region 鼠标松开事件 —— override void OnMouseUp(OpenTKViewport viewport, PointerReleasedEventArgs eventArgs)
        /// <summary>
        /// 鼠标松开事件
        /// </summary>
        public override void OnMouseUp(OpenTKViewport viewport, PointerReleasedEventArgs eventArgs)
        {
            base.OnMouseUp(viewport, eventArgs);
            this._command.OnMouseUp(viewport, eventArgs);
        }
        #endregion

        #region 鼠标移动事件 —— override void OnMouseMove(OpenTKViewport viewport, PointerEventArgs eventArgs)
        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        public override void OnMouseMove(OpenTKViewport viewport, PointerEventArgs eventArgs)
        {
            base.OnMouseMove(viewport, eventArgs);
            this._command.OnMouseMove(viewport, eventArgs);
        }
        #endregion

        #region 鼠标滚轮事件 —— override void OnMouseWheel(OpenTKViewport viewport, PointerWheelEventArgs eventArgs)
        /// <summary>
        /// 鼠标滚轮事件
        /// </summary>
        public override void OnMouseWheel(OpenTKViewport viewport, PointerWheelEventArgs eventArgs)
        {
            base.OnMouseWheel(viewport, eventArgs);
            this._command.OnMouseWheel(viewport, eventArgs);
        }
        #endregion

        #region 键盘按下事件 —— override void OnKeyDown(OpenTKViewport viewport, KeyEventArgs eventArgs)
        /// <summary>
        /// 键盘按下事件
        /// </summary>
        public override void OnKeyDown(OpenTKViewport viewport, KeyEventArgs eventArgs)
        {
            base.OnKeyDown(viewport, eventArgs);
            this._command.OnKeyDown(viewport, eventArgs);
        }
        #endregion

        #region 键盘松开事件 —— override void OnKeyUp(OpenTKViewport viewport, KeyEventArgs eventArgs)
        /// <summary>
        /// 键盘松开事件
        /// </summary>
        public override void OnKeyUp(OpenTKViewport viewport, KeyEventArgs eventArgs)
        {
            base.OnKeyUp(viewport, eventArgs);
            this._command.OnKeyUp(viewport, eventArgs);
        }
        #endregion

        #endregion
    }
}
