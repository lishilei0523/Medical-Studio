using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace MedicalSharp.Primitives.Maths
{
    /// <summary>
    /// 三角形
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Triangle
    {
        #region # 字段及构造器

        /// <summary>
        /// 创建三角形构造器
        /// </summary>
        /// <param name="pointA">点A</param>
        /// <param name="pointB">点B</param>
        /// <param name="pointC">点C</param>
        public Triangle(Vector3 pointA, Vector3 pointB, Vector3 pointC)
        {
            this.PointA = pointA;
            this.PointB = pointB;
            this.PointC = pointC;

            //计算法向量（归一化）
            Vector3 edge1 = pointB - pointA;
            Vector3 edge2 = pointC - pointA;
            this.Normal = Vector3.Normalize(Vector3.Cross(edge1, edge2));

            //计算中心点
            this.Center = (pointA + pointB + pointC) / 3.0f;
        }

        #endregion

        #region # 属性

        #region 点A —— Vector3 PointA
        /// <summary>
        /// 点A
        /// </summary>
        public Vector3 PointA { get; private set; }
        #endregion

        #region 点B —— Vector3 PointB
        /// <summary>
        /// 点B
        /// </summary>
        public Vector3 PointB { get; private set; }
        #endregion

        #region 点C —— Vector3 PointC
        /// <summary>
        /// 点C
        /// </summary>
        public Vector3 PointC { get; private set; }
        #endregion

        #region 中心点 —— Vector3 Center
        /// <summary>
        /// 中心点
        /// </summary>
        public Vector3 Center { get; private set; }
        #endregion

        #region 法向量 —— Vector3 Normal
        /// <summary>
        /// 法向量
        /// </summary>
        public Vector3 Normal { get; private set; }
        #endregion

        #endregion

        #region # 方法

        #region 检测射线相交 —— bool IntersectRay(Ray ray, out float t, out float u, out float v)
        /// <summary>
        /// 检测射线相交
        /// </summary>
        /// <param name="ray">射线</param>
        /// <param name="t">射线参数方程的距离</param>
        /// <param name="u">重心坐标（第1个权重）</param>
        /// <param name="v">重心坐标（第2个权重）</param>
        /// <returns>是否相交</returns>
        /// <remarks>Möller–Trumbore算法</remarks>
        public bool IntersectRay(Ray ray, out float t, out float u, out float v)
        {
            t = u = v = 0;
            const float epsilon = 1e-6f;

            Vector3 edge1 = this.PointB - this.PointA;
            Vector3 edge2 = this.PointC - this.PointA;
            Vector3 pvec = Vector3.Cross(ray.Direction, edge2);

            float det = Vector3.Dot(edge1, pvec);

            //背面剔除（可选，根据需求决定）
            if (det > -epsilon && det < epsilon)
            {
                return false;
            }

            float invDet = 1.0f / det;
            Vector3 tvec = ray.Position - this.PointA;
            u = Vector3.Dot(tvec, pvec) * invDet;

            if (u < 0 || u > 1)
            {
                return false;
            }

            Vector3 qvec = Vector3.Cross(tvec, edge1);
            v = Vector3.Dot(ray.Direction, qvec) * invDet;

            if (v < 0 || u + v > 1)
            {
                return false;
            }

            t = Vector3.Dot(edge2, qvec) * invDet;

            return t > epsilon;
        }
        #endregion

        #region 获取三角形上的点 —— Vector3 GetPoint(float u, float v)
        /// <summary>
        /// 获取三角形上的点
        /// </summary>
        /// <param name="u">重心坐标（第1个权重）</param>
        /// <param name="v">重心坐标（第2个权重）</param>
        public Vector3 GetPoint(float u, float v)
        {
            float w = 1 - u - v;
            Vector3 position = this.PointA * w + this.PointB * u + this.PointC * v;

            return position;
        }
        #endregion 

        #endregion
    }
}
