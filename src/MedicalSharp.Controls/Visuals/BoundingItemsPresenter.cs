using Avalonia;
using Avalonia.Collections;

namespace MedicalSharp.Controls.Visuals
{
    /// <summary>
    /// 边界3D元素列表容器
    /// </summary>
    public class BoundingItemsPresenter : Visual3D
    {
        #region # 字段及构造器

        /// <summary>
        /// 边界3D元素列表依赖属性
        /// </summary>
        public static readonly StyledProperty<AvaloniaList<BoundingVisual3D>> ItemsSourceProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static BoundingItemsPresenter()
        {
            ItemsSourceProperty = AvaloniaProperty.Register<BoundingItemsPresenter, AvaloniaList<BoundingVisual3D>>(nameof(ItemsSource), []);
        }

        #endregion

        #region # 属性

        #region 依赖属性 - 边界3D元素列表 —— AvaloniaList<BoundingVisual3D> Content
        /// <summary>
        /// 依赖属性 - 边界3D元素列表
        /// </summary>
        public AvaloniaList<BoundingVisual3D> ItemsSource
        {
            get => this.GetValue(ItemsSourceProperty);
            set => this.SetValue(ItemsSourceProperty, value);
        }
        #endregion

        #endregion
    }
}
