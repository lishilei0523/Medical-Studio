using System.Runtime.InteropServices;

namespace MedicalSharp.Engine.ValueTypes
{
    /// <summary>
    /// 窗宽窗位
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct WindowLevel
    {
        /// <summary>
        /// 窗宽
        /// </summary>
        public float Width;

        /// <summary>
        /// 窗位
        /// </summary>
        public float Center;
    }
}
