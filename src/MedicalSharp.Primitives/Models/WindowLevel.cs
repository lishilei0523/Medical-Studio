using System.Runtime.InteropServices;

namespace MedicalSharp.Primitives.Models
{
    /// <summary>
    /// 窗宽/窗位
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct WindowLevel
    {
        /// <summary>
        /// 创建窗宽/窗位构造器
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="windowWidth">窗宽</param>
        /// <param name="windowCenter">窗位</param>
        public WindowLevel(string name, int windowWidth, int windowCenter)
            : this()
        {
            this.Name = name;
            this.WindowWidth = windowWidth;
            this.WindowCenter = windowCenter;
        }

        /// <summary>
        /// 名称
        /// </summary>
        private readonly string Name;

        /// <summary>
        /// 窗宽
        /// </summary>
        public readonly int WindowWidth;

        /// <summary>
        /// 窗位
        /// </summary>
        public readonly int WindowCenter;

        /// <summary>
        /// 转换字符串
        /// </summary>
        public override string ToString()
        {
            return $"{this.Name} ({this.WindowWidth}, {this.WindowCenter})";
        }
    }
}
