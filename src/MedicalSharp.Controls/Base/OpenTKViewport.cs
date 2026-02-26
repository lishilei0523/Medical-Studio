using Avalonia;
using Avalonia.Media;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Rendering;
using OpenTK.Graphics.OpenGL4;

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
        /// 静态构造器
        /// </summary>
        static OpenTKViewport()
        {
            BackgroundProperty = AvaloniaProperty.Register<OpenTKViewport, Color>(nameof(Background), Colors.Black);
        }


        /// <summary>
        /// FBO
        /// </summary>
        private int _frameBufferId;

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
            return true;
        }
        #endregion

        #region OpenGL初始化事件 —— override void OnOpenGlInit(GlInterface glInterface)
        /// <summary>
        /// OpenGL初始化事件
        /// </summary>
        /// <param name="glInterface">OpenGL接口</param>
        protected override void OnOpenGlInit(GlInterface glInterface)
        {
            base.OnOpenGlInit(glInterface);

            //加载OpenTK绑定
            AvaloniaBindingsContext bindingsContext = new AvaloniaBindingsContext(glInterface);
            GL.LoadBindings(bindingsContext);
        }
        #endregion

        #region OpenGL渲染事件 —— override void OnOpenGlRender(GlInterface glInterface, int frameBufferId)
        /// <summary>
        /// OpenGL渲染事件
        /// </summary>
        protected override void OnOpenGlRender(GlInterface glInterface, int frameBufferId)
        {
            this._frameBufferId = frameBufferId;

            //设置视口尺寸
            PixelSize size = new PixelSize((int)this.Bounds.Width, (int)this.Bounds.Height);
            GL.Viewport(0, 0, size.Width, size.Height);

            //设置背景色
            float r = this.Background.R * 1.0f / 255.0f;
            float g = this.Background.G * 1.0f / 255.0f;
            float b = this.Background.B * 1.0f / 255.0f;
            float a = this.Background.A * 1.0f / 255.0f;
            GL.ClearColor(r, g, b, a);

            //清理颜色及深度缓存
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //OpenTK渲染
            this.OnOpenTKRender(size);
        }
        #endregion

        #region OpenTK渲染事件 —— abstract void OnOpenTKRender(PixelSize viewportSize)
        /// <summary>
        /// OpenTK渲染事件
        /// </summary>
        /// <param name="viewportSize">视口尺寸</param>
        protected abstract void OnOpenTKRender(PixelSize viewportSize);
        #endregion 

        #endregion
    }
}
