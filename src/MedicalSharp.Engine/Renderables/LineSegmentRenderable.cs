using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Builders;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Engine.Renderables
{
    /// <summary>
    /// 线段渲染对象
    /// </summary>
    public class LineSegmentRenderable : ShapeRenderable
    {
        #region # 字段及构造器

        /// <summary>
        /// 线框顶点缓冲区
        /// </summary>
        private VertexBuffer _strokeBuffer;

        /// <summary>
        /// 默认构造器
        /// </summary>
        private LineSegmentRenderable()
        {
            //默认值
            this.Stroke = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            this.StrokeThickness = 1.0f;
        }

        /// <summary>
        /// 创建线段渲染对象构造器
        /// </summary>
        /// <param name="startPoint">起始点</param>
        /// <param name="endPoint">终止点</param>
        public LineSegmentRenderable(Vector3 startPoint, Vector3 endPoint)
            : this()
        {
            #region # 验证

            if (startPoint == endPoint)
            {
                throw new ArgumentNullException(nameof(endPoint), "起始点与终止点不可相等！");
            }

            #endregion

            this.StartPoint = startPoint;
            this.EndPoint = endPoint;

            MeshGeometry lineGeometry = MeshFactory.CreateLine(startPoint, endPoint);
            this._strokeBuffer = new VertexBuffer(lineGeometry);
            this._strokeBuffer.Setup();
        }

        #endregion

        #region # 属性

        #region 起始点 —— Vector3 StartPoint
        /// <summary>
        /// 起始点
        /// </summary>
        public Vector3 StartPoint { get; private set; }
        #endregion

        #region 终止点 —— Vector3 EndPoint
        /// <summary>
        /// 终止点
        /// </summary>
        public Vector3 EndPoint { get; private set; }
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

        #region 更新线段渲染对象 —— void Update(Vector3 startPoint, Vector3 endPoint)
        /// <summary>
        /// 更新线段渲染对象
        /// </summary>
        /// <param name="startPoint">起始点</param>
        /// <param name="endPoint">终止点</param>
        public void Update(Vector3 startPoint, Vector3 endPoint)
        {
            #region # 验证

            if (this.StartPoint == startPoint && this.EndPoint == endPoint)
            {
                return;
            }
            if (startPoint == endPoint)
            {
                throw new ArgumentNullException(nameof(endPoint), "起始点与终止点不可相等！");
            }

            #endregion

            this.StartPoint = startPoint;
            this.EndPoint = endPoint;

            //先释放旧的
            this._strokeBuffer.Dispose();

            MeshGeometry lineGeometry = MeshFactory.CreateLine(startPoint, endPoint);
            this._strokeBuffer = new VertexBuffer(lineGeometry);
            this._strokeBuffer.Setup();

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
            this.StrokeBuffer.Draw(PrimitiveType.Lines);
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

            //快速剔除：先检测包围盒
            if (!this.BoundingBox.Intersects(ray, out _))
            {
                return false;
            }

            //精确检测
            const float tolerance = 0.01f;
            Vector3 segmentDir = this.EndPoint - this.StartPoint;
            Vector3 rayToStart = this.StartPoint - ray.Position;

            //计算射线方向与线段方向的各种点积
            float a = Vector3.Dot(segmentDir, segmentDir);      //线段长度的平方
            float b = Vector3.Dot(ray.Direction, segmentDir);   //射线与线段夹角余弦 × 长度
            float c = Vector3.Dot(ray.Direction, rayToStart);
            float d = Vector3.Dot(segmentDir, rayToStart);
            float denominator = a - b * b;  // 等价于|segment × ray|^2

            //射线与线段不平行
            if (Math.Abs(denominator) > float.Epsilon)
            {
                //计算线段上的最近点参数 s
                float s = (b * c - d) / denominator;
                s = Math.Clamp(s, 0f, 1f);  //限制在线段范围内

                //计算射线上的最近点参数 t
                float t = (b * s + c);

                //射线只能向前
                if (t < 0)
                {
                    //检查线段起点是否在射线前方
                    if (c < 0 && Vector3.Dot(ray.Direction, this.EndPoint - ray.Position) < 0)
                    {
                        return false;
                    }

                    //如果射线起点到线段的最短距离在容差内，也算相交
                    Vector3 closestOnRay = ray.Position;
                    Vector3 closestOnSegment = this.StartPoint + segmentDir * s;
                    if (Vector3.DistanceSquared(closestOnRay, closestOnSegment) <= tolerance * tolerance)
                    {
                        distance = 0;
                        hitPoint = closestOnSegment;
                        return true;
                    }

                    return false;
                }

                //计算两个最近点
                Vector3 pointOnSegment = this.StartPoint + segmentDir * s;
                Vector3 pointOnRay = ray.GetPoint(t);

                //检查距离是否在容差内
                float distSq = Vector3.DistanceSquared(pointOnSegment, pointOnRay);
                if (distSq <= tolerance * tolerance)
                {
                    distance = t;
                    hitPoint = pointOnSegment;  //交点取线段上的点

                    return true;
                }
            }
            //射线与线段平行
            else
            {
                //检查射线起点到线段所在直线的距离
                Vector3 cross = Vector3.Cross(ray.Direction, rayToStart);
                float distToLineSq = cross.LengthSquared / a;
                if (distToLineSq > tolerance * tolerance)
                {
                    return false;  //不共线
                }

                //共线情况：计算线段端点在射线上的投影
                float t0 = Vector3.Dot(ray.Direction, this.StartPoint - ray.Position);
                float t1 = Vector3.Dot(ray.Direction, this.EndPoint - ray.Position);

                float tMin = Math.Min(t0, t1);
                float tMax = Math.Max(t0, t1);

                //重叠检测
                if (tMax < -tolerance)
                {
                    return false;  //线段完全在射线后方
                }

                //计算交点（取重叠部分的起点）
                if (tMin < 0)
                {
                    distance = 0;
                    hitPoint = ray.Position;

                    //保交点在容差内
                    float distToSegment = (t0 < 0 && t1 < 0)
                        ? Math.Min(Vector3.Distance(ray.Position, this.StartPoint), Vector3.Distance(ray.Position, this.EndPoint))
                        : 0;

                    return distToSegment <= tolerance;
                }

                distance = tMin;
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
            BoundingBox boundingBox = new BoundingBox(this.StartPoint, this.EndPoint);

            return boundingBox;
        }
        #endregion

        #endregion
    }
}
