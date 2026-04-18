using Avalonia.Controls;
using MedicalSharp.Primitives.Maths;

namespace MedicalSharp.Controls.Visuals
{
    /// <summary>
    /// 3D元素
    /// </summary>
    public abstract class Visual3D : Control
    {
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
    }
}
