using MIConvexHull;
using OpenTK.Mathematics;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MedicalSharp.Engine.Algorithms
{
    /// <summary>
    /// 几何算法
    /// </summary>
    public static class GeometryAlgorithms
    {
        #region # 保存图像 —— static void SaveImage(int viewportWidth, int viewportHeight...
        /// <summary>
        /// 保存图像
        /// </summary>
        /// <remarks>用于调试</remarks>
        public static unsafe void SaveImage(int viewportWidth, int viewportHeight, byte[] layerPixels, Vector2[] screenCorners)
        {
            //翻转生成SK图像
            using SKBitmap bitmap = new SKBitmap(viewportWidth, viewportHeight, SKColorType.Rgba8888, SKAlphaType.Unpremul);
            byte* targetPtr = (byte*)bitmap.GetPixels().ToPointer();
            fixed (byte* sourcePtr = layerPixels)
            {
                int stride = viewportWidth * 4;
                for (int y = 0; y < viewportHeight; y++)
                {
                    int sourceY = viewportHeight - 1 - y;  //翻转Y轴
                    byte* sourceRow = sourcePtr + sourceY * stride;
                    byte* targetRow = targetPtr + y * stride;

                    //复制整行（RGBA -> RGBA，顺序相同）
                    Buffer.MemoryCopy(sourceRow, targetRow, stride, stride);
                }
            }

            //定义矩形
            int minX = (int)Math.Min(Math.Min(screenCorners[0].X, screenCorners[1].X), Math.Min(screenCorners[2].X, screenCorners[3].X));
            int maxX = (int)Math.Max(Math.Max(screenCorners[0].X, screenCorners[1].X), Math.Max(screenCorners[2].X, screenCorners[3].X));
            int minY = (int)Math.Min(Math.Min(screenCorners[0].Y, screenCorners[1].Y), Math.Min(screenCorners[2].Y, screenCorners[3].Y));
            int maxY = (int)Math.Max(Math.Max(screenCorners[0].Y, screenCorners[1].Y), Math.Max(screenCorners[2].Y, screenCorners[3].Y));
            SKRect reactangle = SKRect.Create(minX, minY, maxX - minX, maxY - minY);

            //绘制矩形
            using SKCanvas canvas = new SKCanvas(bitmap);
            using SKPaint fill = new SKPaint();
            using SKPaint stroke = new SKPaint();
            fill.Style = SKPaintStyle.Fill;
            fill.Color = SKColors.White;
            fill.IsAntialias = true;
            stroke.Style = SKPaintStyle.Stroke;
            stroke.Color = SKColors.Black;
            stroke.StrokeWidth = 1;
            stroke.IsAntialias = true;
            canvas.DrawRect(reactangle, fill);
            canvas.DrawRect(reactangle, stroke);

            //保存文件
            using FileStream stream = File.OpenWrite("MPR.png");
            bitmap.Encode(SKEncodedImageFormat.Png, 80).SaveTo(stream);
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
        /// <returns>是否在多边形内</returns>
        /// <remarks>射线投射法</remarks>
        public static bool IsPointInPolygon(in Vector2 point, IReadOnlyList<Vector2> polygon)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                Vector2 vi = polygon[i];
                Vector2 vj = polygon[j];
                bool intersect = ((vi.Y > point.Y) != (vj.Y > point.Y)) &&
                                 (point.X < (vj.X - vi.X) * (point.Y - vi.Y) / (vj.Y - vi.Y) + vi.X);
                if (intersect)
                {
                    inside = !inside;
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

        #region # 判断体素是否在立方体内 —— static bool IsVoxelInBox(in Vector3i voxelPosition, in Vector3 boxMin...
        /// <summary>
        /// 判断体素是否在立方体内
        /// </summary>
        /// <param name="voxelPosition">体素坐标</param>
        /// <param name="volumeSize">体积尺寸</param>
        /// <param name="volumeScale">体积缩放</param>
        /// <param name="boxMin">立方体最小点（局部空间）</param>
        /// <param name="boxMax">立方体最大点（局部空间）</param>
        /// <param name="worldToLocal">世界到局部变换矩阵</param>
        /// <returns>是否在立方体内</returns>
        public static bool IsVoxelInBox(in Vector3i voxelPosition, in Vector3i volumeSize, in Vector3 volumeScale, in Vector3 boxMin, in Vector3 boxMax, in Matrix4 worldToLocal)
        {
            //体素坐标 -> 纹理坐标 -> 世界坐标 -> 局部坐标
            Vector3 texCoord = (voxelPosition + new Vector3(0.5f)) / volumeSize;
            Vector3 worldPos = (texCoord - new Vector3(0.5f)) * volumeScale;
            Vector3 localPos = Vector3.TransformPosition(worldPos, worldToLocal);
            bool isInBox = localPos.X >= boxMin.X && localPos.X <= boxMax.X &&
                           localPos.Y >= boxMin.Y && localPos.Y <= boxMax.Y &&
                           localPos.Z >= boxMin.Z && localPos.Z <= boxMax.Z;

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
        /// <param name="boxMin">立方体最小点（局部空间）</param>
        /// <param name="boxMax">立方体最大点（局部空间）</param>
        /// <param name="worldToLocal">世界到局部变换矩阵</param>
        /// <returns>是否在立方体边界</returns>
        /// <remarks>6个面</remarks>
        public static bool IsVoxelOnBoxBoundary(in Vector3i voxelPosition, in Vector3i volumeSize, in Vector3 volumeScale, in Vector3 boxMin, in Vector3 boxMax, in Matrix4 worldToLocal)
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
                if (!IsVoxelInBox(neighborPosition, volumeSize, volumeScale, boxMin, boxMax, worldToLocal))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
