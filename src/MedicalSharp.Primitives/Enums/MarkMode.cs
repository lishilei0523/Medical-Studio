namespace MedicalSharp.Primitives.Enums
{
    /// <summary>
    /// 标记模式
    /// </summary>
    public enum MarkMode : byte
    {
        /// <summary>
        /// 正常模式
        /// </summary>
        /// <remarks>显示全部体积，忽略标记</remarks>
        Normal = 0,

        /// <summary>
        /// 保留模式
        /// </summary>
        /// <remarks>只显示标记区域内的体积</remarks>
        Keep = 1,

        /// <summary>
        /// 切除模式
        /// </summary>
        /// <remarks>隐藏标记区域内的体积</remarks>
        Cut = 2,

        /// <summary>
        /// 高亮
        /// </summary>
        /// <remarks>标记区域内用高亮颜色显示</remarks>
        Highlight = 3
    }
}
