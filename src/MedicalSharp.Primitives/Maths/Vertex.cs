using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace MedicalSharp.Primitives.Maths
{
    /// <summary>
    /// 顶点
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        /// <summary>
        /// 位置
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// 纹理坐标
        /// </summary>
        public Vector2 TextureCoord;

        /// <summary>
        /// 法向量
        /// </summary>
        public Vector3 Normal;
    }
}
