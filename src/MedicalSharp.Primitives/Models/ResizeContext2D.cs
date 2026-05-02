using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace MedicalSharp.Primitives.Models
{
    /// <summary>
    /// 调整尺寸上下文(2D)
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ResizeContext2D
    {
        /// <summary>
        /// 伸缩轴方向
        /// </summary>
        /// <remarks>UV空间</remarks>
        public Vector2 Axis;

        /// <summary>
        /// 当前值
        /// </summary>
        /// <remarks>宽度或高度</remarks>
        public float CurrentValue;
    }
}
