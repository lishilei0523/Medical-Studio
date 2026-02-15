using Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Rendering;
using MedicalSharp.Controls.Extensions;
using OpenTK.Graphics.OpenGL4;

namespace MedicalSharp.Controls.Base
{
    /// <summary>
    /// OpenTK视口
    /// </summary>
    public abstract class OpenTKViewport : OpenGlControlBase, ICustomHitTest
    {
        /// <summary>
        /// 命中测试
        /// </summary>
        /// <param name="point">点</param>
        /// <returns>是否命中</returns>
        public bool HitTest(Point point)
        {
            return true;
        }

        /// <summary>
        /// OpenGL初始化事件
        /// </summary>
        /// <param name="glInterface">OpenGL接口</param>
        protected override void OnOpenGlInit(GlInterface glInterface)
        {
            base.OnOpenGlInit(glInterface);

            //加载OpenTK绑定
            AvaloniaBindingsContext bindingsContext = new AvaloniaBindingsContext(glInterface);
            GL.LoadBindings(bindingsContext);
        }
    }
}
