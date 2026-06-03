using Avalonia.Input;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace MedicalSharp.Controls.Base
{
    /// <summary>
    /// 视口命令
    /// </summary>
    public abstract class ViewportCommand : IViewportCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// 鼠标2D位置
        /// </summary>
        protected Vector2? _mousePos2D;

        /// <summary>
        /// 创建视口命令构造器
        /// </summary>
        protected ViewportCommand()
        {
            this._mousePos2D = null;
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

        #endregion

        #region # 方法

        #region 鼠标按下事件 —— virtual void OnMouseDown(OpenTKViewport viewport, PointerPressedEventArgs eventArgs)
        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        public virtual void OnMouseDown(OpenTKViewport viewport, PointerPressedEventArgs eventArgs)
        {
            this._mousePos2D = eventArgs.GetPosition(viewport).ToVector2();
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

        #region 鼠标松开事件 —— virtual void OnMouseUp(OpenTKViewport viewport, PointerReleasedEventArgs eventArgs)
        /// <summary>
        /// 鼠标松开事件
        /// </summary>
        public virtual void OnMouseUp(OpenTKViewport viewport, PointerReleasedEventArgs eventArgs)
        {
            this._mousePos2D = null;
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

        #region 获取上下文菜单项列表 —— virtual IReadOnlyList<ContextMenuItem> GetContextMenuItems(...
        /// <summary>
        /// 获取上下文菜单项列表
        /// </summary>
        /// <returns>上下文菜单项列表</returns>
        /// <remarks>右键点击松开时调用，返回null或空列表表示不弹出菜单</remarks>
        public virtual IReadOnlyList<ContextMenuItem> GetContextMenuItems(OpenTKViewport viewport, PointerReleasedEventArgs eventArgs)
        {
            return null;
        }
        #endregion

        #region 失效命令 —— virtual void Deactivate()
        /// <summary>
        /// 失效命令
        /// </summary>
        /// <remarks>命令被停用时调用，切换命令前</remarks>
        public virtual void Deactivate()
        {

        }
        #endregion

        #endregion
    }
}
