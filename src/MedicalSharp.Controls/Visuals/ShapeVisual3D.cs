using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Media;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Primitives.Maths;
using Transform = MedicalSharp.Primitives.Maths.Transform;

namespace MedicalSharp.Controls.Visuals
{
    /// <summary>
    /// 形状3D元素
    /// </summary>
    public abstract class ShapeVisual3D : Visual3D
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
        static ShapeVisual3D()
        {
            StrokeProperty = AvaloniaProperty.Register<ShapeVisual3D, Color>(nameof(Stroke), Colors.Red);
            StrokeThicknessProperty = AvaloniaProperty.Register<ShapeVisual3D, float>(nameof(StrokeThickness), 1.0f);
            FillProperty = AvaloniaProperty.Register<ShapeVisual3D, Color>(nameof(Fill), Colors.Transparent);
        }

        #endregion

        #region # 属性

        #region 形状渲染对象 —— ShapeRenderable Renderable
        /// <summary>
        /// 形状渲染对象
        /// </summary>
        public ShapeRenderable Renderable { get; protected set; }
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

        #region 克隆 —— abstract ShapeVisual3D Clone()
        /// <summary>
        /// 克隆
        /// </summary>
        /// <returns>形状副本</returns>
        public abstract ShapeVisual3D Clone();
        #endregion

        #region 复制 —— abstract void Copy(ShapeVisual3D shapeVisual3D)
        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="shapeVisual3D">形状</param>
        public abstract void Copy(ShapeVisual3D shapeVisual3D);
        #endregion

        #region 确保渲染对象 —— abstract void EnsureRenderable()
        /// <summary>
        /// 确保渲染对象
        /// </summary>
        internal abstract void EnsureRenderable();
        #endregion

        #region 更新渲染对象 —— abstract void UpdateRenderable()
        /// <summary>
        /// 更新渲染对象
        /// </summary>
        internal abstract void UpdateRenderable();
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
