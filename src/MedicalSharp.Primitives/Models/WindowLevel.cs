namespace MedicalSharp.Primitives.Models
{
    /// <summary>
    /// 窗宽/窗位
    /// </summary>
    public record WindowLevel
    {
        /// <summary>
        /// 创建窗宽/窗位构造器
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="windowWidth">窗宽</param>
        /// <param name="windowCenter">窗位</param>
        public WindowLevel(string name, int windowWidth, int windowCenter)
        {
            this.Name = name;
            this.WindowWidth = windowWidth;
            this.WindowCenter = windowCenter;
        }

        /// <summary>
        /// 名称
        /// </summary>
        private string Name { get; }

        /// <summary>
        /// 窗宽
        /// </summary>
        public int WindowWidth { get; }

        /// <summary>
        /// 窗位
        /// </summary>
        public int WindowCenter { get; }

        /// <summary>
        /// 转换字符串
        /// </summary>
        public override string ToString()
        {
            return $"{this.Name} ({this.WindowWidth}, {this.WindowCenter})";
        }
    }
}
