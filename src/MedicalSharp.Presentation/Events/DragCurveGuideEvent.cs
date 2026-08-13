using SD.Infrastructure.Avalonia.Caliburn.Base;

namespace MedicalSharp.Presentation.Events
{
    /// <summary>
    /// 拖拽曲线引导线事件
    /// </summary>
    public class DragCurveGuideEvent : CaliburnEvent
    {
        /// <summary>
        /// 弧长位置
        /// </summary>
        public float ArcPosition { get; set; }
    }
}
