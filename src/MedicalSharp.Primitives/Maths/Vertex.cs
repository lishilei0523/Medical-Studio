using OpenTK.Mathematics;
using System;
using System.Runtime.InteropServices;

namespace MedicalSharp.Primitives.Maths
{
    /// <summary>
    /// 顶点
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex : IEquatable<Vertex>
    {
        /// <summary>
        /// 位置
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// 颜色
        /// </summary>
        public Vector4 Color;

        /// <summary>
        /// 纹理坐标
        /// </summary>
        public Vector2 TextureCoord;

        /// <summary>
        /// 法向量
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// 是否相等
        /// </summary>
        public bool Equals(Vertex other)
        {
            bool equals = this.Position.Equals(other.Position) &&
                          this.Color.Equals(other.Color) &&
                          this.TextureCoord.Equals(other.TextureCoord) &&
                          this.Normal.Equals(other.Normal);

            return equals;
        }

        /// <summary>
        /// 是否相等
        /// </summary>
        public override bool Equals(object obj)
        {
            bool equals = obj is Vertex other && this.Equals(other);

            return equals;
        }

        /// <summary>
        /// 获取哈希码
        /// </summary>
        public override int GetHashCode()
        {
            int hashCode = HashCode.Combine(this.Position, this.Color, this.TextureCoord, this.Normal);

            return hashCode;
        }

        /// <summary>
        /// 相等运算符
        /// </summary>
        public static bool operator ==(Vertex left, Vertex right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 不等运算符
        /// </summary>
        public static bool operator !=(Vertex left, Vertex right)
        {
            return !left.Equals(right);
        }
    }
}
