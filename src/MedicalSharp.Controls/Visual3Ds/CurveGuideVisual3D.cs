using Avalonia;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Maths;
using System.Linq;

namespace MedicalSharp.Controls.Visual3Ds
{
    /// <summary>
    /// 曲线引导线3D元素
    /// </summary>
    public class CurveGuideVisual3D : ShapeVisual3D, ILineBasedVisual3D, IDraggableAlongCurve, IFunctionalVisual3D
    {
        #region # 字段及构造器

        /// <summary>
        /// 曲线依赖属性
        /// </summary>
        public static readonly StyledProperty<Curve> CurveProperty;

        /// <summary>
        /// 弧长位置依赖属性
        /// </summary>
        public static readonly StyledProperty<float> ArcPositionProperty;

        /// <summary>
        /// 旋转角度依赖属性
        /// </summary>
        public static readonly StyledProperty<float> RotationAngleProperty;

        /// <summary>
        /// 线段长度依赖属性
        /// </summary>
        public static readonly StyledProperty<float> LineLengthProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static CurveGuideVisual3D()
        {
            CurveProperty = AvaloniaProperty.Register<CurveGuideVisual3D, Curve>(nameof(Curve));
            ArcPositionProperty = AvaloniaProperty.Register<CurveGuideVisual3D, float>(nameof(ArcPosition), 0.5f);
            RotationAngleProperty = AvaloniaProperty.Register<CurveGuideVisual3D, float>(nameof(RotationAngle), 0f);
            LineLengthProperty = AvaloniaProperty.Register<CurveGuideVisual3D, float>(nameof(LineLength), 0.3f);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public CurveGuideVisual3D()
        {

        }

        #endregion

        #region # 属性

        #region 依赖属性 - 曲线 —— Curve Curve
        /// <summary>
        /// 依赖属性 - 曲线
        /// </summary>
        public Curve Curve
        {
            get => this.GetValue(CurveProperty);
            set => this.SetValue(CurveProperty, value);
        }
        #endregion

        #region 依赖属性 - 弧长位置 —— float ArcPosition
        /// <summary>
        /// 依赖属性 - 弧长位置
        /// </summary>
        /// <remarks>归一化值0~1</remarks>
        public float ArcPosition
        {
            get => this.GetValue(ArcPositionProperty);
            set => this.SetValue(ArcPositionProperty, value);
        }
        #endregion

        #region 依赖属性 - 旋转角度 —— float RotationAngle
        /// <summary>
        /// 依赖属性 - 旋转角度
        /// </summary>
        public float RotationAngle
        {
            get => this.GetValue(RotationAngleProperty);
            set => this.SetValue(RotationAngleProperty, value);
        }
        #endregion

        #region 依赖属性 - 线段长度 —— float LineLength
        /// <summary>
        /// 依赖属性 - 线段长度
        /// </summary>
        /// <remarks>世界空间</remarks>
        public float LineLength
        {
            get => this.GetValue(LineLengthProperty);
            set => this.SetValue(LineLengthProperty, value);
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
            #region # 验证

            if (this.Curve == null || !this.Curve.FrenetFrames.Any())
            {
                return;
            }

            #endregion

            if (this.Renderable == null)
            {
                CurveGuideRenderable renderable = new CurveGuideRenderable(this.Curve, this.ArcPosition, this.RotationAngle, this.LineLength);
                renderable.SetStroke(this.Stroke.ToVector4(), this.StrokeThickness);
                this.Renderable = renderable;
            }
            else
            {
                CurveGuideRenderable renderable = (CurveGuideRenderable)this.Renderable;
                renderable.Update(this.ArcPosition, this.RotationAngle, this.LineLength);
                renderable.SetStroke(this.Stroke.ToVector4(), this.StrokeThickness);
            }
        }
        #endregion

        #endregion
    }
}
