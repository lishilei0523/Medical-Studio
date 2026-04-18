using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Primitives.Maths;
using OpenTK.Mathematics;

namespace MedicalSharp.Controls.Commands.Arguments
{
    /// <summary>
    /// 3D元素拾取事件参数
    /// </summary>
    public class Visual3DPickedEventArgs : CommandEventArgs
    {
        /// <summary>
        /// 拾取的3D元素
        /// </summary>
        /// <remarks>null表示未拾取到</remarks>
        public Visual3D PickedVisual { get; set; }

        /// <summary>
        /// 命中位置
        /// </summary>
        public Vector3? HitPoint { get; set; }

        /// <summary>
        /// 法向量
        /// </summary>
        public Vector3? Normal { get; set; }

        /// <summary>
        /// 射线
        /// </summary>
        public Ray? Ray { get; set; }
    }
}
