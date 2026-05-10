using MedicalSharp.Controls.Visuals;
using SD.Infrastructure.Avalonia.Caliburn.Base;

namespace MedicalSharp.Presentation.Events
{
    /// <summary>
    /// 形状已删除事件
    /// </summary>
    public class ShapeRemovedEvent : CaliburnEvent
    {
        /// <summary>
        /// 形状
        /// </summary>
        public ShapeVisual3D Shape { get; set; }
    }
}
