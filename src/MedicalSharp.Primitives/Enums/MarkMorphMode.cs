using System.ComponentModel;

namespace MedicalSharp.Primitives.Enums
{
    /// <summary>
    /// 标记形态学模式
    /// </summary>
    public enum MarkMorphMode : byte
    {
        /// <summary>
        /// 无
        /// </summary>
        [Description("无")]
        None = 0,

        /// <summary>
        /// 平滑
        /// </summary>
        [Description("平滑")]
        Smooth = 1,

        /// <summary>
        /// 填洞
        /// </summary>
        [Description("填洞")]
        FillHoles = 2
    }
}
