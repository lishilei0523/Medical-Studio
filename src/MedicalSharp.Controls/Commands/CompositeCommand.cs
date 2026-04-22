using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Primitives.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalSharp.Controls.Commands
{
    /// <summary>
    /// 复合命令
    /// </summary>
    public class CompositeCommand : ViewportCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// 命令列表
        /// </summary>
        private readonly HashSet<IViewportCommand> _commands;

        /// <summary>
        /// 创建复合命令构造器
        /// </summary>
        public CompositeCommand()
        {
            this._commands = new HashSet<IViewportCommand>();
        }

        /// <summary>
        /// 创建复合命令构造器
        /// </summary>
        /// <param name="commands">命令列表</param>
        public CompositeCommand(params IViewportCommand[] commands)
            : this()
        {
            foreach (IViewportCommand command in commands)
            {
                this._commands.Add(command);
            }
        }

        #endregion

        #region # 属性

        #region 只读属性 - 命令列表 —— IReadOnlySet<IViewportCommand> Commands
        /// <summary>
        /// 只读属性 - 命令列表
        /// </summary>
        public IReadOnlySet<IViewportCommand> Commands
        {
            get => this._commands;
        }
        #endregion

        #endregion

        #region # 方法

        #region 添加命令 —— void AddCommand(IViewportCommand command)
        /// <summary>
        /// 添加命令
        /// </summary>
        /// <param name="command">视口命令</param>
        public void AddCommand(IViewportCommand command)
        {
            #region # 验证

            if (command == null)
            {
                throw new ArgumentNullException(nameof(command), "命令不可为null！");
            }

            #endregion

            this._commands.Add(command);
        }
        #endregion

        #region 删除命令 —— void RemoveCommand(IViewportCommand command)
        /// <summary>
        /// 删除命令
        /// </summary>
        /// <param name="command">视口命令</param>
        public void RemoveCommand(IViewportCommand command)
        {
            #region # 验证

            if (command == null)
            {
                throw new ArgumentNullException(nameof(command), "命令不可为null！");
            }

            #endregion

            this._commands.Remove(command);
        }
        #endregion

        #region 清空命令 —— void Clear()
        /// <summary>
        /// 清空命令
        /// </summary>
        public void Clear()
        {
            this._commands.Clear();
        }
        #endregion

        #region 鼠标按下事件 —— override void OnMouseDown(OpenTKViewport viewport...
        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        public override void OnMouseDown(OpenTKViewport viewport, PointerPressedEventArgs eventArgs)
        {
            base.OnMouseDown(viewport, eventArgs);
            foreach (IViewportCommand command in this._commands)
            {
                command.OnMouseDown(viewport, eventArgs);
            }
        }
        #endregion

        #region 鼠标松开事件 —— override void OnMouseUp(OpenTKViewport viewport
        /// <summary>
        /// 鼠标松开事件
        /// </summary>
        public override void OnMouseUp(OpenTKViewport viewport, PointerReleasedEventArgs eventArgs)
        {
            base.OnMouseUp(viewport, eventArgs);
            foreach (IViewportCommand command in this._commands)
            {
                command.OnMouseUp(viewport, eventArgs);
            }
        }
        #endregion

        #region 鼠标移动事件 —— override void OnMouseMove(OpenTKViewport viewport...
        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        public override void OnMouseMove(OpenTKViewport viewport, PointerEventArgs eventArgs)
        {
            base.OnMouseMove(viewport, eventArgs);
            foreach (IViewportCommand command in this._commands)
            {
                command.OnMouseMove(viewport, eventArgs);
            }
        }
        #endregion

        #region 鼠标滚轮事件 —— override void OnMouseWheel(OpenTKViewport viewport...
        /// <summary>
        /// 鼠标滚轮事件
        /// </summary>
        public override void OnMouseWheel(OpenTKViewport viewport, PointerWheelEventArgs eventArgs)
        {
            base.OnMouseWheel(viewport, eventArgs);
            foreach (IViewportCommand command in this._commands)
            {
                command.OnMouseWheel(viewport, eventArgs);
            }
        }
        #endregion

        #region 键盘按下事件 —— override void OnKeyDown(OpenTKViewport viewport...
        /// <summary>
        /// 键盘按下事件
        /// </summary>
        public override void OnKeyDown(OpenTKViewport viewport, KeyEventArgs eventArgs)
        {
            base.OnKeyDown(viewport, eventArgs);
            foreach (IViewportCommand command in this._commands)
            {
                command.OnKeyDown(viewport, eventArgs);
            }
        }
        #endregion

        #region 键盘松开事件 —— override void OnKeyUp(OpenTKViewport viewport...
        /// <summary>
        /// 键盘松开事件
        /// </summary>
        public override void OnKeyUp(OpenTKViewport viewport, KeyEventArgs eventArgs)
        {
            base.OnKeyUp(viewport, eventArgs);
            foreach (IViewportCommand command in this._commands)
            {
                command.OnKeyUp(viewport, eventArgs);
            }
        }
        #endregion

        #region 获取上下文菜单项列表 —— virtual IReadOnlyList<ContextMenuItem> GetContextMenuItems(...
        /// <summary>
        /// 获取上下文菜单项列表
        /// </summary>
        /// <returns>上下文菜单项列表</returns>
        /// <remarks>右键点击松开时调用，返回null或空列表表示不弹出菜单</remarks>
        public override IReadOnlyList<ContextMenuItem> GetContextMenuItems(OpenTKViewport viewport, PointerReleasedEventArgs eventArgs)
        {
            List<ContextMenuItem> contextMenuItems = [];
            bool hasPrevious = false;
            foreach (IViewportCommand command in this.Commands)
            {
                IReadOnlyList<ContextMenuItem> subContextMenuItems = command.GetContextMenuItems(viewport, eventArgs);
                if (subContextMenuItems != null && subContextMenuItems.Any())
                {
                    if (hasPrevious)
                    {
                        contextMenuItems.Add(ContextMenuItem.CreateSeparator());
                    }
                    contextMenuItems.AddRange(subContextMenuItems);
                    hasPrevious = true;
                }
            }

            return contextMenuItems.AsReadOnly();
        }
        #endregion

        #region 失效命令 —— virtual void Deactivate()
        /// <summary>
        /// 失效命令
        /// </summary>
        /// <remarks>命令被停用时调用，切换命令前</remarks>
        public override void Deactivate()
        {
            foreach (IViewportCommand command in this.Commands)
            {
                command.Deactivate();
            }
        }
        #endregion

        #endregion
    }
}
