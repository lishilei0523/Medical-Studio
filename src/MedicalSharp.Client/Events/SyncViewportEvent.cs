using Caliburn.Micro;

namespace MedicalSharp.Client.Events
{
    /// <summary>
    /// 同步视口事件
    /// </summary>
    public class SyncViewportEvent
    {
        /// <summary>
        /// 事件发布者
        /// </summary>
        public Screen Publisher { get; set; }
    }
}
