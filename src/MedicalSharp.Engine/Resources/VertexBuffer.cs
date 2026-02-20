using MedicalSharp.Engine.ValueTypes;
using OpenTK.Graphics.OpenGL4;
using System;
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
        private VertexBuffer()
        {
            this._vao = GL.GenVertexArray();
            this._vbo = GL.GenBuffer();
            this._ebo = GL.GenBuffer();
        }

        /// <summary>
        /// 创建顶点缓冲区构造器
        /// </summary>
        /// <param name="primitiveType">图元类型</param>
        /// <param name="meshGeometry">网格几何</param>
        public VertexBuffer(PrimitiveType primitiveType, MeshGeometry meshGeometry)
            : this()
        {
            #region # 验证

            if (meshGeometry == null)
            {
                throw new ArgumentNullException(nameof(meshGeometry), "网格几何不可为空！");
            }

            #endregion

            this.PrimitiveType = primitiveType;
            this.MeshGeometry = meshGeometry;
        }

        /// <summary>
        /// 析构器
        /// </summary>
        ~VertexBuffer()
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
            this.Bind();

            //上传顶点到VBO
            GL.BufferData(BufferTarget.ArrayBuffer, this.MeshGeometry.Vertices.Length * sizeof(Vertex), this.MeshGeometry.Vertices, BufferUsageHint.DynamicDraw);

            //如果使用索引，上传索引到EBO
            if (this.MeshGeometry.Indices.Length > 0)
            {
                GL.BufferData(BufferTarget.ElementArrayBuffer, this.MeshGeometry.Indices.Length * sizeof(uint), this.MeshGeometry.Indices, BufferUsageHint.DynamicDraw);
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

        #region 更新缓冲区 —— unsafe void Update(MeshGeometry meshGeometry)
        /// <summary>
        /// 更新缓冲区
        /// </summary>
        public unsafe void Update(MeshGeometry meshGeometry)
        {
            #region # 验证

            if (meshGeometry == null)
            {
                throw new ArgumentNullException(nameof(meshGeometry), "网格几何不可为空！");
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

        #region 绘制 —— void Draw()
        /// <summary>
        /// 绘制
        /// </summary>
        public void Draw()
        {
            this.Bind();

            if (this.MeshGeometry.Indices.Length > 0)
            {
                GL.DrawElements(this.PrimitiveType, this.MeshGeometry.Indices.Length, DrawElementsType.UnsignedInt, 0);
            }
            else
            {
                GL.DrawArrays(this.PrimitiveType, 0, this.MeshGeometry.Vertices.Length);
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
