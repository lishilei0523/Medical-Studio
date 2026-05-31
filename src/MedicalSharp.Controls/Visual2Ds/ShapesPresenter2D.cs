using Avalonia;
using Avalonia.Collections;
using Avalonia.Interactivity;
using System.Collections.Specialized;

namespace MedicalSharp.Controls.Visual2Ds
{
    /// <summary>
    /// 形状Overlay元素列表容器
    /// </summary>
    public class ShapesPresenter2D : Visual2D
    {
        #region # 字段及构造器

        /// <summary>
        /// 形状Overlay元素列表依赖属性
        /// </summary>
        public static readonly StyledProperty<AvaloniaList<ShapeOverlay2D>> ItemsSourceProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static ShapesPresenter2D()
        {
            ItemsSourceProperty = AvaloniaProperty.Register<ShapesPresenter2D, AvaloniaList<ShapeOverlay2D>>(nameof(ItemsSource), []);

            //属性改变事件
            ItemsSourceProperty.Changed.AddClassHandler<ShapesPresenter2D, AvaloniaList<ShapeOverlay2D>>(OnItemsSourceChanged);
        }

        /// <summary>
        /// 默认构造器
        /// </summary>
        public ShapesPresenter2D()
        {
            this.ItemsSource.CollectionChanged += this.OnItemsSourceItemChanged;
        }

        #endregion

        #region # 属性

        #region 依赖属性 - 形状Overlay元素列表 —— AvaloniaList<ShapeOverlay2D> ItemsSource
        /// <summary>
        /// 依赖属性 - 形状Overlay元素列表
        /// </summary>
        public AvaloniaList<ShapeOverlay2D> ItemsSource
        {
            get => this.GetValue(ItemsSourceProperty);
            set => this.SetValue(ItemsSourceProperty, value);
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
            foreach (ShapeOverlay2D shapeOverlay2D in this.ItemsSource)
            {
                shapeOverlay2D.Renderable?.Dispose();
            }
        }
        #endregion

        #region 形状Overlay元素列表改变事件 —— static void OnItemsSourceChanged(ShapesPresenter2D...
        /// <summary>
        /// 形状Overlay元素列表改变事件
        /// </summary>
        private static void OnItemsSourceChanged(ShapesPresenter2D presenter, AvaloniaPropertyChangedEventArgs<AvaloniaList<ShapeOverlay2D>> eventArgs)
        {
            if (eventArgs.OldValue.Value != null)
            {
                eventArgs.OldValue.Value.CollectionChanged -= presenter.OnItemsSourceItemChanged;
            }
            if (eventArgs.NewValue.Value != null)
            {
                eventArgs.NewValue.Value.CollectionChanged += presenter.OnItemsSourceItemChanged;
            }
        }
        #endregion

        #region 形状Overlay元素列表元素改变事件 —— void OnItemsSourceItemChanged(object sender...
        /// <summary>
        /// 形状Overlay元素列表元素改变事件
        /// </summary>
        private void OnItemsSourceItemChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            if (eventArgs.Action == NotifyCollectionChangedAction.Remove && eventArgs.OldItems != null)
            {
                foreach (ShapeOverlay2D shapeOverlay2D in eventArgs.OldItems)
                {
                    shapeOverlay2D.Renderable?.Dispose();
                }
            }
        }
        #endregion

        #endregion
    }
}
