using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Cameras;
using MedicalSharp.Primitives.Models;
using System;
using System.Collections.Generic;

namespace MedicalSharp.Engine.Renderers
{
    /// <summary>
    /// 文本渲染器
    /// </summary>
    public class TextRenderer : Renderer
    {
        #region # 字段及构造器

        /// <summary>
        /// 渲染对象列表
        /// </summary>
        private readonly ICollection<TextRenderable> _renderables;

        /// <summary>
        /// 创建文本渲染器构造器
        /// </summary>
        /// <param name="camera">相机</param>
        public TextRenderer(Camera camera)
            : base(camera)
        {
            this._renderables = new HashSet<TextRenderable>();
            this.InitShaderProgram();
        }

        /// <summary>
        /// 创建文本渲染器构造器
        /// </summary>
        /// <param name="camera">相机</param>
        /// <param name="program">Shader程序</param>
        public TextRenderer(Camera camera, ShaderProgram program)
            : base(camera, program)
        {
            this._renderables = new HashSet<TextRenderable>();
        }

        #endregion

        #region # 属性

        #region 只读属性 - 渲染对象列表 —— IReadOnlySet<TextRenderable> Renderables
        /// <summary>
        /// 只读属性 - 渲染对象列表
        /// </summary>
        public IReadOnlySet<TextRenderable> Renderables
        {
            get => (IReadOnlySet<TextRenderable>)this._renderables;
        }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 追加渲染对象 —— void AppendItem(TextRenderable renderable)
        /// <summary>
        /// 追加渲染对象
        /// </summary>
        /// <param name="renderable">渲染对象</param>
        public void AppendItem(TextRenderable renderable)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable), "文本渲染对象不可为空！");
            }

            this._renderables.Add(renderable);
        }
        #endregion

        #region 删除渲染对象 —— void RemoveItem(TextRenderable renderable)
        /// <summary>
        /// 删除渲染对象
        /// </summary>
        /// <param name="renderable">渲染对象</param>
        public void RemoveItem(TextRenderable renderable)
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
            #region 验证

            if (this.Program == null) return;
            if (this.Camera == null) return;

            #endregion

            //设置相机视口尺寸
            this.Camera.SetViewportSize(viewportWidth, viewportHeight);

            //渲染上下文
            RenderContext renderContext = new RenderContext(viewportWidth, viewportHeight, this.Camera.CameraPosition, this.Camera.LookDirection, this.Camera.ProjectionMatrix, this.Camera.ViewMatrix);

            //开启Shader程序
            this.Program.Use();

            //设置投影矩阵、视图矩阵
            this.Program.SetUniformMatrix4("u_ProjectionMatrix", this.Camera.ProjectionMatrix);
            this.Program.SetUniformMatrix4("u_ViewMatrix", this.Camera.ViewMatrix);

            foreach (TextRenderable renderable in this._renderables)
            {
                //渲染
                renderable.Render(this.Program, this.Camera);

                //触发渲染事件
                renderable.OnRender(renderContext);
            }

            //取消使用
            this.Program.Unuse();
        }
        #endregion

        #region 释放资源 —— override void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            foreach (TextRenderable text in this._renderables)
            {
                text.Dispose();
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
            this.Program = new ShaderProgram();
            this.Program.ReadVertexShaderFromFile("Resources/GLSLs/text.vert");
            this.Program.ReadFragmentShaderFromFile("Resources/GLSLs/text.frag");
            this.Program.Build();
        }
        #endregion

        #endregion
    }
}
