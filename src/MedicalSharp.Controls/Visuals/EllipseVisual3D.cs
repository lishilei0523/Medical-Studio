using Avalonia;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Primitives.Builders;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Models;

namespace MedicalSharp.Controls.Visuals
{
    /// <summary>
    /// 椭圆形3D元素
    /// </summary>
    public class EllipseVisual3D : ShapeVisual3D
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
        /// 法向量依赖属性
        /// </summary>
        public static readonly StyledProperty<Vector3D> NormalProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static EllipseVisual3D()
        {
            WidthProperty = AvaloniaProperty.Register<EllipseVisual3D, float>(nameof(Width), 1.0f);
            HeightProperty = AvaloniaProperty.Register<EllipseVisual3D, float>(nameof(Height), 1.0f);
            CenterProperty = AvaloniaProperty.Register<EllipseVisual3D, Vector3D>(nameof(Center), new Vector3D(0, 0, 0));
            NormalProperty = AvaloniaProperty.Register<EllipseVisual3D, Vector3D>(nameof(Normal), new Vector3D(0, 0, 1));

            //属性改变事件
            WidthProperty.Changed.AddClassHandler<EllipseVisual3D, float>(OnWidthChanged);
            HeightProperty.Changed.AddClassHandler<EllipseVisual3D, float>(OnHeightChanged);
            CenterProperty.Changed.AddClassHandler<EllipseVisual3D, Vector3D>(OnCenterChanged);
            NormalProperty.Changed.AddClassHandler<EllipseVisual3D, Vector3D>(OnNormalChanged);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public EllipseVisual3D()
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
                MeshGeometry strokeMesh = MeshFactory.CreateEllipse(this.Center.ToVector3(), this.Width, this.Height, this.Normal.ToVector3(), 64, GraphicPrimitiveType.Lines);
                MeshGeometry fillMesh = MeshFactory.CreateEllipse(this.Center.ToVector3(), this.Width, this.Height, this.Normal.ToVector3(), 64, GraphicPrimitiveType.Triangles);

                WildframeRenderable renderable = new WildframeRenderable(strokeMesh, fillMesh);
                renderable.SetWildframe(this.Stroke.ToVector4(), this.StrokeThickness, this.Fill.ToVector4());

                this.Renderable = renderable;
            }
        }
        #endregion

        #region 更新渲染对象 —— void UpdateRenderable()
        /// <summary>
        /// 更新渲染对象
        /// </summary>
        private void UpdateRenderable()
        {
            if (this.Renderable != null)
            {
                MeshGeometry strokeMesh = MeshFactory.CreateEllipse(this.Center.ToVector3(), this.Width, this.Height, this.Normal.ToVector3(), 64, GraphicPrimitiveType.Lines);
                MeshGeometry fillMesh = MeshFactory.CreateEllipse(this.Center.ToVector3(), this.Width, this.Height, this.Normal.ToVector3(), 64, GraphicPrimitiveType.Triangles);

                WildframeRenderable renderable = (WildframeRenderable)this.Renderable;
                renderable.Update(strokeMesh, fillMesh);
                renderable.SetWildframe(this.Stroke.ToVector4(), this.StrokeThickness, this.Fill.ToVector4());
            }
        }
        #endregion

        #region 宽度改变事件 —— static void OnWidthChanged(EllipseVisual3D visual3D...
        /// <summary>
        /// 宽度改变事件
        /// </summary>
        private static void OnWidthChanged(EllipseVisual3D visual3D, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 高度改变事件 —— static void OnHeightChanged(EllipseVisual3D visual3D...
        /// <summary>
        /// 高度改变事件
        /// </summary>
        private static void OnHeightChanged(EllipseVisual3D visual3D, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 中心位置改变事件 —— static void OnCenterChanged(EllipseVisual3D visual3D...
        /// <summary>
        /// 中心位置改变事件
        /// </summary>
        private static void OnCenterChanged(EllipseVisual3D visual3D, AvaloniaPropertyChangedEventArgs<Vector3D> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 法向量改变事件 —— static void OnNormalChanged(EllipseVisual3D visual3D...
        /// <summary>
        /// 法向量改变事件
        /// </summary>
        private static void OnNormalChanged(EllipseVisual3D visual3D, AvaloniaPropertyChangedEventArgs<Vector3D> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #endregion
    }
}
