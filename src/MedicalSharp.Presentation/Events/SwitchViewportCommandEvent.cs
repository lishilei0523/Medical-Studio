using MedicalSharp.Controls.Interfaces;
using SD.Infrastructure.Avalonia.Caliburn.Base;

namespace MedicalSharp.Presentation.Events
{
    /// <summary>
    /// 切换视口命令事件
    /// </summary>
    public class SwitchViewportCommandEvent : CaliburnEvent
    {
        /// <summary>
        /// 视口命令
        /// </summary>
        public IViewportCommand Command { get; set; }
    }
}
