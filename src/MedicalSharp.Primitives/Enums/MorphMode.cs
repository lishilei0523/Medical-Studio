using System.ComponentModel;

namespace MedicalSharp.Primitives.Enums
{
    /// <summary>
    /// 形态学模式
    /// </summary>
    public enum MorphMode : byte
    {
        /// <summary>
        /// 腐蚀
        /// </summary>
        [Description("腐蚀")]
        Erode = 0,

        /// <summary>
        /// 膨胀
        /// </summary>
        [Description("膨胀")]
        Dilate = 1,

        /// <summary>
        /// 开运算
        /// </summary>
        [Description("开运算")]
        Open = 2,

        /// <summary>
        /// 闭运算
        /// </summary>
        [Description("闭运算")]
        Close = 3,

        /// <summary>
        /// 礼帽运算
        /// </summary>
        [Description("礼帽运算")]
        TopHat = 4,

        /// <summary>
        /// 黑帽运算
        /// </summary>
        [Description("黑帽运算")]
        BlackHat = 5,

        /// <summary>
        /// 梯度运算
        /// </summary>
        [Description("梯度运算")]
        Gradient = 6
    }
}
