using Avalonia.OpenGL;
using OpenTK;
using System;

namespace MedicalSharp.Controls.Base
{
    /// <summary>
    /// Avalonia-OpenTK函数绑定上下文
    /// </summary>
    internal class AvaloniaBindingsContext : IBindingsContext
    {
        /// <summary>
        /// Avalonia OpenGL接口
        /// </summary>
        private readonly GlInterface _glInterface;

        /// <summary>
        /// 默认构造器
        /// </summary>
        /// <param name="glInterface">Avalonia OpenGL接口</param>
        internal AvaloniaBindingsContext(GlInterface glInterface)
        {
            this._glInterface = glInterface;
        }

        /// <summary>
        /// 获取函数地址
        /// </summary>
        /// <param name="procName">函数名称</param>
        /// <returns>函数地址</returns>
        public IntPtr GetProcAddress(string procName)
        {
            IntPtr procAddress = this._glInterface.GetProcAddress(procName);

            return procAddress;
        }
    }
}
