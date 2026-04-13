using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;

namespace MedicalSharp.Primitives.Interfaces
{
    /// <summary>
    /// 可调整尺寸接口
    /// </summary>
    public interface IResizable
    {
        /// <summary>
        /// 尝试获取伸缩方向
        /// </summary>
        /// <param name="localRay">射线（局部空间）</param>
        /// <param name="resizeContext">调整尺寸上下文</param>
        /// <returns>是否成功</returns>
        bool TryGetResizeAxis(Ray localRay, out ResizeContext resizeContext);

        /// <summary>
        /// 应用调整尺寸
        /// </summary>
        /// <param name="resizeContext">调整尺寸上下文</param>
        /// <param name="hitPoint">命中点</param>
        void ApplyResize(ResizeContext resizeContext, Vector3 hitPoint);
    }
}
