using System.Runtime.InteropServices;

namespace MedicalSharp.Dicoms.ValueTypes
{
    /// <summary>
    /// 尺寸
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Size3I
    {
        /// <summary>
        /// 宽度
        /// </summary>
        public readonly int Width;

        /// <summary>
        /// 高度
        /// </summary>
        public readonly int Height;

        /// <summary>
        /// 深度
        /// </summary>
        public readonly int Depth;

        /// <summary>
        /// 创建尺寸构造器
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">深度</param>
        public Size3I(int width, int height, int depth)
            : this()
        {
            this.Width = width;
            this.Height = height;
            this.Depth = depth;
        }
    }
}
