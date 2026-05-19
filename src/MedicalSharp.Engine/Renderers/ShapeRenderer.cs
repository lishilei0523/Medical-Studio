using MedicalSharp.Engine.Managers;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Cameras;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace MedicalSharp.Engine.Renderers
{
    /// <summary>
    /// 形状渲染器
    /// </summary>
    public class ShapeRenderer : Renderer
    {
        #region # 字段及构造器

        /// <summary>
        /// 渲染对象列表
        /// </summary>
        private readonly HashSet<ShapeRenderable> _renderables;

        /// <summary>
        /// 创建形状渲染器构造器
        /// </summary>
        /// <param name="camera">相机</param>
        public ShapeRenderer(Camera camera)
            : base(camera)
        {
            //默认值
            this._renderables = new HashSet<ShapeRenderable>();
        }

        #endregion

        #region # 属性

        #region 只读属性 - 渲染对象列表 —— IReadOnlySet<ShapeRenderable> Renderables
        /// <summary>
        /// 只读属性 - 渲染对象列表
        /// </summary>
        public IReadOnlySet<ShapeRenderable> Renderables
        {
            get => this._renderables;
        }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 追加渲染对象 —— void AppendItem(ShapeRenderable renderable)
        /// <summary>
        /// 追加渲染对象
        /// </summary>
        /// <param name="renderable">渲染对象</param>
        public void AppendItem(ShapeRenderable renderable)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable), "形状渲染对象不可为空！");
            }

            this._renderables.Add(renderable);
        }
        #endregion

        #region 删除渲染对象 —— void RemoveItem(ShapeRenderable renderable)
        /// <summary>
        /// 删除渲染对象
        /// </summary>
        /// <param name="renderable">渲染对象</param>
        public void RemoveItem(ShapeRenderable renderable)
        {
            if (renderable == null)
            {
                return;
            }

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

            //设置相机视口尺寸
            this.Camera.SetViewportSize(viewportWidth, viewportHeight);

            //渲染上下文
            float zoomFactor = 1.0f;
            Matrix4 viewMatrix = this.Camera.ViewMatrix;
            if (this.Camera is MPRCamera mprCamera)
            {
                zoomFactor = mprCamera.ZoomFactor;

                //横断位特殊处理
                if (mprCamera.TargetPlane.OriginalPlaneType == MPRPlaneType.Axial)
                {
                    viewMatrix = Matrix4.CreateScale(-1, 1, 1) * viewMatrix;
                }
            }

            RenderContext renderContext = new RenderContext(glContext, viewportWidth, viewportHeight, this.Camera.CameraMode, this.Camera.CameraPosition, this.Camera.LookDirection, this.Camera.ProjectionMatrix, viewMatrix, zoomFactor);

            //开启Shader程序
            ShaderProgram program = ShaderManager.ShapeProgram;
            program.Use();

            //设置投影矩阵、视图矩阵、相机位置
            program.SetUniformMatrix4("u_ProjectionMatrix", this.Camera.ProjectionMatrix);
            program.SetUniformMatrix4("u_ViewMatrix", viewMatrix);
            program.SetUniformVector3("u_CameraPosition", this.Camera.CameraPosition);

            foreach (ShapeRenderable renderable in this._renderables)
            {
                //设置模型矩阵
                program.SetUniformMatrix4("u_ModelMatrix", renderable.ModelMatrix);

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

            foreach (ShapeRenderable renderable in this._renderables)
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
