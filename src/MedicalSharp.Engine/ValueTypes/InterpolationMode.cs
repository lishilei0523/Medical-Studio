namespace MedicalSharp.Engine.ValueTypes
{
    /// <summary>
    /// 插值模式
    /// </summary>
    public enum InterpolationMode
    {
        /// <summary>
        /// 最近邻插值
        /// </summary>
        Nearest = 0,

        /// <summary>
        /// 线性插值
        /// </summary>
        Linear = 1,

        /// <summary>
        /// 三次插值
        /// </summary>
        Cubic = 2
    }
}
