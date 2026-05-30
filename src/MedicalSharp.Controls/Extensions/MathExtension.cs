using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Controls.Visual3Ds;
using MedicalSharp.Engine.Algorithms;
using MedicalSharp.Primitives.Maths;
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
        #region # Vector3D转Vector3 —— static Vector3 ToVector3(this in Vector3D vector3D)
        /// <summary>
        /// Vector3D转Vector3
        /// </summary>
        public static Vector3 ToVector3(this in Vector3D vector3D)
        {
            return new Vector3((float)vector3D.X, (float)vector3D.Y, (float)vector3D.Z);
        }
        #endregion

        #region # Vector3转Vector3D —— static Vector3D ToVector3(this in Vector3 vector3)
        /// <summary>
        /// Vector3转Vector3D
        /// </summary>
        public static Vector3D ToVector3(this in Vector3 vector3)
        {
            return new Vector3D(vector3.X, vector3.Y, vector3.Z);
        }
        #endregion

        #region # Color转Vector4 —— static Vector4 ToVector4(this in Color color)
        /// <summary>
        /// Color转Vector4
        /// </summary>
        public static Vector4 ToVector4(this in Color color)
        {
            float r = color.R * 1.0f / 255.0f;
            float g = color.G * 1.0f / 255.0f;
            float b = color.B * 1.0f / 255.0f;
            float a = color.A * 1.0f / 255.0f;

            return new Vector4(r, g, b, a);
        }
        #endregion

        #region # Vector4转Color —— static Color ToColor(this in Vector4 vector4)
        /// <summary>
        /// Vector4转Color
        /// </summary>
        public static Color ToColor(this in Vector4 vector4)
        {
            byte r = (byte)Math.Floor(vector4.X * 255.0f);
            byte g = (byte)Math.Floor(vector4.Y * 255.0f);
            byte b = (byte)Math.Floor(vector4.Z * 255.0f);
            byte a = (byte)Math.Floor(vector4.W * 255.0f);

            return new Color(a, r, g, b);
        }
        #endregion

        #region # Point转Vector2 —— static Vector2 ToVector2(this in Point point)
        /// <summary>
        /// Point转Vector2
        /// </summary>
        public static Vector2 ToVector2(this in Point point)
        {
            return new Vector2((float)point.X, (float)point.Y);
        }
        #endregion

        #region # Vector2转Point —— static Point ToPoint(this in Vector2 vector2)
        /// <summary>
        /// Vector2转Point
        /// </summary>
        public static Point ToPoint(this in Vector2 vector2)
        {
            return new Point(vector2.X, vector2.Y);
        }
        #endregion

        #region # PixelSize转Vector2 —— static Vector2 ToVector2(this in PixelSize pixelSize)
        /// <summary>
        /// PixelSize转Vector2
        /// </summary>
        public static Vector2 ToVector2(this in PixelSize pixelSize)
        {
            return new Vector2(pixelSize.Width, pixelSize.Height);
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
    }
}
