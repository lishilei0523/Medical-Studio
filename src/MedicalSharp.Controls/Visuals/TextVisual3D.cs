using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Media;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Maths;
using Transform = MedicalSharp.Primitives.Maths.Transform;

namespace MedicalSharp.Controls.Visuals
{
    /// <summary>
    /// 文本3D元素
    /// </summary>
    public class TextVisual3D : Visual3D, IVisual2DIn3D, ITranslatable
    {
        #region # 字段及构造器

        /// <summary>
        /// 文本内容依赖属性
        /// </summary>
        public static readonly StyledProperty<string> TextProperty;

        /// <summary>
        /// 位置依赖属性
        /// </summary>
        public static readonly StyledProperty<Vector3D> PositionProperty;

        /// <summary>
        /// 字体大小依赖属性
        /// </summary>
        public static readonly StyledProperty<float> FontSizeProperty;

        /// <summary>
        /// 文本颜色依赖属性
        /// </summary>
        public static readonly StyledProperty<Color> ColorProperty;

        /// <summary>
        /// 渲染模式依赖属性
        /// </summary>
        public static readonly StyledProperty<TextRenderMode> RenderModeProperty;

        /// <summary>
        /// 法向量依赖属性
        /// </summary>
        public static readonly StyledProperty<Vector3D> NormalProperty;

