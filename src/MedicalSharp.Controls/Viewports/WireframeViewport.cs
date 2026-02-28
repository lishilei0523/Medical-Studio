using Avalonia;
using Avalonia.Collections;
using Avalonia.Metadata;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Engine.Cameras;
using MedicalSharp.Engine.Renderers;

namespace MedicalSharp.Controls.Viewports
{
    /// <summary>
    /// 线框渲染视口
    /// </summary>
    public class WireframeViewport : OpenTKViewport
    {

        /// <summary>
        /// 深度依赖属性
        /// </summary>
        public static readonly StyledProperty<OrbitCamera> CameraProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static WireframeViewport()
        {
            CameraProperty = AvaloniaProperty.Register<OpenTKViewport, OrbitCamera>(nameof(Camera));
        }

        /// <summary>
        /// 线框渲染器
        /// </summary>
        private WireframeRenderer _renderer;

        /// <summary>
        /// 默认构造器
        /// </summary>
        public WireframeViewport()
        {
            this.Children = new AvaloniaList<Visual3D>();
        }


        /// <summary>
        /// 子元素列表
        /// </summary>
        [Content]
        public AvaloniaList<Visual3D> Children { get; }

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
