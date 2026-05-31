using MedicalSharp.Engine.Renderables;

namespace MedicalSharp.Controls.Visual3Ds
{
    /// <summary>
    /// 坐标轴3D元素
    /// </summary>
    public class AxisVisual3D : ShapeVisual3D
    {
        #region # 字段及构造器

        /// <summary>
        /// 默认构造器
        /// </summary>
        public AxisVisual3D()
        {

        }

        #endregion

        #region # 属性

        //

        #endregion

        #region # 方法

        #region 确保渲染对象 —— override void EnsureRenderable()
        /// <summary>
        /// 确保渲染对象
        /// </summary>
        internal override void EnsureRenderable()
        {
            if (this.Renderable == null)
            {
                AxisRenderable renderable = new AxisRenderable();
                this.Renderable = renderable;
            }
        }
        #endregion

        #endregion
    }
}
