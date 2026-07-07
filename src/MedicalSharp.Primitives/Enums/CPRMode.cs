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
        /// 切线投影图
        /// </summary>
        /// <remarks>水平，沿血管</remarks>
        TangentProjected = 1,

        /// <summary>
        /// 法向量投影图
        /// </summary>
        /// <remarks>垂直，侧面看</remarks>
        NormalProjected = 2,

        /// <summary>
        /// 剖面图
        /// </summary>
        [Description("剖面图")]
        CrossSectional = 3
    }
}
