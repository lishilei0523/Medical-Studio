using OpenTK.Graphics.OpenGL4;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 像素缓冲区(读)
    /// </summary>
    /// <remarks>GPU -> CPU</remarks>
    public class ReadPixelBuffer : PixelBuffer
    {
        //TODO 实现

        /// <summary>
        /// 创建像素缓冲区(读)构造器
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="pixelFormat">像素格式</param>
        public ReadPixelBuffer(int width, int height, PixelFormat pixelFormat = PixelFormat.Rgba)
            : base(width, height, pixelFormat)
        {

        }
    }
}
