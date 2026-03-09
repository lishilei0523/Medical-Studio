using Avalonia;
using MedicalSharp.Controls.Base;
using OpenTK.Graphics.OpenGL4;

namespace MedicalSharp.Controls.Viewports
{
    /// <summary>
    /// MPR渲染视口
    /// </summary>
    public class MPRViewport : OpenTKViewport
    {
        //TODO 实现

        #region OpenTK渲染事件 —— override void OnOpenTKRender(PixelSize viewportSize)
        /// <summary>
        /// OpenTK渲染事件
        /// </summary>
        /// <param name="viewportSize">视口尺寸</param>
        protected override void OnOpenTKRender(PixelSize viewportSize)
        {
            //开启深度测试
            GL.Enable(EnableCap.DepthTest);

            //关闭混合
            GL.Disable(EnableCap.Blend);

        }
        #endregion 
    }
}
