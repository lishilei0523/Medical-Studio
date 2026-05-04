using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Builders;
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
    /// 折线渲染对象
    /// </summary>
    public class PolylineRenderable : ShapeRenderable
    {
        #region # 字段及构造器

        /// <summary>
        /// 顶点缓冲区
        /// </summary>
        private VertexBuffer _vertexBuffer;

        /// <summary>
        /// 默认构造器
        /// </summary>
        private PolylineRenderable()
        {
            //默认值
            this.Stroke = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            this.StrokeThickness = 1.0f;
        }

        /// <summary>
        /// 创建折线渲染对象构造器
        /// </summary>
        /// <param name="positions">位置列表</param>
        /// <param name="closed">是否闭合</param>
        /// <param name="drawVertex">是否绘制顶点</param>
        public PolylineRenderable(IReadOnlyList<Vector3> positions, bool closed = false, bool drawVertex = true)
            : this()
        {
            this.Positions = positions;
            this.Closed = closed;
            this.DrawVertex = drawVertex;

            //初始化缓冲区
            MeshGeometry polylineGeometry = MeshFactory.CreatePolyline(positions, this.Closed);
            this._vertexBuffer = new VertexBuffer(polylineGeometry);
            this._vertexBuffer.Setup();
        }

        #endregion

        #region # 属性

        #region 位置列表 —— IReadOnlyList<Vector3> Positions
        /// <summary>
        /// 位置列表
        /// </summary>
        public IReadOnlyList<Vector3> Positions { get; private set; }
        #endregion

        #region 是否闭合 —— bool Closed
        /// <summary>
        /// 是否闭合
        /// </summary>
        public bool Closed { get; private set; }
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

        #region 是否绘制顶点 —— bool DrawVertex
        /// <summary>
        /// 是否绘制顶点
        /// </summary>
        public bool DrawVertex { get; private set; }
        #endregion

        #region 只读属性 - 顶点缓冲区 —— VertexBuffer VertexBuffer
        /// <summary>
        /// 只读属性 - 顶点缓冲区
        /// </summary>
        internal VertexBuffer VertexBuffer
        {
            get => this._vertexBuffer;
        }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 更新折线渲染对象 —— void Update(IReadOnlyList<Vector3> positions)
        /// <summary>
        /// 更新折线渲染对象
        /// </summary>
        /// <param name="positions">位置列表</param>
        public void Update(IReadOnlyList<Vector3> positions)
        {
            #region # 验证

            if (positions == null || !positions.Any())
            {
                throw new ArgumentNullException(nameof(positions), "位置列表不可为空！");
            }
            if (ReferenceEquals(positions, this.Positions))
            {
                return;
            }
            if (this.Positions.SequenceEqual(positions))
            {
                return;
            }

            #endregion

            this.Positions = positions;

            //先释放旧的
            this._vertexBuffer.Dispose();

            MeshGeometry polylineGeometry = MeshFactory.CreatePolyline(positions, this.Closed);
            this._vertexBuffer = new VertexBuffer(polylineGeometry);
            this._vertexBuffer.Setup();

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
            this._vertexBuffer.Draw(PrimitiveType.Lines);

            if (this.DrawVertex)
            {
                //点尺寸
                float pointSize = Math.Clamp(this.StrokeThickness * 3.0f, 5f, 20f);
                GL.PointSize(pointSize);

                //点颜色
                Vector4 invertedStroke = this.Stroke.Invert();
                float contrast = Math.Abs(invertedStroke.X - this.Stroke.X) +
                                 Math.Abs(invertedStroke.Y - this.Stroke.Y) +
                                 Math.Abs(invertedStroke.Z - this.Stroke.Z);
                if (contrast < 0.5f)
                {
                    invertedStroke = ColorFactory.Yellow(); //固定用亮黄色
                }

                //绘制控制点
                program.SetUniformVector4("u_Color", invertedStroke);
                this._vertexBuffer.Draw(PrimitiveType.Points);
            }
        }
        #endregion

        #region 检测射线相交 —— override bool IntersectsRay(Ray ray, out float distance...
        /// <summary>
        /// 检测射线相交
        /// </summary>
        /// <param name="ray">射线（世界空间）</param>
        /// <param name="distance">相交距离</param>
        /// <param name="hitPoint">命中点坐标</param>
        /// <param name="hitNormal">命中点法向量</param>
        /// <param name="hitTriangleIndex">命中三角形索引</param>
        /// <returns>是否相交</returns>
        public override bool IntersectsRay(Ray ray, out float distance, out Vector3 hitPoint, out Vector3 hitNormal, out int hitTriangleIndex)
        {
            distance = float.MaxValue;
            hitPoint = Vector3.Zero;
            hitNormal = Vector3.Zero;
            hitTriangleIndex = -1;

            //将射线变换到局部空间
            Matrix4 worldToLocal = Matrix4.Invert(this.ModelMatrix);
            Ray localRay = ray.Transform(worldToLocal);

            //快速剔除：先检测包围盒
            if (this.BoundingBox.Intersects(localRay, out distance))
            {
                hitPoint = ray.GetPoint(distance);

                return true;
            }

            return false;
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

            this._vertexBuffer.Dispose();

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
            BoundingBox boundingBox = BoundingBox.FromPoints(this.Positions);

            return boundingBox;
        }
        #endregion

        #endregion
    }
}
