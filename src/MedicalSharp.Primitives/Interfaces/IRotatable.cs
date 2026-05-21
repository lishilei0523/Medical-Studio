using MedicalSharp.Primitives.Maths;

namespace MedicalSharp.Primitives.Interfaces
{
    /// <summary>
    /// 可旋转接口
    /// </summary>
    public interface IRotatable
    {
        /// <summary>
        /// 可否旋转
        /// </summary>
        bool CanRotate { get; }

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
