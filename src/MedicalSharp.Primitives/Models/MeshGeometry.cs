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

        #region 提取三角形面 —— IList<Triangle> ExtractTriangles()
        /// <summary>
        /// 提取三角形面
        /// </summary>
        /// <returns>三角形面列表</returns>
        public IList<Triangle> ExtractTriangles()
        {
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

            return triangles;
        }
        #endregion

        #endregion
    }
}
