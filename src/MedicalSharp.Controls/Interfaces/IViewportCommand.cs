using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Primitives.Models;
using System.Collections.Generic;

namespace MedicalSharp.Controls.Interfaces
{
    /// <summary>
    /// 视口命令接口
    /// </summary>
    public interface IViewportCommand
    {
        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        void OnMouseDown(OpenTKViewport viewport, PointerPressedEventArgs eventArgs);

        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        void OnMouseMove(OpenTKViewport viewport, PointerEventArgs eventArgs);

        /// <summary>
        /// 鼠标松开事件
        /// </summary>
        void OnMouseUp(OpenTKViewport viewport, PointerReleasedEventArgs eventArgs);

        /// <summary>
        /// 鼠标滚轮事件
        /// </summary>
        void OnMouseWheel(OpenTKViewport viewport, PointerWheelEventArgs eventArgs);

        /// <summary>
        /// 键盘按下事件
        /// </summary>
        void OnKeyDown(OpenTKViewport viewport, KeyEventArgs eventArgs);

        /// <summary>
        /// 键盘松开事件
        /// </summary>
        void OnKeyUp(OpenTKViewport viewport, KeyEventArgs eventArgs);

        /// <summary>
        /// 获取上下文菜单项列表
        /// </summary>
        /// <returns>上下文菜单项列表</returns>
        /// <remarks>右键点击松开时调用，返回null或空列表表示不弹出菜单</remarks>
        IReadOnlyList<ContextMenuItem> GetContextMenu(OpenTKViewport viewport, PointerReleasedEventArgs eventArgs);

        /// <summary>
        /// 失效命令
        /// </summary>
        /// <remarks>命令被停用时调用，切换命令前</remarks>
        void Deactivate();
    }
}
