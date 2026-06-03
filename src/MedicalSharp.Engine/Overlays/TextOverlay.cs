using MedicalSharp.Engine.Base;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Builders;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Managers;
using MedicalSharp.Primitives.Models;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SkiaSharp;
using System;

namespace MedicalSharp.Engine.Overlays
{
    /// <summary>
    /// 文本Overlay
    /// </summary>
    public class TextOverlay : Renderable2D
    {
        #region # 字段及构造器

        /// <summary>
        /// 释放标识
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// 文本纹理
        /// </summary>
        private Texture2D _texture;

        /// <summary>
        /// 顶点缓冲区
        /// </summary>
        private VertexBuffer _vertexBuffer;

        /// <summary>
        /// 创建文本Overlay构造器
        /// </summary>
        /// <param name="text">文本内容</param>
        /// <param name="position">位置</param>
        /// <param name="fontSize">字体大小</param>
        /// <param name="color">文本颜色</param>
        public TextOverlay(string text, Vector2 position, float fontSize = 16.0f, Vector4 color = default)
        {
            this.Text = text;
            this.ScreenPosition = position;
            this.FontSize = fontSize;
            this.Color = color == default ? ColorFactory.White() : color;

            this.CreateTexture();
            this.CreateContainer();
        }

        #endregion

        #region # 属性

        #region 文本内容 —— string Text
        /// <summary>
        /// 文本内容
        /// </summary>
        public string Text { get; private set; }
        #endregion

        #region 字体大小 —— float FontSize
        /// <summary>
        /// 字体大小
        /// </summary>
        public float FontSize { get; private set; }
        #endregion

        #region 文本颜色 —— Vector4 Color
        /// <summary>
        /// 文本颜色
        /// </summary>
        public Vector4 Color { get; private set; }
        #endregion

        #region 只读属性 - 顶点缓冲区 —— VertexBuffer VertexBuffer
        /// <summary>
        /// 只读属性 - 顶点缓冲区
        /// </summary>
        internal VertexBuffer VertexBuffer
        {
            get => this._vertexBuffer;
        }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 更新文本渲染对象 —— void Update(string text, float fontSize)
        /// <summary>
        /// 更新文本渲染对象
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="fontSize">字体大小</param>
        public void Update(string text, float fontSize)
        {
            #region # 验证

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentNullException(nameof(text), "文本不可为空！");
            }
            if (this.Text == text && this.FontSize.Equals(fontSize))
            {
                return;
            }

            #endregion

            this.Text = text;
            this.FontSize = fontSize;

            this.CreateTexture();
            this.CreateContainer();
        }
        #endregion

        #region 设置颜色 —— void SetColor(Vector4 color)
        /// <summary>
        /// 设置颜色
        /// </summary>
        /// <param name="color">颜色</param>
        public void SetColor(Vector4 color)
        {
            this.Color = color;
        }
        #endregion

        #region 渲染 —— override void Render(ShaderProgram program, RenderContext2D context)
        /// <summary>
        /// 渲染
        /// </summary>
        /// <param name="program">Shader程序</param>
        /// <param name="context">渲染上下文</param>
        public override void Render(ShaderProgram program, RenderContext2D context)
        {
            #region # 验证

            if (this._texture == null || this._vertexBuffer == null)
            {
                return;
            }
            if (context == null)
            {
                return;
            }

            #endregion

            //构建模型矩阵
            Matrix4 modelMatrix = Matrix4.CreateTranslation(new Vector3(this.ScreenPosition.X, this.ScreenPosition.Y, 0));
            program.SetUniformMatrix4("u_ModelMatrix", modelMatrix);

            this._texture.Bind(0);
            program.SetUniformInt("u_ColorMode", (int)ColorMode.Mixed);
            program.SetUniformVector4("u_Color", this.Color);
            program.SetUniformInt("u_Texture", 0);

            this._vertexBuffer.Draw(context.GlContext, PrimitiveType.Triangles);
            this._texture.Unbind();
        }
        #endregion

        #region 释放资源 —— override void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this._texture?.Dispose();
            this._vertexBuffer?.Dispose();
            this._disposed = true;
        }
        #endregion


        //Private

        #region 创建纹理 —— void CreateTexture()
        /// <summary>
        /// 创建纹理
        /// </summary>
        private void CreateTexture()
        {
            this._texture?.Dispose();
            if (string.IsNullOrWhiteSpace(this.Text))
            {
                this._texture = null;
                return;
            }

            using SKTypeface typeface = SKTypeface.FromFile(ResourceManager.FontPath) ?? SKTypeface.Default;
            using SKFont font = new SKFont(typeface, this.FontSize);
            using SKPaint paint = new SKPaint();
            paint.Color = SKColors.White;
            paint.IsAntialias = true;

            font.MeasureText(this.Text, out SKRect bounds, paint);

            int width = Math.Max((int)Math.Ceiling(bounds.Width), 1);
            int height = Math.Max((int)Math.Ceiling(bounds.Height), 1);
            this.ScreenSize = new Vector2(width, height);

            SKImageInfo imageInfo = new SKImageInfo(width, height, SKColorType.Gray8);
            using SKSurface surface = SKSurface.Create(imageInfo);
            using SKCanvas canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            canvas.DrawText(this.Text, -bounds.Left, -bounds.Top, font, paint);

            using SKImage image = surface.Snapshot();
            using SKBitmap bitmap = SKBitmap.FromImage(image);
            IntPtr pixels = bitmap.GetPixels();

            this._texture = new Texture2D(width, height, PixelInternalFormat.Rgba, PixelFormat.Bgra, PixelType.UnsignedByte);
            this._texture.AllocateMemory(pixels);
            this._texture.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Linear);
            this._texture.SetWrapMode(TextureWrapMode.ClampToEdge);
        }
        #endregion

        #region 创建容器 —— void CreateContainer()
        /// <summary>
        /// 创建容器
        /// </summary>
        private void CreateContainer()
        {
            #region # 验证

            if (this._texture == null)
            {
                return;
            }

            #endregion

            MeshGeometry meshGeometry = MeshFactory.CreateOverlayContainer(this.ScreenSize.X, this.ScreenSize.Y);
            if (this._vertexBuffer == null)
            {
                this._vertexBuffer = new VertexBuffer(meshGeometry);
            }
            else
            {
                this._vertexBuffer.Update(meshGeometry);
            }
        }
        #endregion

        #endregion
    }
}
