using Avalonia;
using Avalonia.Metadata;

namespace MedicalSharp.Controls.Visuals
{
    /// <summary>
    /// 边界3D元素容器
    /// </summary>
    public class BoundingItemPresenter : Visual3D
    {
        #region # 字段及构造器

        /// <summary>
        /// 边界3D元素依赖属性
        /// </summary>
        public static readonly StyledProperty<BoundingVisual3D> ContentProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static BoundingItemPresenter()
        {
            ContentProperty = AvaloniaProperty.Register<BoundingItemPresenter, BoundingVisual3D>(nameof(Content));
        }

        #endregion

        #region # 属性

        #region 依赖属性 - 边界3D元素 —— BoundingVisual3D Content
        /// <summary>
        /// 依赖属性 - 边界3D元素
        /// </summary>
        [Content]
        public BoundingVisual3D Content
        {
            get => this.GetValue(ContentProperty);
            set => this.SetValue(ContentProperty, value);
        }
        #endregion

        #endregion
    }
}
