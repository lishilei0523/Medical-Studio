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
    /// 线条渲染对象
    /// </summary>
    public class LineStripRenderable : ShapeRenderable, IHasTriangles
    {
        #region # 字段及构造器

        /// <summary>
        /// 线框顶点缓冲区
        /// </summary>
        private VertexBuffer _strokeBuffer;

        /// <summary>
        /// 三角形列表
        /// </summary>
        private IList<Triangle> _triangles;

        /// <summary>
        /// 默认构造器
        /// </summary>
        private LineStripRenderable()
        {
            //默认值
            this.Stroke = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            this.StrokeThickness = 1.0f;
        }

        /// <summary>
        /// 创建线条渲染对象构造器
        /// </summary>
        /// <param name="strokeMesh">线框网格</param>
        public LineStripRenderable(MeshGeometry strokeMesh)
            : this()
        {
            #region # 验证

            if (strokeMesh == null)
            {
                throw new ArgumentNullException(nameof(strokeMesh), "线框网格不可为空！");
            }
            if (strokeMesh.Vertices.Length <= 2)
            {
                throw new ArgumentOutOfRangeException(nameof(strokeMesh), "线条顶点数必须大于2！");
            }

            #endregion

            this._strokeBuffer = new VertexBuffer(strokeMesh);
            this._strokeBuffer.Setup();

            //提取三角形面
            this._triangles = this.StrokeBuffer.MeshGeometry.ExtractTriangles();
        }

        #endregion

        #region # 属性

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

        #region 只读属性 - 三角形列表 —— IList<Triangle> Triangles
        /// <summary>
        /// 只读属性 - 三角形列表
        /// </summary>
        public IList<Triangle> Triangles
        {
            get => this._triangles;
        }
        #endregion

        #region 只读属性 - 线框顶点缓冲区 —— VertexBuffer StrokeBuffer
        /// <summary>
        /// 只读属性 - 线框顶点缓冲区
        /// </summary>
        internal VertexBuffer StrokeBuffer
        {
            get => this._strokeBuffer;
        }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 更新线条渲染对象 —— void Update(MeshGeometry strokeMesh)
        /// <summary>
        /// 更新线条渲染对象
        /// </summary>
        /// <param name="strokeMesh">线框网格</param>
        public void Update(MeshGeometry strokeMesh)
        {
            #region # 验证

            if (strokeMesh == null)
            {
                throw new ArgumentNullException(nameof(strokeMesh), "线框网格不可为空！");
            }

            #endregion

            //先释放旧的
            this._strokeBuffer.Dispose();

            this._strokeBuffer = new VertexBuffer(strokeMesh);
            this._strokeBuffer.Setup();

            //提取三角形面
            this._triangles = this.StrokeBuffer.MeshGeometry.ExtractTriangles();

            //标记包围盒/包围球为脏
            base.InvalidateBoundings();
        }
        #endregion

        #region 设置线框 —— void SetStroke(Vector4 stroke, float strokeThickness)
        /// <summary>
        /// 设置线框
        /// </summary>
        /// <param name="stroke">线框颜色</param>
        /// <param name="strokeThickness">线框粗细</param>
        public void SetStroke(Vector4 stroke, float strokeThickness)
        {
            this.Stroke = stroke;
            this.StrokeThickness = strokeThickness;
        }
        #endregion

        #region 渲染 —— override void Render(ShaderProgram program)
        /// <summary>
        /// 渲染
        /// </summary>
        /// <param name="program">Shader程序</param>
        public override void Render(ShaderProgram program)
        {
            //绘制线框模型
            GL.LineWidth(this.StrokeThickness);
            program.SetUniformVector4("u_Color", this.Stroke);
            this.StrokeBuffer.Draw(PrimitiveType.LineStrip);
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
            this._strokeBuffer.Dispose();

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
            IEnumerable<Vector3> positions = this.StrokeBuffer.MeshGeometry.Vertices.Select(vertex => vertex.Position);
            BoundingBox boundingBox = BoundingBox.FromPoints([.. positions]);

            return boundingBox;
        }
        #endregion

        #endregion
    }
}
