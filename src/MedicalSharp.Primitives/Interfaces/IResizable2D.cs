using OpenTK.Mathematics;

namespace MedicalSharp.Primitives.Interfaces
{
    /// <summary>
    /// 可调整尺寸接口(2D)
    /// </summary>
    public interface IResizable2D
    {
        /// <summary>
        /// 开始调整尺寸
        /// </summary>
        /// <param name="startPos2D">起始位置（UV空间）</param>
        void BeginResize(Vector2 startPos2D);

        /// <summary>
        /// 适用调整尺寸
        /// </summary>
        /// <param name="currentPos2D">当前位置（UV空间）</param>
        void ApplyResize(Vector2 currentPos2D);
    }
}
