using OpenTK.Graphics.OpenGL4;
using System;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 统一缓冲区
    /// </summary>
    public class UniformBuffer : IDisposable
    {
        //TODO 实现

        #region 统一缓冲区Id —— int Id
        /// <summary>
        /// 统一缓冲区Id
        /// </summary>
        public int Id { get; private set; }
        #endregion



        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            GL.DeleteBuffer(this.Id);
        }
        #endregion 
    }
}
