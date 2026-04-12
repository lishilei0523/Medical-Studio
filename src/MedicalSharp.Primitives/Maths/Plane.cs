using OpenTK.Mathematics;
using System;
using System.Runtime.InteropServices;

namespace MedicalSharp.Primitives.Maths
{
    /// <summary>
    /// 平面
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Plane : IEquatable<Plane>
    {
        #region # 字段及构造器

        /// <summary>
        /// 法向量
        /// </summary>
        private Vector3 _normal;

        /// <summary>
        /// 平面到原点距离
        /// </summary>
        /// <remarks>沿法向量方向</remarks>
        private float _distance;

        /// <summary>
        /// 创建平面构造器
        /// </summary>
        /// <param name="normal">平面法向量</param>
        /// <param name="distance">平面到原点的距离</param>
        public Plane(Vector3 normal, float distance)
        {
            this._normal = normal;
            this._distance = distance;
        }

        /// <summary>
        /// 创建平面构造器
        /// </summary>
        /// <param name="normal">平面法向量</param>
        /// <param name="point">平面上一点</param>
        public Plane(Vector3 normal, Vector3 point)
        {
            this._normal = Vector3.Normalize(normal);
            this._distance = -Vector3.Dot(this._normal, point);
        }

        /// <summary>
        /// 创建平面构造器
        /// </summary>
        /// <param name="pointA">点A</param>
        /// <param name="pointB">点B</param>
        /// <param name="pointC">点C</param>
        public Plane(Vector3 pointA, Vector3 pointB, Vector3 pointC)
        {
            Vector3 edge1 = pointB - pointA;
            Vector3 edge2 = pointC - pointA;
            this._normal = Vector3.Normalize(Vector3.Cross(edge1, edge2));
            this._distance = -Vector3.Dot(this._normal, pointA);
        }

        #endregion

        #region # 属性

        #region 只读属性 - 法向量 —— Vector3 Normal
        /// <summary>
        /// 只读属性 - 法向量
        /// </summary>
        public Vector3 Normal
        {
            get => this._normal;
        }
        #endregion

        #region 只读属性 - 平面到原点距离 —— float Distance
        /// <summary>
        /// 只读属性 - 平面到原点距离
        /// </summary>
        /// <remarks>沿法向量方向</remarks>
        public float Distance
        {
            get => this._distance;
        }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 翻转平面法向量方向 —— void Flip()
        /// <summary>
        /// 翻转平面法向量方向
        /// </summary>
        public void Flip()
        {
            this._normal = -this._normal;
            this._distance = -this._distance;
        }
        #endregion

        #region 获取点到平面距离 —— float GetDistanceToPoint(Vector3 point)
        /// <summary>
        /// 获取点到平面距离
        /// </summary>
        /// <param name="point">点</param>
        /// <returns>距离</returns>
        /// <remarks>有符号距离，正表示点在法向量方向一侧</remarks>
        public float GetDistanceToPoint(Vector3 point)
        {
            return Vector3.Dot(this._normal, point) + this._distance;
        }
        #endregion

        #region 检测是否与射线相交 —— bool Intersects(Ray ray, out float distance)
        /// <summary>
        /// 检测是否与射线相交
        /// </summary>
        /// <param name="ray">射线</param>
        /// <param name="distance">射线起点到交点的距离</param>
        /// <returns>是否相交</returns>
        public bool Intersects(Ray ray, out float distance)
        {
            float denominator = Vector3.Dot(this._normal, ray.Direction);

            //如果分母接近0，射线与平面平行
            if (Math.Abs(denominator) < 1e-6f)
            {
                distance = 0;
                return false;
            }

            distance = -(Vector3.Dot(this._normal, ray.Position) + this._distance) / denominator;

            //TODO 只返回正向的交点
            //return distance >= 0;

            return true;
        }
        #endregion

        #region 检测是否与射线相交 —— bool Intersects(Ray ray, out Vector3 hitPoint)
        /// <summary>
        /// 检测是否与射线相交
        /// </summary>
        /// <param name="ray">射线</param>
        /// <param name="hitPoint">交点坐标</param>
        /// <returns>是否相交</returns>
        public bool Intersects(Ray ray, out Vector3 hitPoint)
        {
            if (this.Intersects(ray, out float distance))
            {
                hitPoint = ray.Position + ray.Direction * distance;
                return true;
            }

            hitPoint = Vector3.Zero;

            return false;
        }
        #endregion

        #region 检测是否与另一个平面相等 —— bool Equals(Plane other)
        /// <summary>
        /// 检测是否与另一个平面相等
        /// </summary>
        public bool Equals(Plane other)
        {
            return this._normal.Equals(other._normal) && Math.Abs(this._distance - other._distance) < 1e-6f;
        }
        #endregion

        #region 检测点是否在平面上 —— bool ContainsPoint(Vector3 point, float tolerance = 1e-6f)
        /// <summary>
        /// 检测点是否在平面上
        /// </summary>
        /// <param name="point">要判断的点</param>
        /// <param name="tolerance">容差值</param>
        /// <returns>是否在平面上</returns>
        public bool ContainsPoint(Vector3 point, float tolerance = 1e-6f)
        {
            return Math.Abs(this.GetDistanceToPoint(point)) <= tolerance;
        }
        #endregion

        #region 投影点到平面 —— Vector3 ProjectPoint(Vector3 point)
        /// <summary>
        /// 投影点到平面
        /// </summary>
        /// <param name="point">点</param>
        /// <returns>投影点坐标</returns>
        public Vector3 ProjectPoint(Vector3 point)
        {
            float distance = this.GetDistanceToPoint(point);
            return point - this._normal * distance;
        }
        #endregion

        #region 获取平面上距离原点最近的点 —— Vector3 GetClosestPointToOrigin()
        /// <summary>
        /// 获取平面上距离原点最近的点
        /// </summary>
        /// <returns>平面上最接近原点的点</returns>
        public Vector3 GetClosestPointToOrigin()
        {
            return this._normal * -this._distance;
        }
        #endregion


        //IEquatable

        #region 是否相等 —— override bool Equals(object obj)
        /// <summary>
        /// 是否相等
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is Plane other && this.Equals(other);
        }
        #endregion

        #region 获取哈希码 —— override int GetHashCode()
        /// <summary>
        /// 获取哈希码
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(this._normal, this._distance);
        }
        #endregion

        #region 相等运算符 —— static bool operator ==(Plane left, Plane right)
        /// <summary>
        /// 相等运算符
        /// </summary>
        public static bool operator ==(Plane left, Plane right)
        {
            return left.Equals(right);
        }
        #endregion

        #region 不等运算符 —— static bool operator !=(Plane left, Plane right)
        /// <summary>
        /// 不等运算符
        /// </summary>
        public static bool operator !=(Plane left, Plane right)
        {
            return !left.Equals(right);
        }
        #endregion 

        #endregion
    }
}
