using Avalonia;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Primitives.Builders;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Controls.Visuals
{
    /// <summary>
    /// 网格线3D元素
    /// </summary>
    public class GridLinesVisual3D : ShapeVisual3D, IVisual2DIn3D
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

        #region U轴 —— Vector3 UAxis
        /// <summary>
        /// U轴
        /// </summary>
        public Vector3 UAxis { get; private set; }
        #endregion

        #region V轴 —— Vector3 VAxis
        /// <summary>
        /// V轴
        /// </summary>
        public Vector3 VAxis { get; private set; }
        #endregion

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

        #region 只读属性 - 平面上一点 —— Vector3D PointOnPlane
        /// <summary>
        /// 只读属性 - 平面上一点
        /// </summary>
        public Vector3D PointOnPlane
        {
            get => new Vector3D(0, 0, 0);
        }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 克隆 —— override ShapeVisual3D Clone()
        /// <summary>
        /// 克隆
        /// </summary>
        /// <returns>形状副本</returns>
        public override ShapeVisual3D Clone()
        {
            GridLinesVisual3D copy = new GridLinesVisual3D
            {
                Id = this.Id,
                Stroke = this.Stroke,
                StrokeThickness = this.StrokeThickness,
                Fill = this.Fill,
                UAxis = this.UAxis,
                VAxis = this.VAxis,
                Size = this.Size,
                Divisions = this.Divisions,
                Normal = this.Normal
            };

            return copy;
        }
        #endregion

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
                this.BuildBasis();
            }
        }
        #endregion

        #region 更新渲染对象 —— override void UpdateRenderable()
        /// <summary>
        /// 更新渲染对象
        /// </summary>
        internal override void UpdateRenderable()
        {
            if (this.Renderable != null)
            {
                MeshGeometry strokeMesh = MeshFactory.CreateGridLines(this.Size, this.Divisions, this.Normal.ToVector3(), GraphicPrimitiveType.Lines);
                MeshGeometry fillMesh = MeshFactory.CreateGridLines(this.Size, this.Divisions, this.Normal.ToVector3(), GraphicPrimitiveType.Triangles);

                WildframeRenderable renderable = (WildframeRenderable)this.Renderable;
                renderable.Update(strokeMesh, fillMesh);
                renderable.SetWildframe(this.Stroke.ToVector4(), this.StrokeThickness, this.Fill.ToVector4());
                this.BuildBasis();
            }
        }
        #endregion


        //Events

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


        //Private

        #region 构建UV正交基 —— void BuildBasis()
        /// <summary>
        /// 构建UV正交基
        /// </summary>
        private void BuildBasis()
        {
            Vector3 normal = this.Normal.ToVector3();

            //法向量接近Z轴
            if (Math.Abs(Vector3.Dot(normal, Vector3.UnitZ)) > 0.99f)
            {
                this.UAxis = Vector3.UnitX;
                this.VAxis = Vector3.UnitY;
            }
            //法向量接近Y轴
            else if (Math.Abs(Vector3.Dot(normal, Vector3.UnitY)) > 0.99f)
            {
                this.UAxis = Vector3.UnitX;
                this.VAxis = Vector3.UnitZ;
            }
            //法向量接近X轴
            else if (Math.Abs(Vector3.Dot(normal, Vector3.UnitX)) > 0.99f)
            {
                this.UAxis = Vector3.UnitY;
                this.VAxis = Vector3.UnitZ;
            }
            else
            {
                //如果法线被旋转过，重新构造正交基（保证U在XY平面内优先）
                this.UAxis = Vector3.Normalize(Vector3.Cross(Vector3.UnitZ, normal));
                this.VAxis = Vector3.Normalize(Vector3.Cross(normal, this.UAxis));
            }
        }
        #endregion

        #endregion
    }
}
