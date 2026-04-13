using OpenTK.Mathematics;

namespace MedicalSharp.Primitives.Interfaces
{
    /// <summary>
    /// 可调整尺寸接口
    /// </summary>
    public interface IResizable
    {
        /// <summary>
        /// 调整尺寸
        /// </summary>
        /// <param name="startPos2D">起始2D位置</param>
        /// <param name="endPos2D">中止2D位置</param>
        /// <param name="startPos3D">起始3D位置</param>
        /// <param name="endPos3D">中止3D位置</param>
        /// <param name="hitNormal">命中法向量</param>
        void Resize(Vector2 startPos2D, Vector2 endPos2D, Vector3 startPos3D, Vector3 endPos3D, Vector3 hitNormal);
    }
}
