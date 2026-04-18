using OpenTK.Mathematics;

namespace MedicalSharp.Controls.Base
{
    /// <summary>
    /// 命令事件参数
    /// </summary>
    public abstract class CommandEventArgs
    {
        /// <summary>
        /// 视口
        /// </summary>
        public OpenTKViewport Viewport { get; set; }

        /// <summary>
        /// 鼠标2D位置
        /// </summary>
        public Vector2 MousePos2D { get; set; }

        /// <summary>
        /// 是否已处理
        /// </summary>
        /// <remarks>防止冒泡</remarks>
        public bool Handled { get; set; }
    }
}
