using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace MedicalSharp.Engine.Models
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
        /// 法向量
        /// </summary>
        public Vector3 Normal;
    }
}
