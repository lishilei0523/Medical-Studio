using Avalonia;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Engine.Renderables;

namespace MedicalSharp.Controls.Visual3Ds
{
    /// <summary>
    /// ViewBox 3D元素
    /// </summary>
    public class ViewBoxVisual3D : ShapeVisual3D, IFunctionalVisual3D
    {
        #region # 字段及构造器

        /// <summary>
        /// 棱长依赖属性
        /// </summary>
        public static readonly StyledProperty<float> SideLengthProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static ViewBoxVisual3D()
        {
            SideLengthProperty = AvaloniaProperty.Register<ViewBoxVisual3D, float>(nameof(SideLength), 0.1f);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public ViewBoxVisual3D()
        {

        }

        #endregion

        #region # 属性

        #region 依赖属性 - 棱长 —— float SideLength
        /// <summary>
        /// 依赖属性 - 棱长
        /// </summary>
        public float SideLength
        {
            get => this.GetValue(SideLengthProperty);
            set => this.SetValue(SideLengthProperty, value);
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
                ViewBoxRenderable renderable = new ViewBoxRenderable(this.SideLength);
                this.Renderable = renderable;
            }
        }
        #endregion

        #endregion
    }
}
