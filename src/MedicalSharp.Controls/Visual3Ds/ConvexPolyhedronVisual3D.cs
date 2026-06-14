using Avalonia;
using Avalonia.Collections;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Engine.Algorithms;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Algorithms;
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
                //局部 -> 世界
                Vector3 worldA = Vector3.TransformPosition(triangle.PointA, localToWorld);
                Vector3 worldB = Vector3.TransformPosition(triangle.PointB, localToWorld);
                Vector3 worldC = Vector3.TransformPosition(triangle.PointC, localToWorld);

                //世界 -> 毫米
                Vector3 mmA = worldA.ToMillimeterPosition(metadata);
                Vector3 mmB = worldB.ToMillimeterPosition(metadata);
                Vector3 mmC = worldC.ToMillimeterPosition(metadata);

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
            Matrix4 localToWorld = this.Transform.Matrix;

            //获取世界空间的凸包顶点
            IReadOnlyList<Vector3> hull = this.GetConvexHullPositions();
            if (hull.Count < 4)
            {
                return 0;
            }

            //计算质心作为参考点
            Vector3 centroid = Vector3.Zero;
            Vector3[] mmHull = hull
                .Select(localPos => Vector3.TransformPosition(localPos, localToWorld))
                .Select(worldPos => worldPos.ToMillimeterPosition(metadata))
                .ToArray();
            foreach (Vector3 mmPosition in mmHull)
            {
                centroid += mmPosition;
            }
            centroid /= mmHull.Length;

            //获取三角形面
            WildframeRenderable renderable = (WildframeRenderable)this.Renderable;
            if (renderable?.Triangles == null)
            {
                return 0;
            }

            float volume = 0;
            foreach (Triangle triangle in renderable.Triangles)
            {
                //局部 -> 世界
                Vector3 worldA = Vector3.TransformPosition(triangle.PointA, localToWorld);
                Vector3 worldB = Vector3.TransformPosition(triangle.PointB, localToWorld);
                Vector3 worldC = Vector3.TransformPosition(triangle.PointC, localToWorld);

                //世界 -> 毫米
                Vector3 mmA = worldA.ToMillimeterPosition(metadata);
                Vector3 mmB = worldB.ToMillimeterPosition(metadata);
                Vector3 mmC = worldC.ToMillimeterPosition(metadata);

                //四面体体积
                Vector3 a = mmA - centroid;
                Vector3 b = mmB - centroid;
                Vector3 c = mmC - centroid;
                volume += Vector3.Dot(a, Vector3.Cross(b, c));
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

            Matrix4 localToWorld = this.Transform.Matrix;

            //获取所有顶点世界坐标
            IReadOnlyList<Vector3> worldVertices = this.GetConvexHullPositions();

            //计算凸多面体在世界空间中的包围盒
            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;
            float minZ = float.MaxValue, maxZ = float.MinValue;
            foreach (Vector3 vertex in worldVertices)
            {
                minX = Math.Min(minX, vertex.X); maxX = Math.Max(maxX, vertex.X);
                minY = Math.Min(minY, vertex.Y); maxY = Math.Max(maxY, vertex.Y);
                minZ = Math.Min(minZ, vertex.Z); maxZ = Math.Max(maxZ, vertex.Z);
            }

            //转换到体素坐标
            Vector3i volumeSize = volumeData.Metadata.VolumeSize;
            Vector3 volumeScale = volumeData.Metadata.VolumeScale;
            Vector3 minWorld = new Vector3(minX, minY, minZ);
            Vector3 maxWorld = new Vector3(maxX, maxY, maxZ);
            Vector3 minTexCoord = (minWorld / volumeScale) + new Vector3(0.5f);
            Vector3 maxTexCoord = (maxWorld / volumeScale) + new Vector3(0.5f);
            int minVoxelX = (int)(minTexCoord.X * volumeSize.X);
            int maxVoxelX = (int)(maxTexCoord.X * volumeSize.X);
            int minVoxelY = (int)(minTexCoord.Y * volumeSize.Y);
            int maxVoxelY = (int)(maxTexCoord.Y * volumeSize.Y);
            int minVoxelZ = (int)(minTexCoord.Z * volumeSize.Z);
            int maxVoxelZ = (int)(maxTexCoord.Z * volumeSize.Z);

            //裁剪到体积范围
            minVoxelX = Math.Max(0, minVoxelX);
            maxVoxelX = Math.Min(volumeSize.X - 1, maxVoxelX);
            minVoxelY = Math.Max(0, minVoxelY);
            maxVoxelY = Math.Min(volumeSize.Y - 1, maxVoxelY);
            minVoxelZ = Math.Max(0, minVoxelZ);
            maxVoxelZ = Math.Min(volumeSize.Z - 1, maxVoxelZ);
            Vector3i minVoxelPos = new Vector3i(minVoxelX, minVoxelY, minVoxelZ);
            Vector3i maxVoxelPos = new Vector3i(maxVoxelX, maxVoxelY, maxVoxelZ);

            // 获取局部空间的面并转换到世界空间
            Vector4[] localPlanes = this.MeshGeometry.ExtractPlanes();
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

            //计算几何指标
            float surfaceArea = this.CalculateSurfaceArea(volumeData.Metadata);
            float volume = this.CalculateVolume(volumeData.Metadata);
            int voxelsCount = (int)Math.Round(volume / volumeData.Metadata.VoxelVolume);

            StatisticResult result = await Task.Run(() => volumeData.ApplyConvexPolyhedronAnalyse(minVoxelPos, maxVoxelPos, worldFaces, markValue));
            result.SurfaceArea = surfaceArea;
            result.Volume = volume;
            result.VoxelsCount = voxelsCount;
            result.CalculateExpectations();

            return result;
        }
        #endregion

        #endregion
    }
}
