using Caliburn.Micro;
using MedicalSharp.Controls.Visuals;

namespace MedicalSharp.Client.Events
{
    /// <summary>
    /// 形状同步事件
    /// </summary>
    public class ShapeSyncEvent
    {
        /// <summary>
        /// 事件发布者
        /// </summary>
        public Screen Publisher { get; set; }

        /// <summary>
        /// 形状
        /// </summary>
        public ShapeVisual3D Shape { get; set; }
    }
}
