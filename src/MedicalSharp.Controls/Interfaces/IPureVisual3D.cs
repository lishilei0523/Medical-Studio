using OpenTK.Mathematics;
using System.Collections.Generic;

namespace MedicalSharp.Controls.Interfaces
{
    /// <summary>
    /// 纯3D元素接口
    /// </summary>
    public interface IPureVisual3D
    {
        /// <summary>
        /// 获取凸包位置列表
        /// </summary>
        /// <returns>位置列表（世界空间）</returns>
        IReadOnlyList<Vector3> GetConvexHullPositions();
    }
}
