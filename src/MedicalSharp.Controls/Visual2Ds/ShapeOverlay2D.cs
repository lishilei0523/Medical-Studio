using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Media;
using MedicalSharp.Engine.Base;

namespace MedicalSharp.Controls.Visual2Ds
{
    /// <summary>
    /// 形状Overlay元素
    /// </summary>
    public abstract class ShapeOverlay2D : Visual2D
    {
        #region # 字段及构造器

        /// <summary>
        /// 位置依赖属性
        /// </summary>
        public static readonly StyledProperty<Point> PositionProperty;

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
        static ShapeOverlay2D()
        {
            PositionProperty = AvaloniaProperty.Register<ShapeOverlay2D, Point>(nameof(Position), new Point(0, 0));
            StrokeProperty = AvaloniaProperty.Register<ShapeOverlay2D, Color>(nameof(Stroke), Colors.Red);
            StrokeThicknessProperty = AvaloniaProperty.Register<ShapeOverlay2D, float>(nameof(StrokeThickness), 1.0f);
            FillProperty = AvaloniaProperty.Register<ShapeOverlay2D, Color>(nameof(Fill), Colors.Transparent);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        protected ShapeOverlay2D()
        {

        }

        #endregion

        #region # 属性

        #region 2D渲染对象 —— Renderable2D Renderable
        /// <summary>
        /// 2D渲染对象
        /// </summary>
        public Renderable2D Renderable { get; protected set; }
        #endregion

        #region 依赖属性 - 位置 —— Point Position
        /// <summary>
        /// 依赖属性 - 位置
        /// </summary>
        public Point Position
        {
            get => this.GetValue(PositionProperty);
            set => this.SetValue(PositionProperty, value);
        }
        #endregion

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

        #endregion

        #region #  方法

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

        #endregion
    }
}
