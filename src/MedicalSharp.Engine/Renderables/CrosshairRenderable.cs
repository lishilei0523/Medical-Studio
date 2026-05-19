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
    /// 十字线渲染对象
    /// </summary>
    public class CrosshairRenderable : ShapeRenderable
    {
        #region # 字段及构造器

        /// <summary>
        /// 水平缓冲区
        /// </summary>
        private readonly VertexBuffer _horizontalBuffer;

        /// <summary>
        /// 垂直缓冲区
        /// </summary>
        private readonly VertexBuffer _verticalBuffer;

        /// <summary>
        /// 默认构造器
        /// </summary>
        private CrosshairRenderable()
        {
            //默认值
            this.HorizontalStroke = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            this.VerticalStroke = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
            this.StrokeThickness = 1.0f;
        }

        /// <summary>
        /// 创建十字线渲染对象构造器
        /// </summary>
        /// <param name="center">中心位置</param>
        /// <param name="uAxis">U轴</param>
        /// <param name="vAxis">V轴</param>
        /// <param name="horizontalLength">水平长度</param>
        /// <param name="verticalLength">垂直长度</param>
        public CrosshairRenderable(Vector3 center, Vector3 uAxis, Vector3 vAxis, float horizontalLength, float verticalLength)
            : this()
        {
            #region # 验证

            if (horizontalLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(horizontalLength), "水平长度必须大于0！");
            }
            if (verticalLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(verticalLength), "垂直长度必须大于0！");
            }

            #endregion

            this.Center = center;
            this.UAxis = uAxis;
            this.VAxis = vAxis;
            this.HorizontalLength = horizontalLength;
            this.VerticalLength = verticalLength;

            //初始化缓冲区
            float halfHorizontalLength = horizontalLength / 2.0f;
            float halfVerticalLength = verticalLength / 2.0f;
            MeshGeometry horizontalLine = MeshFactory.CreateLineSegment(
                center - uAxis * halfHorizontalLength,
                center + uAxis * halfHorizontalLength);
            MeshGeometry verticalLine = MeshFactory.CreateLineSegment(
                center - vAxis * halfVerticalLength,
                center + vAxis * halfVerticalLength);
            this._horizontalBuffer = new VertexBuffer(horizontalLine);
            this._verticalBuffer = new VertexBuffer(verticalLine);
        }

        #endregion

        #region # 属性

        #region 中心位置 —— Vector3 Center
        /// <summary>
        /// 中心位置
        /// </summary>
        public Vector3 Center { get; private set; }
        #endregion

        #region U轴 —— Vector3 UAxis
        /// <summary>
        /// U轴
        /// </summary>
        public Vector3 UAxis { get; private set; }
        #endregion

        #region V轴 —— Vector3 VAxis
        /// <summary>
        /// V轴
        /// </summary>
        public Vector3 VAxis { get; private set; }
        #endregion

        #region 水平长度 —— float HorizontalLength
        /// <summary>
        /// 水平长度
        /// </summary>
        public float HorizontalLength { get; private set; }
        #endregion

        #region 垂直长度 —— float VerticalLength
        /// <summary>
        /// 垂直长度
        /// </summary>
        public float VerticalLength { get; private set; }
        #endregion

        #region 水平颜色 —— Vector4 HorizontalStroke
        /// <summary>
        /// 水平颜色
        /// </summary>
        public Vector4 HorizontalStroke { get; private set; }
        #endregion

        #region 垂直颜色 —— Vector4 VerticalStroke
        /// <summary>
        /// 垂直颜色
        /// </summary>
        public Vector4 VerticalStroke { get; private set; }
        #endregion

        #region 线框粗细 —— float StrokeThickness
        /// <summary>
        /// 线框粗细
        /// </summary>
        public float StrokeThickness { get; private set; }
        #endregion

        #region 只读属性 - 水平缓冲区 —— VertexBuffer HorizontalBuffer
        /// <summary>
        /// 只读属性 - 水平缓冲区
        /// </summary>
        internal VertexBuffer HorizontalBuffer
        {
            get => this._horizontalBuffer;
        }
        #endregion

        #region 只读属性 - 垂直缓冲区 —— VertexBuffer VerticalBuffer
        /// <summary>
        /// 只读属性 - 垂直缓冲区
        /// </summary>
        internal VertexBuffer VerticalBuffer
        {
            get => this._verticalBuffer;
        }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 更新十字线渲染对象 —— void Update(Vector3 center, Vector3 uAxis...
        /// <summary>
        /// 更新十字线渲染对象
        /// </summary>
        /// <param name="center">中心位置</param>
        /// <param name="uAxis">U轴</param>
        /// <param name="vAxis">V轴</param>
        /// <param name="horizontalLength">水平长度</param>
        /// <param name="verticalLength">垂直长度</param>
        public void Update(Vector3 center, Vector3 uAxis, Vector3 vAxis, float horizontalLength, float verticalLength)
        {
            #region # 验证

            if (horizontalLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(horizontalLength), "水平长度必须大于0！");
            }
            if (verticalLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(verticalLength), "垂直长度必须大于0！");
            }
            if (this.Center == center && this.UAxis == uAxis && this.VAxis == vAxis &&
                this.HorizontalLength.Equals(horizontalLength) && this.VerticalLength.Equals(verticalLength))
            {
                return;
            }

            #endregion

            this.Center = center;
            this.UAxis = uAxis;
            this.VAxis = vAxis;
            this.HorizontalLength = horizontalLength;
            this.VerticalLength = verticalLength;

            //更新VBO
            float halfHorizontalLength = horizontalLength / 2.0f;
            float halfVerticalLength = verticalLength / 2.0f;
            MeshGeometry horizontalLine = MeshFactory.CreateLineSegment(
                center - uAxis * halfHorizontalLength,
                center + uAxis * halfHorizontalLength);
            MeshGeometry verticalLine = MeshFactory.CreateLineSegment(
                center - vAxis * halfVerticalLength,
                center + vAxis * halfVerticalLength);
            this._horizontalBuffer.Update(horizontalLine);
            this._verticalBuffer.Update(verticalLine);

            //标记包围盒/包围球为脏
            base.InvalidateBoundings();
        }
        #endregion

        #region 设置线框 —— void SetStroke(Vector4 horizontalStroke, Vector4 verticalStroke...
        /// <summary>
        /// 设置线框
        /// </summary>
        /// <param name="horizontalStroke">水平颜色</param>
        /// <param name="verticalStroke">垂直颜色</param>
        /// <param name="strokeThickness">线框粗细</param>
        public void SetStroke(Vector4 horizontalStroke, Vector4 verticalStroke, float strokeThickness)
        {
            this.HorizontalStroke = horizontalStroke;
            this.VerticalStroke = verticalStroke;
            this.StrokeThickness = strokeThickness;
        }
        #endregion

        #region 渲染 —— override void Render(ShaderProgram program, RenderContext context)
        /// <summary>
        /// 渲染
        /// </summary>
        /// <param name="program">Shader程序</param>
        /// <param name="context">渲染上下文</param>
        public override void Render(ShaderProgram program, RenderContext context)
        {
            //设置线宽
            GL.LineWidth(this.StrokeThickness);

            //绘制水平线
            program.SetUniformInt("u_HasTexture", 0);
            program.SetUniformVector4("u_Color", this.HorizontalStroke);
            this._horizontalBuffer.Draw(context.GlContext, PrimitiveType.Lines);

            //绘制垂直线
            program.SetUniformInt("u_HasTexture", 0);
            program.SetUniformVector4("u_Color", this.VerticalStroke);
            this._verticalBuffer.Draw(context.GlContext, PrimitiveType.Lines);
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

            this._horizontalBuffer.Dispose();
            this._verticalBuffer.Dispose();

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
            IEnumerable<Vector3> positionsH = this._horizontalBuffer.MeshGeometry.Vertices.Select(vertex => vertex.Position);
            IEnumerable<Vector3> positionsV = this._verticalBuffer.MeshGeometry.Vertices.Select(vertex => vertex.Position);
            BoundingBox boundingBox = BoundingBox.FromPoints([.. positionsH, .. positionsV]);

            return boundingBox;
        }
        #endregion

        #endregion
    }
}
