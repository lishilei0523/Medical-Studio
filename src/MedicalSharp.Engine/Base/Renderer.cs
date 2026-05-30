using MedicalSharp.Primitives.Cameras;
using System;

namespace MedicalSharp.Engine.Base
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
        protected bool _disposed;

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

        #endregion

        #region # 属性

        #region 相机 —— Camera Camera
        /// <summary>
        /// 相机
        /// </summary>
        public Camera Camera { get; private set; }
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

        #region 渲染帧 —— abstract void RenderFrame(float viewportWidth, float viewportHeight...
        /// <summary>
        /// 渲染帧
        /// </summary>
        /// <param name="viewportWidth">视口宽度</param>
        /// <param name="viewportHeight">视口高度</param>
        /// <param name="glContext">OpenGL上下文句柄</param>
        public abstract void RenderFrame(float viewportWidth, float viewportHeight, IntPtr glContext);
        #endregion

        #region 释放资源 —— virtual void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {

        }
        #endregion 

        #endregion
    }
}
