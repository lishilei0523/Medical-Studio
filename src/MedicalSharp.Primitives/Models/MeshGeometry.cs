using MedicalSharp.Primitives.Maths;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalSharp.Primitives.Models
{
    /// <summary>
    /// 网格几何
    /// </summary>
    public sealed class MeshGeometry
    {
        #region # 字段及构造器

        /// <summary>
        /// 默认构造器
        /// </summary>
        private MeshGeometry()
        {
            this.Vertices = [];
            this.Indices = [];
        }

        /// <summary>
        /// 创建网格几何构造器
        /// </summary>
        /// <param name="vertices">顶点列表</param>
        public MeshGeometry(ICollection<Vertex> vertices)
            : this()
        {
            if (vertices == null || !vertices.Any())
            {
                throw new ArgumentNullException(nameof(vertices), "顶点列表不可为空！");
            }

            this.Vertices = [.. vertices];
        }

        /// <summary>
        /// 创建网格几何构造器
        /// </summary>
        /// <param name="vertices">顶点列表</param>
        /// <param name="indices">顶点索引列表</param>
        public MeshGeometry(ICollection<Vertex> vertices, ICollection<uint> indices)
            : this()
        {
            if (vertices == null || !vertices.Any())
            {
                throw new ArgumentNullException(nameof(vertices), "顶点列表不可为空！");
            }
            if (indices == null || !indices.Any())
            {
                throw new ArgumentNullException(nameof(indices), "顶点索引列表不可为空！");
            }

            this.Vertices = [.. vertices];
            this.Indices = [.. indices];
        }

        #endregion

        #region # 属性

        #region 顶点列表 —— Vertex[] Vertices
        /// <summary>
        /// 顶点列表
        /// </summary>
        public Vertex[] Vertices { get; private set; }
        #endregion

        #region 顶点索引列表 —— uint[] Indices
        /// <summary>
        /// 顶点索引列表
        /// </summary>
        public uint[] Indices { get; private set; }
        #endregion

        #endregion

        #region # 方法

        #region 设置顶点列表 —— void SetVertices(ICollection<Vertex> vertices)
        /// <summary>
        /// 设置顶点列表
        /// </summary>
        /// <param name="vertices">顶点列表</param>
        public void SetVertices(ICollection<Vertex> vertices)
        {
            if (vertices == null || !vertices.Any())
            {
                throw new ArgumentNullException(nameof(vertices), "顶点列表不可为空！");
            }

            this.Vertices = [.. vertices];
        }
        #endregion

        #region 设置顶点索引列表 —— void SetIndices(ICollection<uint> indices)
        /// <summary>
        /// 设置顶点索引列表
        /// </summary>
        /// <param name="indices">顶点索引列表</param>
        public void SetIndices(ICollection<uint> indices)
        {
            if (indices == null || !indices.Any())
            {
                this.Indices = [];
                return;
            }

            this.Indices = [.. indices];
        }
        #endregion

        #region 提取三角形面列表 —— Triangle[] ExtractTriangles()
        /// <summary>
        /// 提取三角形面列表
        /// </summary>
        /// <returns>三角形面列表</returns>
        public Triangle[] ExtractTriangles()
        {
            #region # 验证

            if (this.Indices?.Length < 3)
            {
                return [];
            }

            #endregion

            IList<Triangle> triangles = new List<Triangle>();

            //获取顶点数据
            Vertex[] vertices = this.Vertices;
            uint[] indices = this.Indices;
            if (indices != null && indices.Any())
            {
                //有索引：按索引构建三角形
                for (int index = 0; index < indices.Length; index += 3)
                {
                    Vector3 pointA = vertices[indices[index]].Position;
                    Vector3 pointB = vertices[indices[index + 1]].Position;
                    Vector3 pointC = vertices[indices[index + 2]].Position;
                    triangles.Add(new Triangle(pointA, pointB, pointC));
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
                    triangles.Add(new Triangle(pointA, pointB, pointC));
                }
            }

            return [.. triangles];
        }
        #endregion

        #region 提取平面方程列表 —— Vector4[] ExtractPlanes()
        /// <summary>
        /// 提取平面方程列表
        /// </summary>
        /// <returns>平面方程列表</returns>
        public Vector4[] ExtractPlanes()
        {
            Vector3[] vertices = this.Vertices.Select(x => x.Position).ToArray();
            uint[] indices = this.Indices;

            ICollection<Vector4> planes = new HashSet<Vector4>();
            for (int index = 0; index < indices.Length; index += 3)
            {
                Vector3 v0 = vertices[indices[index]];
                Vector3 v1 = vertices[indices[index + 1]];
                Vector3 v2 = vertices[indices[index + 2]];

                //计算法向量（归一化）
                Vector3 normal = Vector3.Normalize(Vector3.Cross(v1 - v0, v2 - v0));

                //计算距离（满足 dot(normal, v0) + distance = 0）
                float distance = -Vector3.Dot(normal, v0);

                //归一化平面方程（可选，确保精度）
                Vector4 plane = new Vector4(normal.X, normal.Y, normal.Z, distance);

                //去重（相同法向量+相近d视为同一平面）
                planes.Add(plane);
            }

            return planes.ToArray();
        }
        #endregion

        #endregion
    }
}
