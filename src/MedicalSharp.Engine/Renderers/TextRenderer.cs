using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Cameras;
using OpenTK.Mathematics;
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
        /// 固定朝向文本列表
        /// </summary>
        private readonly ICollection<TextRenderable> _fixedTextRenderables;

        /// <summary>
        /// 广告牌文本列表
        /// </summary>
        private readonly ICollection<TextRenderable> _billboardTextRenderables;

        /// <summary>
        /// 创建文本渲染器构造器
        /// </summary>
        /// <param name="camera">相机</param>
        public TextRenderer(Camera camera)
            : base(camera)
        {
            this._fixedTextRenderables = new HashSet<TextRenderable>();
            this._billboardTextRenderables = new HashSet<TextRenderable>();
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
            this._fixedTextRenderables = new HashSet<TextRenderable>();
            this._billboardTextRenderables = new HashSet<TextRenderable>();
        }

        #endregion

        #region # 属性

        #region 只读属性 - 渲染对象列表 —— IReadOnlySet<ShapeRenderable> Renderables
        /// <summary>
        /// 只读属性 - 渲染对象列表
        /// </summary>
        public IReadOnlySet<ShapeRenderable> Renderables
        {
            get => (IReadOnlySet<ShapeRenderable>)this._renderables;
        }
        #endregion

        #endregion

        #region 方法

        /// <summary>
        /// 添加固定朝向文本
        /// </summary>
        /// <param name="text">文本内容</param>
        /// <param name="position">位置</param>
        /// <param name="fontSize">字体大小</param>
        /// <param name="color">文本颜色</param>
        /// <param name="planeNormal">平面法向量</param>
        /// <param name="fontPath">字体文件路径</param>
        /// <returns>文本对象</returns>
        public TextRenderable AddFixedText(string text, Vector3 position, float fontSize = 24.0f,
            Vector4? color = null, Vector3? planeNormal = null, string fontPath = "msyh.ttf")
        {
            TextRenderable textObj = new TextRenderable(text, position, fontSize, color, planeNormal, fontPath);
            this._fixedTextRenderables.Add(textObj);
            return textObj;
        }

        /// <summary>
        /// 添加广告牌文本
        /// </summary>
        /// <param name="text">文本内容</param>
        /// <param name="position">位置</param>
        /// <param name="fontSize">字体大小</param>
        /// <param name="color">文本颜色</param>
        /// <param name="lockYAxis">是否锁定Y轴</param>
        /// <param name="fontPath">字体文件路径</param>
        /// <returns>文本对象</returns>
        public TextRenderable AddBillboardText(string text, Vector3 position, float fontSize = 24.0f,
            Vector4? color = null, bool lockYAxis = true, string fontPath = "msyh.ttf")
        {
            TextRenderable textObj = new TextRenderable(text, position, fontSize, color, lockYAxis, fontPath);
            this._billboardTextRenderables.Add(textObj);
            return textObj;
        }

        /// <summary>
        /// 移除固定朝向文本
        /// </summary>
        /// <param name="text">文本对象</param>
        public void RemoveFixedText(TextRenderable text)
        {
            this._fixedTextRenderables.Remove(text);
            text.Dispose();
        }

        /// <summary>
        /// 移除广告牌文本
        /// </summary>
        /// <param name="text">文本对象</param>
        public void RemoveBillboardText(TextRenderable text)
        {
            this._billboardTextRenderables.Remove(text);
            text.Dispose();
        }


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

            //设置相机视口
            this.Camera.SetViewportSize(viewportWidth, viewportHeight);

            //使用Shader
            this.Program.Use();
            this.Program.SetUniformMatrix4("u_ProjectionMatrix", this.Camera.ProjectionMatrix);
            this.Program.SetUniformMatrix4("u_ViewMatrix", this.Camera.ViewMatrix);

            //渲染固定朝向文本
            foreach (TextRenderable text in this._fixedTextRenderables)
            {
                this.Program.SetUniformMatrix4("u_ModelMatrix", text.ModelMatrix);
                text.Render(this.Program);
            }

            //渲染广告牌文本
            foreach (TextRenderable text in this._billboardTextRenderables)
            {
                text.RenderBillboard(this.Program, this.Camera);
            }

            //取消使用
            this.Program.Unuse();
        }

        #endregion

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

        #region 释放资源 —— override void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            foreach (TextRenderable text in this._fixedTextRenderables)
            {
                text.Dispose();
            }
            foreach (TextRenderable text in this._billboardTextRenderables)
            {
                text.Dispose();
            }
            this._fixedTextRenderables.Clear();
            this._billboardTextRenderables.Clear();
        }
        #endregion
    }
}