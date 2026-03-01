using Avalonia;
using Avalonia.Collections;
using Avalonia.Input;
using Avalonia.Metadata;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Inputs;
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
        #region # 字段及构造器

        /// <summary>
        /// 轨道相机依赖属性
        /// </summary>
        public static readonly StyledProperty<OrbitCamera> CameraProperty;

        /// <summary>
        /// 输入管理器依赖属性
        /// </summary>
        public static readonly StyledProperty<InputManager> InputManagerProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static WireframeViewport()
        {
            CameraProperty = AvaloniaProperty.Register<OpenTKViewport, OrbitCamera>(nameof(Camera));
            InputManagerProperty = AvaloniaProperty.Register<OpenTKViewport, InputManager>(nameof(InputManager));
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

        #endregion

        #region # 属性

        #region 子元素列表 —— AvaloniaList<Visual3D> Children
        /// <summary>
        /// 子元素列表
        /// </summary>
        [Content]
        public AvaloniaList<Visual3D> Children { get; private set; }
        #endregion

        #region 依赖属性 - 轨道相机 —— OrbitCamera Camera
        /// <summary>
        /// 依赖属性 - 轨道相机
        /// </summary>
        public OrbitCamera Camera
        {
            get => this.GetValue(CameraProperty);
            set => this.SetValue(CameraProperty, value);
        }
        #endregion

        #region 依赖属性 - 输入管理器 —— InputManager InputManager
        /// <summary>
        /// 依赖属性 - 输入管理器
        /// </summary>
        public InputManager InputManager
        {
            get => this.GetValue(InputManagerProperty);
            set => this.SetValue(InputManagerProperty, value);
        }
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

            MouseButton mouseButton = MouseButton.Left;
            if (eventArgs.Properties.IsLeftButtonPressed)
            {
                mouseButton = MouseButton.Left;
            }
            if (eventArgs.Properties.IsMiddleButtonPressed)
            {
                mouseButton = MouseButton.Middle;
            }
            if (eventArgs.Properties.IsRightButtonPressed)
            {
                mouseButton = MouseButton.Right;
            }

            this.InputManager.OnMouseDown(this, mouseButton, eventArgs.GetPosition(this));
        }
        #endregion

        #region 指针松开事件 —— override void OnPointerReleased(PointerReleasedEventArgs eventArgs)
        /// <summary>
        /// 指针松开事件
        /// </summary>
        protected override void OnPointerReleased(PointerReleasedEventArgs eventArgs)
        {
            base.OnPointerReleased(eventArgs);

            MouseButton mouseButton = MouseButton.Left;
            if (eventArgs.Properties.IsLeftButtonPressed)
            {
                mouseButton = MouseButton.Left;
            }
            if (eventArgs.Properties.IsMiddleButtonPressed)
            {
                mouseButton = MouseButton.Middle;
            }
            if (eventArgs.Properties.IsRightButtonPressed)
            {
                mouseButton = MouseButton.Right;
            }

            this.InputManager.OnMouseUp(this, mouseButton, eventArgs.GetPosition(this));
        }
        #endregion

        #region 指针移动事件 —— override void OnPointerReleased(PointerEventArgs eventArgs)
        /// <summary>
        /// 指针移动事件
        /// </summary>
        protected override void OnPointerMoved(PointerEventArgs eventArgs)
        {
            base.OnPointerMoved(eventArgs);

            MouseButton mouseButton = MouseButton.Left;
            if (eventArgs.Properties.IsLeftButtonPressed)
            {
                mouseButton = MouseButton.Left;
            }
            if (eventArgs.Properties.IsMiddleButtonPressed)
            {
                mouseButton = MouseButton.Middle;
            }
            if (eventArgs.Properties.IsRightButtonPressed)
            {
                mouseButton = MouseButton.Right;
            }

            this.InputManager.OnMouseMove(this, mouseButton, eventArgs.GetPosition(this));
        }
        #endregion

        #region 指针滚轮事件 —— override void OnPointerReleased(PointerWheelEventArgs eventArgs)
        /// <summary>
        /// 指针滚轮事件
        /// </summary>
        protected override void OnPointerWheelChanged(PointerWheelEventArgs eventArgs)
        {
            base.OnPointerWheelChanged(eventArgs);

            this.InputManager.OnMouseWheel(this, eventArgs.Delta.X, eventArgs.Delta.Y);
        }
        #endregion

        #region 键盘按下事件 —— override void OnKeyDown(KeyEventArgs eventArgs)
        /// <summary>
        /// 键盘按下事件
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs eventArgs)
        {
            base.OnKeyDown(eventArgs);

            this.InputManager.OnKeyDown(this, eventArgs.Key);
        }
        #endregion

        #endregion
    }
}
