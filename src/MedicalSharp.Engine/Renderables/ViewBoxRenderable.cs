using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Builders;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SkiaSharp;
using System;

namespace MedicalSharp.Engine.Renderables
{
    /// <summary>
    /// ViewBox渲染对象
    /// </summary>
    public class ViewBoxRenderable : ShapeRenderable
    {
        #region # 字段及构造器

        /// <summary>
        /// 目标屏幕尺寸
        /// </summary>
        /// <remarks>像素</remarks>
        private const float TargetScreenSize = 50f;

        /// <summary>
        /// 纹理尺寸
        /// </summary>
        private const int TextureSize = 256;

        /// <summary>
        /// 坐标轴距离
        /// </summary>
        private const float Distance = 1.8f;

        /// <summary>
        /// 顶点缓冲区
        /// </summary>
        private readonly VertexBuffer _vertexBuffer;

        /// <summary>
        /// 6个面的纹理
        /// </summary>
        private readonly Texture2D[] _faceTextures;

        /// <summary>
        /// 创建ViewBox渲染对象构造器
        /// </summary>
        /// <param name="sideLength">棱长</param>
        public ViewBoxRenderable(float sideLength)
        {
            this.SideLength = sideLength;

            //创建几何体
            MeshGeometry boxMesh = MeshFactory.CreateBox(this.SideLength, this.SideLength, this.SideLength);
            this._vertexBuffer = new VertexBuffer(boxMesh);

            //创建纹理
            this._faceTextures = this.CreateFaceTextures();
        }

        #endregion

        #region # 属性

        #region 棱长 —— float SideLength
        /// <summary>
        /// 棱长
        /// </summary>
        public float SideLength { get; private set; }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 渲染 —— override void Render(ShaderProgram program, RenderContext3D context)
        /// <summary>
        /// 渲染
        /// </summary>
        /// <param name="program">Shader程序</param>
        /// <param name="context">渲染上下文</param>
        public override void Render(ShaderProgram program, RenderContext3D context)
        {
            //计算位置（固定在屏幕右下角）
            Vector3 worldPosition = context.CameraPosition
                                  + context.LookDirection * Distance
                                  + context.RightDirection * 0.55f
                                  - context.UpDirection * 0.35f;
            this.Transform.SetPosition(worldPosition);

            //计算缩放
            float fieldOfView = MathHelper.DegreesToRadians(context.FieldOfView);
            float worldHeightAtDistance = 2.0f * Distance * MathF.Tan(fieldOfView * 0.5f);
            float scale = (TargetScreenSize / context.ViewportHeight) * worldHeightAtDistance / this.SideLength;
            this.Transform.SetScale(new Vector3(scale));

            //设置模型矩阵
            program.SetUniformMatrix4("u_ModelMatrix", this.ModelMatrix);
            program.SetUniformInt("u_HasTexture", 1);
            program.SetUniformVector4("u_Color", Vector4.One);

            //渲染6个面（每个面6个索引）
            for (int index = 0; index < 6; index++)
            {
                this._faceTextures[index].Bind(0);

                program.SetUniformInt("u_Texture", 0);
                this._vertexBuffer.DrawRange(context.GlContext, PrimitiveType.Triangles, index * 6, 6);

                this._faceTextures[index].Unbind();
            }
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

            this._vertexBuffer?.Dispose();
            if (this._faceTextures != null)
            {
                foreach (Texture2D texture in this._faceTextures)
                {
                    texture?.Dispose();
                }
            }

            this._disposed = true;
        }
        #endregion


        //Protected && Private

        #region 计算包围盒 —— override BoundingBox CalculateBoundingBox()
        /// <summary>
        /// 计算包围盒
        /// </summary>
        protected override BoundingBox CalculateBoundingBox()
        {
            return default;
        }
        #endregion

        #region 创建纹理 —— Texture2D[] CreateFaceTextures()
        /// <summary>
        /// 创建6个面的纹理
        /// </summary>
        private Texture2D[] CreateFaceTextures()
        {
            //面的顺序与 CreateBox 一致
            //0: Y+ (Posterior)
            //1: Y- (Anterior)
            //2: Z+ (Superior)
            //3: Z- (Inferior)
            //4: X- (Right)
            //5: X+ (Left)
            var faces = new[]
            {
                new { Name = "P", Color = new SKColor(51, 255, 51), Text = "P", Desc = "Posterior" },  // Y+ - 亮绿
                new { Name = "A", Color = new SKColor(0, 102, 0), Text = "A", Desc = "Anterior" },     // Y- - 暗绿
                new { Name = "S", Color = new SKColor(51, 51, 255), Text = "S", Desc = "Superior" },   // Z+ - 亮蓝
                new { Name = "I", Color = new SKColor(0, 0, 102), Text = "I", Desc = "Inferior" },     // Z- - 暗蓝
                new { Name = "R", Color = new SKColor(255, 51, 51), Text = "R", Desc = "Right" },      // X- - 亮红
                new { Name = "L", Color = new SKColor(102, 0, 0), Text = "L", Desc = "Left" }          // X+ - 暗红
            };

            Texture2D[] textures = new Texture2D[6];
            for (int index = 0; index < faces.Length; index++)
            {
                textures[index] = this.CreateFaceTexture(faces[index].Color, faces[index].Text);
            }

            return textures;
        }
        #endregion

        #region 创建单个面纹理 —— Texture2D CreateFaceTexture(SKColor color, string text)
        /// <summary>
        /// 创建单个面纹理
        /// </summary>
        private Texture2D CreateFaceTexture(SKColor color, string text)
        {
            int size = TextureSize;

            using SKSurface surface = SKSurface.Create(new SKImageInfo(size, size));
            SKCanvas canvas = surface.Canvas;
            canvas.Clear(color);

            //绘制边框
            using (SKPaint paint = new SKPaint { Color = SKColors.Black, StrokeWidth = 4, IsStroke = true })
            {
                canvas.DrawRect(new SKRect(2, 2, size - 2, size - 2), paint);
            }

            //绘制文字
            using (SKPaint paint = new SKPaint())
            {
                paint.Color = SKColors.White;
                paint.TextSize = size * 0.5f;
                paint.IsAntialias = true;
                paint.TextAlign = SKTextAlign.Center;
                paint.Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);

                float centerX = size / 2f;
                float centerY = size / 2f + size * 0.15f;
                canvas.DrawText(text, centerX, centerY, paint);
            }

            //创建纹理
            using (SKImage image = surface.Snapshot())
            using (SKBitmap bitmap = SKBitmap.FromImage(image))
            {
                IntPtr pixels = bitmap.GetPixels();
                Texture2D texture = new Texture2D(size, size, PixelInternalFormat.Rgba, PixelFormat.Bgra, PixelType.UnsignedByte);
                texture.AllocateMemory(pixels);
                texture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
                texture.SetWrapMode(TextureWrapMode.ClampToEdge);

                return texture;
            }
        }
        #endregion

        #endregion
    }
}
