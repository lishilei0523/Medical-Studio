using Avalonia;
using Avalonia.Metadata;
using MedicalSharp.Primitives.Maths;
using System;

namespace MedicalSharp.Controls.Visuals
{
    /// <summary>
    /// 形状3D元素容器
    /// </summary>
    public class ShapePresenter : Visual3D
    {
        #region # 字段及构造器

        /// <summary>
        /// 3D元素依赖属性
        /// </summary>
        public static readonly StyledProperty<ShapeVisual3D> ContentProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static ShapePresenter()
        {
            ContentProperty = AvaloniaProperty.Register<ShapePresenter, ShapeVisual3D>(nameof(Content));
        }

        #endregion

        #region # 属性

        #region 依赖属性 - 形状3D元素 —— ShapeVisual3D Content
        /// <summary>
        /// 依赖属性 - 形状3D元素
        /// </summary>
        [Content]
        public ShapeVisual3D Content
        {
            get => this.GetValue(ContentProperty);
            set => this.SetValue(ContentProperty, value);
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
