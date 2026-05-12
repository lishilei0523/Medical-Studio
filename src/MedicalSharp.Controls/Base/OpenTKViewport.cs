using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Rendering;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Engine.Managers;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Cameras;
using MedicalSharp.Primitives.Maths;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SkiaSharp;
using System;
using System.Reflection;
using IInputManager = MedicalSharp.Controls.Interfaces.IInputManager;

namespace MedicalSharp.Controls.Base
{
    /// <summary>
    /// OpenTK视口
    /// </summary>
    public abstract class OpenTKViewport : OpenGlControlBase, ICustomHitTest
    {
        #region # 字段及构造器

        /// <summary>
        /// GPU厂商缓存
        /// </summary>
        /// <remarks>-1: 未知, 0: 非NVIDIA, 1: NVIDIA</remarks>
        private static int _GpuVendorCache = -1;

        /// <summary>
        /// 帧令牌依赖属性
        /// </summary>
        public static readonly StyledProperty<int> FrameTokenProperty;

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
        public static readonly StyledProperty<IInputManager> InputManagerProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static OpenTKViewport()
        {
            FrameTokenProperty = AvaloniaProperty.Register<OpenTKViewport, int>(nameof(FrameToken));
            BackgroundProperty = AvaloniaProperty.Register<OpenTKViewport, Color>(nameof(Background), Colors.Black);
            CameraProperty = AvaloniaProperty.Register<OpenTKViewport, Camera>(nameof(Camera));
            InputManagerProperty = AvaloniaProperty.Register<OpenTKViewport, IInputManager>(nameof(InputManager));

            //属性改变事件
            FrameTokenProperty.Changed.AddClassHandler<OpenTKViewport, int>(OnFrameTokenChanged);
        }


        /// <summary>
        /// FBO
        /// </summary>
        private int _frameBufferId;

        /// <summary>
        /// OpenGL上下文
        /// </summary>
        private IGlContext _glContext;

        /// <summary>
        /// OpenGL是否已初始化
        /// </summary>
        protected bool _glInitialized;

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
            this._glInitialized = false;
            this.Focusable = true;
        }

        #endregion

        #region # 属性

        #region 依赖属性 - 帧令牌 —— int FrameToken
        /// <summary>
        /// 依赖属性 - 帧令牌
        /// </summary>
        public int FrameToken
        {
            get => this.GetValue(FrameTokenProperty);
            set => this.SetValue(FrameTokenProperty, value);
        }
        #endregion

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

        #region 依赖属性 - 输入管理器 —— IInputManager InputManager
        /// <summary>
        /// 依赖属性 - 输入管理器
        /// </summary>
        public IInputManager InputManager
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

        #region 只读属性 - 视口尺寸 —— PixelSize ViewportSize
        /// <summary>
        /// 只读属性 - 视口尺寸
        /// </summary>
        public PixelSize ViewportSize
        {
            get => this._viewportSize;
        }
        #endregion

        #region 只读属性 - 是否NVIDIA卡 —— static bool IsNvidia
        /// <summary>
        /// 只读属性 - 是否NVIDIA卡
        /// </summary>
        private static bool IsNvidia
        {
            get
            {
                if (_GpuVendorCache == -1)
                {
                    string vendor = GL.GetString(StringName.Vendor);
                    _GpuVendorCache = (vendor != null && vendor.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase))
                        ? 1
                        : 0;
                }
                return _GpuVendorCache == 1;
            }
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

        #region 截屏 —— SKBitmap Capture()
        /// <summary>
        /// 截屏
        /// </summary>
        /// <returns>截屏图像</returns>
        public SKBitmap Capture()
        {
            int width = this._viewportSize.Width;
            int height = this._viewportSize.Height;
            using ReadPixelBuffer2D pixelBuffer = new ReadPixelBuffer2D(width, height);
            pixelBuffer.ReadFrameBuffer(null, this.FrameBufferId);
            byte[] buffer = pixelBuffer.GetCpuBuffer();

            SKBitmap bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
            IntPtr pixels = bitmap.GetPixels();
            unsafe
            {
                byte* targetPtr = (byte*)pixels.ToPointer();
                fixed (byte* sourcePtr = buffer)
                {
                    int stride = width * 4;
                    for (int y = 0; y < height; y++)
                    {
                        int srcY = height - 1 - y;  //翻转Y轴
                        byte* srcRow = sourcePtr + srcY * stride;
                        byte* dstRow = targetPtr + y * stride;

                        //复制整行（RGB -> RGBA，顺序相同）
                        System.Buffer.MemoryCopy(srcRow, dstRow, stride, stride);
                    }
                }
            }

            return bitmap;
        }
        #endregion

