using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System;

namespace MedicalSharp.Controls.Extensions
{
    /// <summary>
    /// 缩放扩展
    /// </summary>
    public static class ScaleExtension
    {
        #region # 获取渲染缩放率 —— static double GetRenderScaling(this Visual visual)
        /// <summary>
        /// 获取渲染缩放率
        /// </summary>
        /// <param name="visual">视觉元素</param>
        /// <returns>渲染缩放率</returns>
        public static double GetRenderScaling(this Visual visual)
        {
            TopLevel topLevel = TopLevel.GetTopLevel(visual);

            #region # 验证

            if (topLevel == null)
            {
                throw new InvalidOperationException("无法获取 TopLevel，请确保 relativeTo 已挂载到视觉树上。");
            }

            #endregion

            return topLevel.RenderScaling;
        }
        #endregion

        #region # 获取鼠标像素位置 —— static Point GetPixelPosition(this PointerEventArgs eventArgs...
        /// <summary>
        /// 获取鼠标像素位置
        /// </summary>
        /// <param name="eventArgs">鼠标事件参数</param>
        /// <param name="relativeTo">相对元素</param>
        /// <returns>像素位置</returns>
        public static Point GetPixelPosition(this PointerEventArgs eventArgs, Visual relativeTo)
        {
            double scaling = relativeTo.GetRenderScaling();
            Point position = eventArgs.GetPosition(relativeTo);
            int pixelX = (int)Math.Round(position.X * scaling);
            int pixelY = (int)Math.Round(position.Y * scaling);
            Point pixelPosition = new Point(pixelX, pixelY);

            return pixelPosition;
        }
        #endregion
    }
}
