using Avalonia;
using Avalonia.Collections;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Engine.Renderables;
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
    /// 点云3D元素
    /// </summary>
    public class PointCloudVisual3D : ShapeVisual3D, ITranslatable, IVertexEditable
    {
        #region # 字段及构造器

        /// <summary>
        /// 位置列表依赖属性
        /// </summary>
        public static readonly StyledProperty<AvaloniaList<Vector3D>> PositionsProperty;

        /// <summary>
        /// 点尺寸依赖属性
        /// </summary>
        public static readonly StyledProperty<float> PointSizeProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static PointCloudVisual3D()
        {
            PositionsProperty = AvaloniaProperty.Register<PointCloudVisual3D, AvaloniaList<Vector3D>>(nameof(Positions), []);
            PointSizeProperty = AvaloniaProperty.Register<PointCloudVisual3D, float>(nameof(PointSize), 2.0f);

            //属性改变事件
            PositionsProperty.Changed.AddClassHandler<PointCloudVisual3D, AvaloniaList<Vector3D>>(OnPositionsChanged);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public PointCloudVisual3D()
        {
            this.Positions.CollectionChanged += this.OnPositionsItemChanged;
        }

        #endregion

        #region # 属性

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

        #region 依赖属性 - 点尺寸 —— float PointSize
        /// <summary>
        /// 依赖属性 - 点尺寸
        /// </summary>
        public float PointSize
        {
            get => this.GetValue(PointSizeProperty);
            set => this.SetValue(PointSizeProperty, value);
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
            PointCloudVisual3D copy = new PointCloudVisual3D
            {
                Id = this.Id,
                Stroke = this.Stroke,
                StrokeThickness = this.StrokeThickness,
                Fill = this.Fill,
                Positions = this.Positions,
                PointSize = this.PointSize
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
            if (this.Renderable == null && this.Positions != null)
            {
                IEnumerable<Vector3> positions = this.Positions.Select(x => x.ToVector3());
                PointCloudRenderable renderable = new PointCloudRenderable([.. positions]);
                renderable.SetFill(this.Fill.ToVector4(), this.PointSize);

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
            if (this.Renderable != null && this.Positions != null)
            {
                IEnumerable<Vector3> positions = this.Positions.Select(x => x.ToVector3());
                PointCloudRenderable renderable = (PointCloudRenderable)this.Renderable;
                renderable.Update([.. positions]);
                renderable.SetFill(this.Fill.ToVector4(), this.PointSize);
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

            //遍历所有点，找到距离射线最近且在拾取半径内的点
            for (int index = 0; index < this.Positions.Count; index++)
            {
                Vector3 point = this.Positions[index].ToVector3();
                float distance = localRay.CalculateDistanceToPoint(point);

                //拾取半径：基于点尺寸的屏幕空间映射（简化：局部空间固定值）
                float pickRadius = this.PointSize * 0.1f;
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

            #region # 验证

            if (this.Positions == null)
            {
                return false;
            }

            #endregion

            //直接添加新点
            int newIndex = this.Positions.Count;
            this.Positions.Add(localHitPoint.ToVector3());

            constraint = new VertexDragConstraint
            {
                VertexIndex = newIndex,
                Anchor = localHitPoint,
                Normal = localLookDirection
            };

            return true;
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
            #region # 验证

            if (this.Positions == null || this.Positions.Count == 0)
            {
                return false;
            }

            #endregion

            this.Positions.RemoveAt(vertexIndex);

            return true;
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

            this.Positions[constraint.VertexIndex] = localHitPoint.ToVector3();
        }
        #endregion


        //Events

        #region 位置列表改变事件 —— static void OnPositionsChanged(PointCloudVisual3D visual3D...
        /// <summary>
        /// 位置列表改变事件
        /// </summary>
        private static void OnPositionsChanged(PointCloudVisual3D visual3D, AvaloniaPropertyChangedEventArgs<AvaloniaList<Vector3D>> eventArgs)
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
