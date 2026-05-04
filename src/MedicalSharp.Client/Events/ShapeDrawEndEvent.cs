using MedicalSharp.Controls.Visuals;
using SD.Infrastructure.Avalonia.Caliburn.Base;

namespace MedicalSharp.Client.Events
{
    /// <summary>
    /// 形状绘制完成事件
    /// </summary>
    public class ShapeDrawEndEvent : CaliburnEvent
    {
        /// <summary>
        /// 形状
        /// </summary>
        public ShapeVisual3D Shape { get; set; }
    }
}
