using MedicalSharp.Engine.Cameras;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Shaders;
using MedicalSharp.Engine.ValueTypes;
using System;
using System.Collections.Generic;

namespace MedicalSharp.Engine.Renderers
{
    /// <summary>
    /// 线框渲染器
    /// </summary>
    public class WireframeRenderer : IDisposable
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
        {
            #region # 验证

            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera), "相机不可为空！");
            }

            #endregion

            this.Camera = camera;

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
        {
            #region # 验证

            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera), "相机不可为空！");
            }
            if (program == null)
            {
                throw new ArgumentNullException(nameof(program), "Shader程序不可为空！");
            }

            #endregion

            this.Camera = camera;
            this.Program = program;

            //默认值
            this._renderables = new HashSet<WireframeRenderable>();
        }

        #endregion

        #region # 属性

        #region Shader程序 —— ShaderProgram Program
        /// <summary>
        /// Shader程序
        /// </summary>
        public ShaderProgram Program { get; private set; }
        #endregion

        #region 相机 —— Camera Camera
        /// <summary>
        /// 相机
        /// </summary>
        public Camera Camera { get; private set; }
        #endregion

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

        #region 渲染帧 —— void RenderFrame(float viewportWidth, float viewportHeight)
        /// <summary>
        /// 渲染帧
        /// </summary>
        /// <param name="viewportWidth">视口宽度</param>
        /// <param name="viewportHeight">视口高度</param>
        public void RenderFrame(float viewportWidth, float viewportHeight)
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

            RenderContext context = new RenderContext(viewportWidth, viewportHeight, this.Camera.CameraPosition, this.Camera.LookDirection, this.Camera.ProjectionMatrix, this.Camera.ViewMatrix);
            foreach (WireframeRenderable renderable in this._renderables)
            {
                renderable.Render(this.Program, context);
            }
        }
        #endregion

        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            foreach (WireframeRenderable renderable in this._renderables)
            {
                renderable.Dispose();
            }

            this._renderables.Clear();
            this.Program.Dispose();
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
            this.Program.ReadVertexShaderFromFile("Shaders/GLSLs/wireframe.vert");
            this.Program.ReadFragmentShaderFromFile("Shaders/GLSLs/wireframe.frag");
            this.Program.Build();
        }
        #endregion 

        #endregion
    }
}
