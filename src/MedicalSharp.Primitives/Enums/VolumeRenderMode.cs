namespace MedicalSharp.Primitives.Enums
{
    /// <summary>
    /// 体积渲染模式
    /// </summary>
    public enum VolumeRenderMode : byte
    {
        /// <summary>
        /// 光线投射
        /// </summary>
        Raycast = 0,

        /// <summary>
        /// 平均密度投影
        /// </summary>
        AIP = 1,

        /// <summary>
        /// 最大密度投影
        /// </summary>
        MIP = 2,

        /// <summary>
        /// 最小密度投影
        /// </summary>
        MinIP = 3,

        /// <summary>
        /// 表面阴影显示
        /// </summary>
        SSD = 4
    }
}
