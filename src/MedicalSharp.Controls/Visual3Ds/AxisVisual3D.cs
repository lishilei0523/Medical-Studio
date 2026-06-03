using Avalonia;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Engine.Renderables;

namespace MedicalSharp.Controls.Visual3Ds
{
    /// <summary>
    /// 坐标轴3D元素
    /// </summary>
    public class AxisVisual3D : ShapeVisual3D, IFunctionalVisual3D
    {
        #region # 字段及构造器

        /// <summary>
        /// 轴线长度依赖属性
        /// </summary>
        public static readonly StyledProperty<float> ShaftLengthProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static AxisVisual3D()
        {
            ShaftLengthProperty = AvaloniaProperty.Register<AxisVisual3D, float>(nameof(ShaftLength), 0.1f);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public AxisVisual3D()
        {

        }

        #endregion

        #region # 属性

        #region 依赖属性 - 轴线长度 —— float ShaftLength
        /// <summary>
        /// 依赖属性 - 轴线长度
        /// </summary>
        public float ShaftLength
        {
            get => this.GetValue(ShaftLengthProperty);
            set => this.SetValue(ShaftLengthProperty, value);
        }
        #endregion

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
                AxisRenderable renderable = new AxisRenderable(this.ShaftLength);
                this.Renderable = renderable;
            }
        }
        #endregion

        #endregion
    }
}
