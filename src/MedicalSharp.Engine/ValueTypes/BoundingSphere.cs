using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MedicalSharp.Engine.ValueTypes
{
    /// <summary>
    /// 包围球
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BoundingSphere : IEquatable<BoundingSphere>
    {
        #region # 字段及构造器

        /// <summary>
        /// 球心
        /// </summary>
        private Vector3 _center;

        /// <summary>
        /// 半径
        /// </summary>
        private float _radius;

        /// <summary>
        /// 创建包围球构造器
        /// </summary>
        /// <param name="center">球心</param>
        /// <param name="radius">半径</param>
        public BoundingSphere(Vector3 center, float radius)
        {
            this._center = center;
            this._radius = radius;
        }

        #endregion

        #region # 属性

        #region 只读属性 - 球心 —— Vector3 Center
        /// <summary>
        /// 只读属性 - 球心
        /// </summary>
        public Vector3 Center
        {
            get => this._center;
        }
        #endregion

        #region 只读属性 - 半径 —— float Radius
        /// <summary>
        /// 只读属性 - 半径
        /// </summary>
        public float Radius
        {
            get => this._radius;
        }
        #endregion

        #endregion

        #region # 方法

        //Static

        #region 从点集创建包围球 —— static BoundingSphere FromPoints(IList<Vector3> points)
        /// <summary>
        /// 从点集创建包围球
        /// </summary>
        /// <remarks>Ritter算法</remarks>
        public static BoundingSphere FromPoints(IList<Vector3> points)
        {
            #region # 验证

            if (points == null || !points.Any())
            {
                throw new ArgumentException("至少需要一个点！");
            }

            #endregion

            //找到最小和最大点来确定初始中心
            Vector3 min = points[0];
            Vector3 max = points[0];
            foreach (Vector3 point in points)
            {
                min = Vector3.ComponentMin(min, point);
                max = Vector3.ComponentMax(max, point);
            }

            Vector3 center = (min + max) * 0.5f;
            float radius = 0f;

            //计算初始半径
            foreach (Vector3 point in points)
            {
                float distance = Vector3.Distance(center, point);
                if (distance > radius)
                {
                    radius = distance;
                }
            }

            //Ritter算法优化
            for (int i = 0; i < 2; i++)
            {
                foreach (Vector3 point in points)
                {
                    Vector3 direction = point - center;
                    float distance = direction.Length;

                    if (distance > radius)
                    {
                        //扩展球体以包含该点
                        float half = (distance - radius) * 0.5f;
                        radius += half;
                        center += direction * (half / distance);
                    }
                }
            }

            return new BoundingSphere(center, radius);
        }
        #endregion

        #region 从包围盒创建包围球 —— static BoundingSphere FromBox(BoundingBox box)
        /// <summary>
        /// 从包围盒创建包围球
        /// </summary>
        public static BoundingSphere FromBox(BoundingBox box)
        {
            return box.ToBoundingSphere();
        }
        #endregion


        //Public

        #region 检查点是否在包围球内 —— bool Contains(Vector3 point)
        /// <summary>
        /// 检查点是否在包围球内
        /// </summary>
        public bool Contains(Vector3 point)
        {
            return Vector3.DistanceSquared(this.Center, point) <= this.Radius * this.Radius;
        }
        #endregion

        #region 检查另一个包围球是否包含在包围球内 —— bool Contains(BoundingSphere other)
        /// <summary>
        /// 检查另一个包围球是否包含在包围球内
        /// </summary>
        public bool Contains(BoundingSphere other)
        {
            float distance = Vector3.Distance(this.Center, other.Center);
            return distance + other.Radius <= this.Radius;
        }
        #endregion

        #region 检查是否与另一个包围球相交 —— bool Intersects(BoundingSphere other)
        /// <summary>
        /// 检查是否与另一个包围球相交
        /// </summary>
        public bool Intersects(BoundingSphere other)
        {
            float distance = Vector3.Distance(this.Center, other.Center);
            return distance <= this.Radius + other.Radius;
        }
        #endregion

        #region 检查是否与包围盒相交 —— bool Intersects(BoundingBox box)
        /// <summary>
        /// 检查是否与包围盒相交
        /// </summary>
        public bool Intersects(BoundingBox box)
        {
            //找到球心到盒子的最近点
            Vector3 closestPoint = new Vector3(
                Math.Max(box.Minimum.X, Math.Min(this.Center.X, box.Maximum.X)),
                Math.Max(box.Minimum.Y, Math.Min(this.Center.Y, box.Maximum.Y)),
                Math.Max(box.Minimum.Z, Math.Min(this.Center.Z, box.Maximum.Z))
            );

            float distanceSquared = Vector3.DistanceSquared(this.Center, closestPoint);
            return distanceSquared <= this.Radius * this.Radius;
        }
        #endregion

        #region 检查射线是否与包围球相交 —— bool Intersects(Ray ray, out float distance)
        /// <summary>
        /// 检查射线是否与包围球相交
        /// </summary>
        public bool Intersects(Ray ray, out float distance)
        {
            return ray.Intersects(this, out distance);
        }
        #endregion

        #region 检查射线是否与包围球相等 —— bool Equals(BoundingSphere other)
        /// <summary>
        /// 检查射线是否与包围球相等
        /// </summary>
        public bool Equals(BoundingSphere other)
        {
            return this.Center.Equals(other.Center) && this.Radius.Equals(other.Radius);
        }
        #endregion

        #region 合并包围球 —— BoundingSphere Union(BoundingSphere other)
        /// <summary>
        /// 合并包围球
        /// </summary>
        public BoundingSphere Union(BoundingSphere other)
        {
            Vector3 direction = other.Center - this.Center;
            float distance = direction.Length;

            if (distance + other.Radius <= this.Radius)
            {
                return this;
            }

            if (distance + this.Radius <= other.Radius)
            {
                return other;
            }

            //计算新的中心
            float newRadius = (distance + this.Radius + other.Radius) * 0.5f;
            Vector3 newCenter = this.Center + direction * ((newRadius - this.Radius) / distance);

            return new BoundingSphere(newCenter, newRadius);
        }
        #endregion

        #region 扩大包围球以包含点 —— void Expand(Vector3 point)
        /// <summary>
        /// 扩大包围球以包含点
        /// </summary>
        public void Expand(Vector3 point)
        {
            Vector3 direction = point - this.Center;
            float distance = direction.Length;

            if (distance > this.Radius)
            {
                float half = (distance - this.Radius) * 0.5f;
                this._radius += half;
                this._center += direction * (half / distance);
            }
        }
        #endregion

        #region 转换为包围盒 —— BoundingBox ToBoundingBox()
        /// <summary>
        /// 转换为包围盒
        /// </summary>
        public BoundingBox ToBoundingBox()
        {
            Vector3 extent = new Vector3(this.Radius);
            return new BoundingBox(this.Center - extent, this.Center + extent);
        }
        #endregion


        //IEquatable

        #region 是否相等 —— override bool Equals(object obj)
        /// <summary>
        /// 是否相等
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is BoundingSphere other && this.Equals(other);
        }
        #endregion

        #region 获取哈希码 —— override int GetHashCode()
        /// <summary>
        /// 获取哈希码
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Center, this.Radius);
        }
        #endregion

        #region 相等运算符 —— static bool operator ==(BoundingSphere left, BoundingSphere right)
        /// <summary>
        /// 相等运算符
        /// </summary>
        public static bool operator ==(BoundingSphere left, BoundingSphere right)
        {
            return left.Equals(right);
        }
        #endregion

        #region 不等运算符 —— static bool operator !=(BoundingSphere left, BoundingSphere right)
        /// <summary>
        /// 不等运算符
        /// </summary>
        public static bool operator !=(BoundingSphere left, BoundingSphere right)
        {
            return !left.Equals(right);
        }
        #endregion 

        #endregion
    }
}
