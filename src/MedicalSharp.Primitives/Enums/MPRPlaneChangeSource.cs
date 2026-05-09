namespace MedicalSharp.Primitives.Enums
{
    /// <summary>
    /// MPR平面变化源
    /// </summary>
    public enum MPRPlaneChangeSource : byte
    {
        /// <summary>
        /// 滚轮切切片
        /// </summary>
        SliceScroll = 0,

        /// <summary>
        /// 十字线拖动
        /// </summary>
        CrosshairDrag = 1,

        /// <summary>
        /// 外部平面同步
        /// </summary>
        /// <remarks>平移/旋转</remarks>
        ExternalSync = 2
    }
}
