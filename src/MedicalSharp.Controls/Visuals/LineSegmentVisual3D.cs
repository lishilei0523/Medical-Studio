using Avalonia;
using MedicalSharp.Controls.Extensions;
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
    public class LineSegmentVisual3D : ShapeVisual3D, IVertexEditable
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

        #region 更新渲染对象 —— void UpdateRenderable()
        /// <summary>
        /// 更新渲染对象
        /// </summary>
        private void UpdateRenderable()
        {
            if (this.Renderable != null)
            {
                LineSegmentRenderable renderable = (LineSegmentRenderable)this.Renderable;
                renderable.Update(this.StartPoint.ToVector3(), this.EndPoint.ToVector3());
                renderable.SetStroke(this.Stroke.ToVector4(), this.StrokeThickness);
            }
        }
        #endregion

        #region 尝试获取顶点拖拽 —— bool TryGetVertexDrag(Ray localRay, out VertexDragConstraint constraint)
        /// <summary>
        /// 尝试获取顶点拖拽
        /// </summary>
        /// <param name="localRay">局部空间射线</param>
        /// <param name="constraint">拖拽约束</param>
        /// <returns>是否点中了顶点</returns>
        public bool TryGetVertexDrag(Ray localRay, out VertexDragConstraint constraint)
        {
            constraint = default;

            Vector3 start = this.StartPoint.ToVector3();
            Vector3 end = this.EndPoint.ToVector3();
            Vector3 cameraDir = -localRay.Direction;

            float distToStart = localRay.CalculateDistanceToPoint(start);
            float distToEnd = localRay.CalculateDistanceToPoint(end);

            const float pickRadius = 0.3f;

            // 优先选择距离更近的顶点
            if (distToStart < pickRadius && distToStart <= distToEnd)
            {
                constraint = new VertexDragConstraint
                {
                    VertexIndex = 0,
                    AnchorPoint = start,
                    PlaneNormal = cameraDir
                };
                return true;
            }
            else if (distToEnd < pickRadius)
            {
                constraint = new VertexDragConstraint
                {
                    VertexIndex = 1,
                    AnchorPoint = end,
                    PlaneNormal = cameraDir
                };
                return true;
            }

            return false;
        }
        #endregion

        #region 移动顶点 —— void MoveVertex(VertexDragConstraint constraint, Vector3 localHitPoint)
        /// <summary>
        /// 移动顶点
        /// </summary>
        /// <param name="constraint">拖拽约束</param>
        /// <param name="localHitPoint">局部空间命中点</param>
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
