using Avalonia;
using Avalonia.Collections;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Engine.Algorithms;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Algorithms;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalSharp.Controls.Visual3Ds
{
    /// <summary>
    /// 曲线3D元素
    /// </summary>
    public class CurveVisual3D : ShapeVisual3D, ILineBasedVisual3D, ITranslatable3D, IVertexEditable, IHasPerimeter, IHasSurfaceArea, ICutVolume, IAnalyseVolume2D
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
        /// 等弧长重采样点数依赖属性
        /// </summary>
        public static readonly StyledProperty<int> ResampleCountProperty;

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
            ResampleCountProperty = AvaloniaProperty.Register<CurveVisual3D, int>(nameof(ResampleCount), 200);
            ClosedProperty = AvaloniaProperty.Register<CurveVisual3D, bool>(nameof(Closed), false);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public CurveVisual3D()
        {

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

        #region 依赖属性 - 等弧长重采样点 —— int ResampleCount
        /// <summary>
        /// 依赖属性 - 等弧长重采样点
        /// </summary>
        public int ResampleCount
        {
            get => this.GetValue(ResampleCountProperty);
            set => this.SetValue(ResampleCountProperty, value);
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

        #region 只读属性 - 曲线 —— Curve Curve
        /// <summary>
        /// 只读属性 - 曲线
        /// </summary>
        public Curve Curve
        {
            get
            {
                if (this.Renderable is CurveRenderable curveRenderable)
                {
                    return curveRenderable.Curve;
                }

                return null;
            }
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

            if (this.ControlPositions == null || !this.ControlPositions.Any())
            {
                return;
            }

            #endregion

            IReadOnlyList<Vector3> controlPositions = this.ControlPositions.Select(x => x.ToVector3()).ToList();
            Curve curve = this.Curve != null && this.Curve.ControlPoints.SequenceEqual(controlPositions)
                ? this.Curve
                : new Curve(controlPositions, this.Tessellation, this.ResampleCount, this.Closed);
            if (this.Renderable == null)
            {
                CurveRenderable renderable = new CurveRenderable(curve);
                renderable.SetStroke(this.Stroke.ToVector4(), this.StrokeThickness, this.Fill.ToVector4());
                this.Renderable = renderable;
            }
            else
            {
                CurveRenderable renderable = (CurveRenderable)this.Renderable;
                renderable.Update(curve);
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

            if (this.ControlPositions == null || !this.ControlPositions.Any())
            {
                return false;
            }

            #endregion

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
        /// <remarks>
        /// 在曲线上插入新控制点的流程：
        ///     1、遍历所有连续控制点对构成的线段；
        ///     2、计算命中点到每条线段的最短距离；
        ///     3、找到距离最短的线段，将新控制点插入到该线段的两端控制点之间；
        ///     4、返回拖拽约束，新插入的点可立即被拖拽；
        /// </remarks>
        public bool TryInsertVertex(Ray localRay, Vector3 localLookDirection, Vector3 localHitPoint, out VertexDragConstraint constraint)
        {
            constraint = default;

            #region # 验证

            if (this.ControlPositions == null || this.ControlPositions.Count < 2)
            {
                return false;
            }
            if (this.Curve == null || this.Curve.ResampledPoints == null || !this.Curve.ResampledPoints.Any())
            {
                return false;
            }

            #endregion

            //找命中点投影到哪两个连续控制点之间最近
            int bestSegmentIndex = -1;
            float bestDistance = float.MaxValue;
            for (int index = 0; index < this.Curve.ControlPoints.Count - 1; index++)
            {
                Vector3 lineSegmentStart = this.Curve.ControlPoints[index];
                Vector3 lineSegmentEnd = this.Curve.ControlPoints[index + 1];

                //计算命中点到线段的最短距离
                Vector3 closestPoint = GeometryAlgorithms.ClosestPointOnSegment(localHitPoint, lineSegmentStart, lineSegmentEnd);
                float distance = Vector3.Distance(localHitPoint, closestPoint);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestSegmentIndex = index;
                }
            }

            if (bestSegmentIndex < 0)
            {
                return false;
            }

            //插入位置 = 最近段的起点索引 + 1，即插入到线段两端控制点之间
            int insertIndex = bestSegmentIndex + 1;

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
            if (this.Curve == null || this.Curve.ResampledPoints == null || !this.Curve.ResampledPoints.Any())
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
            #region # 验证

            if (this.ControlPositions == null || constraint.VertexIndex < 0 || constraint.VertexIndex >= this.ControlPositions.Count)
            {
                return;
            }

            #endregion

            //更新控制点位置
            this.ControlPositions[constraint.VertexIndex] = localHitPoint.ToVector3();
        }
        #endregion

        #region 计算周长 —— float CalculatePerimeter(VolumeMetadata metadata)
        /// <summary>
        /// 计算周长
        /// </summary>
        /// <param name="metadata">体积元数据</param>
        /// <returns>周长（mm）</returns>
        public float CalculatePerimeter(VolumeMetadata metadata)
        {
            #region # 验证

            if (this.Curve == null || this.Curve.ResampledPoints == null || this.Curve.ResampledPoints.Count < 2)
            {
                return 0;
            }

            #endregion

            Matrix4 localToWorld = this.Transform.Matrix;
            float perimeter = 0;

            //计算所有线段长度之和
            for (int index = 0; index < this.Curve.ResampledPoints.Count - 1; index++)
            {
                Vector3 localStart = this.Curve.ResampledPoints[index];
                Vector3 localEnd = this.Curve.ResampledPoints[index + 1];

                //局部 -> 世界 -> 毫米
                Vector3 worldStart = Vector3.TransformPosition(localStart, localToWorld);
                Vector3 worldEnd = Vector3.TransformPosition(localEnd, localToWorld);
                Vector3 mmStart = worldStart.ToMillimeterPosition(metadata);
                Vector3 mmEnd = worldEnd.ToMillimeterPosition(metadata);

                perimeter += Vector3.Distance(mmStart, mmEnd);
            }

            //如果是闭合图形，加上首尾相连的线段
            if (this.Closed && this.Curve.ResampledPoints.Count >= 3)
            {
                Vector3 localStart = this.Curve.ResampledPoints[^1];
                Vector3 localEnd = this.Curve.ResampledPoints[0];

                //局部 -> 世界 -> 毫米
                Vector3 worldStart = Vector3.TransformPosition(localStart, localToWorld);
                Vector3 worldEnd = Vector3.TransformPosition(localEnd, localToWorld);
                Vector3 mmStart = worldStart.ToMillimeterPosition(metadata);
                Vector3 mmEnd = worldEnd.ToMillimeterPosition(metadata);

                perimeter += Vector3.Distance(mmStart, mmEnd);
            }

            return perimeter;
        }
        #endregion

        #region 计算表面积 —— float CalculateSurfaceArea(VolumeMetadata metadata)
        /// <summary>
        /// 计算表面积
        /// </summary>
        /// <param name="metadata">体积元数据</param>
        /// <returns>表面积（mm²）</returns>
        public float CalculateSurfaceArea(VolumeMetadata metadata)
        {
            #region # 验证

            if (!this.Closed || this.Curve == null || this.Curve.ResampledPoints == null || this.Curve.ResampledPoints.Count < 3)
            {
                return 0;
            }

            #endregion

            //获取世界空间的凸包顶点
            Matrix4 localToWorld = this.Transform.Matrix;
            List<Vector3> worldVertices = new List<Vector3>(this.Curve.ResampledPoints.Count);
            foreach (Vector3 localPos in this.Curve.ResampledPoints)
            {
                worldVertices.Add(Vector3.TransformPosition(localPos, localToWorld));
            }

            //转换到毫米空间
            Vector3[] mmVertices = worldVertices.Select(position => position.ToMillimeterPosition(metadata)).ToArray();

            //拟合2D多边形
            PolygonFit2D polygon2D = new PolygonFit2D(mmVertices);
            Vector2[] vertices = polygon2D.Vertices2D;
            if (vertices.Length < 3)
            {
                return 0;
            }

            float area = 0;
            for (int i = 0, j = vertices.Length - 1; i < vertices.Length; j = i++)
            {
                area += vertices[i].X * vertices[j].Y - vertices[j].X * vertices[i].Y;
            }
            area = Math.Abs(area) / 2.0f;

            return area;
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
            volumeData.ApplyPolygonCut(markTexture, this.Curve.ResampledPoints, localToWorld, cutMode, markValue);
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
            if (this.Curve == null || this.Curve.ResampledPoints == null || this.Curve.ResampledPoints.Count < 3)
            {
                return default;
            }

            #endregion

            //获取所有顶点世界坐标
            Vector3[] worldVertices = this.Curve.ResampledPoints.Select(pos => Vector3.TransformPosition(pos, this.Transform.Matrix)).ToArray();

            //投影到屏幕坐标
            Vector2[] screenVertices = new Vector2[worldVertices.Length];
            for (int index = 0; index < worldVertices.Length; index++)
            {
                screenVertices[index] = viewport.Project(worldVertices[index]);
            }

            int viewportWidth = viewport.ViewportSize.Width;
            int viewportHeight = viewport.ViewportSize.Height;

            //计算几何指标
            float perimeter = this.CalculatePerimeter(viewport.VolumeData.Metadata);
            float surfaceArea = this.CalculateSurfaceArea(viewport.VolumeData.Metadata);
            float voxelArea = viewport.Plane.GetVoxelArea();
            int voxelsCount = (int)Math.Round(surfaceArea / voxelArea);

            byte[] layerPixels = viewport.MPRRenderer.RenderStatistic(viewportWidth, viewportHeight, viewport.GlContextHandle);
            StatisticResult result = viewport.VolumeData.ApplyPolygonAnalyse(screenVertices, viewportWidth, viewportHeight, layerPixels, markValue);
            result.Perimeter = perimeter;
            result.SurfaceArea = surfaceArea;
            result.VoxelsCount = voxelsCount;
            result.CalculateExpectations();

            return result;
        }
        #endregion

        #endregion
    }
}
