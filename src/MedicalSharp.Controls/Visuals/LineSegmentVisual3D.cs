using Avalonia;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;

namespace MedicalSharp.Controls.Visuals
{
    /// <summary>
    /// 线段3D元素
    /// </summary>
    public class LineSegmentVisual3D : ShapeVisual3D, ILineBasedVisual3D, ITranslatable, IVertexEditable
    {
        #region # 字段及构造器

        /// <summary>
        /// 起始点依赖属性
        /// </summary>
        public static readonly StyledProperty<Vector3D> StartPointProperty;

        /// <summary>
        /// 终止点依赖属性
        /// </summary>
        public static readonly StyledProperty<Vector3D> EndPointProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static LineSegmentVisual3D()
        {
            StartPointProperty = AvaloniaProperty.Register<LineSegmentVisual3D, Vector3D>(nameof(StartPoint));
            EndPointProperty = AvaloniaProperty.Register<LineSegmentVisual3D, Vector3D>(nameof(EndPoint));

            //属性改变事件
            StartPointProperty.Changed.AddClassHandler<LineSegmentVisual3D, Vector3D>(OnStartPointChanged);
            EndPointProperty.Changed.AddClassHandler<LineSegmentVisual3D, Vector3D>(OnEndPointChanged);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public LineSegmentVisual3D()
        {

        }

        #endregion

        #region # 属性

        #region 依赖属性 - 起始点 —— Vector3D StartPoint
        /// <summary>
        /// 依赖属性 - 起始点
        /// </summary>
        public Vector3D StartPoint
        {
            get => this.GetValue(StartPointProperty);
            set => this.SetValue(StartPointProperty, value);
        }
        #endregion

        #region 依赖属性 - 终止点 —— Vector3D EndPoint
        /// <summary>
        /// 依赖属性 - 终止点
        /// </summary>
        public Vector3D EndPoint
        {
            get => this.GetValue(EndPointProperty);
            set => this.SetValue(EndPointProperty, value);
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
            LineSegmentVisual3D copy = new LineSegmentVisual3D
            {
                Id = this.Id,
                Stroke = this.Stroke,
                StrokeThickness = this.StrokeThickness,
                Fill = this.Fill,
                StartPoint = this.StartPoint,
                EndPoint = this.EndPoint
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
            if (shapeVisual3D is LineSegmentVisual3D shape)
            {
                this.Stroke = shape.Stroke;
                this.StrokeThickness = shape.StrokeThickness;
                this.Fill = shape.Fill;
                this.StartPoint = shape.StartPoint;
                this.EndPoint = shape.EndPoint;
                this.Transform.SetMatrix(shape.Transform.Matrix);
            }
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
                LineSegmentRenderable renderable = new LineSegmentRenderable(this.StartPoint.ToVector3(), this.EndPoint.ToVector3());
                renderable.SetStroke(this.Stroke.ToVector4(), this.StrokeThickness);

                this.Renderable = renderable;
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
                LineSegmentRenderable renderable = (LineSegmentRenderable)this.Renderable;
                renderable.Update(this.StartPoint.ToVector3(), this.EndPoint.ToVector3());
                renderable.SetStroke(this.Stroke.ToVector4(), this.StrokeThickness);
            }
        }
        #endregion

        #region 尝试获取顶点拖拽约束 —— bool TryGetVertexDrag(Ray localRay, Vector3 localLookDirection...
        /// <summary>
        /// 尝试获取顶点拖拽约束
        /// </summary>
        /// <param name="localRay">射线（局部空间）</param>
        /// <param name="localLookDirection">视角方向（局部空间）</param>
        /// <param name="constraint">拖拽约束</param>
        /// <returns>是否命中顶点</returns>
        public bool TryGetVertexDrag(Ray localRay, Vector3 localLookDirection, out VertexDragConstraint constraint)
        {
            constraint = default;

            Vector3 start = this.StartPoint.ToVector3();
            Vector3 end = this.EndPoint.ToVector3();
            float distanceToStart = localRay.CalculateDistanceToPoint(start);
            float distanceToEnd = localRay.CalculateDistanceToPoint(end);
            const float pickRadius = 0.05f;

            //优先选择距离更近的顶点
            if (distanceToStart < pickRadius && distanceToStart <= distanceToEnd)
            {
                constraint = new VertexDragConstraint
                {
                    VertexIndex = 0,
                    Anchor = start,
                    Normal = localLookDirection
                };

                return true;
            }
            if (distanceToEnd < pickRadius)
            {
                constraint = new VertexDragConstraint
                {
                    VertexIndex = 1,
                    Anchor = end,
                    Normal = localLookDirection
                };

                return true;
            }

            return false;
        }
        #endregion

        #region 尝试获取插入顶点拖拽约束 —— bool TryInsertVertex(Ray localRay, Vector3 localLookDirection...
        /// <summary>
        /// 尝试获取插入顶点拖拽约束
        /// </summary>
        /// <param name="localRay">射线（局部空间）</param>
        /// <param name="localLookDirection">视角方向（局部空间）</param>
        /// <param name="localHitPoint">命中点（局部空间）</param>
        /// <param name="constraint">拖拽约束</param>
        /// <returns>是否插入顶点</returns>
        public bool TryInsertVertex(Ray localRay, Vector3 localLookDirection, Vector3 localHitPoint, out VertexDragConstraint constraint)
        {
            constraint = default;
            return false;
        }
        #endregion

        #region 尝试删除顶点 —— bool TryRemoveVertex(int vertexIndex)
        /// <summary>
        /// 尝试删除顶点
        /// </summary>
        /// <param name="vertexIndex">顶点索引</param>
        /// <returns>是否删除成功</returns>
        public bool TryRemoveVertex(int vertexIndex)
        {
            return false;
        }
        #endregion

        #region 移动命中顶点 —— void MoveVertex(VertexDragConstraint constraint, Vector3 localHitPoint)
        /// <summary>
        /// 移动命中顶点
        /// </summary>
        /// <param name="constraint">拖拽约束</param>
        /// <param name="localHitPoint">命中点（局部空间）</param>
        public void MoveVertex(VertexDragConstraint constraint, Vector3 localHitPoint)
        {
            if (constraint.VertexIndex == 0)
            {
                this.StartPoint = localHitPoint.ToVector3();
            }
            else if (constraint.VertexIndex == 1)
            {
                this.EndPoint = localHitPoint.ToVector3();
            }
        }
        #endregion


        //Events

        #region 起始点改变事件 —— static void OnStartPointChanged(LineSegmentVisual3D visual3D...
        /// <summary>
        /// 起始点改变事件
        /// </summary>
        private static void OnStartPointChanged(LineSegmentVisual3D visual3D, AvaloniaPropertyChangedEventArgs<Vector3D> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 终止点改变事件 —— static void OnEndPointChanged(LineSegmentVisual3D visual3D...
        /// <summary>
        /// 终止点改变事件
        /// </summary>
        private static void OnEndPointChanged(LineSegmentVisual3D visual3D, AvaloniaPropertyChangedEventArgs<Vector3D> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #endregion
    }
}
