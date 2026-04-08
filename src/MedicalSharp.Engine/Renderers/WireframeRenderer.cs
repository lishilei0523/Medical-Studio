using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Cameras;
using MedicalSharp.Primitives.Models;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace MedicalSharp.Engine.Renderers
{
    /// <summary>
    /// 线框渲染器
    /// </summary>
    public class WireframeRenderer : Renderer
    {
        #region # 字段及构造器

        /// <summary>
        /// 渲染对象列表
        /// </summary>
        private readonly ICollection<WireframeRenderable> _renderables;

        /// <summary>
        /// 创建线框渲染器构造器
        /// </summary>
        /// <param name="camera">相机</param>
        public WireframeRenderer(Camera camera)
            : base(camera)
        {
            //默认值
            this._renderables = new HashSet<WireframeRenderable>();
            this.InitShaderProgram();
        }

        /// <summary>
        /// 创建线框渲染器构造器
        /// </summary>
        /// <param name="camera">相机</param>
        /// <param name="program">Shader程序</param>
        public WireframeRenderer(Camera camera, ShaderProgram program)
            : base(camera, program)
        {
            //默认值
            this._renderables = new HashSet<WireframeRenderable>();
        }

        #endregion

        #region # 属性

        #region 只读属性 - 渲染对象列表 —— IReadOnlySet<WireframeRenderable> Renderables
        /// <summary>
        /// 只读属性 - 渲染对象列表
        /// </summary>
        public IReadOnlySet<WireframeRenderable> Renderables
        {
            get => (IReadOnlySet<WireframeRenderable>)this._renderables;
        }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 追加渲染对象 —— void AppendItem(WireframeRenderable renderable)
        /// <summary>
        /// 追加渲染对象
        /// </summary>
        /// <param name="renderable">渲染对象</param>
        public void AppendItem(WireframeRenderable renderable)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable), "线框渲染对象不可为空！");
            }

            this._renderables.Add(renderable);
        }
        #endregion

        #region 删除渲染对象 —— void RemoveItem(WireframeRenderable renderable)
        /// <summary>
        /// 删除渲染对象
        /// </summary>
        /// <param name="renderable">渲染对象</param>
        public void RemoveItem(WireframeRenderable renderable)
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

        #region 渲染帧 —— override void RenderFrame(float viewportWidth, float viewportHeight)
        /// <summary>
        /// 渲染帧
        /// </summary>
        /// <param name="viewportWidth">视口宽度</param>
        /// <param name="viewportHeight">视口高度</param>
        public override void RenderFrame(float viewportWidth, float viewportHeight)
        {
            #region # 验证

            if (viewportWidth <= 0 || viewportHeight <= 0)
            {
                return;
            }
            if (this.Program == null)
            {
                throw new InvalidOperationException("Shader程序不可为空！");
            }
            if (this.Camera == null)
            {
                throw new InvalidOperationException("相机不可为空！");
            }

            #endregion

            //设置相机视口尺寸
            this.Camera.SetViewportSize(viewportWidth, viewportHeight);

            //渲染上下文
            RenderContext renderContext = new RenderContext(viewportWidth, viewportHeight, this.Camera.CameraPosition, this.Camera.LookDirection, this.Camera.ProjectionMatrix, this.Camera.ViewMatrix);

            //开启Shader程序
            this.Program.Use();

            //设置投影矩阵、视图矩阵、相机位置
            this.Program.SetUniformMatrix4("u_ProjectionMatrix", this.Camera.ProjectionMatrix);
            this.Program.SetUniformMatrix4("u_ViewMatrix", this.Camera.ViewMatrix);
            this.Program.SetUniformVector3("u_CameraPosition", this.Camera.CameraPosition);

            foreach (WireframeRenderable renderable in this._renderables)
            {
                //设置模型矩阵
                this.Program.SetUniformMatrix4("u_ModelMatrix", renderable.ModelMatrix);

                //绘制填充模型	
                GL.DepthMask(false);//禁用深度写入、让透明面可以互相混合
                this.Program.SetUniformVector4("u_Color", renderable.Fill);
                renderable.FillBuffer.Draw(PrimitiveType.Triangles);
                GL.DepthMask(true);//恢复状态

                //绘制线框模型
                GL.LineWidth(renderable.StrokeThickness);
                this.Program.SetUniformVector4("u_Color", renderable.Stroke);
                renderable.StrokeBuffer.Draw(PrimitiveType.Lines);

                //触发渲染事件
                renderable.OnRender(renderContext);
            }

            //取消使用
            this.Program.Unuse();
        }
        #endregion

        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            foreach (WireframeRenderable renderable in this._renderables)
            {
                renderable.Dispose();
            }

            this._renderables.Clear();
        }
        #endregion


        //Private

        #region 初始化Shader程序 —— void InitShaderProgram()
        /// <summary>
        /// 初始化Shader程序
        /// </summary>
        private void InitShaderProgram()
        {
            base.Program = new ShaderProgram();
            base.Program.ReadVertexShaderFromFile("Resources/GLSLs/wireframe.vert");
            base.Program.ReadFragmentShaderFromFile("Resources/GLSLs/wireframe.frag");
            base.Program.Build();
        }
        #endregion 

        #endregion
    }
}
