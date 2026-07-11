using System.ComponentModel;

namespace MedicalSharp.Primitives.Enums
{
    /// <summary>
    /// CPR拉直方向
    /// </summary>
    public enum CPRStraightenDirection : byte
    {
        /// <summary>
        /// 水平
        /// </summary>
        [Description("水平拉直图")]
        Horizontal = 0,

        /// <summary>
        /// 垂直
        /// </summary>
        [Description("垂直拉直图")]
        Vertical = 1
    }
}
