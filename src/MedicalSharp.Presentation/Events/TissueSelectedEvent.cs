using MedicalSharp.Presentation.Models;
using SD.Infrastructure.Avalonia.Caliburn.Base;

namespace MedicalSharp.Presentation.Events
{
    /// <summary>
    /// 组织选中事件
    /// </summary>
    public class TissueSelectedEvent : CaliburnEvent
    {
        /// <summary>
        /// 组织信息
        /// </summary>
        public TissueInfo TissueInfo { get; set; }
    }
}
