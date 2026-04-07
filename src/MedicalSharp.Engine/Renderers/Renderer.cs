using MedicalSharp.Engine.Cameras;
using MedicalSharp.Engine.Resources;
using System;

namespace MedicalSharp.Engine.Renderers
{
    /// <summary>
    /// 渲染器
    /// </summary>
    public abstract class Renderer : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 释放标识
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// 无参构造器
        /// </summary>
        protected Renderer()
        {

        }

        /// <summary>
        /// 创建渲染器构造器
        /// </summary>
        /// <param name="camera">相机</param>
        protected Renderer(Camera camera)
            : this()
        {
            #region # 验证

            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera), "相机不可为空！");
            }

            #endregion

            this.Camera = camera;
        }

        /// <summary>
        /// 创建线框渲染器构造器
        /// </summary>
        /// <param name="camera">相机</param>
        /// <param name="program">Shader程序</param>
        protected Renderer(Camera camera, ShaderProgram program)
            : this(camera)
        {
            #region # 验证

            if (program == null)
            {
                throw new ArgumentNullException(nameof(program), "Shader程序不可为空！");
            }

            #endregion

            this.Program = program;
        }

        #endregion

        #region # 属性

        #region 相机 —— Camera Camera
        /// <summary>
        /// 相机
        /// </summary>
        public Camera Camera { get; private set; }
        #endregion 

        #region Shader程序 —— ShaderProgram Program
        /// <summary>
        /// Shader程序
        /// </summary>
        public ShaderProgram Program { get; protected set; }
        #endregion

        #endregion

        #region # 方法

        #region 设置相机 —— void SetCamera(Camera camera)
        /// <summary>
        /// 设置相机
        /// </summary>
        /// <param name="camera">相机</param>
        public void SetCamera(Camera camera)
        {
            #region # 验证

            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera), "相机不可为空！");
            }

            #endregion

            this.Camera = camera;
        }
        #endregion

        #region 设置Shader程序 —— void SetShaderProgram(ShaderProgram program)
        /// <summary>
        /// 设置Shader程序
        /// </summary>
        /// <param name="program">Shader程序</param>
        public void SetShaderProgram(ShaderProgram program)
        {
            #region # 验证

            if (program == null)
            {
                throw new ArgumentNullException(nameof(program), "Shader程序不可为空！");
            }

            #endregion

            this.Program = program;
        }
        #endregion

        #region 渲染帧 —— abstract void RenderFrame(float viewportWidth, float viewportHeight)
        /// <summary>
        /// 渲染帧
        /// </summary>
        /// <param name="viewportWidth">视口宽度</param>
        /// <param name="viewportHeight">视口高度</param>
        public abstract void RenderFrame(float viewportWidth, float viewportHeight);
        #endregion

        #region 释放资源 —— virtual void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this.Program?.Dispose();
            this._disposed = true;
        }
        #endregion 

        #endregion
    }
}
