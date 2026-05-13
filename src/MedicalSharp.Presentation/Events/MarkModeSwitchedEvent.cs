using MedicalSharp.Primitives.Enums;
using SD.Infrastructure.Avalonia.Caliburn.Base;

namespace MedicalSharp.Presentation.Events
{
    /// <summary>
    /// 标记模式切换事件
    /// </summary>
    public class MarkModeSwitchedEvent : CaliburnEvent
    {
        /// <summary>
        /// 标记值
        /// </summary>
        public byte MarkValue { get; set; }

        /// <summary>
        /// 标记值
        /// </summary>
        public MarkMode MarkMode { get; set; }
    }
}
