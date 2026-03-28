using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Rendering;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Inputs;
using MedicalSharp.Engine.Cameras;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Reflection;

namespace MedicalSharp.Controls.Base
{
    /// <summary>
    /// OpenTK视口
    /// </summary>
    public abstract class OpenTKViewport : OpenGlControlBase, ICustomHitTest
    {
        #region # 字段及构造器

        /// <summary>
        /// 背景颜色依赖属性
        /// </summary>
        public static readonly StyledProperty<Color> BackgroundProperty;

        /// <summary>
        /// 相机依赖属性
        /// </summary>
        public static readonly StyledProperty<Camera> CameraProperty;

        /// <summary>
        /// 输入管理器依赖属性
        /// </summary>
        public static readonly StyledProperty<InputManager> InputManagerProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static OpenTKViewport()
        {
            BackgroundProperty = AvaloniaProperty.Register<OpenTKViewport, Color>(nameof(Background), Colors.Black);
            CameraProperty = AvaloniaProperty.Register<OpenTKViewport, Camera>(nameof(Camera));
            InputManagerProperty = AvaloniaProperty.Register<OpenTKViewport, InputManager>(nameof(InputManager));
        }


        /// <summary>
        /// OpenGL上下文
        /// </summary>
        private IGlContext _glContext;

        /// <summary>
        /// FBO
        /// </summary>
        private int _frameBufferId;

        /// <summary>
        /// 视口尺寸
        /// </summary>
        protected PixelSize _viewportSize;

        /// <summary>
        /// 默认构造器
        /// </summary>
        protected OpenTKViewport()
        {
            this._frameBufferId = 0;
            this.Focusable = true;
        }

        #endregion

        #region # 属性

        #region 依赖属性 - 背景颜色 —— Color Background
        /// <summary>
        /// 依赖属性 - 背景颜色
        /// </summary>
        public Color Background
        {
            get => this.GetValue(BackgroundProperty);
            set => this.SetValue(BackgroundProperty, value);
        }
        #endregion

        #region 依赖属性 - 相机 —— Camera Camera
        /// <summary>
        /// 依赖属性 - 相机
        /// </summary>
        public Camera Camera
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

        #region 只读属性 - OpenGL上下文 —— IGlContext GlContext
        /// <summary>
        /// 只读属性 - OpenGL上下文
        /// </summary>
        public IGlContext GlContext
        {
            get => this._glContext;
        }
        #endregion

        #region 只读属性 - FBO —— int FrameBufferId
        /// <summary>
        /// 只读属性 - FBO
        /// </summary>
        public int FrameBufferId
        {
            get => this._frameBufferId;
        }
        #endregion

        #endregion

        #region # 方法

        #region 命中测试 —— bool HitTest(Point point)
        /// <summary>
        /// 命中测试
        /// </summary>
        /// <param name="point">点</param>
        /// <returns>是否命中</returns>
        public bool HitTest(Point point)
        {
            return point.X >= 0 && point.X <= this.Bounds.Width &&
                   point.Y >= 0 && point.Y <= this.Bounds.Height;
        }
        #endregion

        #region OpenGL初始化事件 —— sealed override void OnOpenGlInit(GlInterface glInterface)
        /// <summary>
        /// OpenGL初始化事件
        /// </summary>
        /// <param name="glInterface">OpenGL接口</param>
        protected sealed override void OnOpenGlInit(GlInterface glInterface)
        {
            base.OnOpenGlInit(glInterface);

            //获取OpenGL上下文
            Type controlType = typeof(OpenGlControlBase);
            FieldInfo resField = controlType.GetField("_resources", BindingFlags.NonPublic | BindingFlags.Instance);
            object fieldValue = resField!.GetValue(this);
            Type resType = fieldValue!.GetType();
            PropertyInfo contextProperty = resType.GetProperty("Context");
            this._glContext = (IGlContext)contextProperty!.GetValue(fieldValue);

            //加载OpenTK绑定
            AvaloniaBindingsContext bindingsContext = new AvaloniaBindingsContext(glInterface);
            GL.LoadBindings(bindingsContext);

            this.OnOpenTKInit();
        }
        #endregion

        #region OpenGL卸载事件 —— sealed override void OnOpenGlDeinit(GlInterface glInterface)
        /// <summary>
        /// OpenGL卸载事件
        /// </summary>
        protected sealed override void OnOpenGlDeinit(GlInterface glInterface)
        {
            base.OnOpenGlDeinit(glInterface);
            this.OnOpenTKDeinit();
        }
        #endregion

        #region OpenGL渲染事件 —— sealed override void OnOpenGlRender(GlInterface glInterface, int frameBufferId)
        /// <summary>
        /// OpenGL渲染事件
        /// </summary>
        protected sealed override void OnOpenGlRender(GlInterface glInterface, int frameBufferId)
        {
            this._frameBufferId = frameBufferId;

            //设置视口尺寸
            this._viewportSize = new PixelSize((int)this.Bounds.Width, (int)this.Bounds.Height);
            GL.Viewport(0, 0, this._viewportSize.Width, this._viewportSize.Height);

            //设置背景色
            Vector4 background = this.Background.ToVector4();
            GL.ClearColor(background.X, background.Y, background.Z, background.W);

            //清理颜色及深度缓存
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //OpenTK渲染
            this.OnOpenTKRender(this._viewportSize);
        }
        #endregion

        #region OpenTK初始化事件 —— virtual void OnOpenTKInit()
        /// <summary>
        /// OpenTK初始化事件
        /// </summary>
        protected virtual void OnOpenTKInit()
        {

        }
        #endregion

        #region OpenTK卸载事件 —— virtual void OnOpenTKDeinit()
        /// <summary>
        /// OpenTK卸载事件
        /// </summary>
        protected virtual void OnOpenTKDeinit()
        {

        }
        #endregion

        #region OpenTK渲染事件 —— abstract void OnOpenTKRender(PixelSize viewportSize)
        /// <summary>
        /// OpenTK渲染事件
        /// </summary>
        /// <param name="viewportSize">视口尺寸</param>
        protected abstract void OnOpenTKRender(PixelSize viewportSize);
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

        #region 键盘松开事件 —— override void OnKeyUp(KeyEventArgs eventArgs)
        /// <summary>
        /// 键盘松开事件
        /// </summary>
        protected override void OnKeyUp(KeyEventArgs eventArgs)
        {
            base.OnKeyUp(eventArgs);

            this.InputManager.OnKeyUp(this, eventArgs.Key);
        }
        #endregion

        #endregion
    }
}
