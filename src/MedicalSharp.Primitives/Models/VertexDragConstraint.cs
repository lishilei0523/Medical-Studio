using OpenTK.Mathematics;

namespace MedicalSharp.Primitives.Models
{
    /// <summary>
    /// 顶点拖拽约束
    /// </summary>
    public struct VertexDragConstraint
    {
        /// <summary>
        /// 顶点索引
        /// </summary>
        public int VertexIndex;

        /// <summary>
        /// 约束平面锚点
        /// </summary>
        public Vector3 AnchorPoint;

        /// <summary>
        /// 约束平面法线
        /// </summary>
        public Vector3 PlaneNormal;
    }
}
