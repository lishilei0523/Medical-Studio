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
        /// 腐蚀
        /// </summary>
        [Description("腐蚀")]
        Erode = 1,

        /// <summary>
        /// 膨胀
        /// </summary>
        [Description("膨胀")]
        Dilate = 2,

        /// <summary>
        /// 开运算
        /// </summary>
        [Description("开运算")]
        Open = 3,

        /// <summary>
        /// 闭运算
        /// </summary>
        [Description("闭运算")]
        Close = 4
    }
}
