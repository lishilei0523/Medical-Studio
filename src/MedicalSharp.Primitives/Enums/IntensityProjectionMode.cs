using System.ComponentModel;

namespace MedicalSharp.Primitives.Enums
{
    /// <summary>
    /// 密度投影模式
    /// </summary>
    public enum IntensityProjectionMode : byte
    {
        /// <summary>
        /// 平均密度投影
        /// </summary>
        [Description("平均密度投影")]
        AIP = 0,

        /// <summary>
        /// 最大密度投影
        /// </summary>
        [Description("最大密度投影")]
        MIP = 1,

        /// <summary>
        /// 最小密度投影
        /// </summary>
        [Description("最小密度投影")]
        MinIP = 2
    }
}
