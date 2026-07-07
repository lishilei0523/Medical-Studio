using System.ComponentModel;

namespace MedicalSharp.Primitives.Enums
{
    /// <summary>
    /// CPR模式
    /// </summary>
    public enum CPRMode : byte
    {
        /// <summary>
        /// 拉直图
        /// </summary>
        [Description("拉直图")]
        Straightened = 0,

        /// <summary>
        /// 投影图
        /// </summary>
        [Description("投影图")]
        Projected = 1,

        /// <summary>
        /// 剖面图
        /// </summary>
        [Description("剖面图")]
        CrossSectional = 2
    }
}
