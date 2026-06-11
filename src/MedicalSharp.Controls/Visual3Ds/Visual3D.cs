using Avalonia;
using Avalonia.Controls;
using MedicalSharp.Primitives.Maths;
using System;

namespace MedicalSharp.Controls.Visual3Ds
{
    /// <summary>
    /// 3D元素
    /// </summary>
    public abstract class Visual3D : Control
    {
        #region # 字段及构造器

        /// <summary>
        /// 显示名称依赖属性
        /// </summary>
        public static readonly StyledProperty<string> DisplayNameProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static Visual3D()
        {
            DisplayNameProperty = AvaloniaProperty.Register<Visual3D, string>(nameof(DisplayName));
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        protected Visual3D()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        #endregion

        #region # 属性

        #region 标识Id —— string Id
        /// <summary>
        /// 标识Id
        /// </summary>
        public string Id { get; internal set; }
        #endregion

        #region 依赖属性 - 显示名称 —— string DisplayName
        /// <summary>
        /// 依赖属性 - 显示名称
        /// </summary>
        public string DisplayName
        {
            get => this.GetValue(DisplayNameProperty);
            set => this.SetValue(DisplayNameProperty, value);
        }
        #endregion

        #region 只读属性 - 变换 —— abstract Transform Transform
        /// <summary>
        /// 只读属性 - 变换
        /// </summary>
        public abstract Transform Transform { get; }
        #endregion

        #region 只读属性 - 包围盒 —— new abstract BoundingBox Bounds
        /// <summary>
        /// 只读属性 - 包围盒
        /// </summary>
        public new abstract BoundingBox Bounds { get; }
        #endregion

        #endregion

        #region # 方法

        //

        #endregion
    }
}
