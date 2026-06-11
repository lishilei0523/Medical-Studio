using System.ComponentModel;

namespace MedicalSharp.Primitives.Enums
{
    /// <summary>
    /// 插值模式
    /// </summary>
    public enum InterpolationMode : byte
    {
        /// <summary>
        /// 线性
        /// </summary>
        [Description("线性")]
        Linear = 0,

        /// <summary>
        /// 步进
        /// </summary>
        [Description("步进")]
        Step = 1,

        /// <summary>
        /// 平滑步进
        /// </summary>
        [Description("平滑步进")]
        SmoothStep = 2
    }
}
