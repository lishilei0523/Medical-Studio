using Avalonia;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Engine.Builders;
using MedicalSharp.Engine.Renderables;
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
        /// 中心位置依赖属性
        /// </summary>
        public static readonly StyledProperty<Vector3D> CenterProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static BoundingBoxVisual3D()
        {
            WidthProperty = AvaloniaProperty.Register<BoundingBoxVisual3D, float>(nameof(Width), 1.0f);
            HeightProperty = AvaloniaProperty.Register<BoundingBoxVisual3D, float>(nameof(Height), 1.0f);
            DepthProperty = AvaloniaProperty.Register<BoundingBoxVisual3D, float>(nameof(Depth), 1.0f);
            CenterProperty = AvaloniaProperty.Register<BoundingBoxVisual3D, Vector3D>(nameof(Center), new Vector3D(0, 0, 0));

            //属性改变事件
            WidthProperty.Changed.AddClassHandler<BoundingBoxVisual3D, float>(OnWidthChanged);
            HeightProperty.Changed.AddClassHandler<BoundingBoxVisual3D, float>(OnHeightChanged);
            DepthProperty.Changed.AddClassHandler<BoundingBoxVisual3D, float>(OnDepthChanged);
            CenterProperty.Changed.AddClassHandler<BoundingBoxVisual3D, Vector3D>(OnCenterChanged);
        }


        /// <summary>
        /// 线框渲染对象
        /// </summary>
        private WireframeRenderable _renderable;

        /// <summary>
        /// 默认构造器
        /// </summary>
        public BoundingBoxVisual3D()
        {
            this._renderable = null;
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

        #region 只读属性 - 线框渲染对象 —— override WireframeRenderable Renderable
        /// <summary>
        /// 只读属性 - 线框渲染对象
        /// </summary>
        public override WireframeRenderable Renderable
        {
            get
            {
                if (this._renderable == null)
                {
                    MeshGeometry strokeMesh = MeshFactory.CreateBoundingBox(this.Width, this.Height, this.Depth, this.Center.ToVector3(), PrimitiveType.Lines);
                    MeshGeometry fillMesh = MeshFactory.CreateBoundingBox(this.Width, this.Height, this.Depth, this.Center.ToVector3(), PrimitiveType.Triangles);
                    this._renderable = new WireframeRenderable(strokeMesh, fillMesh);
                    this._renderable.SetColor(this.Stroke.ToVector4(), this.StrokeThickness, this.Fill.ToVector4());
                }

                return this._renderable;
            }
        }
        #endregion

        #endregion

        #region # 方法

        #region 更新渲染对象 —— void UpdateRenderable()
        /// <summary>
        /// 更新渲染对象
        /// </summary>
        private void UpdateRenderable()
        {
            if (this._renderable != null)
            {
                MeshGeometry strokeMesh = MeshFactory.CreateBoundingBox(this.Width, this.Height, this.Depth, this.Center.ToVector3(), PrimitiveType.Lines);
                MeshGeometry fillMesh = MeshFactory.CreateBoundingBox(this.Width, this.Height, this.Depth, this.Center.ToVector3(), PrimitiveType.Triangles);
                this._renderable.Update(strokeMesh, fillMesh);
            }
        }
        #endregion

        #region 宽度改变事件 —— static void OnWidthChanged(BoundingBoxVisual3D visual3D...
        /// <summary>
        /// 宽度改变事件
        /// </summary>
        private static void OnWidthChanged(BoundingBoxVisual3D visual3D, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 高度改变事件 —— static void OnHeightChanged(BoundingBoxVisual3D visual3D...
        /// <summary>
        /// 高度改变事件
        /// </summary>
        private static void OnHeightChanged(BoundingBoxVisual3D visual3D, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 深度改变事件 —— static void OnDepthChanged(BoundingBoxVisual3D visual3D...
        /// <summary>
        /// 深度改变事件
        /// </summary>
        private static void OnDepthChanged(BoundingBoxVisual3D visual3D, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 中心位置改变事件 —— static void OnCenterChanged(BoundingBoxVisual3D visual3D...
        /// <summary>
        /// 中心位置改变事件
        /// </summary>
        private static void OnCenterChanged(BoundingBoxVisual3D visual3D, AvaloniaPropertyChangedEventArgs<Vector3D> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #endregion
    }
}
