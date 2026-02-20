using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace MedicalSharp.Engine.ValueTypes
{
    /// <summary>
    /// 传输函数控制点
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public record struct TFControlPoint
    {
        /// <summary>
        /// 位置
        /// </summary>
        /// <remarks>值域: [0, 1]</remarks>
        public float Position;

        /// <summary>
        /// 颜色
        /// </summary>
        public Vector4 Color;

        /// <summary>
        /// 创建传输函数控制点构造器
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="color">颜色</param>
        public TFControlPoint(float position, Vector4 color)
            : this()
        {
            this.Position = MathHelper.Clamp(position, 0.0f, 1.0f);
            this.Color = color;
        }
    }
}
