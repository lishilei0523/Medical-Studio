using MedicalSharp.Primitives.Models;
using SD.Infrastructure.Avalonia.Caliburn.Base;

namespace MedicalSharp.Presentation.Events
{
    /// <summary>
    /// 统计完成事件
    /// </summary>
    public class StatisticFinishedEvent : CaliburnEvent
    {
        /// <summary>
        /// 统计结果
        /// </summary>
        public StatisticResult StatisticResult { get; set; }
    }
}
