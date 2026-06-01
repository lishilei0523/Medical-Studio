using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Builders;
using MedicalSharp.Primitives.Enums;
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
        private const float TargetScreenSize = 40f;

        /// <summary>
        /// 纹理尺寸
        /// </summary>
        private const int TextureSize = 256;

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
            this.Distance = this.SideLength * 18f;

            //创建几何体
            MeshGeometry boxMesh = MeshFactory.CreateViewBox(this.SideLength, this.SideLength, this.SideLength);
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

        #region ViewBox距离 —— float Distance
        /// <summary>
        /// ViewBox距离
        /// </summary>
        public float Distance { get; private set; }
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
                                  + context.LookDirection * this.Distance
                                  + context.RightDirection * 0.6f
                                  - context.UpDirection * 0.36f;
            this.Transform.SetPosition(worldPosition);

            //计算缩放
            float fieldOfView = MathHelper.DegreesToRadians(context.FieldOfView);
            float worldHeightAtDistance = 2.0f * this.Distance * MathF.Tan(fieldOfView * 0.5f);
            float scale = (TargetScreenSize / context.ViewportHeight) * worldHeightAtDistance / this.SideLength;
            this.Transform.SetScale(new Vector3(scale));

            //设置模型矩阵
            program.SetUniformMatrix4("u_ModelMatrix", this.ModelMatrix);

            //设置正交投影矩阵
            const float size = 0.5f;
            float aspect = context.ViewportWidth / context.ViewportHeight;
            float left, right, bottom, top;
            if (aspect >= 1.0f)
            {
                left = -size * aspect;
                right = size * aspect;
                bottom = -size;
                top = size;
            }
            else
            {
                left = -size;
                right = size;
                bottom = -size / aspect;
                top = size / aspect;
            }
            Matrix4 projectionMatrix = Matrix4.CreateOrthographicOffCenter(left, right, bottom, top, 0.0f, 2.0f);
            program.SetUniformMatrix4("u_ProjectionMatrix", projectionMatrix);

            //渲染6个面（每个面6个索引）
            for (int index = 0; index < 6; index++)
            {
                this._faceTextures[index].Bind(0);

                program.SetUniformInt("u_ColorMode", (int)ColorMode.Texture);
                program.SetUniformInt("u_Texture", 0);
                this._vertexBuffer.DrawRange(context.GlContext, PrimitiveType.Triangles, index * 6, 6);

                this._faceTextures[index].Unbind();
            }

            //恢复透视投影矩阵
            program.SetUniformMatrix4("u_ProjectionMatrix", context.ProjectionMatrix);
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
        /// 创建纹理
        /// </summary>
        private Texture2D[] CreateFaceTextures()
        {
            var faces = new[]
            {
                new { Name = "P", Color = new SKColor(255, 51, 51), Rotation = 180, FlipHorizontal = false },
                new { Name = "A", Color = new SKColor(255, 51, 51), Rotation = -90, FlipHorizontal = false },
                new { Name = "S", Color = new SKColor(0, 102, 0),   Rotation = 180, FlipHorizontal = true },
                new { Name = "I", Color = new SKColor(0, 102, 0),   Rotation = 90,  FlipHorizontal = false },
                new { Name = "R", Color = new SKColor(51, 51, 255), Rotation = -90, FlipHorizontal = true },
                new { Name = "L", Color = new SKColor(51, 51, 255), Rotation = 180, FlipHorizontal = false }
            };

            Texture2D[] textures = new Texture2D[6];
            for (int index = 0; index < faces.Length; index++)
            {
                var face = faces[index];
                textures[index] = this.CreateFaceTexture(face.Color, face.Name, face.Rotation, face.FlipHorizontal);
            }

            return textures;
        }
        #endregion

        #region 创建面纹理 —— Texture2D CreateFaceTexture(SKColor background, string text...
        /// <summary>
        /// 创建面纹理
        /// </summary>
        /// <param name="background">背景颜色</param>
        /// <param name="text">文本</param>
        /// <param name="rotation">旋转角度</param>
        /// <param name="flipHorizontal">水平翻转</param>
        private Texture2D CreateFaceTexture(in SKColor background, string text, float rotation, bool flipHorizontal = false)
        {
            SKImageInfo imageInfo = new SKImageInfo(TextureSize, TextureSize);
            using SKSurface surface = SKSurface.Create(imageInfo);

            //绘制背景
            using SKCanvas canvas = surface.Canvas;
            this.ClearBackground(canvas, background);

            //调整角度
            canvas.Translate(TextureSize / 2f, TextureSize / 2f);
            canvas.RotateDegrees(rotation);
            if (flipHorizontal)
            {
                canvas.Scale(-1, 1);
            }
            canvas.Translate(-TextureSize / 2f, -TextureSize / 2f);

            //绘制边框
            using SKPaint stroke = new SKPaint();
            stroke.Color = SKColors.LightGray;
            stroke.StrokeWidth = 10;
            stroke.IsStroke = true;
            SKRect rectangle = new SKRect(2, 2, TextureSize - 2, TextureSize - 2);
            canvas.DrawRect(rectangle, stroke);

            //绘制文本
            using SKTypeface typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold);
            using SKFont font = new SKFont(typeface, TextureSize * 0.5f);
            using SKPaint textPaint = new SKPaint();
            textPaint.Color = SKColors.White;
            textPaint.IsAntialias = true;
            const float centerX = TextureSize / 2f;
            const float centerY = TextureSize / 2f + TextureSize * 0.15f;
            canvas.DrawText(text, centerX, centerY, SKTextAlign.Center, font, textPaint);

            //创建纹理
            using SKImage image = surface.Snapshot();
            using SKBitmap bitmap = SKBitmap.FromImage(image);
            IntPtr pixels = bitmap.GetPixels();
            Texture2D texture = new Texture2D(TextureSize, TextureSize, PixelInternalFormat.Rgba, PixelFormat.Bgra);
            texture.AllocateMemory(pixels);
            texture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
            texture.SetWrapMode(TextureWrapMode.ClampToEdge);

            return texture;
        }
        #endregion

        #region 清空背景色 —— void ClearBackground(SKCanvas canvas, in SKColor background)
        /// <summary>
        /// 清空背景色
        /// </summary>
        private void ClearBackground(SKCanvas canvas, in SKColor background)
        {
            //计算渐变颜色
            SKColor lightColor = new SKColor(
                (byte)Math.Min(255, background.Red + (255 - background.Red) * 0.6f),
                (byte)Math.Min(255, background.Green + (255 - background.Green) * 0.6f),
                (byte)Math.Min(255, background.Blue + (255 - background.Blue) * 0.6f),
                background.Alpha
            );
            SKColor darkColor = new SKColor(
                (byte)(background.Red * 0.5f),
                (byte)(background.Green * 0.5f),
                (byte)(background.Blue * 0.5f),
                background.Alpha
            );

            //对角线渐变
            using SKPaint gradientPaint = new SKPaint();
            SKPoint start = new SKPoint(0, 0);
            SKPoint end = new SKPoint(TextureSize, TextureSize);
            SKColor[] gradientColors = [lightColor, background, darkColor];
            float[] gradientPositions = [0.0f, 0.5f, 1.0f];
            using SKShader shader = SKShader.CreateLinearGradient(start, end, gradientColors, gradientPositions, SKShaderTileMode.Clamp);
            gradientPaint.Shader = shader;
            canvas.DrawRect(new SKRect(0, 0, TextureSize, TextureSize), gradientPaint);

            //添加暗角
            using SKPaint vignettePaint = new SKPaint();
            vignettePaint.Color = new SKColor(0, 0, 0, 50);
            vignettePaint.IsAntialias = true;
            canvas.DrawRect(new SKRect(0, 0, TextureSize, TextureSize), vignettePaint);
        }
        #endregion

        #endregion
    }
}
