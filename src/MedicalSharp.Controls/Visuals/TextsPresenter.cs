using Avalonia;
using Avalonia.Collections;
using Avalonia.Interactivity;
using MedicalSharp.Primitives.Maths;
using System;
using System.Collections.Specialized;

namespace MedicalSharp.Controls.Visuals
{
    /// <summary>
    /// 文本3D元素列表容器
    /// </summary>
    public class TextsPresenter : Visual3D
    {
        #region # 字段及构造器

        /// <summary>
        /// 文本3D元素列表依赖属性
        /// </summary>
        public static readonly StyledProperty<AvaloniaList<TextVisual3D>> ItemsSourceProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static TextsPresenter()
        {
            ItemsSourceProperty = AvaloniaProperty.Register<TextsPresenter, AvaloniaList<TextVisual3D>>(nameof(ItemsSource), []);

            //属性改变事件
            ItemsSourceProperty.Changed.AddClassHandler<TextsPresenter, AvaloniaList<TextVisual3D>>(OnItemsSourceChanged);
        }

        /// <summary>
        /// 默认构造器
        /// </summary>
        public TextsPresenter()
        {
            this.ItemsSource.CollectionChanged += this.OnItemsSourceItemChanged;
        }

        #endregion

        #region # 属性

        #region 依赖属性 - 文本3D元素列表 —— AvaloniaList<TextVisual3D> ItemsSource
        /// <summary>
        /// 依赖属性 - 文本3D元素列表
        /// </summary>
        public AvaloniaList<TextVisual3D> ItemsSource
        {
            get => this.GetValue(ItemsSourceProperty);
            set => this.SetValue(ItemsSourceProperty, value);
        }
        #endregion

        #region 只读属性 - 变换 —— override Transform Transform
        /// <summary>
        /// 只读属性 - 变换
        /// </summary>
        public override Transform Transform
        {
            get => throw new NotSupportedException();
        }
        #endregion

        #region 只读属性 - 包围盒 —— override BoundingBox Bounds
        /// <summary>
        /// 只读属性 - 包围盒
        /// </summary>
        public override BoundingBox Bounds
        {
            get => throw new NotSupportedException();
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
            foreach (TextVisual3D textVisual3D in this.ItemsSource)
            {
                textVisual3D.Renderable?.Dispose();
            }
        }
        #endregion

        #region 文本3D元素列表改变事件 —— static void OnItemsSourceChanged(TextsPresenter...
        /// <summary>
        /// 文本3D元素列表改变事件
        /// </summary>
        private static void OnItemsSourceChanged(TextsPresenter presenter, AvaloniaPropertyChangedEventArgs<AvaloniaList<TextVisual3D>> eventArgs)
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

        #region 文本3D元素列表元素改变事件 —— void OnItemsSourceItemChanged(object sender...
        /// <summary>
        /// 文本3D元素列表元素改变事件
        /// </summary>
        private void OnItemsSourceItemChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            if (eventArgs.Action == NotifyCollectionChangedAction.Remove && eventArgs.OldItems != null)
            {
                foreach (TextVisual3D textVisual3D in eventArgs.OldItems)
                {
                    textVisual3D.Renderable?.Dispose();
                }
            }
        }
        #endregion

        #endregion
    }
}
