using MedicalSharp.Primitives.Maths;

namespace MedicalSharp.Presentation.Models
{
    /// <summary>
    /// 光线投射协议控制点
    /// </summary>
    public class RaycastProtocolPoint
    {
        /// <summary>
        /// 默认构造器
        /// </summary>
        public RaycastProtocolPoint() { }

        /// <summary>
        /// 创建光线投射协议控制点构造器
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="color">颜色</param>
        public RaycastProtocolPoint(float position, Color4f color)
            : this()
        {
            this.Position = position;
            this.Color = color;
        }

        /// <summary>
        /// 位置
        /// </summary>
        /// <remarks>值域: [0, 1]</remarks>
        public float Position { get; set; }

        /// <summary>
        /// 颜色
        /// </summary>
        public Color4f Color { get; set; }
    }
}
