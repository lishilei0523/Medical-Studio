using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace MedicalSharp.Primitives.Models
{
    /// <summary>
    /// 传递函数控制点
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public record struct TFControlPoint
    {
        /// <summary>
        /// 创建传递函数控制点构造器
        /// </summary>
        /// <param name="huValue">HU值</param>
        /// <param name="color">颜色</param>
        public TFControlPoint(short huValue, Vector4 color)
            : this()
        {
            this.Color = color;
            this.HU = huValue;
            this.Position = float.Epsilon;
        }

        /// <summary>
        /// HU值
        /// </summary>
        public short HU;

        /// <summary>
        /// 颜色
        /// </summary>
        public Vector4 Color;

        /// <summary>
        /// 位置
        /// </summary>
        /// <remarks>值域: [0, 1]</remarks>
        public float Position;
    }
}
