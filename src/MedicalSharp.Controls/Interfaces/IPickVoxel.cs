using MedicalSharp.Primitives.Maths;
using OpenTK.Mathematics;

namespace MedicalSharp.Controls.Interfaces
{
    /// <summary>
    /// 拾取体素接口
    /// </summary>
    public interface IPickVoxel
    {
        /// <summary>
        /// 查找最近体素
        /// </summary>
        /// <param name="position">2D位置</param>
        /// <param name="textureCoord">纹理坐标</param>
        /// <param name="voxelPosition">体素坐标</param>
        /// <param name="voxelValue">体素HU值</param>
        /// <param name="markValue">标记值</param>
        /// <param name="ray">射线</param>
        /// <returns>是否成功</returns>
        bool FindNearestVoxel(Vector2 position, out Vector3 textureCoord, out Vector3i voxelPosition, out short voxelValue, out byte markValue, out Ray ray);
    }
}
