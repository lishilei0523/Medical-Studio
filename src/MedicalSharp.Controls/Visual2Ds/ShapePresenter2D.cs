using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Metadata;

namespace MedicalSharp.Controls.Visual2Ds
{
    /// <summary>
    /// 形状Overlay元素容器
    /// </summary>
    public class ShapePresenter2D : Visual2D
    {
        #region # 字段及构造器

        /// <summary>
        /// 形状Overlay元素依赖属性
        /// </summary>
        public static readonly StyledProperty<ShapeOverlay2D> ContentProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static ShapePresenter2D()
        {
            ContentProperty = AvaloniaProperty.Register<ShapePresenter2D, ShapeOverlay2D>(nameof(Content));

            //属性改变事件
            ContentProperty.Changed.AddClassHandler<ShapePresenter2D, ShapeOverlay2D>(OnContentChanged);
        }

        #endregion

        #region # 属性

        #region 依赖属性 - 形状Overlay元素 —— ShapeOverlay2D Content
        /// <summary>
        /// 依赖属性 - 形状Overlay元素
        /// </summary>
        [Content]
        public ShapeOverlay2D Content
        {
            get => this.GetValue(ContentProperty);
            set => this.SetValue(ContentProperty, value);
        }
        #endregion

        #endregion

        #region # 方法

        #region 元素卸载事件 —— override void OnUnloaded(RoutedEventArgs eventArgs)
        /// <summary>
        /// 元素卸载事件
        /// </summary>
        protected override void OnUnloaded(RoutedEventArgs eventArgs)
        {
            this.Content?.Renderable?.Dispose();
        }
        #endregion

        #region 形状Overlay元素改变事件 —— static void OnContentChanged(ShapePresenter2D...
        /// <summary>
        /// 形状Overlay元素改变事件
        /// </summary>
        private static void OnContentChanged(ShapePresenter2D shapePresenter, AvaloniaPropertyChangedEventArgs<ShapeOverlay2D> eventArgs)
        {
            eventArgs.OldValue.Value?.Renderable?.Dispose();
        }
        #endregion

        #endregion
    }
}