        #region 投影 —— Vector2 Project(Vector3 worldPos3D)
        /// <summary>
        /// 投影
        /// </summary>
        /// <param name="worldPos3D">世界3D位置</param>
        /// <returns>屏幕2D位置</returns>
        public Vector2 Project(Vector3 worldPos3D)
        {
            #region # 验证

            if (this._viewportSize.Width == 0 || this._viewportSize.Height == 0)
            {
                return Vector2.Zero;
            }

            #endregion

            Vector2 screenPos2D = Ray.Project(worldPos3D, this._viewportSize.ToVector2(), this.Camera.ProjectionMatrix, this.Camera.ViewMatrix);

            return screenPos2D;
        }
        #endregion

        #region 反投影 —— virtual Ray UnProject(Vector2 screenPos2D)
        /// <summary>
        /// 反投影
        /// </summary>
        /// <param name="screenPos2D">屏幕2D位置</param>
        /// <returns>射线</returns>
        public virtual Ray UnProject(Vector2 screenPos2D)
        {
            Ray ray = Ray.UnProject(screenPos2D, this.Camera.CameraPosition, this._viewportSize.ToVector2(), this.Camera.ProjectionMatrix, this.Camera.ViewMatrix);

            return ray;
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

            this._glInitialized = true;

            //初始化着色器
            ShaderManager.Initialize();
            ComputerManager.Initialize();

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

            //开启深度测试
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            //OpenTK渲染
            this.OnOpenTKRender(this._viewportSize);

            //N卡兼容性处理
            if (IsNvidia)
            {
                GL.Finish();
            }
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
            this.InputManager.OnMouseDown(this, eventArgs);
        }
        #endregion

        #region 指针松开事件 —— override void OnPointerReleased(PointerReleasedEventArgs eventArgs)
        /// <summary>
        /// 指针松开事件
        /// </summary>
        protected override void OnPointerReleased(PointerReleasedEventArgs eventArgs)
        {
            base.OnPointerReleased(eventArgs);
            this.InputManager.OnMouseUp(this, eventArgs);
        }
        #endregion

        #region 指针移动事件 —— override void OnPointerReleased(PointerEventArgs eventArgs)
        /// <summary>
        /// 指针移动事件
        /// </summary>
        protected override void OnPointerMoved(PointerEventArgs eventArgs)
        {
            base.OnPointerMoved(eventArgs);
            this.InputManager.OnMouseMove(this, eventArgs);
        }
        #endregion

        #region 指针滚轮事件 —— override void OnPointerReleased(PointerWheelEventArgs eventArgs)
        /// <summary>
        /// 指针滚轮事件
        /// </summary>
        protected override void OnPointerWheelChanged(PointerWheelEventArgs eventArgs)
        {
            base.OnPointerWheelChanged(eventArgs);
            this.InputManager.OnMouseWheel(this, eventArgs);
        }
        #endregion

        #region 键盘按下事件 —— override void OnKeyDown(KeyEventArgs eventArgs)
        /// <summary>
        /// 键盘按下事件
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs eventArgs)
        {
            base.OnKeyDown(eventArgs);

            this.InputManager.OnKeyDown(this, eventArgs);
        }
        #endregion

        #region 键盘松开事件 —— override void OnKeyUp(KeyEventArgs eventArgs)
        /// <summary>
        /// 键盘松开事件
        /// </summary>
        protected override void OnKeyUp(KeyEventArgs eventArgs)
        {
            base.OnKeyUp(eventArgs);

            this.InputManager.OnKeyUp(this, eventArgs);
        }
        #endregion

        #region 帧令牌改变事件 —— static void OnFrameTokenChanged(OpenTKViewport viewport...
        /// <summary>
        /// 帧令牌改变事件
        /// </summary>
        private static void OnFrameTokenChanged(OpenTKViewport viewport, AvaloniaPropertyChangedEventArgs<int> eventArgs)
        {
            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #endregion
    }
}
