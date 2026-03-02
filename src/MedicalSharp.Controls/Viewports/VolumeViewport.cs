using Avalonia;
using Avalonia.Collections;
using Avalonia.Metadata;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Inputs;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Engine.Cameras;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Renderers;
using OpenTK.Graphics.OpenGL4;

namespace MedicalSharp.Controls.Viewports
{
    /// <summary>
    /// 体积渲染视口
    /// </summary>
    public class VolumeViewport : OpenTKViewport
    {
        #region # 字段及构造器

        /// <summary>
        /// 体积渲染对象依赖属性
        /// </summary>
        public static readonly StyledProperty<VolumeRenderable> RenderableProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static VolumeViewport()
        {
            RenderableProperty = AvaloniaProperty.Register<VolumeViewport, VolumeRenderable>(nameof(Renderable));
        }


        /// <summary>
        /// 体积渲染器
        /// </summary>
        private VolumeRenderer _volumeRenderer;

        /// <summary>
        /// 线框渲染器
        /// </summary>
        private WireframeRenderer _wireframeRenderer;

        /// <summary>
        /// 默认构造器
        /// </summary>
        public VolumeViewport()
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

        #region 依赖属性 - 体积渲染对象 —— VolumeRenderable Renderable
        /// <summary>
        /// 依赖属性 - 体积渲染对象
        /// </summary>
        public VolumeRenderable Renderable
        {
            get => this.GetValue(RenderableProperty);
            set => this.SetValue(RenderableProperty, value);
        }
        #endregion

        #region 只读属性 - 体积渲染器 —— VolumeRenderer VolumeRenderer
        /// <summary>
        /// 只读属性 - 体积渲染器
        /// </summary>
        public VolumeRenderer VolumeRenderer
        {
            get => this._volumeRenderer;
        }
        #endregion

        #region 只读属性 - 线框渲染器 —— WireframeRenderer WireframeRenderer
        /// <summary>
        /// 只读属性 - 线框渲染器
        /// </summary>
        public WireframeRenderer WireframeRenderer
        {
            get => this._wireframeRenderer;
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

            //初始化体积渲染器
            this._volumeRenderer = new VolumeRenderer(this.Camera);

            //初始化线框渲染器
            this._wireframeRenderer = new WireframeRenderer(this.Camera);
            foreach (BoundingVisual3D visual3D in this.Children)
            {
                this._wireframeRenderer.AppendItem(visual3D.Renderable);
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

            this._volumeRenderer.SetRenderable(this.Renderable);
            this._volumeRenderer.RenderFrame(viewportSize.Width, viewportSize.Height);

            this._wireframeRenderer.RenderFrame(viewportSize.Width, viewportSize.Height);
        }
        #endregion

        #region OpenTK卸载事件 —— override void OnOpenTKDeinit()
        /// <summary>
        /// OpenTK卸载事件
        /// </summary>
        protected override void OnOpenTKDeinit()
        {
            this._volumeRenderer?.Dispose();
            this._wireframeRenderer?.Dispose();
        }
        #endregion 

        #endregion
    }
}
