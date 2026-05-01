namespace MedicalSharp.Primitives.Enums
{
    /// <summary>
    /// 插值模式
    /// </summary>
    public enum InterpolationMode : byte
    {
        /// <summary>
        /// 线性
        /// </summary>
        Linear = 0,

        /// <summary>
        /// 步进
        /// </summary>
        Step = 1,

        /// <summary>
        /// 平滑步进
        /// </summary>
        SmoothStep = 2
    }
}
