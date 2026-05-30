using MedicalSharp.Engine.Base;
using MedicalSharp.Engine.Managers;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Cameras;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace MedicalSharp.Engine.Renderers
{
    /// <summary>
    /// Overlay渲染器
    /// </summary>
    public class OverlayRenderer : Renderer
    {
        #region # 字段及构造器

        /// <summary>
        /// 渲染对象列表
        /// </summary>
        private readonly HashSet<Renderable2D> _renderables;

        /// <summary>
        /// 创建Overlay渲染器构造器
        /// </summary>
        /// <param name="camera">相机</param>
        public OverlayRenderer(Camera camera)
            : base(camera)
        {
            //默认值
            this._renderables = [];
        }

        #endregion

        #region 属性

        #region 只读属性 - 渲染对象列表 —— IReadOnlySet<Renderable2D> Renderables
        /// <summary>
        /// 只读属性 - 渲染对象列表
        /// </summary>
        public IReadOnlySet<Renderable2D> Renderables
        {
            get => this._renderables;
        }
        #endregion

        #endregion

        #region 方法

        #region 追加渲染对象 —— void AppendItem(Renderable2D renderable)
        /// <summary>
        /// 追加渲染对象
        /// </summary>
        /// <param name="renderable">渲染对象</param>
        public void AppendItem(Renderable2D renderable)
        {
            #region # 验证

            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable), "2D渲染对象不可为空！");
            }

            #endregion

            this._renderables.Add(renderable);
        }
        #endregion

        #region 删除渲染对象 —— void RemoveItem(Renderable2D renderable)
        /// <summary>
        /// 删除渲染对象
        /// </summary>
        /// <param name="renderable">渲染对象</param>
        public void RemoveItem(Renderable2D renderable)
        {
            #region # 验证

            if (renderable == null)
            {
                return;
            }

            #endregion

            this._renderables.Remove(renderable);
        }
        #endregion

        #region 清空渲染对象 —— void ClearItems()
        /// <summary>
        /// 清空渲染对象
        /// </summary>
        public void ClearItems()
        {
            this._renderables.Clear();
        }
        #endregion

        #region 渲染帧 —— override void RenderFrame(float viewportWidth, float viewportHeight...
        /// <summary>
        /// 渲染帧
        /// </summary>
        /// <param name="viewportWidth">视口宽度</param>
        /// <param name="viewportHeight">视口高度</param>
        /// <param name="glContext">OpenGL上下文句柄</param>
        public override void RenderFrame(float viewportWidth, float viewportHeight, IntPtr glContext)
        {
            #region # 验证

            if (viewportWidth <= 0 || viewportHeight <= 0)
            {
                return;
            }
            if (this.Camera == null)
            {
                throw new InvalidOperationException("相机不可为空！");
            }

            #endregion

            //获取相机旋转矩阵
            Matrix4 cameraRotation = this.Camera.GetRotation();

            //渲染上下文
            RenderContext2D renderContext = new RenderContext2D(glContext, (int)viewportWidth, (int)viewportHeight, cameraRotation);

            //开启Shader程序
            ShaderProgram program = ShaderManager.ShapeProgram;
            program.Use();

            //设置投影矩阵、视图矩阵
            program.SetUniformMatrix4("u_ProjectionMatrix", renderContext.OrthoMatrix);
            program.SetUniformMatrix4("u_ViewMatrix", Matrix4.Identity);

            foreach (Renderable2D renderable in this._renderables)
            {
                //渲染
                renderable.Render(program, renderContext);
            }

            //取消使用
            program.Unuse();
        }
        #endregion

        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            foreach (Renderable2D renderable in this._renderables)
            {
                renderable.Dispose();
            }
            this._renderables.Clear();

            this._disposed = true;
        }
        #endregion

        #endregion
    }
}
