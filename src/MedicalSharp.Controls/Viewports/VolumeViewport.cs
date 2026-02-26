using Avalonia;
using MedicalSharp.Controls.Base;

namespace MedicalSharp.Controls.Viewports
{
    /// <summary>
    /// 体积渲染视口
    /// </summary>
    public class VolumeViewport : OpenTKViewport
    {

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
