using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Controls.Visual3Ds;
using MedicalSharp.Primitives.Algorithms;
using MedicalSharp.Primitives.Builders;
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

        #region # Vector4转Color —— static Color ToSolidColor(this in Vector4 vector4)
        /// <summary>
        /// Vector4转Color
        /// </summary>
        public static Color ToSolidColor(this in Vector4 vector4)
        {
            byte r = (byte)Math.Floor(vector4.X * 255.0f);
            byte g = (byte)Math.Floor(vector4.Y * 255.0f);
            byte b = (byte)Math.Floor(vector4.Z * 255.0f);

            return new Color(255, r, g, b);
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
            //线条类/点云：始终显示
            if (visual3D is ILineBasedVisual3D or PointCloudVisual3D)
            {
                return true;
            }

            //文本
            if (visual3D is TextVisual3D text)
            {
                Vector3 localNormal = text.Normal.ToVector3();
                Vector3 worldNormal = Vector3.TransformNormal(localNormal, visual3D.Transform.Matrix).Normalized();
                Vector3 planeNormal = plane.WorldNormal.Normalized();
                if (Math.Abs(Vector3.Dot(worldNormal, planeNormal)) < 0.999f)
                {
                    return false;
                }

                float shapeDistance = Vector3.Dot(text.Transform.Position, planeNormal);
                float planeDistance = Vector3.Dot(plane.WorldCenter, planeNormal);
                float diffDistance = Math.Abs(shapeDistance - planeDistance);

                return diffDistance < epsilon;
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

            Vector3D[] intersectionPoints = GeometryAlgorithms.IntersectConvexHullWithPlane(hullPositions, plane)?.Select(x => x.ToVector3()).ToArray();

            #region # 验证

            if (intersectionPoints == null || intersectionPoints.Length < 3)
            {
                return null;
            }

            #endregion

            bool isSelected = ((ShapeVisual3D)pureVisual3D).IsSelected;
            PolylineVisual3D polyline = new PolylineVisual3D
            {
                Stroke = isSelected ? ColorFactory.SelectedStroke.ToColor() : ColorFactory.Stroke3D.ToColor(),
                Fill = isSelected ? ColorFactory.SelectedFill.ToColor() : ColorFactory.Fill3D.ToColor(),
                Positions = new AvaloniaList<Vector3D>(intersectionPoints),
                Closed = true,
                Fixed = true
            };

            return polyline;
        }
        #endregion

        #region # 创建CPR形状 —— static ShapeVisual3D CreateCprShape(this ShapeVisual3D shape...
        /// <summary>
        /// 创建CPR形状
        /// </summary>
        /// <param name="shape">原始形状</param>
        /// <param name="viewport">CPR渲染视口</param>
        /// <returns>CPR局部空间中的临时形状</returns>
        /// <remarks>创建临时形状用于CPR视图渲染，将世界空间形状转换为CPR局部空间（UnitPlane -0.5~0.5）</remarks>
        public static ShapeVisual3D CreateCprShape(this ShapeVisual3D shape, CPRViewport viewport)
        {
            if (shape is LineSegmentVisual3D lineSegment)
            {
                //获取线段两端点的世界坐标
                Vector3 worldStart = Vector3.TransformPosition(lineSegment.StartPoint.ToVector3(), lineSegment.Transform.Matrix);
                Vector3 worldEnd = Vector3.TransformPosition(lineSegment.EndPoint.ToVector3(), lineSegment.Transform.Matrix);

                //世界坐标 -> CPR局部坐标
                Vector3 localStart = worldStart.ToCprLocalPosition(viewport.Curve, viewport.CPRMode, viewport.RadialWidth, viewport.RotationAngle, viewport.StraightenDirection, viewport.ProjectionAxis, viewport.CPRRenderer.ProjectionRange, viewport.CrossSectionSize);
                Vector3 localEnd = worldEnd.ToCprLocalPosition(viewport.Curve, viewport.CPRMode, viewport.RadialWidth, viewport.RotationAngle, viewport.StraightenDirection, viewport.ProjectionAxis, viewport.CPRRenderer.ProjectionRange, viewport.CrossSectionSize);
                LineSegmentVisual3D cprLine = new LineSegmentVisual3D
                {
                    StartPoint = localStart.ToVector3(),
                    EndPoint = localEnd.ToVector3(),
                    Stroke = lineSegment.Stroke,
                    StrokeThickness = lineSegment.StrokeThickness
                };

                return cprLine;
            }

            return shape;
        }
        #endregion
    }
}
