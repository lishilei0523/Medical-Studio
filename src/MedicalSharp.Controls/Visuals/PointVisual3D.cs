using Avalonia;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Primitives.Interfaces;

namespace MedicalSharp.Controls.Visuals
{
    /// <summary>
    /// 点3D元素
    /// </summary>
    public class PointVisual3D : ShapeVisual3D, IPureVisual3D, ITranslatable
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
            if (this.Renderable == null)
            {
                PointRenderable renderable = new PointRenderable(this.Position.ToVector3());
                renderable.SetFill(this.Fill.ToVector4(), this.PointSize);

                this.Renderable = renderable;
            }
            else
            {
                PointRenderable renderable = (PointRenderable)this.Renderable;
                renderable.Update(this.Position.ToVector3());
                renderable.SetFill(this.Fill.ToVector4(), this.PointSize);
            }
        }
        #endregion

        #region 克隆 —— override ShapeVisual3D Clone()
        /// <summary>
        /// 克隆
        /// </summary>
        /// <returns>形状副本</returns>
        public override ShapeVisual3D Clone()
        {
            PointVisual3D copy = new PointVisual3D
            {
                Id = this.Id,
                Stroke = this.Stroke,
                StrokeThickness = this.StrokeThickness,
                Fill = this.Fill,
                Position = this.Position,
                PointSize = this.PointSize
            };

            return copy;
        }
        #endregion

        #region 复制 —— override void Copy(ShapeVisual3D shapeVisual3D)
        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="shapeVisual3D">形状</param>
        public override void Copy(ShapeVisual3D shapeVisual3D)
        {
            if (shapeVisual3D is PointVisual3D shape)
            {
                this.Stroke = shape.Stroke;
                this.StrokeThickness = shape.StrokeThickness;
                this.Fill = shape.Fill;
                this.Position = shape.Position;
                this.PointSize = shape.PointSize;
                this.Transform.SetMatrix(shape.Transform.Matrix);
            }
        }
        #endregion

        #endregion
    }
}
