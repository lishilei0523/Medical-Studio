using Avalonia.Media;
using SD.Infrastructure.Avalonia.Caliburn.Base;

namespace MedicalSharp.Presentation.Events
{
    /// <summary>
    /// 标记颜色改变事件
    /// </summary>
    public class MarkColorChangedEvent : CaliburnEvent
    {
        /// <summary>
        /// 标记值
        /// </summary>
        public byte MarkValue { get; set; }

        /// <summary>
        /// 颜色
        /// </summary>
        public Color Color { get; set; }
    }
}
