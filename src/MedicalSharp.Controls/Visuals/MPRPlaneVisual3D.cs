using Avalonia;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Primitives.Builders;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;

namespace MedicalSharp.Controls.Visuals
{
    /// <summary>
    /// MPR平面3D元素
    /// </summary>
    public class MPRPlaneVisual3D : ShapeVisual3D, IVisual2DIn3D, ITranslatableNormal, IRotatable
    {
        #region # 字段及构造器

        /// <summary>
        /// 宽度依赖属性
        /// </summary>
        public new static readonly StyledProperty<float> WidthProperty;

        /// <summary>
        /// 高度依赖属性
        /// </summary>
        public new static readonly StyledProperty<float> HeightProperty;

        /// <summary>
        /// 中心位置依赖属性
        /// </summary>
        public static readonly StyledProperty<Vector3D> CenterProperty;

        /// <summary>
        /// U轴依赖属性
        /// </summary>
        public static readonly StyledProperty<Vector3> UAxisProperty;

        /// <summary>
        /// V轴依赖属性
        /// </summary>
        public static readonly StyledProperty<Vector3> VAxisProperty;

        /// <summary>
        /// 法向量依赖属性
        /// </summary>
        public static readonly StyledProperty<Vector3D> NormalProperty;

        /// <summary>
        /// MPR平面类型依赖属性
        /// </summary>
        public static readonly StyledProperty<MPRPlaneType> PlaneTypeProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static MPRPlaneVisual3D()
        {
            WidthProperty = AvaloniaProperty.Register<MPRPlaneVisual3D, float>(nameof(Width), 1.0f);
            HeightProperty = AvaloniaProperty.Register<MPRPlaneVisual3D, float>(nameof(Height), 1.0f);
            CenterProperty = AvaloniaProperty.Register<MPRPlaneVisual3D, Vector3D>(nameof(Center), new Vector3D(0, 0, 0));
            UAxisProperty = AvaloniaProperty.Register<MPRPlaneVisual3D, Vector3>(nameof(UAxis), new Vector3(1, 0, 0));
            VAxisProperty = AvaloniaProperty.Register<MPRPlaneVisual3D, Vector3>(nameof(VAxis), new Vector3(0, 0, 1));
            NormalProperty = AvaloniaProperty.Register<MPRPlaneVisual3D, Vector3D>(nameof(Normal), new Vector3D(0, 1, 0));
            PlaneTypeProperty = AvaloniaProperty.Register<MPRPlaneVisual3D, MPRPlaneType>(nameof(PlaneType), MPRPlaneType.Axial);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public MPRPlaneVisual3D()
        {

        }

        #endregion

        #region # 属性

        #region 依赖属性 - 宽度 —— float Width
        /// <summary>
        /// 依赖属性 - 宽度
        /// </summary>
        public new float Width
        {
            get => this.GetValue(WidthProperty);
            set => this.SetValue(WidthProperty, value);
        }
        #endregion

        #region 依赖属性 - 高度 —— float Height
        /// <summary>
        /// 依赖属性 - 高度
        /// </summary>
        public new float Height
        {
            get => this.GetValue(HeightProperty);
            set => this.SetValue(HeightProperty, value);
        }
        #endregion

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

        #region 依赖属性 - U轴 —— Vector3 UAxis
        /// <summary>
        /// 依赖属性 - U轴
        /// </summary>
        public Vector3 UAxis
        {
            get => this.GetValue(UAxisProperty);
            set => this.SetValue(UAxisProperty, value);
        }
        #endregion

        #region 依赖属性 - V轴 —— Vector3 VAxis
        /// <summary>
        /// 依赖属性 - V轴
        /// </summary>
        public Vector3 VAxis
        {
            get => this.GetValue(VAxisProperty);
            set => this.SetValue(VAxisProperty, value);
        }
        #endregion

        #region 依赖属性 - 法向量 —— Vector3D Normal
        /// <summary>
        /// 依赖属性 - 法向量
        /// </summary>
        public Vector3D Normal
        {
            get => this.GetValue(NormalProperty);
            set => this.SetValue(NormalProperty, value);
        }
        #endregion

        #region 依赖属性 - MPR平面类型 —— MPRPlaneType PlaneType
        /// <summary>
        /// 依赖属性 - MPR平面类型
        /// </summary>
        public MPRPlaneType PlaneType
        {
            get => this.GetValue(PlaneTypeProperty);
            set => this.SetValue(PlaneTypeProperty, value);
        }
        #endregion

        #region 只读属性 - 平面上一点 —— Vector3D PointOnPlane
        /// <summary>
        /// 只读属性 - 平面上一点
        /// </summary>
        public Vector3D PointOnPlane
        {
            get => this.Center;
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
            Vector3 normal = this.Normal.ToVector3();
            MeshGeometry strokeMesh = MeshFactory.CreateRectangle(center, this.Width, this.Height, normal, GraphicPrimitiveType.Lines);
            MeshGeometry fillMesh = MeshFactory.CreateRectangle(center, this.Width, this.Height, normal, GraphicPrimitiveType.Triangles);
            if (this.Renderable == null)
            {
                WildframeRenderable renderable = new WildframeRenderable(strokeMesh, fillMesh);
                renderable.SetWildframe(this.Stroke.ToVector4(), this.StrokeThickness, this.Fill.ToVector4());
                this.Renderable = renderable;
            }
            else
            {
                WildframeRenderable renderable = (WildframeRenderable)this.Renderable;
                renderable.Update(strokeMesh, fillMesh);
                renderable.SetWildframe(this.Stroke.ToVector4(), this.StrokeThickness, this.Fill.ToVector4());
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
            MPRPlaneVisual3D copy = new MPRPlaneVisual3D
            {
                Id = this.Id,
                Stroke = this.Stroke,
                StrokeThickness = this.StrokeThickness,
                Fill = this.Fill,
                UAxis = this.UAxis,
                VAxis = this.VAxis,
                Width = this.Width,
                Height = this.Height,
                Center = this.Center,
                Normal = this.Normal
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
            if (shapeVisual3D is MPRPlaneVisual3D shape)
            {
                this.Stroke = shape.Stroke;
                this.StrokeThickness = shape.StrokeThickness;
                this.Fill = shape.Fill;
                this.UAxis = shape.UAxis;
                this.VAxis = shape.VAxis;
                this.Width = shape.Width;
                this.Height = shape.Height;
                this.Center = shape.Center;
                this.Normal = shape.Normal;
                this.Transform.SetMatrix(shape.Transform.Matrix);
            }
        }
        #endregion

        #endregion
    }
}
