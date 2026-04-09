using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Media;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Engine.Renderables;

namespace MedicalSharp.Controls.Visuals
{
    /// <summary>
    /// 边界3D元素
    /// </summary>
    public abstract class BoundingVisual3D : Visual3D
    {
        #region # 字段及构造器

        /// <summary>
        /// 线框颜色依赖属性
        /// </summary>
        public static readonly StyledProperty<Color> StrokeProperty;

        /// <summary>
        /// 线框粗细依赖属性
        /// </summary>
        public static readonly StyledProperty<float> StrokeThicknessProperty;

        /// <summary>
        /// 填充颜色依赖属性
        /// </summary>
        public static readonly StyledProperty<Color> FillProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static BoundingVisual3D()
        {
            StrokeProperty = AvaloniaProperty.Register<BoundingVisual3D, Color>(nameof(Stroke), Colors.Red);
            StrokeThicknessProperty = AvaloniaProperty.Register<BoundingVisual3D, float>(nameof(StrokeThickness), 1.0f);
            FillProperty = AvaloniaProperty.Register<BoundingVisual3D, Color>(nameof(Fill), Colors.Transparent);

            //属性改变事件
            StrokeProperty.Changed.AddClassHandler<BoundingVisual3D, Color>(OnStrokeChanged);
            StrokeThicknessProperty.Changed.AddClassHandler<BoundingVisual3D, float>(OnStrokeThicknessChanged);
            FillProperty.Changed.AddClassHandler<BoundingVisual3D, Color>(OnFillChanged);
        }

        #endregion

        #region # 属性

        #region 依赖属性 - 线框颜色 —— Color Stroke
        /// <summary>
        /// 依赖属性 - 线框颜色
        /// </summary>
        public Color Stroke
        {
            get => this.GetValue(StrokeProperty);
            set => this.SetValue(StrokeProperty, value);
        }
        #endregion

        #region 依赖属性 - 线框粗细 —— float StrokeThickness
        /// <summary>
        /// 依赖属性 - 线框粗细
        /// </summary>
        public float StrokeThickness
        {
            get => this.GetValue(StrokeThicknessProperty);
            set => this.SetValue(StrokeThicknessProperty, value);
        }
        #endregion

        #region 依赖属性 - 填充颜色 —— Color Fill
        /// <summary>
        /// 依赖属性 - 填充颜色
        /// </summary>
        public Color Fill
        {
            get => this.GetValue(FillProperty);
            set => this.SetValue(FillProperty, value);
        }
        #endregion

        #region 线框渲染对象 —— WireframeRenderable Renderable
        /// <summary>
        /// 线框渲染对象
        /// </summary>
        public WireframeRenderable Renderable { get; protected set; }
        #endregion

        #endregion

        #region # 方法

        #region 确保渲染对象 —— abstract void EnsureRenderable()
        /// <summary>
        /// 确保渲染对象
        /// </summary>
        internal abstract void EnsureRenderable();
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

        #region 线框颜色改变事件 —— static void OnStrokeChanged(BoundingVisual3D visual3D...
        /// <summary>
        /// 线框颜色改变事件
        /// </summary>
        private static void OnStrokeChanged(BoundingVisual3D visual3D, AvaloniaPropertyChangedEventArgs<Color> eventArgs)
        {
            visual3D.Renderable?.SetColor(eventArgs.NewValue.Value.ToVector4(), visual3D.StrokeThickness, visual3D.Fill.ToVector4());
        }
        #endregion

        #region 线框粗细改变事件 —— static void OnStrokeThicknessChanged(BoundingVisual3D visual3D...
        /// <summary>
        /// 线框粗细改变事件
        /// </summary>
        private static void OnStrokeThicknessChanged(BoundingVisual3D visual3D, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            visual3D.Renderable?.SetColor(visual3D.Stroke.ToVector4(), eventArgs.NewValue.Value, visual3D.Fill.ToVector4());
        }
        #endregion

        #region 填充颜色改变事件 —— static void OnFillChanged(BoundingVisual3D visual3D...
        /// <summary>
        /// 填充颜色改变事件
        /// </summary>
        private static void OnFillChanged(BoundingVisual3D visual3D, AvaloniaPropertyChangedEventArgs<Color> eventArgs)
        {
            visual3D.Renderable?.SetColor(visual3D.Stroke.ToVector4(), visual3D.StrokeThickness, eventArgs.NewValue.Value.ToVector4());
        }
        #endregion

        #endregion
    }
}
