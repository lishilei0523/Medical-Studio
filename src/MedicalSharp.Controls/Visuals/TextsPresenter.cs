using Avalonia;
using Avalonia.Collections;
using MedicalSharp.Primitives.Maths;
using System;

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
    }
}
