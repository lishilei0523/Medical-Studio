using Avalonia;

namespace MedicalSharp.Controls.Interfaces
{
    /// <summary>
    /// 3D中2D元素接口
    /// </summary>
    public interface IVisual2DIn3D
    {
        /// <summary>
        /// 法向量
        /// </summary>
        Vector3D Normal { get; }
    }
}
