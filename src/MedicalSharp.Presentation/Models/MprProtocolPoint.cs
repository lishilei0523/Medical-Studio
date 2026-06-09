using MedicalSharp.Primitives.Maths;

namespace MedicalSharp.Presentation.Models
{
    /// <summary>
    /// MPR伪彩协议控制点
    /// </summary>
    public class MprProtocolPoint
    {
        /// <summary>
        /// 默认构造器
        /// </summary>
        public MprProtocolPoint() { }

        /// <summary>
        /// 创建MPR伪彩协议控制点构造器
        /// </summary>
        /// <param name="hu">HU值</param>
        /// <param name="color">颜色</param>
        public MprProtocolPoint(short hu, Color4f color)
            : this()
        {
            this.HU = hu;
            this.Color = color;
        }

        /// <summary>
        /// HU值
        /// </summary>
        public short HU { get; set; }

        /// <summary>
        /// 颜色
        /// </summary>
        public Color4f Color { get; set; }
    }
}
