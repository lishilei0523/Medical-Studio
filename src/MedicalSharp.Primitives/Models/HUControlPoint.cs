using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace MedicalSharp.Primitives.Models
{
    /// <summary>
    /// HU控制点
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct HUControlPoint
    {
        /// <summary>
        /// 创建HU控制点构造器
        /// </summary>
        /// <param name="hu">HU值</param>
        /// <param name="color">颜色</param>
        public HUControlPoint(short hu, Vector4 color)
            : this()
        {
            this.HU = hu;
            this.Color = color;
        }

        /// <summary>
        /// HU值
        /// </summary>
        public readonly short HU;

        /// <summary>
        /// 颜色
        /// </summary>
        public readonly Vector4 Color;
    }
}
