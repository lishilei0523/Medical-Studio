using MedicalSharp.Primitives.Maths;
using System.Collections.Generic;

namespace MedicalSharp.Primitives.Interfaces
{
    /// <summary>
    /// 含三角形接口
    /// </summary>
    public interface IHasTriangles
    {
        /// <summary>
        /// 三角形列表
        /// </summary>
        IList<Triangle> Triangles { get; }
    }
}
