using Avalonia.Controls;
using MedicalSharp.Primitives.Maths;
using System;

namespace MedicalSharp.Controls.Visuals
{
    /// <summary>
    /// 3D元素
    /// </summary>
    public abstract class Visual3D : Control
    {
        #region # 字段及构造器

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
        public string Id { get; protected set; }
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
