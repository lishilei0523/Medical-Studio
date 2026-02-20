using MedicalSharp.Engine.Resources;
using MedicalSharp.Engine.Shaders;
using MedicalSharp.Engine.ValueTypes;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Engine.Renderables
{
    /// <summary>
    /// 线框渲染对象
    /// </summary>
    public class WireframeRenderable : Renderable, IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 无参构造器
        /// </summary>
        private WireframeRenderable()
        {
            this.Stroke = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            this.StrokeThickness = 1.0f;
            this.Fill = new Vector4(1.0f, 0.0f, 0.0f, 0.1f);
        }

        /// <summary>
        /// 创建线框渲染对象构造器
        /// </summary>
        /// <param name="vertexBuffer">顶点缓冲区</param>
        public WireframeRenderable(VertexBuffer vertexBuffer)
            : this()
        {
            this.VertexBuffer = vertexBuffer;
        }

        /// <summary>
        /// 析构器
        /// </summary>
        ~WireframeRenderable()
        {
            this.Dispose();
        }

        #endregion

        #region # 属性

        #region 顶点缓冲区 —— VertexBuffer VertexBuffer
        /// <summary>
        /// 顶点缓冲区
        /// </summary>
        public VertexBuffer VertexBuffer { get; private set; }
        #endregion

        #region 线框颜色 —— Vector4 Stroke
        /// <summary>
        /// 线框颜色
        /// </summary>
        public Vector4 Stroke { get; private set; }
        #endregion

        #region 线框粗细 —— float StrokeThickness
        /// <summary>
        /// 线框粗细
        /// </summary>
        public float StrokeThickness { get; private set; }
        #endregion

        #region 填充颜色 —— Vector4 Fill
        /// <summary>
        /// 填充颜色
        /// </summary>
        public Vector4 Fill { get; private set; }
        #endregion

        #endregion

        #region # 方法

        #region 设置颜色 —— void SetColor(Vector4 stroke, float strokeThickness...
        /// <summary>
        /// 设置颜色
        /// </summary>
        /// <param name="stroke">线框颜色</param>
        /// <param name="strokeThickness">线框粗细</param>
        /// <param name="fill">填充颜色</param>
        public void SetColor(Vector4 stroke, float strokeThickness, Vector4 fill)
        {
            this.Stroke = stroke;
            this.StrokeThickness = strokeThickness;
            this.Fill = fill;
        }
        #endregion

        #region 渲染 —— override void Render(ShaderProgram program, RenderContext context)
        /// <summary>
        /// 渲染
        /// </summary>
        /// <param name="program">Shader程序</param>
        /// <param name="context">渲染上下文</param>
        public override void Render(ShaderProgram program, RenderContext context)
        {
            //开启Shader程序
            program.Use();

            //设置MVP矩阵、相机位置/方向
            program.SetUniformMatrix("u_ProjectionMatrix", context.ProjectionMatrix);
            program.SetUniformMatrix("u_ViewMatrix", context.ViewMatrix);
            program.SetUniformMatrix("u_ModelMatrix", this.ModelMatrix);
            program.SetUniformVector3("u_CameraPosition", context.CameraPosition);

            //绘制填充模型	
            GL.DepthMask(false);//禁用深度写入、让透明面可以互相混合
            program.SetUniformVector4("u_Color", this.Fill);
            this.VertexBuffer.Draw(PrimitiveType.Triangles);
            GL.DepthMask(true);//恢复状态

            //绘制线框模型
            GL.LineWidth(this.StrokeThickness);
            program.SetUniformVector4("u_Color", this.Stroke);
            this.VertexBuffer.Draw(PrimitiveType.Lines);

            //取消使用
            program.Unuse();
        }
        #endregion

        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this.VertexBuffer?.Dispose();
        }
        #endregion 

        #endregion
    }
}
