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
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public GridLinesVisual3D()
        {

        }

        #endregion

        #region # 属性

        #region U轴 —— Vector3D UAxis
        /// <summary>
        /// U轴
        /// </summary>
        public Vector3D UAxis { get; private set; }
        #endregion

        #region V轴 —— Vector3D VAxis
        /// <summary>
        /// V轴
        /// </summary>
        public Vector3D VAxis { get; private set; }
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

        #region 确保渲染对象 —— override void EnsureRenderable()
        /// <summary>
        /// 确保渲染对象
        /// </summary>
        internal override void EnsureRenderable()
        {
            MeshGeometry strokeMesh = MeshFactory.CreateGridLines(this.Size, this.Divisions, this.Normal.ToVector3(), GraphicPrimitiveType.Lines);
            MeshGeometry fillMesh = MeshFactory.CreateGridLines(this.Size, this.Divisions, this.Normal.ToVector3(), GraphicPrimitiveType.Triangles);
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

            this.BuildBasis();
        }
        #endregion

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

        #region 复制 —— override void Copy(ShapeVisual3D shapeVisual3D)
        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="shapeVisual3D">形状</param>
        public override void Copy(ShapeVisual3D shapeVisual3D)
        {
            if (shapeVisual3D is GridLinesVisual3D shape)
            {
                this.Stroke = shape.Stroke;
                this.StrokeThickness = shape.StrokeThickness;
                this.Fill = shape.Fill;
                this.UAxis = shape.UAxis;
                this.VAxis = shape.VAxis;
                this.Size = shape.Size;
                this.Divisions = shape.Divisions;
                this.Normal = shape.Normal;
                this.Transform.SetMatrix(shape.Transform.Matrix);
            }
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
                this.UAxis = Vector3.UnitX.ToVector3();
                this.VAxis = Vector3.UnitY.ToVector3();
            }
            //法向量接近Y轴
            else if (Math.Abs(Vector3.Dot(normal, Vector3.UnitY)) > 0.99f)
            {
                this.UAxis = Vector3.UnitX.ToVector3();
                this.VAxis = Vector3.UnitZ.ToVector3();
            }
            //法向量接近X轴
            else if (Math.Abs(Vector3.Dot(normal, Vector3.UnitX)) > 0.99f)
            {
                this.UAxis = Vector3.UnitY.ToVector3();
                this.VAxis = Vector3.UnitZ.ToVector3();
            }
            else
            {
                //如果法线被旋转过，重新构造正交基（保证U在XY平面内优先）
                this.UAxis = Vector3.Normalize(Vector3.Cross(Vector3.UnitZ, normal)).ToVector3();
                this.VAxis = Vector3.Normalize(Vector3.Cross(normal, this.UAxis.ToVector3())).ToVector3();
            }
        }
        #endregion

        #endregion
    }
}
