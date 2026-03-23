using OpenTK.Graphics.OpenGL4;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 像素缓冲区(写)
    /// </summary>
    /// <remarks>CPU -> GPU</remarks>
    public class WritePixelBuffer : PixelBuffer
    {
        //TODO 实现

        /// <summary>
        /// 创建像素缓冲区(写)构造器
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="pixelFormat">像素格式</param>
        public WritePixelBuffer(int width, int height, PixelFormat pixelFormat = PixelFormat.Rgba)
            : base(width, height, pixelFormat)
        {

        }
    }
}
