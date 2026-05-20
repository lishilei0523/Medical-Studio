using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Primitives.Maths;
using MIConvexHull;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalSharp.Controls.Extensions
{
    /// <summary>
    /// 数学相关扩展
    /// </summary>
    public static class MathExtension
    {
        //Public

        #region # Vector3D转Vector3 —— static Vector3 ToVector3(this Vector3D vector3D)
        /// <summary>
        /// Vector3D转Vector3
        /// </summary>
        public static Vector3 ToVector3(this Vector3D vector3D)
        {
            return new Vector3((float)vector3D.X, (float)vector3D.Y, (float)vector3D.Z);
        }
        #endregion

        #region # Vector3转Vector3D —— static Vector3D ToVector3(this Vector3 vector3)
        /// <summary>
        /// Vector3转Vector3D
        /// </summary>
        public static Vector3D ToVector3(this Vector3 vector3)
        {
            return new Vector3D(vector3.X, vector3.Y, vector3.Z);
        }
        #endregion

        #region # Color转Vector4 —— static Vector4 ToVector4(this Color color)
        /// <summary>
        /// Color转Vector4
        /// </summary>
        public static Vector4 ToVector4(this Color color)
        {
            float r = color.R * 1.0f / 255.0f;
            float g = color.G * 1.0f / 255.0f;
            float b = color.B * 1.0f / 255.0f;
            float a = color.A * 1.0f / 255.0f;

            return new Vector4(r, g, b, a);
        }
        #endregion

        #region # Vector4转Color —— static Color ToColor(this Vector4 vector4)
        /// <summary>
        /// Vector4转Color
        /// </summary>
        public static Color ToColor(this Vector4 vector4)
        {
            byte r = (byte)Math.Floor(vector4.X * 255.0f);
            byte g = (byte)Math.Floor(vector4.Y * 255.0f);
            byte b = (byte)Math.Floor(vector4.Z * 255.0f);
            byte a = (byte)Math.Floor(vector4.W * 255.0f);

            return new Color(a, r, g, b);
        }
        #endregion

        #region # Point转Vector2 —— static Vector2 ToVector2(this Point point)
        /// <summary>
        /// Point转Vector2
        /// </summary>
        public static Vector2 ToVector2(this Point point)
        {
            return new Vector2((float)point.X, (float)point.Y);
        }
        #endregion

        #region # Vector2转Point —— static Point ToPoint(this Vector2 vector2)
        /// <summary>
        /// Vector2转Point
        /// </summary>
        public static Point ToPoint(this Vector2 vector2)
        {
            return new Point(vector2.X, vector2.Y);
        }
        #endregion

        #region # PixelSize转Vector2 —— static Vector2 ToVector2(this PixelSize pixelSize)
        /// <summary>
        /// PixelSize转Vector2
        /// </summary>
        public static Vector2 ToVector2(this PixelSize pixelSize)
        {
            return new Vector2(pixelSize.Width, pixelSize.Height);
        }
        #endregion

        #region # System.Numerics三维向量转GLM三维向量 —— static Vector3 ToGlmVector3(this in Vector3 vector3)
        /// <summary>
        /// System.Numerics三维向量转GLM三维向量
        /// </summary>
        public static Vector3 ToGlmVector3(this in System.Numerics.Vector3 vector3)
        {
            return new Vector3(vector3.X, vector3.Y, vector3.Z);
        }
        #endregion

        #region # GLM三维向量转System.Numerics三维向量 —— Vector3 ToSystemVector3(this in Vector3 vector3)
        /// <summary>
        /// GLM三维向量转System.Numerics三维向量
        /// </summary>
        public static System.Numerics.Vector3 ToSystemVector3(this in Vector3 vector3)
        {
            return new System.Numerics.Vector3(vector3.X, vector3.Y, vector3.Z);
        }
        #endregion

        #region # 判断3D元素是否在MPR平面上 —— static bool IsOnPlane(this Visual3D visual3D...
        /// <summary>
        /// 判断3D元素是否在MPR平面上
        /// </summary>
        /// <param name="visual3D">3D元素</param>
        /// <param name="plane">MPR平面</param>
        /// <param name="epsilon">容差</param>
        /// <returns>是否在MPR平面上</returns>
        public static bool IsOnPlane(this Visual3D visual3D, MPRPlane plane, float epsilon = 0.001f)
        {
            //线条类：始终显示
            if (visual3D is ILineBasedVisual3D)
            {
                return true;
            }

            //点：世界空间点面距离
            if (visual3D is PointVisual3D pointVisual3D)
            {
                Vector3 localPosition = pointVisual3D.Position.ToVector3();
                Vector3 worldPosition = Vector3.TransformPosition(localPosition, visual3D.Transform.Matrix);
                float shapeDistance = Vector3.Dot(worldPosition, plane.WorldNormal);
                float planeDistance = Vector3.Dot(plane.WorldCenter, plane.WorldNormal);
                float diffDistance = Math.Abs(shapeDistance - planeDistance);

                return diffDistance < epsilon;
            }

            //2D物体：法向量方向一致 + 平面上一点距离
            if (visual3D is IVisual2DIn3D visual2DIn3D)
            {
                Vector3 localNormal = visual2DIn3D.Normal.ToVector3();
                Vector3 worldNormal = Vector3.TransformNormal(localNormal, visual3D.Transform.Matrix).Normalized();
                Vector3 planeNormal = plane.WorldNormal.Normalized();
                if (Math.Abs(Vector3.Dot(worldNormal, planeNormal)) < 0.999f)
                {
                    return false;
                }

                Vector3 localPosition = visual2DIn3D.PointOnPlane.ToVector3();
                Vector3 worldPosition = Vector3.TransformPosition(localPosition, visual3D.Transform.Matrix);
                float shapeDistance = Vector3.Dot(worldPosition, planeNormal);
                float planeDistance = Vector3.Dot(plane.WorldCenter, planeNormal);
                float diffDistance = Math.Abs(shapeDistance - planeDistance);

                return diffDistance < epsilon;
            }

            //纯3D物体：包围盒相交
            if (visual3D is IPureVisual3D)
            {
                BoundingBox worldBox = visual3D.Bounds.Transform(visual3D.Transform.Matrix);
                Vector3 planeNormal = plane.WorldNormal.Normalized();
                float planeDistance = -Vector3.Dot(planeNormal, plane.WorldCenter);

                //包围盒相对于平面的最小/最大有符号距离
                float minDistance = float.MaxValue;
                float maxDistance = float.MinValue;
                foreach (Vector3 corner in worldBox.Corners)
                {
                    float distance = Vector3.Dot(planeNormal, corner) + planeDistance;
                    minDistance = Math.Min(minDistance, distance);
                    maxDistance = Math.Max(maxDistance, distance);
                }

                //平面与包围盒相交：符号距离跨越零
                return minDistance * maxDistance <= 0;
            }

            return false;
        }
        #endregion

        #region # 求交凸包与平面 —— static IReadOnlyList<Vector3D> IntersectConvexHullWithPlane(...
        /// <summary>
        /// 求交凸包与平面
        /// </summary>
        /// <param name="hullPositions">凸包位置列表（世界空间）</param>
        /// <param name="plane">MPR平面</param>
        /// <returns>有序交点列表（用于构建闭合Polyline）</returns>
        public static IReadOnlyList<Vector3D> IntersectConvexHullWithPlane(IReadOnlyList<Vector3> hullPositions, MPRPlane plane)
        {
            #region # 验证

            if (hullPositions == null || hullPositions.Count < 3)
            {
                return [];
            }

            #endregion

            //提取凸包的真实棱边
            HashSet<(int, int)> edges = hullPositions.Count <= 8
                ? EnumerateAllEdges(hullPositions.Count)
                : ComputeConvexHullEdges(hullPositions);

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
                Vector3 intersection = Lerp(hullPositions[i], hullPositions[j], t);

                //去重
                if (!ContainsPoint(intersections, intersection, epsilon))
                {
                    intersections.Add(intersection);
                }
            }

            //剔除共线内部点
            intersections = RemoveInteriorPoints(intersections, 1e-4f);

            #region # 验证

            if (intersections.Count < 3)
            {
                return [];
            }

            #endregion

            //按逆时针排序
            IReadOnlyList<Vector3> positions = SortPointsCounterClockwise(intersections, planeNormal);

            return positions.Select(x => x.ToVector3()).ToList();
        }
        #endregion

        #region # 创建纯3D元素截面多边形 —— static PolylineVisual3D CreateSectionPolygon(this IPureVisual3D...
        /// <summary>
        /// 创建纯3D元素截面多边形
        /// </summary>
        public static PolylineVisual3D CreateSectionPolygon(this IPureVisual3D pureVisual3D, MPRPlane plane)
        {
            IReadOnlyList<Vector3> hullPositions = pureVisual3D.GetConvexHullPositions();

            #region # 验证

            if (hullPositions == null || hullPositions.Count < 3)
            {
                return null;
            }

            #endregion

            IReadOnlyList<Vector3D> intersectionPoints = IntersectConvexHullWithPlane(hullPositions, plane);

            #region # 验证

            if (intersectionPoints == null || intersectionPoints.Count < 3)
            {
                return null;
            }

            #endregion

            PolylineVisual3D polyline = new PolylineVisual3D
            {
                Stroke = new Vector4(0.1f, 0.3f, 0.6f, 1.0f).ToColor(),
                Fill = new Vector4(0.6f, 0.8f, 1.0f, 0.4f).ToColor(),
                Positions = new AvaloniaList<Vector3D>(intersectionPoints),
                Closed = true,
                Fixed = true
            };

            return polyline;
        }
        #endregion


        //Private

        #region # 线性插值 —— static Vector3 Lerp(Vector3 a, Vector3 b, float t)
        /// <summary>
        /// 线性插值
        /// </summary>
        private static Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            return new Vector3(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t, a.Z + (b.Z - a.Z) * t);
        }
        #endregion

        #region # 判断包含 —— static bool ContainsPoint(IReadOnlyList<Vector3> points...
        /// <summary>
        /// 判断包含
        /// </summary>
        private static bool ContainsPoint(IReadOnlyList<Vector3> points, Vector3 point, float epsilon)
        {
            foreach (Vector3 position in points)
            {
                float dx = position.X - point.X;
                float dy = position.Y - point.Y;
                float dz = position.Z - point.Z;
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
        private static IReadOnlyList<Vector3> SortPointsCounterClockwise(IReadOnlyList<Vector3> points, Vector3 planeNormal)
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
        /// <returns>去重后的边列表（顶点索引对）</returns>
        private static HashSet<(int, int)> ComputeConvexHullEdges(IReadOnlyList<Vector3> hullPositions)
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
        /// <returns>所有点对</returns>
        private static HashSet<(int, int)> EnumerateAllEdges(int count)
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
        /// 剔除内部点：如果某点落在其他两点的连线上（共线），则它是内部点，丢弃
        /// </summary>
        private static List<Vector3> RemoveInteriorPoints(List<Vector3> points, float epsilon)
        {
            if (points.Count <= 3)
            {
                return points;
            }

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

        #region # 判断点是否在线段上 —— static bool IsPointOnSegment(Vector3 p, Vector3 a...
        /// <summary>
        /// 判断点p是否在线段ab上
        /// </summary>
        private static bool IsPointOnSegment(Vector3 p, Vector3 a, Vector3 b, float epsilon)
        {
            Vector3 ab = b - a;
            Vector3 ap = p - a;

            float abLengthSq = ab.LengthSquared;
            if (abLengthSq < epsilon * epsilon)
            {
                return false;  //a和b重合
            }

            //投影参数 t = (ap·ab) / |ab|²
            float t = Vector3.Dot(ap, ab) / abLengthSq;

            //t必须在[0, 1]之间
            if (t < 0 || t > 1)
            {
                return false;
            }

            //投影点
            Vector3 projection = a + t * ab;

            //距离判断
            float squaredDistance = (p - projection).LengthSquared;
            bool isOnSegment = squaredDistance < epsilon * epsilon;

            return isOnSegment;
        }
        #endregion
    }
}
