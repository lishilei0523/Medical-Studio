using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using MIConvexHull;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalSharp.Primitives.Algorithms
{
    /// <summary>
    /// 几何算法
    /// </summary>
    public static class GeometryAlgorithms
    {
        #region # 纹理坐标转世界坐标 —— static Vector3 ToWorldPosition(this Vector3 textureCoord...
        /// <summary>
        /// 纹理坐标转世界坐标
        /// </summary>
        /// <param name="textureCoord">纹理坐标</param>
        /// <param name="metadata">体积元数据</param>
        /// <returns>世界位置</returns>
        public static Vector3 ToWorldPosition(this Vector3 textureCoord, VolumeMetadata metadata)
        {
            Vector3 worldPosition = (textureCoord - new Vector3(0.5f)) * metadata.VolumeScale;

            return worldPosition;
        }
        #endregion

        #region # 世界坐标转纹理坐标 —— static Vector3 ToTextureCoord(this Vector3 worldPosition...
        /// <summary>
        /// 世界坐标转纹理坐标
        /// </summary>
        /// <param name="worldPosition">世界位置</param>
        /// <param name="metadata">体积元数据</param>
        /// <returns>纹理坐标</returns>
        /// 世界空间：归一化单位空间（-0.5~0.5），中心在 (0.0, 0.0, 0.0)。
        /// 纹理空间：归一化单位空间（0~1）
        /// 转换路径：世界坐标 -> 中间坐标（撤销VolumeScale）-> 纹理坐标（加0.5）
        public static Vector3 ToTextureCoord(this Vector3 worldPosition, VolumeMetadata metadata)
        {
            //世界空间 -> 中间坐标（撤销VolumeScale）
            Vector3 normalized = new Vector3(
                worldPosition.X / metadata.VolumeScale.X,
                worldPosition.Y / metadata.VolumeScale.Y,
                worldPosition.Z / metadata.VolumeScale.Z
            );

            //中间坐标 -> 纹理坐标
            Vector3 textureCoord = normalized + new Vector3(0.5f);

            return textureCoord;
        }
        #endregion

        #region # 世界坐标转体素坐标 —— static Vector3i ToVoxelPosition(this Vector3 worldPosition...
        /// <summary>
        /// 世界坐标转体素坐标
        /// </summary>
        /// <param name="worldPosition">世界位置</param>
        /// <param name="metadata">体积元数据</param>
        /// <returns>体素位置</returns>
        /// <remarks>
        /// 世界空间：归一化单位空间（-0.5~0.5），中心在 (0.0, 0.0, 0.0)。
        /// 体素空间：原点由VolumeSize决定。
        /// 转换路径：世界坐标 -> 纹理坐标 -> 体素坐标（乘以VolumeSize）
        /// </remarks>
        public static Vector3i ToVoxelPosition(this Vector3 worldPosition, VolumeMetadata metadata)
        {
            //中间坐标 -> 纹理坐标
            Vector3 textureCoord = worldPosition.ToTextureCoord(metadata);

            //纹理坐标 -> 体素坐标（Floor取整，因为体素中心在(i+0.5)处）
            int voxelX = (int)MathF.Floor(textureCoord.X * metadata.VolumeSize.X);
            int voxelY = (int)MathF.Floor(textureCoord.Y * metadata.VolumeSize.Y);
            int voxelZ = (int)MathF.Floor(textureCoord.Z * metadata.VolumeSize.Z);
            voxelX = Math.Clamp(voxelX, 0, metadata.VolumeSize.X - 1);
            voxelY = Math.Clamp(voxelY, 0, metadata.VolumeSize.Y - 1);
            voxelZ = Math.Clamp(voxelZ, 0, metadata.VolumeSize.Z - 1);
            Vector3i voxelPosition = new Vector3i(voxelX, voxelY, voxelZ);

            return voxelPosition;
        }
        #endregion

        #region # 世界坐标转毫米坐标 —— static Vector3 ToMillimeterPosition(this Vector3 worldPosition...
        /// <summary>
        /// 世界坐标转毫米坐标
        /// </summary>
        /// <param name="worldPosition">世界位置</param>
        /// <param name="metadata">体积元数据</param>
        /// <returns>毫米位置</returns>
        /// <remarks>
        /// 世界空间：归一化单位空间（-0.5~0.5），中心在 (0.0, 0.0, 0.0)。
        /// 毫米空间：单位毫米，原点由PhysicalSize决定。
        /// 转换路径：世界坐标 -> 纹理坐标 -> 毫米坐标（乘以PhysicalSize）
        /// </remarks>
        public static Vector3 ToMillimeterPosition(this Vector3 worldPosition, VolumeMetadata metadata)
        {
            //世界坐标 -> 纹理坐标
            Vector3 textureCoord = worldPosition.ToTextureCoord(metadata);

            //纹理坐标 -> 毫米坐标
            Vector3 mmPosition = new Vector3(
                textureCoord.X * metadata.PhysicalSize.X,
                textureCoord.Y * metadata.PhysicalSize.Y,
                textureCoord.Z * metadata.PhysicalSize.Z
            );

            return mmPosition;
        }
        #endregion

        #region # 世界坐标转患者坐标 —— static Vector3 ToPatientPosition(this Vector3 worldPosition...
        /// <summary>
        /// 世界坐标转患者坐标
        /// </summary>
        /// <param name="worldPosition">世界位置</param>
        /// <param name="metadata">体积元数据</param>
        /// <returns>患者位置（DICOM患者坐标系，单位毫米）</returns>
        /// <remarks>
        /// 世界空间：归一化单位空间（-0.5~0.5），中心在 (0.0, 0.0, 0.0)。
        /// 患者空间：DICOM患者坐标系，单位毫米，原点由DICOM Origin决定。
        /// 转换路径：世界坐标 -> 毫米坐标 -> 患者坐标（加上Origin）
        /// </remarks>
        public static Vector3 ToPatientPosition(this Vector3 worldPosition, VolumeMetadata metadata)
        {
            Vector3 mmPosition = worldPosition.ToMillimeterPosition(metadata);
            Vector3 patientPosition = mmPosition + metadata.Origin;

            return patientPosition;
        }
        #endregion

        #region # 判断点是否在顶点列表中 —— static bool ContainsPoint(IReadOnlyList<Vector3> vertices...
        /// <summary>
        /// 判断点是否在顶点列表中
        /// </summary>
        /// <param name="vertices">顶点列表</param>
        /// <param name="point">待判断的点</param>
        /// <param name="epsilon">容差</param>
        /// <returns>是否在列表中</returns>
        public static bool ContainsPoint(IReadOnlyList<Vector3> vertices, Vector3 point, float epsilon)
        {
            foreach (Vector3 vertex in vertices)
            {
                float dx = vertex.X - point.X;
                float dy = vertex.Y - point.Y;
                float dz = vertex.Z - point.Z;
                if (dx * dx + dy * dy + dz * dz < epsilon * epsilon)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region # 逆时针排序 —— static IReadOnlyList<Vector3> SortPointsCounterClockwise(...
        /// <summary>
        /// 逆时针排序
        /// </summary>
        /// <param name="points">待排序的点集（应在同一平面上）</param>
        /// <param name="planeNormal">平面法向量</param>
        /// <returns>逆时针排序后的点集</returns>
        /// <remarks>将平面上的点按逆时针方向排序</remarks>
        public static IReadOnlyList<Vector3> SortPointsCounterClockwise(IReadOnlyList<Vector3> points, in Vector3 planeNormal)
        {
            #region # 验证

            if (points.Count <= 1)
            {
                return points;
            }

            #endregion

            //计算中心点
            Vector3 center = Vector3.Zero;
            foreach (Vector3 point in points)
            {
                center = new Vector3(center.X + point.X, center.Y + point.Y, center.Z + point.Z);
            }
            center = new Vector3(center.X, center.Y, center.Z) / points.Count;

            //选择与法线不平行的向量作为参考
            Vector3 reference = Math.Abs(planeNormal.X) < 0.9 ? new Vector3(1, 0, 0) : new Vector3(0, 1, 0);

            //构建平面局部坐标系（两个正交轴）
            //u = normalize(reference × planeNormal)
            Vector3 u = Vector3.Cross(reference, planeNormal);
            float uLength = MathF.Sqrt(u.X * u.X + u.Y * u.Y + u.Z * u.Z);
            Vector3 uAxis = new Vector3(u.X / uLength, u.Y / uLength, u.Z / uLength);

            //v = normalize(planeNormal × u)
            Vector3 v = Vector3.Cross(planeNormal, u);
            float vLength = MathF.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
            Vector3 vAxis = new Vector3(v.X / vLength, v.Y / vLength, v.Z / vLength);

            //投影到局部2D坐标系并计算角度
            List<(Vector3 point, double angle)> angledPoints = [];
            foreach (Vector3 p in points)
            {
                Vector3 relative = new Vector3(p.X - center.X, p.Y - center.Y, p.Z - center.Z);
                float uCoord = Vector3.Dot(relative, uAxis);
                float vCoord = Vector3.Dot(relative, vAxis);
                float angle = MathF.Atan2(vCoord, uCoord); //返回[-π, π]
                angledPoints.Add((p, angle));
            }

            //按角度排序（逆时针）
            angledPoints.Sort((a, b) => a.angle.CompareTo(b.angle));
            List<Vector3> sortedPoints = angledPoints.Select(p => p.point).ToList();

            return sortedPoints;
        }
        #endregion

        #region # 计算凸包真实棱边 —— static HashSet<(int, int)> ComputeConvexHullEdges(...
        /// <summary>
        /// 计算凸包真实棱边
        /// </summary>
        /// <param name="hullPositions">凸包位置列表</param>
        /// <returns>去重后的棱边列表（顶点索引对）</returns>
        public static HashSet<(int, int)> ComputeConvexHullEdges(IReadOnlyList<Vector3> hullPositions)
        {
            try
            {
                //转成MIConvexHull的输入格式
                DefaultVertex[] vertices = hullPositions.Select(position => new DefaultVertex
                {
                    Position = [position.X, position.Y, position.Z]
                }).ToArray();
                ConvexHullCreationResult<DefaultVertex, DefaultConvexFace<DefaultVertex>> result = ConvexHull.Create(vertices);

                //从凸包面中提取边并去重
                HashSet<(int, int)> edges = [];
                foreach (DefaultConvexFace<DefaultVertex> face in result.Result.Faces)
                {
                    DefaultVertex[] faceVertices = face.Vertices;
                    for (int index = 0; index < faceVertices.Length; index++)
                    {
                        int indexA = Array.IndexOf(vertices, faceVertices[(index)]);
                        int indexB = Array.IndexOf(vertices, faceVertices[(index + 1) % faceVertices.Length]);
                        edges.Add((Math.Min(indexA, indexB), Math.Max(indexA, indexB)));
                    }
                }

                return edges;
            }
            catch
            {
                //MIConvexHull失败（如共面点），回退到全连接
                return EnumerateAllEdges(hullPositions.Count);
            }
        }
        #endregion

        #region # 全连接枚举边 —— static HashSet<(int, int)> EnumerateAllEdges(int count)
        /// <summary>
        /// 全连接枚举边
        /// </summary>
        /// <param name="count">顶点数</param>
        /// <returns>所有可能的边列表（点对）</returns>
        public static HashSet<(int, int)> EnumerateAllEdges(int count)
        {
            HashSet<(int, int)> edges = [];
            for (int i = 0; i < count; i++)
            {
                for (int j = i + 1; j < count; j++)
                {
                    edges.Add((i, j));
                }
            }
            return edges;
        }
        #endregion

        #region # 剔除内部点 —— static List<Vector3> RemoveInteriorPoints(List<Vector3>...
        /// <summary>
        /// 剔除内部点
        /// </summary>
        /// <param name="points">点集（应按顺序排列）</param>
        /// <param name="epsilon">容差</param>
        /// <returns>剔除共线点后的点集</returns>
        /// <remarks>保留端点，如果某点落在其他两点的连线上（共线），则它是内部点，丢弃</remarks>
        public static List<Vector3> RemoveInteriorPoints(List<Vector3> points, float epsilon)
        {
            #region # 验证

            if (points.Count <= 3)
            {
                return points;
            }

            #endregion

            bool[] isInterior = new bool[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = 0; j < points.Count; j++)
                {
                    if (i == j || isInterior[i])
                    {
                        continue;
                    }
                    for (int k = j + 1; k < points.Count; k++)
                    {
                        if (i == k || isInterior[i])
                        {
                            continue;
                        }

                        //判断点i是否在线段jk上
                        if (IsPointOnSegment(points[i], points[j], points[k], epsilon))
                        {
                            isInterior[i] = true;
                            break;
                        }
                    }
                }
            }

            List<Vector3> boundary = [];
            for (int index = 0; index < points.Count; index++)
            {
                if (!isInterior[index])
                {
                    boundary.Add(points[index]);
                }
            }

            return boundary;
        }
        #endregion

        #region # 判断点是否在线段上(2D) —— static bool IsPointOnSegment(in Vector2 point, in Vector2 start...
        /// <summary>
        /// 判断点是否在线段上(2D)
        /// </summary>
        /// <param name="point">待判断的点</param>
        /// <param name="start">线段起点</param>
        /// <param name="end">线段终点</param>
        /// <param name="epsilon">容差</param>
        /// <returns>是否在线段上</returns>
        public static bool IsPointOnSegment(in Vector2 point, in Vector2 start, in Vector2 end, float epsilon)
        {
            //转换为Vector3（Z = 0），复用3D算法
            Vector3 point3D = new Vector3(point.X, point.Y, 0);
            Vector3 start3D = new Vector3(start.X, start.Y, 0);
            Vector3 end3D = new Vector3(end.X, end.Y, 0);
            bool isPointOnSegment = IsPointOnSegment(point3D, start3D, end3D, epsilon);

            return isPointOnSegment;
        }
        #endregion

        #region # 判断点是否在线段上(3D) —— static bool IsPointOnSegment(in Vector3 point, in Vector3 start...
        /// <summary>
        /// 判断是否在线段上(3D)
        /// </summary>
        /// <param name="point">待判断的点</param>
        /// <param name="start">线段起点</param>
        /// <param name="end">线段终点</param>
        /// <param name="epsilon">容差</param>
        /// <returns>是否在线段上</returns>
        public static bool IsPointOnSegment(in Vector3 point, in Vector3 start, in Vector3 end, float epsilon)
        {
            Vector3 ab = end - start;
            Vector3 ap = point - start;
            float abLengthSq = ab.LengthSquared;
            if (abLengthSq < epsilon * epsilon)
            {
                return false;  //起点和终点重合
            }

            //投影参数 t = (ap·ab) / |ab|²
            float t = Vector3.Dot(ap, ab) / abLengthSq;

            //t必须在[0, 1]之间
            if (t < 0 || t > 1)
            {
                return false;
            }

            //投影点
            Vector3 projection = start + t * ab;

            //距离判断
            float squaredDistance = (point - projection).LengthSquared;
            bool isOnSegment = squaredDistance < epsilon * epsilon;

            return isOnSegment;
        }
        #endregion

        #region # 判断点是否在多边形内 —— static bool IsPointInPolygon(in Vector2 point, IReadOnlyList<Vector2>...
        /// <summary>
        /// 判断点是否在多边形内
        /// </summary>
        /// <param name="point">待判断的点</param>
        /// <param name="polygon">多边形顶点（按顺序排列）</param>
        /// <param name="epsilon">容差</param>
        /// <returns>是否在多边形内</returns>
        /// <remarks>射线投射法</remarks>
        public static bool IsPointInPolygon(in Vector2 point, IReadOnlyList<Vector2> polygon, float epsilon = 1e-6f)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                Vector2 vi = polygon[i];
                Vector2 vj = polygon[j];

                //检查射线是否与边相交
                bool intersect = ((vi.Y > point.Y) != (vj.Y > point.Y));
                if (intersect)
                {
                    //计算交点X坐标
                    float xIntersect = vi.X + (vj.X - vi.X) * (point.Y - vi.Y) / (vj.Y - vi.Y);

                    //使用容差避免浮点误差
                    if (point.X < xIntersect - epsilon)
                    {
                        inside = !inside;
                    }
                }
            }

            return inside;
        }
        #endregion

        #region # 判断点是否在多边形边界 —— static bool IsPointOnPolygonEdge(in Vector2 point...
        /// <summary>
        /// 判断点是否在多边形边界
        /// </summary>
        /// <param name="point">待判断的点</param>
        /// <param name="polygon">多边形顶点（按顺序排列）</param>
        /// <param name="tolerance">容差</param>
        /// <returns>是否在多边形边界</returns>
        public static bool IsPointOnPolygonEdge(in Vector2 point, IReadOnlyList<Vector2> polygon, float tolerance)
        {
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if (IsPointOnSegment(point, polygon[i], polygon[j], tolerance))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region # 判断体素是否在立方体内 —— static bool IsVoxelInBox(in Vector3i voxelPosition, in Vector3i volumeSize...
        /// <summary>
        /// 判断体素是否在立方体内
        /// </summary>
        /// <param name="voxelPosition">体素坐标</param>
        /// <param name="volumeSize">体积尺寸</param>
        /// <param name="volumeScale">体积缩放</param>
        /// <param name="boxLocalMin">立方体最小点（局部空间）</param>
        /// <param name="boxLocalMax">立方体最大点（局部空间）</param>
        /// <param name="worldToLocal">世界到局部变换矩阵</param>
        /// <returns>是否在立方体内</returns>
        public static bool IsVoxelInBox(in Vector3i voxelPosition, in Vector3i volumeSize, in Vector3 volumeScale, in Vector3 boxLocalMin, in Vector3 boxLocalMax, Matrix4 worldToLocal)
        {
            //体素坐标 -> 纹理坐标 -> 世界坐标
            Vector3 texCoord = (voxelPosition + new Vector3(0.5f)) / volumeSize;
            Vector3 worldPos = (texCoord - new Vector3(0.5f)) * volumeScale;

            //世界坐标 -> 局部坐标
            Vector3 localPos = Vector3.TransformPosition(worldPos, worldToLocal);

            bool isInBox = localPos.X >= boxLocalMin.X && localPos.X <= boxLocalMax.X &&
                           localPos.Y >= boxLocalMin.Y && localPos.Y <= boxLocalMax.Y &&
                           localPos.Z >= boxLocalMin.Z && localPos.Z <= boxLocalMax.Z;

            return isInBox;
        }
        #endregion

        #region # 判断体素是否在立方体边界 —— static bool IsVoxelOnBoxBoundary(in Vector3i voxelPosition...
        /// <summary>
        /// 判断体素是否在立方体边界
        /// </summary>
        /// <param name="voxelPosition">体素坐标</param>
        /// <param name="volumeSize">体积尺寸</param>
        /// <param name="volumeScale">体积缩放</param>
        /// <param name="boxLocalMin">立方体最小点（局部空间）</param>
        /// <param name="boxLocalMax">立方体最大点（局部空间）</param>
        /// <param name="worldToLocal">世界到局部变换矩阵</param>
        /// <returns>是否在立方体边界</returns>
        /// <remarks>6个面</remarks>
        public static bool IsVoxelOnBoxBoundary(in Vector3i voxelPosition, in Vector3i volumeSize, in Vector3 volumeScale, in Vector3 boxLocalMin, in Vector3 boxLocalMax, Matrix4 worldToLocal)
        {
            //定义6个方向的偏移量
            int[] offsetX = [1, -1, 0, 0, 0, 0];  //右、左、无、无、无、无
            int[] offsetY = [0, 0, 1, -1, 0, 0];  //无、无、前、后、无、无
            int[] offsetZ = [0, 0, 0, 0, 1, -1];  //无、无、无、无、上、下
            for (int index = 0; index < 6; index++)
            {
                int neighborX = voxelPosition.X + offsetX[index];
                int neighborY = voxelPosition.Y + offsetY[index];
                int neighborZ = voxelPosition.Z + offsetZ[index];

                //邻居超出体积边界 -> 当前体素是边界
                if (neighborX < 0 || neighborX >= volumeSize.X ||
                    neighborY < 0 || neighborY >= volumeSize.Y ||
                    neighborZ < 0 || neighborZ >= volumeSize.Z)
                {
                    return true;
                }

                //邻居不在立方体内 -> 当前体素是边界
                Vector3i neighborPosition = new Vector3i(neighborX, neighborY, neighborZ);
                if (!IsVoxelInBox(neighborPosition, volumeSize, volumeScale, boxLocalMin, boxLocalMax, worldToLocal))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region # 判断体素是否在球体内 —— static bool IsVoxelInSphere(in Vector3i voxelPosition, in Vector3 center...
        /// <summary>
        /// 判断体素是否在球体内
        /// </summary>
        /// <param name="voxelPosition">体素坐标</param>
        /// <param name="volumeSize">体积尺寸</param>
        /// <param name="volumeScale">体积缩放</param>
        /// <param name="sphereCenter">球心（世界坐标）</param>
        /// <param name="sphereRadius">半径（世界单位）</param>
        /// <returns>是否在球体内</returns>
        public static bool IsVoxelInSphere(in Vector3i voxelPosition, in Vector3i volumeSize, in Vector3 volumeScale, in Vector3 sphereCenter, float sphereRadius)
        {
            //体素坐标 -> 世界坐标
            Vector3 texCoord = (voxelPosition + new Vector3(0.5f)) / volumeSize;
            Vector3 worldPos = (texCoord - new Vector3(0.5f)) * volumeScale;

            //计算到球心的距离
            float distance = Vector3.Distance(worldPos, sphereCenter);
            bool isVoxelIn = distance <= sphereRadius;

            return isVoxelIn;
        }
        #endregion

        #region # 判断体素是否在球体边界 —— static bool IsVoxelOnSphereBoundary(in Vector3i voxelPosition...
        /// <summary>
        /// 判断体素是否在球体边界
        /// </summary>
        /// <param name="voxelPosition">体素坐标</param>
        /// <param name="volumeSize">体积尺寸</param>
        /// <param name="volumeScale">体积缩放</param>
        /// <param name="sphereCenter">球心（世界坐标）</param>
        /// <param name="sphereRadius">半径（世界单位）</param>
        /// <param name="epsilon">容差</param>
        /// <returns>是否在球体边界</returns>
        public static bool IsVoxelOnSphereBoundary(in Vector3i voxelPosition, in Vector3i volumeSize, in Vector3 volumeScale, in Vector3 sphereCenter, float sphereRadius, float epsilon = 0.005f)
        {
            //体素坐标 -> 世界坐标
            Vector3 texCoord = (voxelPosition + new Vector3(0.5f)) / volumeSize;
            Vector3 worldPos = (texCoord - new Vector3(0.5f)) * volumeScale;

            //计算到球心的距离
            float distance = Vector3.Distance(worldPos, sphereCenter);

            //边界判断：距离在半径附近
            bool isVoxelOn = Math.Abs(distance - sphereRadius) <= epsilon;

            return isVoxelOn;
        }
        #endregion

        #region # 判断体素是否在圆柱体内 —— static bool IsVoxelInCylinder(in Vector3i voxelPosition...
        /// <summary>
        /// 判断体素是否在圆柱体内
        /// </summary>
        /// <param name="voxelPosition">体素坐标</param>
        /// <param name="volumeSize">体积尺寸</param>
        /// <param name="volumeScale">体积缩放</param>
        /// <param name="cylinderCenter">圆柱中心（世界坐标）</param>
        /// <param name="cylinderAxis">圆柱轴方向（归一化）</param>
        /// <param name="cylinderRadius">半径（世界单位）</param>
        /// <param name="cylinderHeight">高度（世界单位）</param>
        /// <returns>是否在圆柱体内</returns>
        public static bool IsVoxelInCylinder(in Vector3i voxelPosition, in Vector3i volumeSize, in Vector3 volumeScale,
            in Vector3 cylinderCenter, in Vector3 cylinderAxis, float cylinderRadius, float cylinderHeight)
        {
            //体素坐标 -> 世界坐标
            Vector3 texCoord = (voxelPosition + new Vector3(0.5f)) / volumeSize;
            Vector3 worldPos = (texCoord - new Vector3(0.5f)) * volumeScale;

            //计算相对圆柱中心的偏移
            Vector3 delta = worldPos - cylinderCenter;

            //沿轴方向的投影距离
            float along = Vector3.Dot(delta, cylinderAxis);

            //超出高度范围
            if (Math.Abs(along) > cylinderHeight / 2.0f)
            {
                return false;
            }

            //垂直轴方向的径向距离
            Vector3 radial = delta - along * cylinderAxis;
            float radialDist = radial.Length;
            bool isVoxelIn = radialDist <= cylinderRadius;

            return isVoxelIn;
        }
        #endregion

        #region # 判断体素是否在圆柱体边界 —— static bool IsVoxelOnCylinderBoundary(in Vector3i voxelPosition...
        /// <summary>
        /// 判断体素是否在圆柱体边界
        /// </summary>
        /// <param name="voxelPosition">体素坐标</param>
        /// <param name="volumeSize">体积尺寸</param>
        /// <param name="volumeScale">体积缩放</param>
        /// <param name="cylinderCenter">圆柱中心（世界坐标）</param>
        /// <param name="cylinderAxis">圆柱轴方向（归一化）</param>
        /// <param name="cylinderRadius">半径（世界单位）</param>
        /// <param name="cylinderHeight">高度（世界单位）</param>
        /// <param name="epsilon">容差</param>
        /// <returns>是否在圆柱体边界</returns>
        public static bool IsVoxelOnCylinderBoundary(in Vector3i voxelPosition, in Vector3i volumeSize, in Vector3 volumeScale, in Vector3 cylinderCenter, in Vector3 cylinderAxis, float cylinderRadius, float cylinderHeight, float epsilon = 0.005f)
        {
            //体素坐标 -> 世界坐标
            Vector3 texCoord = (voxelPosition + new Vector3(0.5f)) / volumeSize;
            Vector3 worldPos = (texCoord - new Vector3(0.5f)) * volumeScale;

            //计算相对圆柱中心的偏移
            Vector3 delta = worldPos - cylinderCenter;

            //沿轴方向的投影距离
            float along = Vector3.Dot(delta, cylinderAxis);

            //垂直轴方向的径向距离
            Vector3 radial = delta - along * cylinderAxis;
            float radialDist = radial.Length;

            //边界判断：侧面边界（径向距离接近半径）或 顶/底边界（沿轴距离接近半高）
            bool onSide = Math.Abs(radialDist - cylinderRadius) <= epsilon;
            bool onTopBottom = Math.Abs(Math.Abs(along) - cylinderHeight / 2.0f) <= epsilon && radialDist <= cylinderRadius;

            return onSide || onTopBottom;
        }
        #endregion

        #region # 判断体素是否在凸多面体内 —— static bool IsVoxelInConvexPolyhedron(in Vector3i voxelPosition...
        /// <summary>
        /// 判断体素是否在凸多面体内
        /// </summary>
        /// <param name="voxelPosition">体素坐标</param>
        /// <param name="volumeSize">体积尺寸</param>
        /// <param name="volumeScale">体积缩放</param>
        /// <param name="faces">凸多面体的面列表（平面方程，满足 dot(normal, point) + d ≤ 0 为内部）</param>
        /// <returns>是否在凸多面体内</returns>
        public static bool IsVoxelInConvexPolyhedron(in Vector3i voxelPosition, in Vector3i volumeSize, in Vector3 volumeScale, IReadOnlyList<Plane> faces)
        {
            //体素坐标 -> 世界坐标
            Vector3 texCoord = (voxelPosition + new Vector3(0.5f)) / volumeSize;
            Vector3 worldPos = (texCoord - new Vector3(0.5f)) * volumeScale;

            //检查是否在所有面的内侧
            foreach (Plane face in faces)
            {
                float distance = Vector3.Dot(face.Normal, worldPos) + face.Distance;
                if (distance > 0)
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region # 判断体素是否在凸多面体边界 —— static bool IsVoxelOnConvexPolyhedronBoundary(in Vector3i voxelPosition...
        /// <summary>
        /// 判断体素是否在凸多面体边界
        /// </summary>
        /// <param name="voxelPosition">体素坐标</param>
        /// <param name="volumeSize">体积尺寸</param>
        /// <param name="volumeScale">体积缩放</param>
        /// <param name="faces">凸多面体的面列表</param>
        /// <param name="epsilon">容差</param>
        /// <returns>是否在凸多面体边界</returns>
        public static bool IsVoxelOnConvexPolyhedronBoundary(in Vector3i voxelPosition, in Vector3i volumeSize, in Vector3 volumeScale, IReadOnlyList<Plane> faces, float epsilon = 0.005f)
        {
            //体素坐标 -> 世界坐标
            Vector3 texCoord = (voxelPosition + new Vector3(0.5f)) / volumeSize;
            Vector3 worldPos = (texCoord - new Vector3(0.5f)) * volumeScale;

            //检查是否有面距离接近0（在边界上）
            foreach (Plane face in faces)
            {
                float distance = Vector3.Dot(face.Normal, worldPos) + face.Distance;
                if (Math.Abs(distance) <= epsilon)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region # 求交凸包与平面 —— static IReadOnlyList<Vector3> IntersectConvexHullWithPlane(...
        /// <summary>
        /// 求交凸包与平面
        /// </summary>
        /// <param name="hullPositions">凸包位置列表（世界空间）</param>
        /// <param name="plane">MPR平面</param>
        /// <returns>有序交点列表（用于构建闭合Polyline）</returns>
        public static IReadOnlyList<Vector3> IntersectConvexHullWithPlane(IReadOnlyList<Vector3> hullPositions, MPRPlane plane)
        {
            #region # 验证

            if (hullPositions == null || hullPositions.Count < 3)
            {
                return [];
            }

            #endregion

            //提取凸包的真实棱边
            HashSet<(int, int)> edges = hullPositions.Count <= 8
                ? GeometryAlgorithms.EnumerateAllEdges(hullPositions.Count)
                : GeometryAlgorithms.ComputeConvexHullEdges(hullPositions);

            List<Vector3> intersections = [];
            Vector3 planeNormal = plane.WorldNormal.Normalized();
            float planeDistance = -Vector3.Dot(planeNormal, plane.WorldCenter);

            //计算每个顶点到平面的有符号距离
            float[] distances = new float[hullPositions.Count];
            for (int index = 0; index < hullPositions.Count; index++)
            {
                distances[index] = Vector3.Dot(planeNormal, hullPositions[index]) + planeDistance;
            }

            //遍历凸包的真实棱边
            const float epsilon = 1e-6f;
            foreach ((int i, int j) in edges)
            {
                float distance1 = distances[i];
                float distance2 = distances[j];

                //边与平面平行或不相交，跳过
                if (Math.Abs(distance1 - distance2) < epsilon)
                {
                    continue;
                }

                //边跨越平面
                if (distance1 * distance2 > 0)
                {
                    continue;
                }

                //插值求交点
                float t = distance1 / (distance1 - distance2);
                Vector3 intersection = Vector3.Lerp(hullPositions[i], hullPositions[j], t);

                //去重
                if (!GeometryAlgorithms.ContainsPoint(intersections, intersection, epsilon))
                {
                    intersections.Add(intersection);
                }
            }

            //剔除共线内部点
            intersections = GeometryAlgorithms.RemoveInteriorPoints(intersections, 1e-4f);

            #region # 验证

            if (intersections.Count < 3)
            {
                return [];
            }

            #endregion

            //按逆时针排序
            IReadOnlyList<Vector3> positions = GeometryAlgorithms.SortPointsCounterClockwise(intersections, planeNormal);

            return positions;
        }
        #endregion

        #region # 查找与给定向量垂直的单位向量 —— static Vector3 FindPerpendicularVector(Vector3 direction)
        /// <summary>
        /// 查找与给定向量垂直的单位向量
        /// </summary>
        /// <param name="direction">方向向量</param>
        /// <returns>垂直单位向量</returns>
        public static Vector3 FindPerpendicularVector(Vector3 direction)
        {
            if (Math.Abs(direction.X) <= Math.Abs(direction.Y) && Math.Abs(direction.X) <= Math.Abs(direction.Z))
            {
                return Vector3.Normalize(Vector3.Cross(direction, Vector3.UnitX));
            }
            else if (Math.Abs(direction.Y) <= Math.Abs(direction.Z))
            {
                return Vector3.Normalize(Vector3.Cross(direction, Vector3.UnitY));
            }
            else
            {
                return Vector3.Normalize(Vector3.Cross(direction, Vector3.UnitZ));
            }
        }
        #endregion

        #region # 绕任意轴旋转方向向量 —— static Vector3 RotateAroundAxis(Vector3 direction, Vector3 axis...
        /// <summary>
        /// 绕任意轴旋转方向向量
        /// </summary>
        /// <param name="direction">方向向量</param>
        /// <param name="axis">旋转轴</param>
        /// <param name="angle">旋转角度</param>
        /// <returns>旋转后方向向量</returns>
        /// <remarks>Rodrigues旋转公式</remarks>
        public static Vector3 RotateAroundAxis(Vector3 direction, Vector3 axis, float angle)
        {
            float cosA = MathF.Cos(angle);
            float sinA = MathF.Sin(angle);

            Vector3 rotatedDirection = direction * cosA
                                       + Vector3.Cross(axis, direction) * sinA
                                       + axis * Vector3.Dot(axis, direction) * (1f - cosA);

            return rotatedDirection;
        }
        #endregion

        #region # 计算点到线段最短距离点 —— static Vector3 ClosestPointOnSegment(Vector3 point...
        /// <summary>
        /// 计算点到线段最短距离点
        /// </summary>
        /// <param name="point">点</param>
        /// <param name="lineSegmentStart">线段起始点</param>
        /// <param name="lineSegmentEnd">线段终止点</param>
        /// <returns>最短距离点</returns>
        public static Vector3 ClosestPointOnSegment(Vector3 point, Vector3 lineSegmentStart, Vector3 lineSegmentEnd)
        {
            Vector3 lineSegment = lineSegmentEnd - lineSegmentStart;
            float lineSegmentLengthSq = lineSegment.LengthSquared;

            if (lineSegmentLengthSq < 1e-8f)
            {
                return lineSegmentStart;
            }

            float t = Math.Clamp(Vector3.Dot(point - lineSegmentStart, lineSegment) / lineSegmentLengthSq, 0, 1);
            Vector3 closestPoint = lineSegmentStart + t * lineSegment;

            return closestPoint;
        }
        #endregion
    }
}
