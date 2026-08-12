using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Algorithms;
using MedicalSharp.Primitives.Builders;
using MedicalSharp.Primitives.Enums;
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
    /// 曲线引导线渲染对象
    /// </summary>
    public class CurveGuideRenderable : ShapeRenderable
    {
        #region # 字段及构造器

        /// <summary>
        /// 顶点缓冲区
        /// </summary>
        private readonly VertexBuffer _vertexBuffer;

        /// <summary>
        /// 默认构造器
        /// </summary>
        private CurveGuideRenderable()
        {
            //默认值
            this.Stroke = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            this.StrokeThickness = 1.0f;
        }

        /// <summary>
        /// 创建曲线引导线渲染对象构造器
        /// </summary>
        /// <param name="curve">曲线</param>
        /// <param name="arcPosition">弧长位置（归一化0~1）</param>
        /// <param name="rotationAngle">旋转角度</param>
        /// <param name="lineLength">线段长度（世界空间）</param>
        public CurveGuideRenderable(Curve curve, float arcPosition, float rotationAngle, float lineLength)
            : this()
        {
            #region # 验证

            if (curve == null || !curve.FrenetFrames.Any())
            {
                throw new ArgumentNullException(nameof(curve), "曲线不可为空且必须包含Frenet框架！");
            }
            if (lineLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lineLength), "线段长度必须大于0！");
            }

            #endregion

            this.Curve = curve;
            this.ArcPosition = arcPosition;
            this.RotationAngle = rotationAngle;
            this.LineLength = lineLength;

            //计算线段端点
            this.CalculateLineEndpoints(out Vector3 start, out Vector3 end);

            //初始化缓冲区
            MeshGeometry lineSegment = MeshFactory.CreateLineSegment(start, end);
            this._vertexBuffer = new VertexBuffer(lineSegment);
        }

        #endregion

        #region # 属性

        #region 曲线 —— Curve Curve
        /// <summary>
        /// 曲线
        /// </summary>
        public Curve Curve { get; private set; }
        #endregion

        #region 弧长位置 —— float ArcPosition
        /// <summary>
        /// 弧长位置
        /// </summary>
        /// <remarks>归一化值0~1</remarks>
        public float ArcPosition { get; private set; }
        #endregion

        #region 旋转角度 —— float RotationAngle
        /// <summary>
        /// 旋转角度
        /// </summary>
        public float RotationAngle { get; private set; }
        #endregion

        #region 线段长度 —— float LineLength
        /// <summary>
        /// 线段长度
        /// </summary>
        /// <remarks>世界空间</remarks>
        public float LineLength { get; private set; }
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

        #region 更新曲线引导线渲染对象 —— void Update(float arcPosition, float rotationAngle...
        /// <summary>
        /// 更新曲线引导线渲染对象
        /// </summary>
        /// <param name="arcPosition">弧长位置（归一化0~1）</param>
        /// <param name="rotationAngle">旋转角度（度）</param>
        /// <param name="lineLength">线段长度（世界空间）</param>
        public void Update(float arcPosition, float rotationAngle, float lineLength)
        {
            #region # 验证

            if (lineLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lineLength), "线段长度必须大于0！");
            }
            if (this.ArcPosition.Equals(arcPosition) && this.RotationAngle.Equals(rotationAngle) && this.LineLength.Equals(lineLength))
            {
                return;
            }

            #endregion

            this.ArcPosition = arcPosition;
            this.RotationAngle = rotationAngle;
            this.LineLength = lineLength;

            //重新计算线段端点
            this.CalculateLineEndpoints(out Vector3 start, out Vector3 end);

            //更新VBO
            MeshGeometry lineSegment = MeshFactory.CreateLineSegment(start, end);
            this._vertexBuffer.Update(lineSegment);

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

        #region 渲染 —— override void Render(ShaderProgram program, RenderContext3D context)
        /// <summary>
        /// 渲染
        /// </summary>
        /// <param name="program">Shader程序</param>
        /// <param name="context">渲染上下文</param>
        public override void Render(ShaderProgram program, RenderContext3D context)
        {
            //绘制线框模型
            GL.LineWidth(this.StrokeThickness);
            program.SetUniformInt("u_ColorMode", (int)ColorMode.Color);
            program.SetUniformVector4("u_Color", this.IsSelected ? this.Stroke.Invert() : this.Stroke);
            this._vertexBuffer.Draw(context.GlContext, PrimitiveType.Lines);
        }
        #endregion

        #region 检测射线相交 —— override bool IntersectsRay(Ray ray, out float distance)
        /// <summary>
        /// 检测射线相交
        /// </summary>
        /// <param name="ray">射线（世界空间）</param>
        /// <param name="distance">相交距离</param>
        /// <returns>是否相交</returns>
        public override bool IntersectsRay(Ray ray, out float distance)
        {
            return this.IntersectsRay(ray, out distance, out _, out _, out _);
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

            //线段的局部空间端点
            Vector3 start = this._vertexBuffer.MeshGeometry.Vertices[0].Position;
            Vector3 end = this._vertexBuffer.MeshGeometry.Vertices[1].Position;

            //计算射线起点到线段的距离
            const float pickRadius = 0.05f;
            float distanceToSegment = GeometryAlgorithms.DistanceToSegment(localRay.Origin, start, end);
            if (distanceToSegment < pickRadius)
            {
                hitPoint = ray.Origin;
                distance = 0;

                return true;
            }

            //检测包围盒
            if (this.BoundingBox.Intersects(localRay, out float intersectedDistance))
            {
                hitPoint = ray.GetPoint(intersectedDistance);

                return true;
            }

            return false;
        }
        #endregion

        #region 计算线段端点 —— void CalculateLineEndpoints(out Vector3 start, out Vector3 end)
        /// <summary>
        /// 计算线段端点
        /// </summary>
        /// <param name="start">起点</param>
        /// <param name="end">终点</param>
        public void CalculateLineEndpoints(out Vector3 start, out Vector3 end)
        {
            //获取当前弧长位置的Frenet框架
            float arcLength = this.ArcPosition * this.Curve.TotalArcLength;
            FrenetFrame frame = this.Curve.GetFrameAtArcLength(arcLength);

            //绕Tangent旋转Normal（度转弧度）
            float rotationRad = MathHelper.DegreesToRadians(this.RotationAngle);
            float cos = MathF.Cos(rotationRad);
            float sin = MathF.Sin(rotationRad);
            Vector3 rotatedNormal = frame.Normal * cos +
                                    Vector3.Cross(frame.Tangent, frame.Normal) * sin +
                                    frame.Tangent * Vector3.Dot(frame.Tangent, frame.Normal) * (1.0f - cos);

            float halfLength = this.LineLength * 0.5f;

            start = frame.Position - rotatedNormal * halfLength;
            end = frame.Position + rotatedNormal * halfLength;
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


        //Protected & Private

        #region 计算包围盒 —— override BoundingBox CalculateBoundingBox()
        /// <summary>
        /// 计算包围盒
        /// </summary>
        protected override BoundingBox CalculateBoundingBox()
        {
            IEnumerable<Vector3> positions = this._vertexBuffer.MeshGeometry.Vertices.Select(vertex => vertex.Position);
            BoundingBox boundingBox = BoundingBox.FromPoints([.. positions]);

            //扩大包围盒便于拾取
            const float padding = 0.05f;
            boundingBox = new BoundingBox(
                boundingBox.Minimum - new Vector3(padding),
                boundingBox.Maximum + new Vector3(padding)
            );

            return boundingBox;
        }
        #endregion

        #endregion
    }
}
