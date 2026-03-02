using Avalonia;
using Avalonia.Media;
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

        #region # Color转Vector4 —— static Color ToColor(this Vector4 vector4)
        /// <summary>
        /// Color转Vector4
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
    }
}
