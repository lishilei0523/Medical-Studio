using System.Runtime.InteropServices;

namespace MedicalSharp.Dicoms.ValueTypes
{
    /// <summary>
    /// 尺寸
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Size3F
    {
        /// <summary>
        /// 宽度
        /// </summary>
        public readonly float Width;

        /// <summary>
        /// 高度
        /// </summary>
        public readonly float Height;

        /// <summary>
        /// 深度
        /// </summary>
        public readonly float Depth;

        /// <summary>
        /// 创建尺寸构造器
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">深度</param>
        public Size3F(float width, float height, float depth)
            : this()
        {
            this.Width = width;
            this.Height = height;
            this.Depth = depth;
        }
    }
}
