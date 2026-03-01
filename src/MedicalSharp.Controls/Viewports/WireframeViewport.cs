using Avalonia;
using Avalonia.Collections;
using Avalonia.Metadata;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Engine.Renderers;
using OpenTK.Graphics.OpenGL4;

namespace MedicalSharp.Controls.Viewports
{
    /// <summary>
    /// 线框渲染视口
    /// </summary>
    public class WireframeViewport : OpenTKViewport
    {
        #region # 字段及构造器

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

        #endregion

        #region # 属性

        #region 子元素列表 —— AvaloniaList<Visual3D> Children
        /// <summary>
        /// 子元素列表
        /// </summary>
        [Content]
        public AvaloniaList<Visual3D> Children { get; private set; }
        #endregion

        #region 只读属性 - 线框渲染器 —— WireframeRenderer Renderer
        /// <summary>
        /// 只读属性 - 线框渲染器
        /// </summary>
        public WireframeRenderer Renderer
        {
            get => this._renderer;
        }
        #endregion

        #endregion

        #region # 方法

        #region OpenTK初始化事件 —— override void OnOpenTKInit()
        /// <summary>
        /// OpenTK初始化事件
        /// </summary>
        protected override void OnOpenTKInit()
        {
            this._renderer = new WireframeRenderer(this.Camera);
            foreach (Visual3D visual3D in this.Children)
            {
                if (visual3D is BoundingBoxVisual3D boundingBoxVisual3D)
                {
                    this._renderer.AppendItem(boundingBoxVisual3D.Renderable);
                }
            }
        }
        #endregion

        #region OpenTK渲染事件 —— override void OnOpenTKRender(PixelSize viewportSize)
        /// <summary>
        /// OpenTK渲染事件
        /// </summary>
        /// <param name="viewportSize">视口尺寸</param>
        protected override void OnOpenTKRender(PixelSize viewportSize)
        {
            //开启深度测试
            GL.Enable(EnableCap.DepthTest);

            //禁用面剔除
            GL.Disable(EnableCap.CullFace);

            this._renderer.RenderFrame(viewportSize.Width, viewportSize.Height);
        }
        #endregion

        #region OpenTK卸载事件 —— override void OnOpenTKDeinit()
        /// <summary>
        /// OpenTK卸载事件
        /// </summary>
        protected override void OnOpenTKDeinit()
        {
            this._renderer?.Dispose();
        }
        #endregion

        #endregion
    }
}
