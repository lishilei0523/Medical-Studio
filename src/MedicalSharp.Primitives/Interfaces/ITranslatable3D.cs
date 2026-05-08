using MedicalSharp.Primitives.Maths;

namespace MedicalSharp.Primitives.Interfaces
{
    /// <summary>
    /// 可3D平移接口
    /// </summary>
    public interface ITranslatable3D
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
