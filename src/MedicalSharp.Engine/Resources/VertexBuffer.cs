using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using Microsoft.CSharp.RuntimeBinder;
using OpenTK.Graphics.OpenGL4;
using System;
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
        /// 默认构造器
        /// </summary>
        private VertexBuffer()
        {
            this._vbo = GL.GenBuffer();
            this._ebo = GL.GenBuffer();

            #region # 验证

            if (this._vbo == 0)
            {
                throw new RuntimeBinderException("创建VBO失败！");
            }
            if (this._ebo == 0)
            {
                throw new RuntimeBinderException("创建EBO失败！");
            }

            #endregion

            this.VertexArray = new VertexArray(this);
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

            //分配内存
            this.AllocateMemory();

            //初始化VAO
            this.VertexArray.Setup();
        }

        #endregion

        #region # 属性

        #region 顶点数组对象 —— VertexArray VertexArray
        /// <summary>
        /// 顶点数组对象
        /// </summary>
        internal VertexArray VertexArray { get; private set; }
        #endregion

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

        #region 绘制顶点缓冲区 —— void Draw(PrimitiveType primitiveType)
        /// <summary>
        /// 绘制顶点缓冲区
        /// </summary>
        /// <param name="primitiveType">图元类型</param>
        public void Draw(PrimitiveType primitiveType)
        {
            this.VertexArray.Bind();
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
            this.VertexArray.Unbind();
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

            //更新顶点数据
            GL.BufferData(BufferTarget.ArrayBuffer, this.MeshGeometry.Vertices.Length * sizeof(Vertex), this.MeshGeometry.Vertices, BufferUsageHint.DynamicDraw);

            //更新索引数据
            if (this.MeshGeometry.Indices.Length > 0)
            {
                GL.BufferData(BufferTarget.ElementArrayBuffer, this.MeshGeometry.Indices.Length * sizeof(uint), this.MeshGeometry.Indices, BufferUsageHint.DynamicDraw);
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

            this.VertexArray.Dispose();
            this._disposed = true;
        }
        #endregion


        //Private

        #region 分配内存 —— void AllocateMemory()
        /// <summary>
        /// 分配内存
        /// </summary>
        private unsafe void AllocateMemory()
        {
            this.Bind();

            Vertex[] vertices = this.MeshGeometry.Vertices;
            uint[] indices = this.MeshGeometry.Indices;

            //上传顶点VBO
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(Vertex), vertices, this.BufferUsage);

            //上传索引EBO
            if (indices.Length > 0)
            {
                GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, this.BufferUsage);
            }

            this.Unbind();
        }
        #endregion

        #endregion
    }
}
