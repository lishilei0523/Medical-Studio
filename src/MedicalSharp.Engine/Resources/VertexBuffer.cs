using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 顶点缓冲区
    /// </summary>
    public class VertexBuffer : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 释放标识
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// VBO
        /// </summary>
        private int _vbo;

        /// <summary>
        /// EBO
        /// </summary>
        private int _ebo;

        /// <summary>
        /// 顶点数组对象字典
        /// </summary>
        /// <remarks>键：OpenGL上下文句柄，值：顶点数组对象</remarks>
        private readonly IDictionary<IntPtr, VertexArray> _vertexArrays;

        /// <summary>
        /// 默认构造器
        /// </summary>
        private VertexBuffer()
        {
            this._vbo = GL.GenBuffer();
            this._ebo = GL.GenBuffer();

            #region # 验证

            if (this._vbo == 0)
            {
                throw new GlException("创建VBO失败！");
            }
            if (this._ebo == 0)
            {
                throw new GlException("创建EBO失败！");
            }

            #endregion

            this._vertexArrays = new ConcurrentDictionary<IntPtr, VertexArray>();
        }

        /// <summary>
        /// 创建顶点缓冲区构造器
        /// </summary>
        /// <param name="meshGeometry">网格几何</param>
        /// <param name="bufferUsage">缓冲区用途</param>
        public VertexBuffer(MeshGeometry meshGeometry, BufferUsageHint bufferUsage = BufferUsageHint.DynamicDraw)
            : this()
        {
            #region # 验证

            if (meshGeometry == null)
            {
                throw new ArgumentNullException(nameof(meshGeometry), "网格几何不可为空！");
            }

            #endregion

            this.MeshGeometry = meshGeometry;
            this.BufferUsage = bufferUsage;

            //更新数据
            this.Update(this.MeshGeometry);
        }

        #endregion

        #region # 属性

        #region 网格几何 —— MeshGeometry MeshGeometry
        /// <summary>
        /// 网格几何
        /// </summary>
        public MeshGeometry MeshGeometry { get; private set; }
        #endregion

        #region 缓冲区用途 —— BufferUsageHint BufferUsage
        /// <summary>
        /// 缓冲区用途
        /// </summary>
        public BufferUsageHint BufferUsage { get; private set; }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 绑定顶点缓冲区 —— void Bind()
        /// <summary>
        /// 绑定顶点缓冲区
        /// </summary>
        public void Bind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, this._vbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this._ebo);
        }
        #endregion

        #region 解绑顶点缓冲区 —— void Unbind()
        /// <summary>
        /// 解绑顶点缓冲区
        /// </summary>
        public void Unbind()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }
        #endregion

        #region 绘制顶点缓冲区 —— void Draw(IntPtr glContext, PrimitiveType...
        /// <summary>
        /// 绘制顶点缓冲区
        /// </summary>
        /// <param name="glContext">OpenGL上下文句柄</param>
        /// <param name="primitiveType">图元类型</param>
        public void Draw(IntPtr glContext, PrimitiveType primitiveType)
        {
            //确保VAO
            VertexArray vertexArray = this.Ensure(glContext);

            vertexArray.Bind();
            this.Bind();

            if (this.MeshGeometry.Indices.Any())
            {
                GL.DrawElements(primitiveType, this.MeshGeometry.Indices.Length, DrawElementsType.UnsignedInt, 0);
            }
            else
            {
                GL.DrawArrays(primitiveType, 0, this.MeshGeometry.Vertices.Length);
            }

            this.Unbind();
            vertexArray.Unbind();
        }
        #endregion

        #region 更新顶点缓冲区 —— void Update(MeshGeometry meshGeometry)
        /// <summary>
        /// 更新顶点缓冲区
        /// </summary>
        /// <param name="meshGeometry">网格几何</param>
        public unsafe void Update(MeshGeometry meshGeometry)
        {
            this.MeshGeometry = meshGeometry;
            this.Bind();

            Vertex[] vertices = this.MeshGeometry.Vertices;
            uint[] indices = this.MeshGeometry.Indices;

            //更新顶点
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(Vertex), vertices, this.BufferUsage);

            //更新索引
            if (this.MeshGeometry.Indices.Length > 0)
            {
                GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, this.BufferUsage);
            }

            //解绑
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

            if (this._vbo != 0)
            {
                GL.DeleteBuffer(this._vbo);
                this._vbo = 0;
            }
            if (this._ebo != 0)
            {
                GL.DeleteBuffer(this._ebo);
                this._ebo = 0;
            }
            foreach (VertexArray vertexArray in this._vertexArrays.Values)
            {
                vertexArray.Dispose();
            }

            this._disposed = true;
        }
        #endregion


        //Private

        #region 确保顶点缓冲区 —— VertexArray Ensure(IntPtr glContext)
        /// <summary>
        /// 确保顶点缓冲区
        /// </summary>
        /// <param name="glContext">OpenGL上下文句柄</param>
        /// <remarks>初始化VAO</remarks>
        private VertexArray Ensure(IntPtr glContext)
        {
            if (!this._vertexArrays.TryGetValue(glContext, out VertexArray vertexArray))
            {
                vertexArray = new VertexArray(glContext, this);
                vertexArray.Setup();
                this._vertexArrays.Add(glContext, vertexArray);
            }

            return vertexArray;
        }
        #endregion

        #endregion
    }
}
