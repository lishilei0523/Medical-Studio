using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;

namespace MedicalSharp.Primitives.Interfaces
{
    /// <summary>
    /// 可调整尺寸接口(2D)
    /// </summary>
    public interface IResizable2D
    {
        /// <summary>
        /// 尝试获取伸缩方向（）
        /// </summary>
        /// <param name="mousePos2D">鼠标位置（UV空间）</param>
        /// <param name="resizeContext">调整尺寸上下文</param>
        /// <returns>是否成功</returns>
        /// <remarks>UV空间</remarks>
        bool TryGetResizeAxis(Vector2 mousePos2D, out ResizeContext2D resizeContext);

        /// <summary>
        /// 应用调整尺寸
        /// </summary>
        /// <param name="resizeContext">调整尺寸上下文</param>
        /// <param name="delta">偏移量（UV空间）</param>
        /// <remarks>UV空间</remarks>
        void ApplyResize(ResizeContext2D resizeContext, Vector2 delta);
    }
}
