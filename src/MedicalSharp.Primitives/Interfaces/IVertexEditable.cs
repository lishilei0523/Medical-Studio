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
        /// 尝试获取顶点拖拽上下文
        /// </summary>
        /// <param name="localRay">局部空间射线</param>
        /// <param name="constraint">拖拽约束</param>
        /// <returns>是否点中了顶点</returns>
        bool TryGetVertexDrag(Ray localRay, out VertexDragConstraint constraint);

        /// <summary>
        /// 移动当前选中的顶点
        /// </summary>
        /// <param name="constraint">拖拽约束</param>
        /// <param name="localHitPoint">局部空间命中点</param>
        void MoveVertex(VertexDragConstraint constraint, Vector3 localHitPoint);
    }
}
