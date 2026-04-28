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
    /// 曲线渲染对象
    /// </summary>
    public class CurveRenderable : ShapeRenderable
    {
        #region # 字段及构造器

        /// <summary>
        /// 控制点缓冲区
        /// </summary>
        private VertexBuffer _pointBuffer;

        /// <summary>
        /// 曲线缓冲区
        /// </summary>
        private VertexBuffer _curveBuffer;

        /// <summary>
        /// 默认构造器
        /// </summary>
        private CurveRenderable()
        {
            //默认值
            this.Stroke = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            this.StrokeThickness = 1.0f;
        }

        /// <summary>
        /// 创建折线渲染对象构造器
        /// </summary>
        /// <param name="controlPositions">控制点列表</param>
        /// <param name="sampledPositions">采样点列表</param>
        /// <param name="closed">是否闭合</param>
        public CurveRenderable(IReadOnlyList<Vector3> controlPositions, IReadOnlyList<Vector3> sampledPositions, bool closed = false)
            : this()
        {
            this.ControlPositions = controlPositions;
            this.SampledPositions = sampledPositions;
            this.Closed = closed;

            //初始化缓冲区
            MeshGeometry pointGeometry = MeshFactory.CreatePointCloud(this.ControlPositions);
            MeshGeometry curveGeometry = MeshFactory.CreatePolyline(this.SampledPositions, this.Closed);
            this._pointBuffer = new VertexBuffer(pointGeometry);
            this._curveBuffer = new VertexBuffer(curveGeometry);
            this._pointBuffer.Setup();
            this._curveBuffer.Setup();
        }

        #endregion

        #region # 属性

        #region 控制点列表 —— IReadOnlyList<Vector3> ControlPositions
        /// <summary>
        /// 控制点列表
        /// </summary>
        public IReadOnlyList<Vector3> ControlPositions { get; private set; }
        #endregion

        #region 采样点列表 —— IReadOnlyList<Vector3> SampledPositions
        /// <summary>
        /// 采样点列表
        /// </summary>
        public IReadOnlyList<Vector3> SampledPositions { get; private set; }
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

        #region 只读属性 - 控制点缓冲区 —— VertexBuffer PointBuffer
        /// <summary>
        /// 只读属性 - 控制点缓冲区
        /// </summary>
        internal VertexBuffer PointBuffer
        {
            get => this._pointBuffer;
        }
        #endregion

        #region 只读属性 - 曲线缓冲区 —— VertexBuffer CurveBuffer
        /// <summary>
        /// 只读属性 - 曲线缓冲区
        /// </summary>
        internal VertexBuffer CurveBuffer
        {
            get => this._curveBuffer;
        }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 更新曲线渲染对象 —— void Update(IReadOnlyList<Vector3> controlPositions...
        /// <summary>
        /// 更新曲线渲染对象
        /// </summary>
        /// <param name="controlPositions">控制点列表</param>
        /// <param name="sampledPositions">采样点列表</param>
        public void Update(IReadOnlyList<Vector3> controlPositions, IReadOnlyList<Vector3> sampledPositions)
        {
            #region # 验证

            if (this.ControlPositions.Equals(controlPositions) && Equals(this.SampledPositions, sampledPositions))
            {
                return;
            }

            #endregion

            this.ControlPositions = controlPositions;
            this.SampledPositions = sampledPositions;

            //先释放旧的
            this._pointBuffer.Dispose();
            this._curveBuffer.Dispose();

            MeshGeometry pointGeometry = MeshFactory.CreatePointCloud(this.ControlPositions);
            MeshGeometry curveGeometry = MeshFactory.CreatePolyline(this.SampledPositions, this.Closed);
            this._pointBuffer = new VertexBuffer(pointGeometry);
            this._curveBuffer = new VertexBuffer(curveGeometry);
            this._pointBuffer.Setup();
            this._curveBuffer.Setup();

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
            this._curveBuffer.Draw(PrimitiveType.Lines);

            //点尺寸
            float pointSize = Math.Clamp(this.StrokeThickness * 3.0f, 5f, 20f);
            GL.PointSize(pointSize);

            //点颜色
            Vector4 invertedStroke = this.Stroke.Invert();
            float contrast = Math.Abs(invertedStroke.X - invertedStroke.X) +
                             Math.Abs(invertedStroke.Y - invertedStroke.Y) +
                             Math.Abs(invertedStroke.Z - invertedStroke.Z);
            if (contrast < 0.5f)
            {
                invertedStroke = ColorFactory.Yellow(); //固定用亮黄色
            }

            //绘制控制点
            program.SetUniformVector4("u_Color", invertedStroke);
            this._pointBuffer.Draw(PrimitiveType.Points);
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
            if (!this.BoundingBox.Intersects(localRay, out _))
            {
                return false;
            }

            //精确检测
            IDictionary<BoundingSphere, float> hitPoints = new Dictionary<BoundingSphere, float>();
            foreach (Vector3 position in this.SampledPositions)
            {
                BoundingSphere sphere = new BoundingSphere(position, 0.05f);
                if (sphere.Intersects(localRay, out float pointDistance))
                {
                    hitPoints.TryAdd(sphere, pointDistance);
                }
            }
            if (hitPoints.Any())
            {
                KeyValuePair<BoundingSphere, float> hit = hitPoints.MinBy(x => x.Value);
                distance = hit.Value;
                hitPoint = hit.Key.Center;

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

            this._pointBuffer.Dispose();
            this._curveBuffer.Dispose();

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
            BoundingBox boundingBox = BoundingBox.FromPoints(this.SampledPositions);

            return boundingBox;
        }
        #endregion

        #endregion
    }
}
