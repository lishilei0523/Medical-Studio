using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalSharp.Engine.ValueTypes
{
    /// <summary>
    /// 网格几何
    /// </summary>
    public sealed class MeshGeometry
    {
        #region # 字段及构造器

        /// <summary>
        /// 无参构造器
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

        #endregion
    }
}
