using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;

namespace MedicalSharp.Primitives.Interfaces
{
    /// <summary>
    /// 可编辑顶点接口
    /// </summary>
    public interface IVertexEditable
    {
        /// <summary>
        /// 尝试获取顶点拖拽约束
        /// </summary>
        /// <param name="localRay">射线（局部空间）</param>
        /// <param name="constraint">拖拽约束</param>
        /// <returns>是否命中顶点</returns>
        bool TryGetVertexDrag(Ray localRay, out VertexDragConstraint constraint);

        /// <summary>
        /// 移动命中顶点
        /// </summary>
        /// <param name="constraint">拖拽约束</param>
        /// <param name="localHitPoint">命中点（局部空间）</param>
        void MoveVertex(VertexDragConstraint constraint, Vector3 localHitPoint);
    }
}
