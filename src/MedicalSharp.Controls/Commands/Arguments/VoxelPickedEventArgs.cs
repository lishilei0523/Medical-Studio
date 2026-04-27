using MedicalSharp.Controls.Base;
using MedicalSharp.Primitives.Maths;
using OpenTK.Mathematics;

namespace MedicalSharp.Controls.Commands.Arguments
{
    /// <summary>
    /// 体素拾取事件参数
    /// </summary>
    public class VoxelPickedEventArgs : CommandEventArgs
    {
        /// <summary>
        /// 拾取的纹理坐标
        /// </summary>
        public Vector3? PickedTextureCoord { get; set; }

        /// <summary>
        /// 拾取的世界位置
        /// </summary>
        public Vector3? PickedWorldPosition { get; set; }

        /// <summary>
        /// 拾取的体素位置
        /// </summary>
        public Vector3i? PickedVoxelPosition { get; set; }

        /// <summary>
        /// 拾取的体素HU值
        /// </summary>
        public short? PickedVoxelValue { get; set; }

        /// <summary>
        /// 拾取的标记值
        /// </summary>
        public byte? PickedMarkValue { get; set; }

        /// <summary>
        /// 射线
        /// </summary>
        public Ray? Ray { get; set; }
    }
}
