using MedicalSharp.Controls.Visual3Ds;
using SD.Infrastructure.Avalonia.Caliburn.Base;

namespace MedicalSharp.Presentation.Events
{
    /// <summary>
    /// 删除形状事件
    /// </summary>
    public class RemoveShapeEvent : CaliburnEvent
    {
        /// <summary>
        /// 形状
        /// </summary>
        public ShapeVisual3D Shape { get; set; }
    }
}
