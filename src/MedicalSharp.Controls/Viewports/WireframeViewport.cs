using Avalonia;
using MedicalSharp.Controls.Base;

namespace MedicalSharp.Controls.Viewports
{
    /// <summary>
    /// 线框渲染视口
    /// </summary>
    public class WireframeViewport : OpenTKViewport
    {
        //TODO 实现

        #region OpenTK渲染事件 —— override void OnOpenTKRender(PixelSize viewportSize)
        /// <summary>
        /// OpenTK渲染事件
        /// </summary>
        /// <param name="viewportSize">视口尺寸</param>
        protected override void OnOpenTKRender(PixelSize viewportSize)
        {

        }
        #endregion 
    }
}
