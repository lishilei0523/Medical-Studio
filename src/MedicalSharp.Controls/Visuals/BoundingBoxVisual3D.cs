using Avalonia;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Engine.Builders;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Engine.ValueTypes;
using OpenTK.Graphics.OpenGL4;

namespace MedicalSharp.Controls.Visuals
{
    /// <summary>
    /// 边界立方体3D元素
    /// </summary>
    public class BoundingBoxVisual3D : BoundingVisual3D
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
        /// 深度依赖属性
        /// </summary>
        public static readonly StyledProperty<float> DepthProperty;

        /// <summary>
        /// 中心点位置依赖属性
        /// </summary>
        public static readonly StyledProperty<Vector3D> CenterProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static BoundingBoxVisual3D()
        {
            WidthProperty = AvaloniaProperty.Register<OpenTKViewport, float>(nameof(Width), 1.0f);
            HeightProperty = AvaloniaProperty.Register<OpenTKViewport, float>(nameof(Height), 1.0f);
            DepthProperty = AvaloniaProperty.Register<OpenTKViewport, float>(nameof(Depth), 1.0f);
            CenterProperty = AvaloniaProperty.Register<OpenTKViewport, Vector3D>(nameof(Center), new Vector3D(0, 0, 0));
        }

        #endregion

        #region # 属性

        #region 线框渲染对象 —— abstract WireframeRenderable Renderable
        /// <summary>
        /// 线框渲染对象
        /// </summary>
        public override WireframeRenderable Renderable
        {
            get
            {
                if (this._renderable == null)
                {
                    MeshGeometry strokeMesh = MeshFactory.CreateBoundingBox(this.Width, this.Height, this.Depth, this.Center.ToVector3(), PrimitiveType.Lines);
                    MeshGeometry fillMesh = MeshFactory.CreateBoundingBox(this.Width, this.Height, this.Depth, this.Center.ToVector3(), PrimitiveType.Triangles);
                    VertexBuffer strokeBuffer = new VertexBuffer(strokeMesh);
                    VertexBuffer fillBuffer = new VertexBuffer(fillMesh);
                    strokeBuffer.Setup();
                    fillBuffer.Setup();

                    this._renderable = new WireframeRenderable(strokeBuffer, fillBuffer);
                    this._renderable.SetColor(this.Stroke.ToVector4(), this.StrokeThickness, this.Fill.ToVector4());
                }

                return this._renderable;
            }
        }
        #endregion

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

        #region 依赖属性 - 深度 —— float Depth
        /// <summary>
        /// 依赖属性 - 深度
        /// </summary>
        public float Depth
        {
            get => this.GetValue(DepthProperty);
            set => this.SetValue(DepthProperty, value);
        }
        #endregion

        #region 依赖属性 - 中心点位置 —— Vector3D Center
        /// <summary>
        /// 依赖属性 - 中心点位置
        /// </summary>
        public Vector3D Center
        {
            get => this.GetValue(CenterProperty);
            set => this.SetValue(CenterProperty, value);
        }
        #endregion

        #endregion
    }
}
