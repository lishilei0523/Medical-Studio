using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using Microsoft.CSharp.RuntimeBinder;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 顶点缓冲区
    /// </summary>
    public class VertexBuffer : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// VAO
        /// </summary>
        private int _vao;

        /// <summary>
        /// VBO
        /// </summary>
        private int _vbo;

        /// <summary>
        /// EBO
        /// </summary>
        private int _ebo;

        /// <summary>
        /// 初始化标识
        /// </summary>
        private bool _initialized;

        /// <summary>
        /// 释放标识
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// 默认构造器
        /// </summary>
        private VertexBuffer()
        {
            int vao = GL.GenVertexArray();
            int vbo = GL.GenBuffer();
            int ebo = GL.GenBuffer();

            #region # 验证

            if (vao == 0)
            {
                throw new RuntimeBinderException("创建VAO失败！");
            }
            if (vbo == 0)
            {
                throw new RuntimeBinderException("创建VBO失败！");
            }
            if (ebo == 0)
            {
                throw new RuntimeBinderException("创建EBO失败！");
            }

            #endregion

            this._vao = vao;
            this._vbo = vbo;
            this._ebo = ebo;
            this._initialized = false;
        }

        /// <summary>
        /// 创建顶点缓冲区构造器
        /// </summary>
        /// <param name="meshGeometry">网格几何</param>
        public VertexBuffer(MeshGeometry meshGeometry)
            : this()
        {
            #region # 验证

            if (meshGeometry == null)
            {
                throw new ArgumentNullException(nameof(meshGeometry), "网格几何不可为空！");
            }

            #endregion

            this.MeshGeometry = meshGeometry;
        }

        #endregion

        #region # 属性

        #region 网格几何 —— MeshGeometry MeshGeometry
        /// <summary>
        /// 网格几何
        /// </summary>
        public MeshGeometry MeshGeometry { get; private set; }
        #endregion

        #endregion

        #region # 方法

        #region 初始化缓冲区 —— unsafe void Setup()
        /// <summary>
        /// 初始化缓冲区
        /// </summary>
        public unsafe void Setup()
        {
            #region # 验证

            if (this._initialized)
            {
                throw new InvalidOperationException("已初始化，不可重复操作！");
            }

            #endregion

            this.Bind();

            //上传顶点到VBO
            GL.BufferData(BufferTarget.ArrayBuffer, this.MeshGeometry.Vertices.Length * sizeof(Vertex), this.MeshGeometry.Vertices, BufferUsageHint.DynamicDraw);

            //如果使用索引，上传索引到EBO
            if (this.MeshGeometry.Indices.Length > 0)
            {
                GL.BufferData(BufferTarget.ElementArrayBuffer, this.MeshGeometry.Indices.Length * sizeof(uint), this.MeshGeometry.Indices, BufferUsageHint.DynamicDraw);
            }

            //位置(location = 0)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(Vertex), Marshal.OffsetOf<Vertex>(nameof(Vertex.Position)).ToInt32());
            GL.EnableVertexAttribArray(0);

            //颜色(location = 1)
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(Vertex), Marshal.OffsetOf<Vertex>(nameof(Vertex.Color)).ToInt32());
            GL.EnableVertexAttribArray(1);

            //纹理坐标(location = 2)
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, sizeof(Vertex), Marshal.OffsetOf<Vertex>(nameof(Vertex.TextureCoord)).ToInt32());
            GL.EnableVertexAttribArray(2);

            //法向量(location = 3)
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, sizeof(Vertex), Marshal.OffsetOf<Vertex>(nameof(Vertex.Normal)).ToInt32());
            GL.EnableVertexAttribArray(3);

            //解绑
            this.Unbind();

            this._initialized = true;
        }
        #endregion

        #region 更新缓冲区 —— unsafe void Update(MeshGeometry meshGeometry)
        /// <summary>
        /// 更新缓冲区
        /// </summary>
        public unsafe void Update(MeshGeometry meshGeometry)
        {
            #region # 验证

            if (!this._initialized)
            {
                throw new InvalidOperationException("未初始化，不可更新！");
            }

            #endregion

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

        #region 绘制 —— void Draw(PrimitiveType primitiveType)
        /// <summary>
        /// 绘制
        /// </summary>
        /// <param name="primitiveType">图元类型</param>
        public void Draw(PrimitiveType primitiveType)
        {
            #region # 验证

            if (!this._initialized)
            {
                throw new InvalidOperationException("未初始化，不可绘制！");
            }

            #endregion

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
        }
        #endregion

        #region 绑定 —— void Bind()
        /// <summary>
        /// 绑定
        /// </summary>
        public void Bind()
        {
            GL.BindVertexArray(this._vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, this._vbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this._ebo);
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
            if (this._disposed)
            {
                return;
            }

            GL.DeleteVertexArray(this._vao);
            GL.DeleteBuffer(this._vbo);
            GL.DeleteBuffer(this._ebo);
            this._vao = 0;
            this._vbo = 0;
            this._ebo = 0;
            this._disposed = true;
        }
        #endregion  

        #endregion
    }
}
