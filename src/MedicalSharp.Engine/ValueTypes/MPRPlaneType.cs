namespace MedicalSharp.Engine.ValueTypes
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
        Axial = 0,

        /// <summary>
        /// 冠状面
        /// </summary>
        /// <remarks>XZ平面</remarks>
        Coronal = 1,

        /// <summary>
        /// 矢状面
        /// </summary>
        /// <remarks>YZ平面</remarks>
        Sagittal = 2,

        /// <summary>
        /// 斜切面
        /// </summary>
        Oblique = 3
    }
}
