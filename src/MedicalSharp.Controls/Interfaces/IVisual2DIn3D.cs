using Avalonia;

namespace MedicalSharp.Controls.Interfaces
{
    /// <summary>
    /// 3D中2D元素接口
    /// </summary>
    public interface IVisual2DIn3D
    {
        /// <summary>
        /// U轴
        /// </summary>
        Vector3D UAxis { get; }

        /// <summary>
        /// V轴
        /// </summary>
        Vector3D VAxis { get; }

        /// <summary>
        /// 法向量
        /// </summary>
        Vector3D Normal { get; }

        /// <summary>
        /// 平面上一点
        /// </summary>
        Vector3D PointOnPlane { get; }
    }
}
