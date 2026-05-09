using MedicalSharp.Primitives.Enums;

namespace MedicalSharp.Primitives.Models.Arguments
{
    /// <summary>
    /// MPR平面变化事件参数
    /// </summary>
    public class MPRPlaneChangedEventArgs
    {
        /// <summary>
        /// 默认构造器
        /// </summary>
        /// <param name="triggerSource">触发源</param>
        public MPRPlaneChangedEventArgs(MPRPlaneChangeSource triggerSource)
        {
            this.TriggerSource = triggerSource;
        }

        /// <summary>
        /// 触发源
        /// </summary>
        public MPRPlaneChangeSource TriggerSource { get; private set; }
    }
}
