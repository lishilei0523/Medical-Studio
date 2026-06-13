using Avalonia;
using Avalonia.Collections;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Engine.Algorithms;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;

namespace MedicalSharp.Controls.Visual3Ds
{
    /// <summary>
    /// 折线3D元素
    /// </summary>
    public class PolylineVisual3D : ShapeVisual3D, ILineBasedVisual3D, ITranslatable3D, IVertexEditable, IFixable, ICutVolume, IAnalyseVolume2D
    {
        #region # 字段及构造器

        /// <summary>
        /// 位置列表依赖属性
        /// </summary>
        public static readonly StyledProperty<AvaloniaList<Vector3D>> PositionsProperty;

        /// <summary>
        /// 是否闭合依赖属性
        /// </summary>
        public static readonly StyledProperty<bool> ClosedProperty;

        /// <summary>
        /// 是否固定依赖属性
        /// </summary>
        public static readonly StyledProperty<bool> FixedProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static PolylineVisual3D()
        {
            PositionsProperty = AvaloniaProperty.Register<PolylineVisual3D, AvaloniaList<Vector3D>>(nameof(Positions), []);
            ClosedProperty = AvaloniaProperty.Register<PolylineVisual3D, bool>(nameof(Closed), false);
            FixedProperty = AvaloniaProperty.Register<PolylineVisual3D, bool>(nameof(Fixed), false);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public PolylineVisual3D()
        {

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

        #region 依赖属性 - 是否固定 —— bool Fixed
        /// <summary>
        /// 依赖属性 - 是否固定
        /// </summary>
        public bool Fixed
        {
            get => this.GetValue(FixedProperty);
            set => this.SetValue(FixedProperty, value);
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
            #region # 验证

            if (this.Positions == null || !this.Positions.Any())
            {
                return;
            }

            #endregion

            IReadOnlyList<Vector3> positions = this.Positions.Select(x => x.ToVector3()).ToList();
            if (this.Renderable == null)
            {
                PolylineRenderable renderable = new PolylineRenderable(positions, this.Closed, !this.Fixed);
                renderable.SetStroke(this.Stroke.ToVector4(), this.StrokeThickness, this.Fill.ToVector4());
                this.Renderable = renderable;
            }
            else
            {
                PolylineRenderable renderable = (PolylineRenderable)this.Renderable;
                renderable.Update(positions);
                renderable.SetStroke(this.Stroke.ToVector4(), this.StrokeThickness, this.Fill.ToVector4());
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

            #region # 验证

            if (this.Positions == null || !this.Positions.Any())
            {
                return false;
            }

            #endregion

            float minDistance = float.MaxValue;
            int bestIndex = -1;
            Vector3 bestAnchor = Vector3.Zero;

            //遍历所有顶点，找到距离射线最近且在拾取半径内的点
            for (int i = 0; i < this.Positions.Count; i++)
            {
                Vector3 point = this.Positions[i].ToVector3();
                float distance = localRay.CalculateDistanceToPoint(point);

                //拾取半径：固定值，可根据需要调整
                const float pickRadius = 0.05f;
                if (distance < pickRadius && distance < minDistance)
                {
                    minDistance = distance;
                    bestIndex = i;
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

            if (this.Positions == null || !this.Positions.Any())
            {
                return false;
            }

            #endregion

            //找到离命中点最近的顶点
            int nearestIndex = -1;
            float nearestDistance = float.MaxValue;
            for (int index = 0; index < this.Positions.Count; index++)
            {
                Vector3 point = this.Positions[index].ToVector3();
                float distance = Vector3.Distance(localHitPoint, point);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestIndex = index;
                }
            }

            if (nearestIndex < 0)
            {
                return false;
            }

            //计算新点插入位置（最近点之后）
            int newIndex = nearestIndex + 1;

            //插入新顶点
            this.Positions.Insert(newIndex, localHitPoint.ToVector3());

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

            if (this.Positions == null || !this.Positions.Any())
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

        #region 适用切割体积 —— void ApplyCutVolume(VolumeData volumeData...
        /// <summary>
        /// 适用切割体积
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="markTexture">标记纹理</param>
        /// <param name="cutMode">切割模式</param>
        /// <param name="markValue">标记值</param>
        public void ApplyCutVolume(VolumeData volumeData, Texture3D markTexture, CutMode cutMode, byte markValue)
        {
            #region # 验证

            if (!this.Closed)
            {
                return;
            }

            #endregion

            Matrix4 localToWorld = this.Transform.Matrix;
            Vector3[] vertices = this.Positions.Select(position => position.ToVector3()).ToArray();
            volumeData.ApplyPolygonCut(markTexture, vertices, localToWorld, cutMode, markValue);
        }
        #endregion

        #region 适用统计体积 —— StatisticResult ApplyAnalyseVolume(MPRViewport viewport...
        /// <summary>
        /// 适用统计体积
        /// </summary>
        /// <param name="viewport">MPR渲染视口</param>
        /// <param name="markValue">标记值</param>
        /// <returns>统计结果</returns>
        public StatisticResult ApplyAnalyseVolume(MPRViewport viewport, byte? markValue)
        {
            #region # 验证

            if (viewport.VolumeData == null)
            {
                return default;
            }
            if (viewport.MPRRenderer == null)
            {
                return default;
            }
            if (viewport.MPRCamera == null)
            {
                return default;
            }
            if (viewport.Plane == null)
            {
                return default;
            }
            if (!this.Closed)
            {
                return default;
            }
            if (this.Positions == null || this.Positions.Count < 3)
            {
                return default;
            }

            #endregion

            //获取所有顶点世界坐标
            Vector3[] worldVertices = this.Positions.Select(pos => Vector3.TransformPosition(pos.ToVector3(), this.Transform.Matrix)).ToArray();

            //投影到屏幕坐标
            Vector2[] screenVertices = new Vector2[worldVertices.Length];
            for (int index = 0; index < worldVertices.Length; index++)
            {
                screenVertices[index] = viewport.Project(worldVertices[index]);
            }

            int viewportWidth = viewport.ViewportSize.Width;
            int viewportHeight = viewport.ViewportSize.Height;
            byte[] layerPixels = viewport.MPRRenderer.RenderStatistic(viewportWidth, viewportHeight, viewport.GlContextHandle);
            StatisticResult result = viewport.VolumeData.ApplyPolygonAnalyse(screenVertices, viewportWidth, viewportHeight, viewport.MPRCamera.ZoomFactor, layerPixels, markValue);

            return result;
        }
        #endregion

        #endregion
    }
}
