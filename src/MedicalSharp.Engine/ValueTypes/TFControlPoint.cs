using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace MedicalSharp.Engine.ValueTypes
{
    /// <summary>
    /// 传输函数控制点
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TFControlPoint
    {
        /// <summary>
        /// 位置
        /// </summary>
        public float Position;

        /// <summary>
        /// 颜色
        /// </summary>
        public Vector4 Color;
    }
}
