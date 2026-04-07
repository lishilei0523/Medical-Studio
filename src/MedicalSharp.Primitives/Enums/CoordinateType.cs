namespace MedicalSharp.Primitives.Enums
{
    /// <summary>
    /// 坐标系类型
    /// </summary>
    public enum CoordinateType
    {
        /// <summary>
        /// X轴向上
        /// </summary>
        XUp = 0,

        /// <summary>
        /// Y轴向上
        /// </summary>
        /// <remarks>OpenGL/DirectX默认</remarks>
        YUp = 1,

        /// <summary>
        /// Z轴向上
        /// </summary>
        /// <remarks>CAD/医学常用</remarks>
        ZUp = 2
    }
}
