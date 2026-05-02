using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace MedicalSharp.Primitives.Models
{
    /// <summary>
    /// 调整尺寸上下文(3D)
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ResizeContext3D
    {
        /// <summary>
        /// 锚点
        /// </summary>
        /// <remarks>世界坐标</remarks>
        public Vector3 Anchor;

        /// <summary>
        /// 伸缩方向
        /// </summary>
        /// <remarks>单位向量，世界坐标</remarks>
        public Vector3 Axis;

        /// <summary>
        /// 当前沿该轴的半长
        /// </summary>
        public float CurrentValue;
    }
}
