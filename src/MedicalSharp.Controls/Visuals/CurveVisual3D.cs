using Avalonia;
using Avalonia.Collections;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Engine.Algorithms;
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
    /// 曲线3D元素
    /// </summary>
    public class CurveVisual3D : ShapeVisual3D, ILineBasedVisual3D, ITranslatable, IVertexEditable, ICutVolume
    {
        #region # 字段及构造器

        /// <summary>
        /// 控制点列表依赖属性
        /// </summary>
        public static readonly StyledProperty<AvaloniaList<Vector3D>> ControlPositionsProperty;

        /// <summary>
        /// 采样密度依赖属性
        /// </summary>
        public static readonly StyledProperty<int> TessellationProperty;

        /// <summary>
        /// 是否闭合依赖属性
        /// </summary>
        public static readonly StyledProperty<bool> ClosedProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static CurveVisual3D()
        {
            ControlPositionsProperty = AvaloniaProperty.Register<CurveVisual3D, AvaloniaList<Vector3D>>(nameof(ControlPositions), []);
            TessellationProperty = AvaloniaProperty.Register<CurveVisual3D, int>(nameof(Tessellation), 20);
            ClosedProperty = AvaloniaProperty.Register<CurveVisual3D, bool>(nameof(Closed), false);

            //属性改变事件
            ControlPositionsProperty.Changed.AddClassHandler<CurveVisual3D, AvaloniaList<Vector3D>>(OnControlPositionsChanged);
            TessellationProperty.Changed.AddClassHandler<CurveVisual3D, int>(OnTessellationChanged);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public CurveVisual3D()
        {
            this.ControlPositions.CollectionChanged += this.OnControlPositionsItemChanged;
        }

        #endregion

        #region # 属性

        #region 依赖属性 - 控制点列表 —— AvaloniaList<Vector3D> ControlPositions
        /// <summary>
        /// 依赖属性 - 控制点列表
        /// </summary>
        public AvaloniaList<Vector3D> ControlPositions
        {
            get => this.GetValue(ControlPositionsProperty);
            set => this.SetValue(ControlPositionsProperty, value);
        }
        #endregion

        #region 依赖属性 - 采样密度 —— int Tessellation
        /// <summary>
        /// 依赖属性 - 采样密度
        /// </summary>
        public int Tessellation
        {
            get => this.GetValue(TessellationProperty);
            set => this.SetValue(TessellationProperty, value);
        }
        #endregion

        #region 依赖属性 - 是否闭合 —— bool Closed
        /// <summary>
        /// 依赖属性 - 是否闭合
        /// </summary>
        public bool Closed
        {
            get => this.GetValue(ClosedProperty);
            set => this.SetValue(ClosedProperty, value);
        }
        #endregion

        #region 只读属性 - 采样点列表 —— IReadOnlyList<Vector3> SampledPositions
        /// <summary>
        /// 只读属性 - 采样点列表
        /// </summary>
        public IReadOnlyList<Vector3> SampledPositions
        {
            get
            {
                if (this.Renderable is CurveRenderable curveRenderable)
                {
                    return curveRenderable.SampledPositions;
                }

                return [];
            }
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
            if (this.Renderable == null && this.ControlPositions != null)
            {
                IReadOnlyList<Vector3> controlPositions = this.ControlPositions.Select(x => x.ToVector3()).ToList();
                IReadOnlyList<Vector3> sampledPositions = CurveFactory.EvaluateCatmullRom(controlPositions, this.Closed, this.Tessellation);

                CurveRenderable renderable = new CurveRenderable(controlPositions, sampledPositions, this.Closed);
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
            if (this.Renderable != null && this.ControlPositions != null)
            {
                IReadOnlyList<Vector3> controlPositions = this.ControlPositions.Select(x => x.ToVector3()).ToList();
                IReadOnlyList<Vector3> sampledPositions = CurveFactory.EvaluateCatmullRom(controlPositions, this.Closed, this.Tessellation);

                CurveRenderable renderable = (CurveRenderable)this.Renderable;
                renderable.Update(controlPositions, sampledPositions);
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

            if (this.ControlPositions == null || this.ControlPositions.Count == 0)
            {
                return false;
            }

            float minDistance = float.MaxValue;
            int bestIndex = -1;
            Vector3 bestAnchor = Vector3.Zero;

            //遍历所有控制点，找到距离射线最近且在拾取半径内的点
            for (int index = 0; index < this.ControlPositions.Count; index++)
            {
                Vector3 point = this.ControlPositions[index].ToVector3();
                float distance = localRay.CalculateDistanceToPoint(point);

                //拾取半径：固定值，可根据需要调整
                const float pickRadius = 0.05f;

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

            if (this.ControlPositions == null || this.ControlPositions.Count < 2)
            {
                return false;
            }
            if (this.SampledPositions == null || this.SampledPositions.Count == 0)
            {
                return false;
            }

            #endregion

            //找到离命中点最近的采样点
            int nearestSampleIndex = -1;
            float nearestDistance = float.MaxValue;
            for (int i = 0; i < this.SampledPositions.Count; i++)
            {
                Vector3 samplePoint = this.SampledPositions[i];
                float distance = Vector3.Distance(localHitPoint, samplePoint);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestSampleIndex = i;
                }
            }

            if (nearestSampleIndex < 0)
            {
                return false;
            }

            //将采样点索引转换为控制点之间的段索引
            int segmentIndex = nearestSampleIndex / this.Tessellation;

            //插入到 segmentIndex 和 segmentIndex + 1 之间
            int insertIndex = segmentIndex + 1;

            //边界检查
            if (insertIndex < 1 || insertIndex >= this.ControlPositions.Count)
            {
                return false;
            }

            //插入新控制点
            this.ControlPositions.Insert(insertIndex, localHitPoint.ToVector3());

            constraint = new VertexDragConstraint
            {
                VertexIndex = insertIndex,
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

            if (this.ControlPositions == null || this.ControlPositions.Count < 2)
            {
                return false;
            }
            if (this.SampledPositions == null || this.SampledPositions.Count == 0)
            {
                return false;
            }

            #endregion

            this.ControlPositions.RemoveAt(vertexIndex);

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
            if (this.ControlPositions == null || constraint.VertexIndex < 0 || constraint.VertexIndex >= this.ControlPositions.Count)
            {
                return;
            }

            //更新控制点位置
            this.ControlPositions[constraint.VertexIndex] = localHitPoint.ToVector3();
        }
        #endregion

        #region 适用切割体积 —— void ApplyCutVolume(VolumeRenderable renderable...
        /// <summary>
        /// 适用切割体积
        /// </summary>
        /// <param name="renderable">体积渲染对象</param>
        /// <param name="cutMode">切割模式</param>
        /// <param name="markValue">标记值</param>
        public void ApplyCutVolume(VolumeRenderable renderable, CutMode cutMode, byte markValue)
        {
            #region # 验证

            if (!this.Closed)
            {
                return;
            }

            #endregion

            Matrix4 localToWorld = this.Transform.Matrix;
            renderable.ApplyPolygonCut(this.SampledPositions, localToWorld, cutMode, markValue);
            renderable.SyncMarkDataFromGpu();
        }
        #endregion


        //Events

        #region 控制点列表改变事件 —— static void OnControlPositionsChanged(CurveVisual3D visual3D...
        /// <summary>
        /// 控制点列表改变事件
        /// </summary>
        private static void OnControlPositionsChanged(CurveVisual3D visual3D, AvaloniaPropertyChangedEventArgs<AvaloniaList<Vector3D>> eventArgs)
        {
            visual3D.UpdateRenderable();
            if (eventArgs.OldValue.Value != null)
            {
                eventArgs.OldValue.Value.CollectionChanged -= visual3D.OnControlPositionsItemChanged;
            }
            if (eventArgs.NewValue.Value != null)
            {
                eventArgs.NewValue.Value.CollectionChanged += visual3D.OnControlPositionsItemChanged;
            }

        }
        #endregion

        #region 控制点列表元素改变事件 —— void OnControlPositionsItemChanged(object sender...
        /// <summary>
        /// 控制点列表元素改变事件
        /// </summary>
        private void OnControlPositionsItemChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            this.UpdateRenderable();
        }
        #endregion

        #region 采样密度改变事件 —— static void OnTessellationChanged(CurveVisual3D visual3D...
        /// <summary>
        /// 采样密度改变事件
        /// </summary>
        private static void OnTessellationChanged(CurveVisual3D visual3D, AvaloniaPropertyChangedEventArgs<int> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #endregion
    }
}
