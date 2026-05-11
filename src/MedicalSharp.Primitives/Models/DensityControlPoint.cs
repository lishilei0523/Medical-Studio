using OpenTK.Mathematics;
using System;
using System.Runtime.InteropServices;

namespace MedicalSharp.Primitives.Models
{
    /// <summary>
    /// 密度控制点
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct DensityControlPoint
    {
        /// <summary>
        /// 创建密度控制点构造器
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="color">颜色</param>
        public DensityControlPoint(float position, Vector4 color)
            : this()
        {
            this.Position = Math.Clamp(position, 0, 1.0f);
            this.Color = color;
        }

        /// <summary>
        /// 位置
        /// </summary>
        /// <remarks>值域: [0, 1]</remarks>
        public readonly float Position;

        /// <summary>
        /// 颜色
        /// </summary>
        public readonly Vector4 Color;
    }
}
