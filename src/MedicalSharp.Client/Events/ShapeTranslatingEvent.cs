using MedicalSharp.Controls.Visuals;
using SD.Infrastructure.Avalonia.Caliburn.Base;

namespace MedicalSharp.Client.Events
{
    /// <summary>
    /// 形状平移中事件
    /// </summary>
    public class ShapeTranslatingEvent : CaliburnEvent
    {
        /// <summary>
        /// 形状
        /// </summary>
        public ShapeVisual3D Shape { get; set; }
    }
}
