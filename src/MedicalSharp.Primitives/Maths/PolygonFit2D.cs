using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MedicalSharp.Primitives.Maths
{
    /// <summary>
    /// 拟合2D多边形
    /// </summary>
    /// <remarks>支持3D到2D投影，用于GPU切割</remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct PolygonFit2D
    {
        #region # 字段及构造器

        /// <summary>
        /// 2D顶点列表
        /// </summary>
        private readonly Vector2[] _vertices2D;

        /// <summary>
        /// 创建拟合2D多边形构造器
        /// </summary>
        /// <param name="vertices3D">3D顶点列表（按顺序）</param>
        public PolygonFit2D(IReadOnlyList<Vector3> vertices3D)
        {
            #region # 验证

            if (vertices3D == null || vertices3D.Count < 3)
            {
                throw new ArgumentException("至少需要3个顶点！");
            }

            #endregion

            //计算拟合平面
            this.ComputeFitPlane(vertices3D);

            //构建UV坐标系
            this.BuildBasis();

            //投影顶点到2D
            this._vertices2D = this.ProjectTo2D(vertices3D);
        }

        #endregion

        #region # 属性

        #region 拟合平面法向量 —— Vector3 FitPlaneNormal
        /// <summary>
        /// 拟合平面法向量
        /// </summary>
        public Vector3 FitPlaneNormal { get; private set; }
        #endregion

        #region 拟合中心位置 —— Vector3 FitCenter
        /// <summary>
        /// 拟合中心位置
        /// </summary>
        public Vector3 FitCenter { get; private set; }
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

        #region 只读属性 - 2D顶点列表 —— Vector2[] Vertices2D
        /// <summary>
        /// 只读属性 - 2D顶点列表
        /// </summary>
        public Vector2[] Vertices2D
        {
            get => this._vertices2D;
        }
        #endregion

        #region 只读属性 - 是否有效 —— bool IsValid
        /// <summary>
        /// 只读属性 - 是否有效
        /// </summary>
        public bool IsValid
        {
            get => this.Vertices2D.Length >= 3;
        }
        #endregion

        #endregion

        #region # 方法

        #region 计算拟合平面 —— void ComputeFitPlane(IReadOnlyList<Vector3> vertices)
        /// <summary>
        /// 计算拟合平面
        /// </summary>
        /// <param name="vertices">顶点列表</param>
        /// <remarks>Newell方法</remarks>
        private void ComputeFitPlane(IReadOnlyList<Vector3> vertices)
        {
            //计算法向量
            Vector3 normal = Vector3.Zero;
            for (int index = 0; index < vertices.Count; index++)
            {
                Vector3 current = vertices[index];
                Vector3 next = vertices[(index + 1) % vertices.Count];
                normal.X += (current.Y - next.Y) * (current.Z + next.Z);
                normal.Y += (current.Z - next.Z) * (current.X + next.X);
                normal.Z += (current.X - next.X) * (current.Y + next.Y);
            }
            this.FitPlaneNormal = Vector3.Normalize(normal);

            //计算中心位置
            Vector3 center = vertices.Aggregate(Vector3.Zero, (current, vertex) => current + vertex);
            this.FitCenter = center / vertices.Count;
        }
        #endregion

        #region 构建UV正交基 —— void BuildBasis()
        /// <summary>
        /// 构建UV正交基
        /// </summary>
        private void BuildBasis()
        {
            //法向量接近Z轴
            if (Math.Abs(Vector3.Dot(this.FitPlaneNormal, Vector3.UnitZ)) > 0.99f)
            {
                this.UAxis = Vector3.UnitX;
                this.VAxis = Vector3.UnitY;
            }
            //法向量接近Y轴
            else if (Math.Abs(Vector3.Dot(this.FitPlaneNormal, Vector3.UnitY)) > 0.99f)
            {
                this.UAxis = Vector3.UnitX;
                this.VAxis = Vector3.UnitZ;
            }
            //法向量接近X轴
            else if (Math.Abs(Vector3.Dot(this.FitPlaneNormal, Vector3.UnitX)) > 0.99f)
            {
                this.UAxis = Vector3.UnitY;
                this.VAxis = Vector3.UnitZ;
            }
            else
            {
                //如果法线被旋转过，重新构造正交基（保证U在XY平面内优先）
                this.UAxis = Vector3.Normalize(Vector3.Cross(Vector3.UnitZ, this.FitPlaneNormal));
                this.VAxis = Vector3.Normalize(Vector3.Cross(this.FitPlaneNormal, this.UAxis));
            }
        }
        #endregion

        #region 投影3D顶点到2D —— Vector2[] ProjectTo2D(IReadOnlyList<Vector3> vertices3D)
        /// <summary>
        /// 投影3D顶点到2D
        /// </summary>
        private Vector2[] ProjectTo2D(IReadOnlyList<Vector3> vertices3D)
        {
            Vector2[] vertices2D = new Vector2[vertices3D.Count];
            for (int index = 0; index < vertices3D.Count; index++)
            {
                Vector3 delta = vertices3D[index] - this.FitCenter;
                float u = Vector3.Dot(delta, this.UAxis);
                float v = Vector3.Dot(delta, this.VAxis);
                vertices2D[index] = new Vector2(u, v);
            }

            return vertices2D;
        }
        #endregion

        #endregion
    }
}
