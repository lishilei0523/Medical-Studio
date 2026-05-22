using MedicalSharp.Primitives.Maths;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.InteropServices;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 顶点数组对象
    /// </summary>
    public class VertexArray : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 释放标识
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// VAO
        /// </summary>
        private int _vao;

        /// <summary>
        /// 创建顶点数组对象构造器
        /// </summary>
        /// <param name="glContext">OpenGL上下文句柄</param>
        /// <param name="vertexBuffer">顶点缓冲区</param>
        internal VertexArray(IntPtr glContext, VertexBuffer vertexBuffer)
        {
            this._vao = GL.GenVertexArray();

            #region # 验证

            if (this._vao == 0)
            {
                throw new GlException("创建VAO失败！");
            }

            #endregion

            this.GlContext = glContext;
            this.VertexBuffer = vertexBuffer;
        }

        #endregion

        #region # 属性

        #region OpenGL上下文句柄 —— IntPtr GlContext
        /// <summary>
        /// OpenGL上下文句柄
        /// </summary>
        public IntPtr GlContext { get; private set; }
        #endregion

        #region 顶点缓冲区 —— VertexBuffer VertexBuffer
        /// <summary>
        /// 顶点缓冲区
        /// </summary>
        public VertexBuffer VertexBuffer { get; private set; }
        #endregion

        #endregion

        #region # 方法

        #region 绑定顶点数组对象 —— void Bind()
        /// <summary>
        /// 绑定顶点数组对象
        /// </summary>
        internal void Bind()
        {
            GL.BindVertexArray(this._vao);
        }
        #endregion

        #region 解绑顶点数组对象 —— void Unbind()
        /// <summary>
        /// 解绑顶点数组对象
        /// </summary>
        internal void Unbind()
        {
            GL.BindVertexArray(0);
        }
        #endregion

        #region 初始化顶点数组 —— void Setup()
        /// <summary>
        /// 初始化顶点数组
        /// </summary>
        internal unsafe void Setup()
        {
            this.Bind();
            this.VertexBuffer.Bind();

            //位置(location = 0)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(Vertex), Marshal.OffsetOf<Vertex>(nameof(Vertex.Position)).ToInt32());
            GL.EnableVertexAttribArray(0);

            //颜色(location = 1)
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, sizeof(Vertex), Marshal.OffsetOf<Vertex>(nameof(Vertex.Color)).ToInt32());
            GL.EnableVertexAttribArray(1);

            //纹理坐标(location = 2)
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, sizeof(Vertex), Marshal.OffsetOf<Vertex>(nameof(Vertex.TextureCoord)).ToInt32());
            GL.EnableVertexAttribArray(2);

            //法向量(location = 3)
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, sizeof(Vertex), Marshal.OffsetOf<Vertex>(nameof(Vertex.Normal)).ToInt32());
            GL.EnableVertexAttribArray(3);

            this.VertexBuffer.Unbind();
            this.Unbind();
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

            if (this._vao != 0)
            {
                GL.DeleteVertexArray(this._vao);
                this._vao = 0;
            }

            this._disposed = true;
        }
        #endregion

        #endregion
    }
}
