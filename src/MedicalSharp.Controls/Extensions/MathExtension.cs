using Avalonia;
using Avalonia.Media;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Primitives.Maths;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Controls.Extensions
{
    /// <summary>
    /// 数学相关扩展
    /// </summary>
    public static class MathExtension
    {
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
        public static bool IsOnPlane(this Visual3D visual3D, MPRPlane plane, float epsilon = 0.002f)
        {
            if (visual3D is ILineBasedVisual3D)
            {
                return true;
            }
            if (visual3D is PointVisual3D)
            {
                float shapeDist = Vector3.Dot(visual3D.Transform.Position, plane.Normal);
                float planeDist = Vector3.Dot(plane.GetPointOnPlane(0, 0), plane.Normal);

                return Math.Abs(shapeDist - planeDist) < epsilon;
            }
            if (visual3D is IVisual2DIn3D visual2DIn3D)
            {
                if (Math.Abs(Vector3.Dot(visual2DIn3D.Normal.ToVector3(), plane.Normal)) < 0.999f)
                {
                    return false;
                }

                float shapeDist = Vector3.Dot(visual3D.Transform.Position, plane.Normal);
                float planeDist = Vector3.Dot(plane.GetPointOnPlane(0, 0), plane.Normal);

                return Math.Abs(shapeDist - planeDist) < epsilon;
            }

            //TODO 3D图形求切面

            return false;
        }
        #endregion
    }
}
