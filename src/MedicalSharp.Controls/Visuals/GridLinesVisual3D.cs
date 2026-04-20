using Avalonia;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Primitives.Builders;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Models;

namespace MedicalSharp.Controls.Visuals
{
    /// <summary>
    /// 网格线3D元素
    /// </summary>
    public class GridLinesVisual3D : ShapeVisual3D
    {
        #region # 字段及构造器

        /// <summary>
        /// 尺寸依赖属性
        /// </summary>
        public static readonly StyledProperty<float> SizeProperty;

        /// <summary>
        /// 分隔数量依赖属性
        /// </summary>
        public static readonly StyledProperty<int> DivisionsProperty;

        /// <summary>
        /// 法向量依赖属性
        /// </summary>
        public static readonly StyledProperty<Vector3D> NormalProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static GridLinesVisual3D()
        {
            SizeProperty = AvaloniaProperty.Register<GridLinesVisual3D, float>(nameof(Size), 10.0f);
            DivisionsProperty = AvaloniaProperty.Register<GridLinesVisual3D, int>(nameof(Divisions), 10);
            NormalProperty = AvaloniaProperty.Register<GridLinesVisual3D, Vector3D>(nameof(Normal), new Vector3D(0, 0, 1));

            //属性改变事件
            SizeProperty.Changed.AddClassHandler<GridLinesVisual3D, float>(OnSizeChanged);
            DivisionsProperty.Changed.AddClassHandler<GridLinesVisual3D, int>(OnDivisionsChanged);
            NormalProperty.Changed.AddClassHandler<GridLinesVisual3D, Vector3D>(OnNormalChanged);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public GridLinesVisual3D()
        {

        }

        #endregion

        #region # 属性

        #region 依赖属性 - 尺寸 —— float Size
        /// <summary>
        /// 依赖属性 - 尺寸
        /// </summary>
        public float Size
        {
            get => this.GetValue(SizeProperty);
            set => this.SetValue(SizeProperty, value);
        }
        #endregion

        #region 依赖属性 - 分隔数量 —— int Divisions
        /// <summary>
        /// 依赖属性 - 分隔数量
        /// </summary>
        public int Divisions
        {
            get => this.GetValue(DivisionsProperty);
            set => this.SetValue(DivisionsProperty, value);
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
                MeshGeometry strokeMesh = MeshFactory.CreateGridLines(this.Size, this.Divisions, this.Normal.ToVector3(), GraphicPrimitiveType.Lines);
                MeshGeometry fillMesh = MeshFactory.CreateGridLines(this.Size, this.Divisions, this.Normal.ToVector3(), GraphicPrimitiveType.Triangles);

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
                MeshGeometry strokeMesh = MeshFactory.CreateGridLines(this.Size, this.Divisions, this.Normal.ToVector3(), GraphicPrimitiveType.Lines);
                MeshGeometry fillMesh = MeshFactory.CreateGridLines(this.Size, this.Divisions, this.Normal.ToVector3(), GraphicPrimitiveType.Triangles);

                WildframeRenderable renderable = (WildframeRenderable)this.Renderable;
                renderable.Update(strokeMesh, fillMesh);
                renderable.SetWildframe(this.Stroke.ToVector4(), this.StrokeThickness, this.Fill.ToVector4());
            }
        }
        #endregion

        #region 尺寸改变事件 —— static void OnSizeChanged(GridLinesVisual3D visual3D...
        /// <summary>
        /// 尺寸改变事件
        /// </summary>
        private static void OnSizeChanged(GridLinesVisual3D visual3D, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 分隔数量改变事件 —— static void OnDivisionsChanged(GridLinesVisual3D visual3D...
        /// <summary>
        /// 分隔数量改变事件
        /// </summary>
        private static void OnDivisionsChanged(GridLinesVisual3D visual3D, AvaloniaPropertyChangedEventArgs<int> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 法向量改变事件 —— static void OnNormalChanged(GridLinesVisual3D visual3D...
        /// <summary>
        /// 法向量改变事件
        /// </summary>
        private static void OnNormalChanged(GridLinesVisual3D visual3D, AvaloniaPropertyChangedEventArgs<Vector3D> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #endregion
    }
}
