using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Media;
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

        #region 只读属性 - 线框渲染对象 —— abstract WireframeRenderable Renderable
        /// <summary>
        /// 只读属性 - 线框渲染对象
        /// </summary>
        public abstract WireframeRenderable Renderable { get; }
        #endregion

        #endregion

        #region # 方法

        #region 元素卸载事件 —— override void OnUnloaded(RoutedEventArgs eventArgs)
        /// <summary>
        /// 元素卸载事件
        /// </summary>
        protected override void OnUnloaded(RoutedEventArgs eventArgs)
        {
            this.Renderable?.Dispose();
        }
        #endregion

        #endregion
    }
}
