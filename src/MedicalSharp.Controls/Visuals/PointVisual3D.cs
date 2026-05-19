using Avalonia;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Primitives.Interfaces;
using OpenTK.Mathematics;

namespace MedicalSharp.Controls.Visuals
{
    /// <summary>
    /// 点3D元素
    /// </summary>
    public class PointVisual3D : ShapeVisual3D, ITranslatable3D
    {
        #region # 字段及构造器

        /// <summary>
        /// 位置依赖属性
        /// </summary>
        public static readonly StyledProperty<Vector3D> PositionProperty;

        /// <summary>
        /// 点尺寸依赖属性
        /// </summary>
        public static readonly StyledProperty<float> PointSizeProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static PointVisual3D()
        {
            PositionProperty = AvaloniaProperty.Register<PointVisual3D, Vector3D>(nameof(Position), new Vector3D(0, 0, 0));
            PointSizeProperty = AvaloniaProperty.Register<PointVisual3D, float>(nameof(PointSize), 2.0f);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public PointVisual3D()
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

        #region 依赖属性 - 点尺寸 —— float PointSize
        /// <summary>
        /// 依赖属性 - 点尺寸
        /// </summary>
        public float PointSize
        {
            get => this.GetValue(PointSizeProperty);
            set => this.SetValue(PointSizeProperty, value);
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
                PointRenderable renderable = new PointRenderable(position);
                renderable.SetFill(this.Fill.ToVector4(), this.PointSize);
                this.Renderable = renderable;
            }
            else
            {
                PointRenderable renderable = (PointRenderable)this.Renderable;
                renderable.Update(position);
                renderable.SetFill(this.Fill.ToVector4(), this.PointSize);
            }
        }
        #endregion

        #endregion
    }
}