        /// <summary>
        /// 是否锁定Y轴依赖属性
        /// </summary>
        public static readonly StyledProperty<bool> LockYAxisProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static TextVisual3D()
        {
            TextProperty = AvaloniaProperty.Register<TextVisual3D, string>(nameof(Text));
            PositionProperty = AvaloniaProperty.Register<TextVisual3D, Vector3D>(nameof(Position), new Vector3D(0, 0, 0));
            FontSizeProperty = AvaloniaProperty.Register<TextVisual3D, float>(nameof(FontSize), 14.0f);
            ColorProperty = AvaloniaProperty.Register<TextVisual3D, Color>(nameof(Color), Colors.White);
            RenderModeProperty = AvaloniaProperty.Register<TextVisual3D, TextRenderMode>(nameof(RenderMode), TextRenderMode.Billboard);
            NormalProperty = AvaloniaProperty.Register<TextVisual3D, Vector3D>(nameof(Normal), new Vector3D(0, 1, 0));
            LockYAxisProperty = AvaloniaProperty.Register<TextVisual3D, bool>(nameof(LockYAxis), true);

            //属性改变事件
            TextProperty.Changed.AddClassHandler<TextVisual3D, string>(OnTextChanged);
            FontSizeProperty.Changed.AddClassHandler<TextVisual3D, float>(OnFontSizeChanged);
            ColorProperty.Changed.AddClassHandler<TextVisual3D, Color>(OnColorChanged);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public TextVisual3D()
        {

        }

        #endregion

        #region # 属性

        #region 依赖属性 - 文本内容 —— string Text
        /// <summary>
        /// 依赖属性 - 文本内容
        /// </summary>
        public string Text
        {
            get => this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }
        #endregion

        #region 依赖属性 - 位置 —— Vector3D Position
        /// <summary>
        /// 依赖属性 - 位置
        /// </summary>
        public Vector3D Position
        {
            get => this.GetValue(PositionProperty);
            set => this.SetValue(PositionProperty, value);
        }
        #endregion

        #region 依赖属性 - 字体大小 —— float FontSize
        /// <summary>
        /// 依赖属性 - 字体大小
        /// </summary>
        public float FontSize
        {
            get => this.GetValue(FontSizeProperty);
            set => this.SetValue(FontSizeProperty, value);
        }
        #endregion

        #region 依赖属性 - 文本颜色 —— Color Color
        /// <summary>
        /// 依赖属性 - 文本颜色
        /// </summary>
        public Color Color
        {
            get => this.GetValue(ColorProperty);
            set => this.SetValue(ColorProperty, value);
        }
        #endregion

        #region 依赖属性 - 渲染模式 —— TextRenderMode RenderMode
        /// <summary>
        /// 依赖属性 - 渲染模式
        /// </summary>
        public TextRenderMode RenderMode
        {
            get => this.GetValue(RenderModeProperty);
            set => this.SetValue(RenderModeProperty, value);
        }
        #endregion

        #region 依赖属性 - 法向量 —— Vector3D Normal
        /// <summary>
        /// 依赖属性 - 法向量
        /// </summary>
        public Vector3D Normal
        {
            get => this.GetValue(NormalProperty);
            set => this.SetValue(NormalProperty, value);
        }
        #endregion

        #region 依赖属性 - 是否锁定Y轴 —— bool LockYAxis
        /// <summary>
        /// 依赖属性 - 是否锁定Y轴
        /// </summary>
        public bool LockYAxis
        {
            get => this.GetValue(LockYAxisProperty);
            set => this.SetValue(LockYAxisProperty, value);
        }
        #endregion

        #region 文本渲染对象 —— TextRenderable Renderable
        /// <summary>
        /// 文本渲染对象
        /// </summary>
        public TextRenderable Renderable { get; protected set; }
        #endregion

        #region 只读属性 - 变换 —— override Transform Transform
        /// <summary>
        /// 只读属性 - 变换
        /// </summary>
        public override Transform Transform
        {
            get => this.Renderable?.Transform;
        }
        #endregion

        #region 只读属性 - 包围盒 —— override BoundingBox Bounds
        /// <summary>
        /// 只读属性 - 包围盒
        /// </summary>
        public override BoundingBox Bounds
        {
            get => this.Renderable.BoundingBox;
        }
        #endregion

        #endregion

        #region # 方法

        #region 确保渲染对象 —— void EnsureRenderable()
        /// <summary>
        /// 确保渲染对象
        /// </summary>
        internal void EnsureRenderable()
        {
            if (this.Renderable == null)
            {
                TextRenderable renderable = this.RenderMode == TextRenderMode.Fixed
                    ? new TextRenderable(this.Text, this.Position.ToVector3(), this.FontSize, this.Color.ToVector4(), this.Normal.ToVector3())
                    : new TextRenderable(this.Text, this.Position.ToVector3(), this.FontSize, this.Color.ToVector4(), this.LockYAxis);

                this.Renderable = renderable;
            }
        }
        #endregion

        #region 更新渲染对象 —— void UpdateRenderable()
        /// <summary>
        /// 更新渲染对象
        /// </summary>
        private void UpdateRenderable()
        {
            if (this.Renderable != null)
            {
                TextRenderable renderable = (TextRenderable)this.Renderable;
                renderable.Update(this.Text, this.FontSize);
                renderable.SetColor(this.Color.ToVector4());
            }
        }
        #endregion

        #region 元素卸载事件 —— override void OnUnloaded(RoutedEventArgs eventArgs)
        /// <summary>
        /// 元素卸载事件
        /// </summary>
        protected override void OnUnloaded(RoutedEventArgs eventArgs)
        {
            this.Renderable?.Dispose();
        }
        #endregion

        #region 文本内容改变事件 —— static void OnTextChanged(TextVisual3D visual3D...
        /// <summary>
        /// 文本内容改变事件
        /// </summary>
        private static void OnTextChanged(TextVisual3D visual3D, AvaloniaPropertyChangedEventArgs<string> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 字体大小改变事件 —— static void OnFontSizeChanged(TextVisual3D visual3D...
        /// <summary>
        /// 字体大小改变事件
        /// </summary>
        private static void OnFontSizeChanged(TextVisual3D visual3D, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 文本颜色改变事件 —— static void OnColorChanged(TextVisual3D visual3D...
        /// <summary>
        /// 文本颜色改变事件
        /// </summary>
        private static void OnColorChanged(TextVisual3D visual3D, AvaloniaPropertyChangedEventArgs<Color> eventArgs)
        {
            visual3D.Renderable?.SetColor(eventArgs.NewValue.Value.ToVector4());
        }
        #endregion

        #endregion
    }
}
