using System.ComponentModel;

namespace MedicalSharp.Primitives.Enums
{
    /// <summary>
    /// CPR投影方向
    /// </summary>
    public enum CPRProjectionDirection : byte
    {
        /// <summary>
        /// 切线投影图
        /// </summary>
        /// <remarks>水平</remarks>
        [Description("水平CPR")]
        Tangent = 0,

        /// <summary>
        /// 法向量投影图
        /// </summary>
        /// <remarks>垂直</remarks>
        [Description("垂直CPR")]
        Normal = 1
    }
}
