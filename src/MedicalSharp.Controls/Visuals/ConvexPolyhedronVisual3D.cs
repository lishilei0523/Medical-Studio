using Avalonia;
using Avalonia.Collections;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Primitives.Builders;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace MedicalSharp.Controls.Visuals
{
    /// <summary>
    /// 凸多面体3D元素
    /// </summary>
    public class ConvexPolyhedronVisual3D : ShapeVisual3D, ITranslatable, IRotatable, IVertexEditable
    {
        #region # 字段及构造器

        /// <summary>
        /// 位置列表依赖属性
        /// </summary>
        public static readonly StyledProperty<AvaloniaList<Vector3D>> PositionsProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static ConvexPolyhedronVisual3D()
        {
            PositionsProperty = AvaloniaProperty.Register<ConvexPolyhedronVisual3D, AvaloniaList<Vector3D>>(nameof(Positions), []);

            //属性改变事件
            PositionsProperty.Changed.AddClassHandler<ConvexPolyhedronVisual3D, AvaloniaList<Vector3D>>(OnControlPositionsChanged);
        }

        /// <summary>
        /// 默认构造器
        /// </summary>
        public ConvexPolyhedronVisual3D()
        {
            this.Positions.CollectionChanged += this.OnPositionsItemChanged;
        }

        #endregion

        #region 属性

        #region 依赖属性 - 位置列表 —— AvaloniaList<Vector3D> Positions
        /// <summary>
        /// 依赖属性 - 位置列表
        /// </summary>
        public AvaloniaList<Vector3D> Positions
        {
            get => this.GetValue(PositionsProperty);
            set => this.SetValue(PositionsProperty, value);
        }
        #endregion

        #endregion

        #region 私有方法

        #region 确保渲染对象 —— override void EnsureRenderable()
        /// <summary>
        /// 确保渲染对象
        /// </summary>
        internal override void EnsureRenderable()
        {
            if (this.Renderable == null && this.Positions != null)
            {
                IReadOnlyList<Vector3> positions = this.Positions.Select(x => x.ToVector3()).ToList();
                MeshGeometry strokeMesh = MeshFactory.CreateConvexPolyhedron(positions, GraphicPrimitiveType.Lines);
                MeshGeometry fillMesh = MeshFactory.CreateConvexPolyhedron(positions, GraphicPrimitiveType.Triangles);

                WildframeRenderable renderable = new WildframeRenderable(strokeMesh, fillMesh, true);
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
            if (this.Renderable != null && this.Positions != null)
            {
                IReadOnlyList<Vector3> positions = this.Positions.Select(x => x.ToVector3()).ToList();
                MeshGeometry strokeMesh = MeshFactory.CreateConvexPolyhedron(positions, GraphicPrimitiveType.Lines);
                MeshGeometry fillMesh = MeshFactory.CreateConvexPolyhedron(positions, GraphicPrimitiveType.Triangles);

                WildframeRenderable renderable = (WildframeRenderable)this.Renderable;
                renderable.Update(strokeMesh, fillMesh);
                renderable.SetWildframe(this.Stroke.ToVector4(), this.StrokeThickness, this.Fill.ToVector4());
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
            if (this.Positions == null || this.Positions.Count == 0)
            {
                return false;
            }

            float minDistance = float.MaxValue;
            int bestIndex = -1;
            Vector3 bestAnchor = Vector3.Zero;

            //遍历所有控制点，找到距离射线最近且在拾取半径内的点
            for (int index = 0; index < this.Positions.Count; index++)
            {
                Vector3 point = this.Positions[index].ToVector3();
                float distance = localRay.CalculateDistanceToPoint(point);

                //拾取半径：固定值，可根据需要调整
                const float pickRadius = 0.3f;
                if (distance < pickRadius && distance < minDistance)
                {
                    minDistance = distance;
                    bestIndex = index;
                    bestAnchor = point;
                }
            }

            if (bestIndex >= 0)
            {
                constraint = new VertexDragConstraint
                {
                    VertexIndex = bestIndex,
                    Anchor = bestAnchor,
                    Normal = localLookDirection
                };

                return true;
            }

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
            if (this.Positions == null || constraint.VertexIndex < 0 || constraint.VertexIndex >= this.Positions.Count)
            {
                return;
            }

            //更新控制点位置
            this.Positions[constraint.VertexIndex] = localHitPoint.ToVector3();
        }
        #endregion

        #region 位置列表改变事件 —— static void OnControlPositionsChanged(ConvexPolyhedronVisual3D visual3D...
        /// <summary>
        /// 位置列表改变事件
        /// </summary>
        private static void OnControlPositionsChanged(ConvexPolyhedronVisual3D visual3D, AvaloniaPropertyChangedEventArgs<AvaloniaList<Vector3D>> eventArgs)
        {
            visual3D.UpdateRenderable();
            if (eventArgs.OldValue.Value != null)
            {
                eventArgs.OldValue.Value.CollectionChanged -= visual3D.OnPositionsItemChanged;
            }
            if (eventArgs.NewValue.Value != null)
            {
                eventArgs.NewValue.Value.CollectionChanged += visual3D.OnPositionsItemChanged;
            }
        }
        #endregion

        #region 位置列表元素改变事件 —— void OnPositionsItemChanged(object sender...
        /// <summary>
        /// 位置列表元素改变事件
        /// </summary>
        private void OnPositionsItemChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            this.UpdateRenderable();
        }
        #endregion

        #endregion
    }
}
