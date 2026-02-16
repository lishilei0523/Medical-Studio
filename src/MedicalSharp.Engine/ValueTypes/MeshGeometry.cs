using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MedicalSharp.Engine.ValueTypes
{
    /// <summary>
    /// 网格几何
    /// </summary>
    public class MeshGeometry : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// VAO
        /// </summary>
        private readonly int _vao;

        /// <summary>
        /// VBO
        /// </summary>
        private readonly int _vbo;

        /// <summary>
        /// EBO
        /// </summary>
        private readonly int _ebo;

        /// <summary>
        /// 无参构造器
        /// </summary>
        private MeshGeometry()
        {
            this._vao = 0;
            this._vbo = 0;
            this._ebo = 0;
            this.Vertices = [];
            this.Indices = [];
        }

        /// <summary>
        /// 创建网格几何构造器
        /// </summary>
        /// <param name="primitiveType">图元类型</param>
        /// <param name="vertices">顶点列表</param>
        public MeshGeometry(PrimitiveType primitiveType, ICollection<Vertex> vertices)
            : this()
        {
            if (vertices == null || !vertices.Any())
            {
                throw new ArgumentNullException(nameof(vertices), "顶点列表不可为空！");
            }

            this.PrimitiveType = primitiveType;
            this.Vertices = [.. vertices];
        }

        /// <summary>
        /// 创建网格几何构造器
        /// </summary>
        /// <param name="primitiveType">图元类型</param>
        /// <param name="vertices">顶点列表</param>
        /// <param name="indices">顶点索引列表</param>
        public MeshGeometry(PrimitiveType primitiveType, ICollection<Vertex> vertices, ICollection<uint> indices)
            : this()
        {
            if (vertices == null || !vertices.Any())
            {
                throw new ArgumentNullException(nameof(vertices), "顶点列表不可为空！");
            }
            if (indices == null || !indices.Any())
            {
                throw new ArgumentNullException(nameof(indices), "顶点索引列表不可为空！");
            }

            this.PrimitiveType = primitiveType;
            this.Vertices = [.. vertices];
            this.Indices = [.. indices];

            this._vao = GL.GenVertexArray();
            this._vbo = GL.GenBuffer();
            this._ebo = GL.GenBuffer();
        }

        /// <summary>
        /// 析构器
        /// </summary>
        ~MeshGeometry()
        {
            this.Dispose();
        }

        #endregion

        #region # 属性

        #region 图元类型 —— PrimitiveType PrimitiveType
        /// <summary>
        /// 图元类型
        /// </summary>
        public PrimitiveType PrimitiveType { get; private set; }
        #endregion

        #region 顶点列表 —— Vertex[] Vertices
        /// <summary>
        /// 顶点列表
        /// </summary>
        public Vertex[] Vertices { get; private set; }
        #endregion

        #region 顶点索引列表 —— uint[] Indices
        /// <summary>
        /// 顶点索引列表
        /// </summary>
        public uint[] Indices { get; private set; }
        #endregion

        #endregion

        #region # 方法

        #region 设置顶点列表 —— void SetVertices(ICollection<Vertex> vertices)
        /// <summary>
        /// 设置顶点列表
        /// </summary>
        /// <param name="vertices">顶点列表</param>
        public void SetVertices(ICollection<Vertex> vertices)
        {
            if (vertices == null || !vertices.Any())
            {
                throw new ArgumentNullException(nameof(vertices), "顶点列表不可为空！");
            }

            this.Vertices = [.. vertices];
        }
        #endregion

        #region 设置顶点索引列表 —— void SetIndices(ICollection<uint> indices)
        /// <summary>
        /// 设置顶点索引列表
        /// </summary>
        /// <param name="indices">顶点索引列表</param>
        public void SetIndices(ICollection<uint> indices)
        {
            if (indices == null || !indices.Any())
            {
                throw new ArgumentNullException(nameof(indices), "顶点索引列表不可为空！");
            }

            this.Indices = [.. indices];
        }
        #endregion

        #region 初始化缓冲区 —— unsafe void SetupBuffers()
        /// <summary>
        /// 初始化缓冲区
        /// </summary>
        public unsafe void SetupBuffers()
        {
            this.Bind();

            //上传顶点到VBO
            GL.BufferData(BufferTarget.ArrayBuffer, this.Vertices.Length * sizeof(Vertex), this.Vertices, BufferUsageHint.DynamicDraw);

            //如果使用索引，上传索引到EBO
            if (this.Indices.Length > 0)
            {
                GL.BufferData(BufferTarget.ElementArrayBuffer, this.Indices.Length * sizeof(uint), this.Indices, BufferUsageHint.DynamicDraw);
            }

            //位置属性 (location = 0)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(Vertex), Marshal.OffsetOf<Vertex>(nameof(Vertex.Position)).ToInt32());
            GL.EnableVertexAttribArray(0);

            //颜色属性 (location = 1)
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(Vertex), Marshal.OffsetOf<Vertex>(nameof(Vertex.Color)).ToInt32());
            GL.EnableVertexAttribArray(1);

            //纹理坐标属性 (location = 2)
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, sizeof(Vertex), Marshal.OffsetOf<Vertex>(nameof(Vertex.TextureCoord)).ToInt32());
            GL.EnableVertexAttribArray(2);

            //法向量属性
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, sizeof(Vertex), Marshal.OffsetOf<Vertex>(nameof(Vertex.Normal)).ToInt32());
            GL.EnableVertexAttribArray(3);

            //解绑
            this.Unbind();
        }
        #endregion

        #region 更新缓冲区 —— unsafe void UpdateBuffers()
        /// <summary>
        /// 更新缓冲区
        /// </summary>
        public unsafe void UpdateBuffers()
        {
            this.Bind();

            //更新顶点数据
            GL.BufferData(BufferTarget.ArrayBuffer, this.Vertices.Length * sizeof(Vertex), this.Vertices, BufferUsageHint.DynamicDraw);

            //更新索引数据
            if (this.Indices.Length > 0)
            {
                GL.BufferData(BufferTarget.ElementArrayBuffer, this.Indices.Length * sizeof(uint), this.Indices, BufferUsageHint.DynamicDraw);
            }

            //解绑
            this.Unbind();
        }
        #endregion

        #region 绘制 —— void Draw()
        /// <summary>
        /// 绘制
        /// </summary>
        public void Draw()
        {
            this.Bind();

            if (this.Indices.Length > 0)
            {
                GL.DrawElements(this.PrimitiveType, this.Indices.Length, DrawElementsType.UnsignedInt, 0);
            }
            else
            {
                GL.DrawArrays(this.PrimitiveType, 0, this.Vertices.Length);
            }

            this.Unbind();
        }
        #endregion

        #region 绑定 —— void Bind()
        /// <summary>
        /// 绑定
        /// </summary>
        public void Bind()
        {
            if (this._vao != 0)
            {
                GL.BindVertexArray(this._vao);
            }
            if (this._vbo != 0)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, this._vbo);
            }
            if (this._ebo != 0)
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, this._ebo);
            }
        }
        #endregion

        #region 解绑 —— void Unbind()
        /// <summary>
        /// 解绑
        /// </summary>
        public void Unbind()
        {
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }
        #endregion

        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (this._vao != 0)
            {
                GL.DeleteVertexArray(this._vao);
            }
            if (this._vbo != 0)
            {
                GL.DeleteBuffer(this._vbo);
            }
            if (this._ebo != 0)
            {
                GL.DeleteBuffer(this._ebo);
            }
        }
        #endregion 

        #endregion
    }
}
