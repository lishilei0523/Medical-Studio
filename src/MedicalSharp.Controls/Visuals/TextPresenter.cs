using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using MedicalSharp.Primitives.Maths;
using System;

namespace MedicalSharp.Controls.Visuals
{
    /// <summary>
    /// 文本3D元素容器
    /// </summary>
    public class TextPresenter : Visual3D
    {
        #region # 字段及构造器

        /// <summary>
        /// 文本3D元素依赖属性
        /// </summary>
        public static readonly StyledProperty<TextVisual3D> ContentProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static TextPresenter()
        {
            ContentProperty = AvaloniaProperty.Register<TextPresenter, TextVisual3D>(nameof(Content));

            //属性改变事件
            ContentProperty.Changed.AddClassHandler<TextPresenter, TextVisual3D>(OnContentChanged);
        }

        #endregion

        #region # 属性

        #region 依赖属性 - 文本3D元素 —— TextVisual3D Content
        /// <summary>
        /// 依赖属性 - 文本3D元素
        /// </summary>
        [Content]
        public TextVisual3D Content
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

        #region 文本3D元素改变事件 —— static void OnContentChanged(TextPresenter...
        /// <summary>
        /// 文本3D元素改变事件
        /// </summary>
        private static void OnContentChanged(TextPresenter textPresenter, AvaloniaPropertyChangedEventArgs<TextVisual3D> eventArgs)
        {
            eventArgs.OldValue.Value?.Renderable?.Dispose();
        }
        #endregion

        #endregion
    }
}
