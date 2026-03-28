using Avalonia;
using Avalonia.Collections;
using Avalonia.Input;
using Avalonia.Metadata;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Inputs;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Engine.Cameras;
using MedicalSharp.Engine.Renderers;
using MedicalSharp.Engine.ValueTypes;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Diagnostics;

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
            this.Children = new AvaloniaList<BoundingVisual3D>();
        }

        #endregion

        #region # 属性

        #region 子元素列表 —— AvaloniaList<BoundingVisual3D> Children
        /// <summary>
        /// 子元素列表
        /// </summary>
        [Content]
        public AvaloniaList<BoundingVisual3D> Children { get; private set; }
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
            //InputManger默认值
            if (this.InputManager == null && this.Camera is OrbitCamera orbitCamera)
            {
                this.InputManager = new OrbitInputManager(orbitCamera);
            }

            this._renderer = new WireframeRenderer(this.Camera);
            foreach (BoundingVisual3D visual3D in this.Children)
            {
                this._renderer.AppendItem(visual3D.Renderable);
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

        #region 指针按下事件 —— override void OnPointerPressed(PointerPressedEventArgs eventArgs)
        /// <summary>
        /// 指针按下事件
        /// </summary>
        protected override void OnPointerPressed(PointerPressedEventArgs eventArgs)
        {
            base.OnPointerPressed(eventArgs);

            if (eventArgs.Properties.IsLeftButtonPressed)
            {
                Point mousePosition = eventArgs.GetPosition(this);
                Ray ray = Ray.UnProject(mousePosition.ToVector2(), this.Camera.CameraPosition, this._viewportSize.ToVector2(), this.Camera.ProjectionMatrix, this.Camera.ViewMatrix);
                foreach (BoundingVisual3D boundingVisual3D in this.Children)
                {
                    bool intersects = boundingVisual3D.Renderable.IntersectsRay(ray, out float distance);
                    if (intersects)
                    {
                        boundingVisual3D.Renderable.IntersectsRay(ray, out distance, out Vector3 hitPoint, out Vector3 hitNormal, out int hitTriangleIndex);
                        Trace.WriteLine(hitPoint);
                    }
                }
            }
        }
        #endregion

        #endregion
    }
}
