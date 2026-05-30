using MedicalSharp.Controls.Visual3Ds;
using MedicalSharp.Primitives.Maths;
using OpenTK.Mathematics;

namespace MedicalSharp.Controls.Interfaces
{
    /// <summary>
    /// 拾取3D元素接口
    /// </summary>
    public interface IPickVisual3D
    {
        /// <summary>
        /// 查找最近元素
        /// </summary>
        /// <param name="position">2D位置</param>
        /// <param name="point">3D位置</param>
        /// <param name="normal">法向量</param>
        /// <param name="visual3D">3D元素</param>
        /// <param name="ray">射线</param>
        /// <returns>是否成功</returns>
        bool FindNearest(Vector2 position, out Vector3 point, out Vector3 normal, out Visual3D visual3D, out Ray ray);
    }
}
