using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;

namespace MedicalSharp.Primitives.Interfaces
{
    /// <summary>
    /// 可调整尺寸接口(3D)
    /// </summary>
    public interface IResizable3D
    {
        /// <summary>
        /// 变换
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// 包围盒
        /// </summary>
        BoundingBox Bounds { get; }

        /// <summary>
        /// 尝试获取伸缩方向
        /// </summary>
        /// <param name="localRay">射线（局部空间）</param>
        /// <param name="resizeContext">调整尺寸上下文</param>
        /// <returns>是否成功</returns>
        bool TryGetResizeAxis(Ray localRay, out ResizeContext3D resizeContext);

        /// <summary>
        /// 适用调整尺寸
        /// </summary>
        /// <param name="resizeContext">调整尺寸上下文</param>
        /// <param name="localHitPoint">命中点（局部空间）</param>
        void ApplyResize(ResizeContext3D resizeContext, Vector3 localHitPoint);
    }
}
