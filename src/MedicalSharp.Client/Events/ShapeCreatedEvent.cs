using MedicalSharp.Controls.Visuals;

namespace MedicalSharp.Client.Events
{
    /// <summary>
    /// 形状创建事件
    /// </summary>
    public class ShapeCreatedEvent
    {
        /// <summary>
        /// 默认构造器
        /// </summary>
        /// <param name="shape">形状</param>
        public ShapeCreatedEvent(ShapeVisual3D shape)
        {
            this.Shape = shape;
        }

        /// <summary>
        /// 形状
        /// </summary>
        public ShapeVisual3D Shape { get; set; }
    }
}
