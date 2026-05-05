using Avalonia;
using Avalonia.Media;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Primitives.Interfaces;
using OpenTK.Mathematics;

namespace MedicalSharp.Controls.Visuals
{
    /// <summary>
    /// 十字线3D元素
    /// </summary>
    public class CrosshairVisual3D : ShapeVisual3D, ILineBasedVisual3D, ITranslatable, IRotatable
    {
        #region # 字段及构造器

        /// <summary>
        /// 中心位置依赖属性
        /// </summary>
        public static readonly StyledProperty<Vector3D> CenterProperty;

        /// <summary>
        /// U轴依赖属性
        /// </summary>
        public static readonly StyledProperty<Vector3D> UAxisProperty;

        /// <summary>
        /// V轴依赖属性
        /// </summary>
        public static readonly StyledProperty<Vector3D> VAxisProperty;

        /// <summary>
        /// 水平长度依赖属性
        /// </summary>
        public static readonly StyledProperty<float> HorizontalLengthProperty;

        /// <summary>
        /// 垂直长度依赖属性
        /// </summary>
        public static readonly StyledProperty<float> VerticalLengthProperty;

        /// <summary>
        /// 水平颜色依赖属性
        /// </summary>
        public static readonly StyledProperty<Color> HorizontalStrokeProperty;

        /// <summary>
        /// 垂直颜色依赖属性
        /// </summary>
        public static readonly StyledProperty<Color> VerticalStrokeProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static CrosshairVisual3D()
        {
            CenterProperty = AvaloniaProperty.Register<CrosshairVisual3D, Vector3D>(nameof(Center));
            UAxisProperty = AvaloniaProperty.Register<CrosshairVisual3D, Vector3D>(nameof(UAxis), new Vector3D(1, 0, 0));
            VAxisProperty = AvaloniaProperty.Register<CrosshairVisual3D, Vector3D>(nameof(VAxis), new Vector3D(0, 1, 0));
            HorizontalLengthProperty = AvaloniaProperty.Register<CrosshairVisual3D, float>(nameof(HorizontalLength), 3.0f);
            VerticalLengthProperty = AvaloniaProperty.Register<CrosshairVisual3D, float>(nameof(VerticalLength), 3.0f);
            HorizontalStrokeProperty = AvaloniaProperty.Register<CrosshairVisual3D, Color>(nameof(HorizontalStroke), Colors.Red);
            VerticalStrokeProperty = AvaloniaProperty.Register<CrosshairVisual3D, Color>(nameof(VerticalStroke), Colors.LimeGreen);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public CrosshairVisual3D()
        {

        }

        #endregion

        #region # 属性

        #region 依赖属性 - 中心位置 —— Vector3D Center
        /// <summary>
        /// 依赖属性 - 中心位置
        /// </summary>
        public Vector3D Center
        {
            get => this.GetValue(CenterProperty);
            set => this.SetValue(CenterProperty, value);
        }
        #endregion

        #region 依赖属性 - U轴 —— Vector3D UAxis
        /// <summary>
        /// 依赖属性 - U轴
        /// </summary>
        public Vector3D UAxis
        {
            get => this.GetValue(UAxisProperty);
            set => this.SetValue(UAxisProperty, value);
        }
        #endregion

        #region 依赖属性 - V轴 —— Vector3D VAxis
        /// <summary>
        /// 依赖属性 - V轴
        /// </summary>
        public Vector3D VAxis
        {
            get => this.GetValue(VAxisProperty);
            set => this.SetValue(VAxisProperty, value);
        }
        #endregion

        #region 依赖属性 - 水平长度 —— float HorizontalLength
        /// <summary>
        /// 依赖属性 - 水平长度
        /// </summary>
        public float HorizontalLength
        {
            get => this.GetValue(HorizontalLengthProperty);
            set => this.SetValue(HorizontalLengthProperty, value);
        }
        #endregion

        #region 依赖属性 - 垂直长度 —— float VerticalLength
        /// <summary>
        /// 依赖属性 - 垂直长度
        /// </summary>
        public float VerticalLength
        {
            get => this.GetValue(VerticalLengthProperty);
            set => this.SetValue(VerticalLengthProperty, value);
        }
        #endregion

        #region 依赖属性 - 水平颜色 —— Color HorizontalStroke
        /// <summary>
        /// 依赖属性 - 水平颜色
        /// </summary>
        public Color HorizontalStroke
        {
            get => this.GetValue(HorizontalStrokeProperty);
            set => this.SetValue(HorizontalStrokeProperty, value);
        }
        #endregion

        #region 依赖属性 - 垂直颜色 —— Color VerticalStroke
        /// <summary>
        /// 依赖属性 - 垂直颜色
        /// </summary>
        public Color VerticalStroke
        {
            get => this.GetValue(VerticalStrokeProperty);
            set => this.SetValue(VerticalStrokeProperty, value);
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
            Vector3 center = this.Center.ToVector3();
            Vector3 uAxis = this.UAxis.ToVector3();
            Vector3 vAxis = this.VAxis.ToVector3();
            if (this.Renderable == null)
            {
                CrosshairRenderable renderable = new CrosshairRenderable(center, uAxis, vAxis, this.HorizontalLength, this.VerticalLength);
                renderable.SetStroke(this.HorizontalStroke.ToVector4(), this.VerticalStroke.ToVector4(), this.StrokeThickness);
                this.Renderable = renderable;
            }
            else
            {
                CrosshairRenderable renderable = (CrosshairRenderable)this.Renderable;
                renderable.Update(center, uAxis, vAxis, this.HorizontalLength, this.VerticalLength);
                renderable.SetStroke(this.HorizontalStroke.ToVector4(), this.VerticalStroke.ToVector4(), this.StrokeThickness);
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
            CrosshairVisual3D copy = new CrosshairVisual3D
            {
                Id = this.Id,
                Stroke = this.Stroke,
                StrokeThickness = this.StrokeThickness,
                Fill = this.Fill,
                Center = this.Center,
                UAxis = this.UAxis,
                VAxis = this.VAxis,
                HorizontalLength = this.HorizontalLength,
                VerticalLength = this.VerticalLength,
                HorizontalStroke = this.HorizontalStroke,
                VerticalStroke = this.VerticalStroke
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
            if (shapeVisual3D is CrosshairVisual3D shape)
            {
                this.Stroke = shape.Stroke;
                this.StrokeThickness = shape.StrokeThickness;
                this.Fill = shape.Fill;
                this.Center = shape.Center;
                this.UAxis = shape.UAxis;
                this.VAxis = shape.VAxis;
                this.HorizontalLength = shape.HorizontalLength;
                this.VerticalLength = shape.VerticalLength;
                this.HorizontalStroke = shape.HorizontalStroke;
                this.VerticalStroke = shape.VerticalStroke;
                this.Transform.SetMatrix(shape.Transform.Matrix);
            }
        }
        #endregion

        #endregion
    }
}
