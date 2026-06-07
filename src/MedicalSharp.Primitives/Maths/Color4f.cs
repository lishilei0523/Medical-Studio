namespace MedicalSharp.Primitives.Maths
{
    /// <summary>
    /// 颜色
    /// </summary>
    public sealed class Color4f
    {
        /// <summary>
        /// 默认构造器
        /// </summary>
        public Color4f() { }

        /// <summary>
        /// 创建颜色构造器
        /// </summary>
        /// <param name="r">R值</param>
        /// <param name="g">G值</param>
        /// <param name="b">B值</param>
        /// <param name="a">A值</param>
        public Color4f(float r, float g, float b, float a)
            : this()
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }

        /// <summary>
        /// R值
        /// </summary>
        public float R { get; set; }

        /// <summary>
        /// G值
        /// </summary>
        public float G { get; set; }

        /// <summary>
        /// B值
        /// </summary>
        public float B { get; set; }

        /// <summary>
        /// A值
        /// </summary>
        public float A { get; set; }
    }
}
