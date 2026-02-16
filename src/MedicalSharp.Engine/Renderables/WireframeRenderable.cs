//using MedicalSharp.Engine.ValueTypes;
//using OpenTK.Graphics.OpenGL4;
//using OpenTK.Mathematics;
//using System;
//using System.Runtime.InteropServices;

//namespace MedicalSharp.Engine.Renderables
//{
//    /// <summary>
//    /// 线框渲染对象
//    /// </summary>
//    public class WireframeRenderable : Renderable, IDisposable
//    {
//        #region # 字段及构造器

//        /// <summary>
//        /// VAO
//        /// </summary>
//        private int _vao;

//        /// <summary>
//        /// VBO
//        /// </summary>
//        private int _vbo;

//        /// <summary>
//        /// EBO
//        /// </summary>
//        private int _ebo;

//        /// <summary>
//        /// 创建线框渲染对象构造器
//        /// </summary>
//        public WireframeRenderable()
//        {
//            this._vao = 0;
//            this._vbo = 0;
//            this._ebo = 0;
//            this.Stroke = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
//            this.StrokeThickness = 1.0f;
//            this.Fill = new Vector4(1.0f, 0.0f, 0.0f, 0.1f);
//        }

//        /// <summary>
//        /// 创建线框渲染器构造器
//        /// </summary>
//        /// <param name="strokeGeometry">线框模型</param>
//        /// <param name="fillGeometry">填充模型</param>
//        public WireframeRenderable(MeshGeometry strokeGeometry, MeshGeometry fillGeometry)
//            : this()
//        {
//            this.StrokeGeometry = strokeGeometry;
//            this.FillGeometry = fillGeometry;
//        }

//        #endregion

//        #region # 属性

//        #region 图元类型 —— PrimitiveType PrimitiveType
//        /// <summary>
//        /// 图元类型
//        /// </summary>
//        public PrimitiveType PrimitiveType { get; private set; }
//        #endregion

//        #region 线框模型 —— MeshGeometry StrokeGeometry
//        /// <summary>
//        /// 线框模型
//        /// </summary>
//        public MeshGeometry StrokeGeometry { get; private set; }
//        #endregion

//        #region 填充模型 —— MeshGeometry FillGeometry
//        /// <summary>
//        /// 填充模型
//        /// </summary>
//        public MeshGeometry FillGeometry { get; private set; }
//        #endregion

//        #region 线框颜色 —— Vector4 Stroke
//        /// <summary>
//        /// 线框颜色
//        /// </summary>
//        public Vector4 Stroke { get; private set; }
//        #endregion

//        #region 线框粗细 —— float StrokeThickness
//        /// <summary>
//        /// 线框粗细
//        /// </summary>
//        public float StrokeThickness { get; private set; }
//        #endregion

//        #region 填充颜色 —— Vector4 Fill
//        /// <summary>
//        /// 填充颜色
//        /// </summary>
//        public Vector4 Fill { get; private set; }
//        #endregion

//        #endregion

//        #region # 方法

//        //Public

//        #region 释放资源 —— void Dispose()
//        /// <summary>
//        /// 释放资源
//        /// </summary>
//        public void Dispose()
//        {
//            if (this._vao != 0)
//            {
//                GL.DeleteVertexArray(this._vao);
//            }
//            if (this._vbo != 0)
//            {
//                GL.DeleteBuffer(this._vbo);
//            }
//            if (this._ebo != 0)
//            {
//                GL.DeleteBuffer(this._ebo);
//            }
//        }
//        #endregion 

//        #region 初始化缓冲区 —— unsafe void SetupBuffers()
//        /// <summary>
//        /// 初始化缓冲区
//        /// </summary>
//        public unsafe void SetupBuffers()
//        {
//            this._vao = GL.GenVertexArray();
//            this._vbo = GL.GenBuffer();
//            this._ebo = GL.GenBuffer();

//            this.Bind();

//            //上传顶点到VBO
//            GL.BufferData(BufferTarget.ArrayBuffer, this.Vertices.Length * sizeof(Vertex), this.Vertices, BufferUsageHint.DynamicDraw);

//            //如果使用索引，上传索引到EBO
//            if (this.Indices.Length > 0)
//            {
//                GL.BufferData(BufferTarget.ElementArrayBuffer, this.Indices.Length * sizeof(uint), this.Indices, BufferUsageHint.DynamicDraw);
//            }

//            //位置属性 (location = 0)
//            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(Vertex), Marshal.OffsetOf<Vertex>(nameof(Vertex.Position)).ToInt32());
//            GL.EnableVertexAttribArray(0);

//            //颜色属性 (location = 1)
//            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(Vertex), Marshal.OffsetOf<Vertex>(nameof(Vertex.Color)).ToInt32());
//            GL.EnableVertexAttribArray(1);

//            //纹理坐标属性 (location = 2)
//            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, sizeof(Vertex), Marshal.OffsetOf<Vertex>(nameof(Vertex.TextureCoord)).ToInt32());
//            GL.EnableVertexAttribArray(2);

//            //法向量属性
//            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, sizeof(Vertex), Marshal.OffsetOf<Vertex>(nameof(Vertex.Normal)).ToInt32());
//            GL.EnableVertexAttribArray(3);

//            //解绑
//            this.Unbind();
//        }
//        #endregion

//        #region 更新缓冲区 —— unsafe void UpdateBuffers()
//        /// <summary>
//        /// 更新缓冲区
//        /// </summary>
//        public unsafe void UpdateBuffers()
//        {
//            this.Bind();

//            //更新顶点数据
//            GL.BufferData(BufferTarget.ArrayBuffer, this.Vertices.Length * sizeof(Vertex), this.Vertices, BufferUsageHint.DynamicDraw);

//            //更新索引数据
//            if (this.Indices.Length > 0)
//            {
//                GL.BufferData(BufferTarget.ElementArrayBuffer, this.Indices.Length * sizeof(uint), this.Indices, BufferUsageHint.DynamicDraw);
//            }

//            //解绑
//            this.Unbind();
//        }
//        #endregion

//        #region 绘制 —— void Draw()
//        /// <summary>
//        /// 绘制
//        /// </summary>
//        public void Draw()
//        {
//            this.Bind();

//            if (this.Indices.Length > 0)
//            {
//                GL.DrawElements(this.PrimitiveType, this.Indices.Length, DrawElementsType.UnsignedInt, 0);
//            }
//            else
//            {
//                GL.DrawArrays(this.PrimitiveType, 0, this.Vertices.Length);
//            }

//            this.Unbind();
//        }
//        #endregion

//        #region 绑定 —— void Bind()
//        /// <summary>
//        /// 绑定
//        /// </summary>
//        public void Bind()
//        {
//            if (this._vao != 0)
//            {
//                GL.BindVertexArray(this._vao);
//            }
//            if (this._vbo != 0)
//            {
//                GL.BindBuffer(BufferTarget.ArrayBuffer, this._vbo);
//            }
//            if (this._ebo != 0)
//            {
//                GL.BindBuffer(BufferTarget.ElementArrayBuffer, this._ebo);
//            }
//        }
//        #endregion

//        #region 解绑 —— void Unbind()
//        /// <summary>
//        /// 解绑
//        /// </summary>
//        public void Unbind()
//        {
//            GL.BindVertexArray(0);
//            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
//            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
//        }
//        #endregion

//        #endregion
//    }
//}
