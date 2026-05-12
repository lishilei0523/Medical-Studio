using System.ComponentModel;

namespace MedicalSharp.Primitives.Enums
{
    /// <summary>
    /// 标记模式
    /// </summary>
    public enum MarkMode : byte
    {
        /// <summary>
        /// 显示
        /// </summary>
        [Description("显示")]
        Visible = 0,

        /// <summary>
        /// 隐藏
        /// </summary>
        [Description("隐藏")]
        Collapsed = 1,

        /// <summary>
        /// 染色
        /// </summary>
        [Description("染色")]
        Tinted = 2
    }
}
