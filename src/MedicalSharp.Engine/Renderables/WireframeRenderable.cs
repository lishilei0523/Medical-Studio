using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalSharp.Engine.Renderables
{
    /// <summary>
    /// 线框渲染对象
    /// </summary>
    public class WireframeRenderable : Renderable, IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 线框顶点缓冲区
        /// </summary>
        private VertexBuffer _strokeBuffer;

        /// <summary>
        /// 填充顶点缓冲区
        /// </summary>
        private VertexBuffer _fillBuffer;

        /// <summary>
        /// 三角形列表
        /// </summary>
        private readonly IList<Triangle> _triangles;

        /// <summary>
        /// 默认构造器
        /// </summary>
        private WireframeRenderable()
        {
            this._triangles = new List<Triangle>();
            this.Stroke = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            this.StrokeThickness = 1.0f;
            this.Fill = new Vector4(1.0f, 0.0f, 0.0f, 0.1f);
        }

        /// <summary>
        /// 创建线框渲染对象构造器
        /// </summary>
        /// <param name="strokeMesh">线框网格</param>
        /// <param name="fillMesh">填充网格</param>
        public WireframeRenderable(MeshGeometry strokeMesh, MeshGeometry fillMesh)
            : this()
        {
            #region # 验证

            if (strokeMesh == null)
            {
                throw new ArgumentNullException(nameof(strokeMesh), "线框网格不可为空！");
            }
            if (fillMesh == null)
            {
                throw new ArgumentNullException(nameof(fillMesh), "填充网格不可为空！");
            }

            #endregion

            this._strokeBuffer = new VertexBuffer(strokeMesh);
            this._fillBuffer = new VertexBuffer(fillMesh);
            this._strokeBuffer.Setup();
            this._fillBuffer.Setup();

            //提取三角形面数据
            this.ExtractTriangles();
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

        #region 填充颜色 —— Vector4 Fill
        /// <summary>
        /// 填充颜色
        /// </summary>
        public Vector4 Fill { get; private set; }
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

        #region 只读属性 - 填充顶点缓冲区 —— VertexBuffer FillBuffer
        /// <summary>
        /// 只读属性 - 填充顶点缓冲区
        /// </summary>
        internal VertexBuffer FillBuffer
        {
            get => this._fillBuffer;
        }
        #endregion

        #region 只读属性 - 三角形列表 —— IReadOnlyList<Triangle> Triangles
        /// <summary>
        /// 只读属性 - 三角形列表
        /// </summary>
        public IReadOnlyList<Triangle> Triangles
        {
            get => this._triangles.AsReadOnly();
        }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 更新线框渲染对象 —— void Update(MeshGeometry strokeMesh, MeshGeometry fillMesh)
        /// <summary>
        /// 更新线框渲染对象
        /// </summary>
        /// <param name="strokeMesh">线框网格</param>
        /// <param name="fillMesh">填充网格</param>
        public void Update(MeshGeometry strokeMesh, MeshGeometry fillMesh)
        {
            #region # 验证

            if (strokeMesh == null)
            {
                throw new ArgumentNullException(nameof(strokeMesh), "线框网格不可为空！");
            }
            if (fillMesh == null)
            {
                throw new ArgumentNullException(nameof(fillMesh), "填充网格不可为空！");
            }

            #endregion

            //先释放旧的
            this._strokeBuffer.Dispose();
            this._fillBuffer.Dispose();

            this._strokeBuffer = new VertexBuffer(strokeMesh);
            this._fillBuffer = new VertexBuffer(fillMesh);
            this._strokeBuffer.Setup();
            this._fillBuffer.Setup();

            //提取三角形面数据
            this.ExtractTriangles();

            //标记包围盒/包围球为脏
            this.InvalidateBoundings();
        }
        #endregion

        #region 设置颜色 —— void SetColor(Vector4 stroke, float strokeThickness...
        /// <summary>
        /// 设置颜色
        /// </summary>
        /// <param name="stroke">线框颜色</param>
        /// <param name="strokeThickness">线框粗细</param>
        /// <param name="fill">填充颜色</param>
        public void SetColor(Vector4 stroke, float strokeThickness, Vector4 fill)
        {
            this.Stroke = stroke;
            this.StrokeThickness = strokeThickness;
            this.Fill = fill;
        }
        #endregion

        #region 检测射线相交 —— bool IntersectsRay(Ray ray, out float distance...
        /// <summary>
        /// 检测射线相交
        /// </summary>
        /// <param name="ray">射线（世界空间）</param>
        /// <param name="distance">相交距离</param>
        /// <param name="hitPoint">命中点坐标</param>
        /// <param name="hitNormal">命中点法向量</param>
        /// <param name="hitTriangleIndex">命中三角形索引</param>
        /// <returns>是否相交</returns>
        public bool IntersectsRay(Ray ray, out float distance, out Vector3 hitPoint, out Vector3 hitNormal, out int hitTriangleIndex)
        {
            distance = float.MaxValue;
            hitPoint = Vector3.Zero;
            hitNormal = Vector3.Zero;
            hitTriangleIndex = -1;

            //快速剔除：先检测包围盒
            if (!this.BoundingBox.Intersects(ray, out float boxDistance))
            {
                return false;
            }

            //将射线变换到局部空间
            Matrix4 worldToLocal = Matrix4.Invert(this.ModelMatrix);
            Ray localRay = ray.Transform(worldToLocal);

            //遍历所有三角形
            for (int index = 0; index < this._triangles.Count; index++)
            {
                Triangle triangle = this._triangles[index];

                //三角形相交检测
                if (triangle.IntersectRay(localRay, out float t, out float u, out float v))
                {
                    if (t > 0 && t < distance)
                    {
                        distance = t;
                        hitTriangleIndex = index;

                        //计算局部交点
                        Vector3 localHitPoint = triangle.GetPoint(u, v);

                        //转换到世界空间
                        hitPoint = Vector3.TransformPosition(localHitPoint, this.ModelMatrix);

                        //计算世界空间法线（考虑变换）
                        hitNormal = Vector3.TransformNormal(triangle.Normal, this.ModelMatrix);
                        hitNormal = Vector3.Normalize(hitNormal);
                    }
                }
            }

            bool hit = hitTriangleIndex >= 0;

            //如果没有命中任何三角形，返回包围盒距离
            if (!hit && boxDistance < float.MaxValue)
            {
                distance = boxDistance;
            }

            return hit;
        }
        #endregion

        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this._triangles.Clear();
            this._strokeBuffer.Dispose();
            this._fillBuffer.Dispose();
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

        #region 提取三角形面数据 —— void ExtractTriangles()
        /// <summary>
        /// 提取三角形面数据
        /// </summary>
        private void ExtractTriangles()
        {
            this._triangles.Clear();

            //获取顶点数据
            Vertex[] vertices = this.FillBuffer.MeshGeometry.Vertices;
            uint[] indices = this.FillBuffer.MeshGeometry.Indices;
            if (indices != null && indices.Any())
            {
                //有索引：按索引构建三角形
                for (int index = 0; index < indices.Length; index += 3)
                {
                    Vector3 pointA = vertices[indices[index]].Position;
                    Vector3 pointB = vertices[indices[index + 1]].Position;
                    Vector3 pointC = vertices[indices[index + 2]].Position;
                    this._triangles.Add(new Triangle(pointA, pointB, pointC));
                }
            }
            else
            {
                //无索引：假设顶点是连续的三角形列表
                for (int index = 0; index < vertices.Length; index += 3)
                {
                    if (index + 2 >= vertices.Length)
                    {
                        break;
                    }

                    Vector3 pointA = vertices[index].Position;
                    Vector3 pointB = vertices[index + 1].Position;
                    Vector3 pointC = vertices[index + 2].Position;
                    this._triangles.Add(new Triangle(pointA, pointB, pointC));
                }
            }
        }
        #endregion

        #endregion
    }
}
