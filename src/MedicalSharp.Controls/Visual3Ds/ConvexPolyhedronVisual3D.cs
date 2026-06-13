using Avalonia;
using Avalonia.Collections;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Engine.Algorithms;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Builders;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedicalSharp.Controls.Visual3Ds
{
    /// <summary>
    /// 凸多面体3D元素
    /// </summary>
    public class ConvexPolyhedronVisual3D : ShapeVisual3D, IPureVisual3D, ITranslatable3D, IRotatable, IVertexEditable, IHasSurfaceArea, IHasVolume, ICutVolume, IAnalyseVolume3D
    {
        #region # 字段及构造器

        /// <summary>
        /// 位置列表依赖属性
        /// </summary>
        public static readonly StyledProperty<AvaloniaList<Vector3D>> PositionsProperty;

        /// <summary>
        /// 可否旋转依赖属性
        /// </summary>
        public static readonly StyledProperty<bool> CanRotateProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static ConvexPolyhedronVisual3D()
        {
            PositionsProperty = AvaloniaProperty.Register<ConvexPolyhedronVisual3D, AvaloniaList<Vector3D>>(nameof(Positions), []);
            CanRotateProperty = AvaloniaProperty.Register<ConvexPolyhedronVisual3D, bool>(nameof(CanRotate), true);
        }

        /// <summary>
        /// 默认构造器
        /// </summary>
        public ConvexPolyhedronVisual3D()
        {

        }

        #endregion

        #region # 属性

        #region 网格模型 —— MeshGeometry MeshGeometry
        /// <summary>
        /// 网格模型
        /// </summary>
        public MeshGeometry MeshGeometry { get; private set; }
        #endregion

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

        #region 依赖属性 - 可否旋转 —— bool CanRotate
        /// <summary>
        /// 依赖属性 - 可否旋转
        /// </summary>
        public bool CanRotate
        {
            get => this.GetValue(CanRotateProperty);
            set => this.SetValue(CanRotateProperty, value);
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
            MeshGeometry strokeMesh = MeshFactory.CreateConvexPolyhedron(positions, GraphicPrimitiveType.Lines);
            MeshGeometry fillMesh = MeshFactory.CreateConvexPolyhedron(positions, GraphicPrimitiveType.Triangles);
            if (this.Renderable == null)
            {
                WildframeRenderable renderable = new WildframeRenderable(strokeMesh, fillMesh, true);
                renderable.SetWildframe(this.Stroke.ToVector4(), this.StrokeThickness, this.Fill.ToVector4());
                this.Renderable = renderable;
            }
            else
            {
                WildframeRenderable renderable = (WildframeRenderable)this.Renderable;
                renderable.Update(strokeMesh, fillMesh);
                renderable.SetWildframe(this.Stroke.ToVector4(), this.StrokeThickness, this.Fill.ToVector4());
            }

            this.MeshGeometry = fillMesh;
        }
        #endregion

        #region 获取凸包位置列表 —— IReadOnlyList<Vector3> GetConvexHullPositions()
        /// <summary>
        /// 获取凸包位置列表
        /// </summary>
        /// <returns>位置列表（世界空间）</returns>
        public IReadOnlyList<Vector3> GetConvexHullPositions()
        {
            #region # 验证

            if (this.Positions == null || !this.Positions.Any())
            {
                return [];
            }

            #endregion

            Matrix4 localToWorld = this.Transform.Matrix;
            Vector3[] convexHullPositions = new Vector3[this.Positions.Count];
            for (int index = 0; index < this.Positions.Count; index++)
            {
                Vector3 localPosition = this.Positions[index].ToVector3();
                Vector3 worldPostion = Vector3.TransformPosition(localPosition, localToWorld);
                convexHullPositions[index] = worldPostion;
            }

            return convexHullPositions;
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
            if (this.Positions == null || !this.Positions.Any())
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

            //插入新顶点（最近点之后）
            int newIndex = nearestIndex + 1;
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

            //更新控制点位置
            this.Positions[constraint.VertexIndex] = localHitPoint.ToVector3();
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
            //获取三角形面
            WildframeRenderable renderable = (WildframeRenderable)this.Renderable;
            if (renderable?.Triangles == null)
            {
                return 0;
            }

            float area = 0;
            Matrix4 localToWorld = this.Transform.Matrix;
            foreach (Triangle triangle in renderable.Triangles)
            {
                //局部 -> 世界 -> 毫米
                Vector3 mmA = Vector3.TransformPosition(triangle.PointA, localToWorld).ToMillimeterPosition(metadata);
                Vector3 mmB = Vector3.TransformPosition(triangle.PointB, localToWorld).ToMillimeterPosition(metadata);
                Vector3 mmC = Vector3.TransformPosition(triangle.PointC, localToWorld).ToMillimeterPosition(metadata);

                Vector3 ab = mmB - mmA;
                Vector3 ac = mmC - mmA;
                area += Vector3.Cross(ab, ac).Length;
            }
            area /= 2.0f;

            return area;
        }
        #endregion

        #region 计算体积 —— float CalculateVolume(VolumeMetadata metadata)
        /// <summary>
        /// 计算体积
        /// </summary>
        /// <param name="metadata">体积元数据</param>
        /// <returns>体积（mm³）</returns>
        public float CalculateVolume(VolumeMetadata metadata)
        {
            //获取世界空间的凸包顶点
            IReadOnlyList<Vector3> hull = GetConvexHullPositions();
            if (hull.Count < 4)
            {
                return 0;
            }

            //转换到毫米空间
            Vector3[] mmHull = hull.Select(p => p.ToMillimeterPosition(metadata)).ToArray();

            //四面体分解法
            float volume = 0;
            Vector3 origin = mmHull[0];
            for (int index = 1; index < mmHull.Length - 1; index++)
            {
                volume += Vector3.Dot(mmHull[index] - origin, Vector3.Cross(mmHull[index + 1] - origin, mmHull[index + 2] - origin));
            }
            volume = Math.Abs(volume) / 6.0f;

            return volume;
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
            Matrix4 localToWorld = this.Transform.Matrix;
            Vector4[] planes = this.MeshGeometry.ExtractPlanes();
            volumeData.ApplyConvexPolyhedronCut(markTexture, planes, localToWorld, cutMode, markValue);
        }
        #endregion

        #region 适用统计体积 —— async Task<StatisticResult> ApplyAnalyseVolume(VolumeData volumeData...
        /// <summary>
        /// 适用统计体积
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="markValue">标记值</param>
        /// <returns>统计结果</returns>
        public async Task<StatisticResult> ApplyAnalyseVolume(VolumeData volumeData, byte? markValue)
        {
            #region # 验证

            if (volumeData == null)
            {
                return default;
            }
            if (this.MeshGeometry == null)
            {
                return default;
            }

            #endregion

            //获取局部空间的面
            Vector4[] localPlanes = this.MeshGeometry.ExtractPlanes();
            Matrix4 localToWorld = this.Transform.Matrix;

            //转换到世界空间
            List<Plane> worldFaces = [];
            foreach (Vector4 localPlane in localPlanes)
            {
                //局部法向量
                Vector3 localNormal = new Vector3(localPlane.X, localPlane.Y, localPlane.Z);
                float localDistance = localPlane.W;

                //局部平面上一点（法向量方向上距离原点最近的点的反方向）
                Vector3 localPoint = -localNormal * localDistance;

                //转换到世界空间
                Vector3 worldPoint = Vector3.TransformPosition(localPoint, localToWorld);

                //转换法向量（使用逆转置矩阵）
                Matrix4 worldToLocal = localToWorld.Inverted();
                Matrix4 inverseTranspose = Matrix4.Transpose(worldToLocal);
                Vector3 worldNormal = Vector3.TransformNormal(localNormal, inverseTranspose).Normalized();

                //计算新的距离
                float worldDistance = -Vector3.Dot(worldNormal, worldPoint);

                worldFaces.Add(new Plane(worldNormal, worldDistance));
            }

            StatisticResult result = await Task.Run(() => volumeData.ApplyConvexPolyhedronAnalyse(worldFaces, markValue));
            result.SurfaceArea = this.CalculateSurfaceArea(volumeData.Metadata);
            result.Volume = this.CalculateVolume(volumeData.Metadata);

            return result;
        }
        #endregion

        #endregion
    }
}
