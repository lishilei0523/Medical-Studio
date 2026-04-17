using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Builders;
using MedicalSharp.Primitives.Cameras;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Managers;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace MedicalSharp.Engine.Renderables
{
    /// <summary>
    /// 文本渲染对象
    /// </summary>
    public class TextRenderable : Renderable, IHasTriangles, IDisposable
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
        /// 三角形列表
        /// </summary>
        private IList<Triangle> _triangles;

        /// <summary>
        /// 创建固定朝向文本渲染对象构造器
        /// </summary>
        /// <param name="text">文本内容</param>
        /// <param name="position">位置</param>
        /// <param name="fontSize">字体大小</param>
        /// <param name="color">文本颜色</param>
        /// <param name="normal">法向量</param>
        public TextRenderable(string text, Vector3 position, float fontSize = 16.0f, Vector4 color = default, Vector3 normal = default)
        {
            this.Text = text;
            this.FontSize = fontSize;
            this.Color = color == default ? ColorFactory.White() : color;
            this.Normal = normal == default ? Vector3.UnitY : normal;
            this.RenderMode = TextRenderMode.Fixed;
            this.LockYAxis = true;

            this.Transform.SetPosition(position);

            this.CreateTexture();
            this.CreateContainer2D();

            //提取三角形面
            this._triangles = this.VertexBuffer.MeshGeometry.ExtractTriangles();
        }

        /// <summary>
        /// 创建广告牌文本渲染对象构造器
        /// </summary>
        /// <param name="text">文本内容</param>
        /// <param name="position">位置</param>
        /// <param name="fontSize">字体大小</param>
        /// <param name="color">文本颜色</param>
        /// <param name="lockYAxis">是否锁定Y轴</param>
        public TextRenderable(string text, Vector3 position, float fontSize = 24.0f, Vector4 color = default, bool lockYAxis = true)
        {
            this.Text = text;
            this.FontSize = fontSize;
            this.Color = color == default ? ColorFactory.White() : color;
            this.LockYAxis = lockYAxis;
            this.Normal = Vector3.UnitZ;
            this.RenderMode = TextRenderMode.Billboard;

            this.Transform.SetPosition(position);

            this.CreateTexture();
            this.CreateContainer2D();

            //提取三角形面
            this._triangles = this.VertexBuffer.MeshGeometry.ExtractTriangles();
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

        #region 渲染模式 —— TextRenderMode RenderMode
        /// <summary>
        /// 渲染模式
        /// </summary>
        public TextRenderMode RenderMode { get; private set; }
        #endregion

        #region 法向量 —— Vector3 Normal
        /// <summary>
        /// 法向量
        /// </summary>
        /// <remarks>固定朝向模式下生效</remarks>
        public Vector3 Normal { get; private set; }
        #endregion

        #region 是否锁定Y轴 —— bool LockYAxis
        /// <summary>
        /// 是否锁定Y轴
        /// </summary>
        /// <remarks>广告牌模式下保持文本直立</remarks>
        public bool LockYAxis { get; private set; }
        #endregion

        #region 文本尺寸 —— Vector2 TextSize
        /// <summary>
        /// 文本尺寸
        /// </summary>
        /// <remarks>像素</remarks>
        public Vector2 TextSize { get; private set; }
        #endregion

        #region 只读属性 - 三角形列表 —— IList<Triangle> Triangles
        /// <summary>
        /// 只读属性 - 三角形列表
        /// </summary>
        public IList<Triangle> Triangles
        {
            get => this._triangles;
        }
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
            this.Text = text;
            this.FontSize = fontSize;

            this.CreateTexture();
            this.CreateContainer2D();
            base.InvalidateBoundings();

            //提取三角形面
            this._triangles = this.VertexBuffer.MeshGeometry.ExtractTriangles();
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

        #region 渲染 —— void Render(ShaderProgram program, Camera camera)
        /// <summary>
        /// 渲染
        /// </summary>
        /// <param name="program">Shader程序</param>
        /// <param name="camera">相机</param>
        /// <remarks>始终面向相机</remarks>
        public void Render(ShaderProgram program, Camera camera)
        {
            #region # 验证

            if (this._texture == null || this._vertexBuffer == null)
            {
                return;
            }
            if (camera == null)
            {
                return;
            }

            #endregion

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.DepthTest);

            this._texture.Bind(0);
            program.SetUniformInt("u_TextTexture", 0);
            program.SetUniformVector4("u_Color", this.Color);

            if (this.RenderMode == TextRenderMode.Fixed)
            {
                program.SetUniformMatrix4("u_ModelMatrix", this.ModelMatrix);
            }
            if (this.RenderMode == TextRenderMode.Billboard)
            {
                //计算广告牌矩阵
                Matrix4 billboardMatrix = this.CalculateBillboardMatrix(camera);
                program.SetUniformMatrix4("u_ModelMatrix", billboardMatrix);
            }

            this._vertexBuffer.Draw(PrimitiveType.Triangles);

            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);
        }
        #endregion

        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
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


        //Protected

        #region 计算包围盒 —— override BoundingBox CalculateBoundingBox()
        /// <summary>
        /// 计算包围盒
        /// </summary>
        protected override BoundingBox CalculateBoundingBox()
        {
            float halfW = this.TextSize.X * 0.5f;
            float halfH = this.TextSize.Y * 0.5f;

            return new BoundingBox(
                new Vector3(-halfW, -halfH, 0),
                new Vector3(halfW, halfH, 0)
            );
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
            this.TextSize = new Vector2(width, height);

            using SKSurface surface = SKSurface.Create(new SKImageInfo(width, height));
            using SKCanvas canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            canvas.DrawText(this.Text, -bounds.Left, -bounds.Top, font, paint);

            using SKImage image = surface.Snapshot();
            using SKBitmap bitmap = SKBitmap.FromImage(image);
            IntPtr pixels = bitmap.GetPixels();

            this._texture = new Texture2D(width, height, PixelInternalFormat.R8, PixelFormat.Red, PixelType.UnsignedByte);
            this._texture.AllocateMemory(pixels);
            this._texture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
        }
        #endregion

        #region 创建2D容器 —— void CreateContainer2D()
        /// <summary>
        /// 创建2D容器
        /// </summary>
        private void CreateContainer2D()
        {
            #region # 验证

            if (this._texture == null)
            {
                this._vertexBuffer?.Dispose();
                this._vertexBuffer = null;
                return;
            }

            #endregion

            this._vertexBuffer?.Dispose();

            MeshGeometry meshGeometry = MeshFactory.CreateContainer2D(this.TextSize.X, this.TextSize.Y, this.Normal);
            this._vertexBuffer = new VertexBuffer(meshGeometry);
            this._vertexBuffer.Setup();
        }
        #endregion

        #region 计算广告牌矩阵 —— Matrix4 CalculateBillboardMatrix(Camera camera)
        /// <summary>
        /// 计算广告牌矩阵
        /// </summary>
        /// <param name="camera">相机</param>
        /// <returns>广告牌矩阵</returns>
        /// <remarks>Z-up</remarks>
        private Matrix4 CalculateBillboardMatrix(Camera camera)
        {
            Vector3 forward = camera.CameraPosition - this.Transform.Position;
            forward.Normalize();

            //锁定Y轴：只绕Z轴旋转，保持文本直立
            if (this.LockYAxis)
            {
                float angle = MathF.Atan2(forward.Y, forward.X) - MathHelper.PiOver2;
                Matrix4 rotation = Matrix4.CreateRotationZ(angle);
                Matrix4 translation = Matrix4.CreateTranslation(this.Transform.Position);

                return rotation * translation;
            }
            //完全面向相机（球形广告牌）
            else
            {
                Vector3 up = Vector3.UnitZ;
                Vector3 right = Vector3.Normalize(Vector3.Cross(up, forward));
                Vector3 newUp = Vector3.Cross(forward, right);

                Matrix4 rotation = new Matrix4(
                    right.X, right.Y, right.Z, 0,
                    newUp.X, newUp.Y, newUp.Z, 0,
                    forward.X, forward.Y, forward.Z, 0,
                    0, 0, 0, 1
                );
                Matrix4 translation = Matrix4.CreateTranslation(this.Transform.Position);

                return rotation * translation;
            }
        }
        #endregion

        #endregion
    }
}
