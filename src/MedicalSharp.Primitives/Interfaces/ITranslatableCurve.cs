using MedicalSharp.Primitives.Maths;

namespace MedicalSharp.Primitives.Interfaces
{
    /// <summary>
    /// 可沿曲线平移接口
    /// </summary>
    public interface ITranslatableCurve
    {
        /// <summary>
        /// 变换
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// 包围盒
        /// </summary>
        BoundingBox Bounds { get; }
    }
}
