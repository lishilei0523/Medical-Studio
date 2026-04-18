using Avalonia.Input;
using MedicalSharp.Controls.Base;

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
    }
}
