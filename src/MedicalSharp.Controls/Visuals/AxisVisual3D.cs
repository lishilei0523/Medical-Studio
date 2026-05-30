using Avalonia;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Engine.Renderables;
using OpenTK.Mathematics;

namespace MedicalSharp.Controls.Visuals
{
    /// <summary>
    /// 坐标轴3D元素
    /// </summary>
    public class AxisVisual3D : ShapeVisual3D
    {
        #region # 字段及构造器

        /// <summary>
        /// 位置依赖属性
        /// </summary>
        public static readonly StyledProperty<Vector3D> PositionProperty;

        /// <summary>
        /// 轴线长度依赖属性
        /// </summary>
        public static readonly StyledProperty<float> ShaftLengthProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static AxisVisual3D()
        {
            PositionProperty = AvaloniaProperty.Register<AxisVisual3D, Vector3D>(nameof(Position));
            ShaftLengthProperty = AvaloniaProperty.Register<AxisVisual3D, float>(nameof(ShaftLength), 0.15f);
        }

        /// <summary>
        /// 默认构造器
        /// </summary>
        public AxisVisual3D()
        {

        }

        #endregion

        #region # 属性

        #region 依赖属性 - 位置 —— Vector3D Position
        /// <summary>
        /// 依赖属性 - 位置
        /// </summary>
        public Vector3D Position
        {
            get => this.GetValue(PositionProperty);
            set => this.SetValue(PositionProperty, value);
        }
        #endregion

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
            Vector3 position = this.Position.ToVector3();
            if (this.Renderable == null)
            {
                AxisRenderable renderable = new AxisRenderable(position, this.ShaftLength, 30);
                this.Renderable = renderable;
            }
        }
        #endregion

        #endregion
    }
}
