using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalSharp.Engine.Renderables
{
    /// <summary>
    /// 固体渲染对象
    /// </summary>
    public class SolidRenderable : ShapeRenderable, IHasTriangles
    {
        #region # 字段及构造器

        /// <summary>
        /// 填充顶点缓冲区
        /// </summary>
        private VertexBuffer _fillBuffer;

        /// <summary>
        /// 三角形列表
        /// </summary>
        private IList<Triangle> _triangles;

        /// <summary>
        /// 默认构造器
        /// </summary>
        private SolidRenderable()
        {
            //默认值
            this.Fill = new Vector4(1.0f, 0.0f, 0.0f, 0.1f);
        }

        /// <summary>
        /// 创建固体渲染对象构造器
        /// </summary>
        /// <param name="fillMesh">填充网格</param>
        public SolidRenderable(MeshGeometry fillMesh)
            : this()
        {
            #region # 验证

            if (fillMesh == null)
            {
                throw new ArgumentNullException(nameof(fillMesh), "填充网格不可为空！");
            }

            #endregion

            this._fillBuffer = new VertexBuffer(fillMesh);
            this._fillBuffer.Setup();

            //提取三角形面
            this._triangles = this.FillBuffer.MeshGeometry.ExtractTriangles();
        }

        #endregion

        #region # 属性

        #region 填充颜色 —— Vector4 Fill
        /// <summary>
        /// 填充颜色
        /// </summary>
        public Vector4 Fill { get; private set; }
        #endregion

        #region 只读属性 - 三角形列表 —— IList<Triangle> Triangles
        /// <summary>
        /// 只读属性 - 三角形列表
        /// </summary>
        public IList<Triangle> Triangles
        {
            get => this._triangles;
        }
        #endregion

        #region 只读属性 - 填充顶点缓冲区 —— VertexBuffer FillBuffer
        /// <summary>
        /// 只读属性 - 填充顶点缓冲区
        /// </summary>
        internal VertexBuffer FillBuffer
        {
            get => this._fillBuffer;
        }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 更新固体渲染对象 —— void Update(MeshGeometry fillMesh)
        /// <summary>
        /// 更新固体渲染对象
        /// </summary>
        /// <param name="fillMesh">填充网格</param>
        public void Update(MeshGeometry fillMesh)
        {
            #region # 验证

            if (fillMesh == null)
            {
                throw new ArgumentNullException(nameof(fillMesh), "填充网格不可为空！");
            }

            #endregion

            //先释放旧的
            this._fillBuffer.Dispose();

            this._fillBuffer = new VertexBuffer(fillMesh);
            this._fillBuffer.Setup();

            //提取三角形面
            this._triangles = this.FillBuffer.MeshGeometry.ExtractTriangles();

            //标记包围盒/包围球为脏
            base.InvalidateBoundings();
        }
        #endregion

        #region 设置填充 —— void SetFill(Vector4 fill)
        /// <summary>
        /// 设置填充
        /// </summary>
        /// <param name="fill">填充颜色</param>
        public void SetFill(Vector4 fill)
        {
            this.Fill = fill;
        }
        #endregion

        #region 渲染 —— override void Render(ShaderProgram program)
        /// <summary>
        /// 渲染
        /// </summary>
        /// <param name="program">Shader程序</param>
        public override void Render(ShaderProgram program)
        {
            //绘制填充模型	
            program.SetUniformVector4("u_Color", this.Fill);
            this.FillBuffer.Draw(PrimitiveType.Triangles);
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

            this._triangles.Clear();
            this._fillBuffer.Dispose();

            this._disposed = true;
        }
        #endregion


        //Protected

        #region 计算包围盒 —— override BoundingBox CalculateBoundingBox()
        /// <summary>
        /// 计算包围盒
        /// </summary>
        protected override BoundingBox CalculateBoundingBox()
        {
            IEnumerable<Vector3> positions = this.FillBuffer.MeshGeometry.Vertices.Select(vertex => vertex.Position);
            BoundingBox boundingBox = BoundingBox.FromPoints([.. positions]);

            return boundingBox;
        }
        #endregion

        #endregion
    }
}
