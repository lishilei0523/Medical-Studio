using System.ComponentModel;

namespace MedicalSharp.Primitives.Enums
{
    /// <summary>
    /// MPR平面类型
    /// </summary>
    public enum MPRPlaneType : byte
    {
        /// <summary>
        /// 横断面
        /// </summary>
        /// <remarks>XY平面</remarks>
        [Description("MPR-Axial")]
        Axial = 0,

        /// <summary>
        /// 冠状面
        /// </summary>
        /// <remarks>XZ平面</remarks>
        [Description("MPR-Coronal")]
        Coronal = 1,

        /// <summary>
        /// 矢状面
        /// </summary>
        /// <remarks>YZ平面</remarks>
        [Description("MPR-Sagittal")]
        Sagittal = 2,

        /// <summary>
        /// 斜切面
        /// </summary>
        [Description("MPR-Oblique")]
        Oblique = 3
    }
}
