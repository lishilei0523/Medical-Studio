using SD.Infrastructure.Avalonia.Caliburn.Base;

namespace MedicalSharp.Presentation.Events
{
    /// <summary>
    /// 全局繁忙事件
    /// </summary>
    public class GlobalBusyEvent : CaliburnEvent
    {
        /// <summary>
        /// 是否繁忙
        /// </summary>
        public bool IsBusy { get; set; }
    }
}
