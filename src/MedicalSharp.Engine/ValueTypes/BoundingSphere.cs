using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Engine.ValueTypes
{
    /// <summary>
    /// 包围球
    /// </summary>
    public struct BoundingSphere : IEquatable<BoundingSphere>
    {
        /// <summary>
        /// 获取或设置球心
        /// </summary>
        public Vector3 Center { get; set; }

        /// <summary>
        /// 获取或设置半径
        /// </summary>
        public float Radius { get; set; }

        public BoundingSphere(Vector3 center, float radius)
        {
            this.Center = center;
            this.Radius = radius;
        }

        /// <summary>
        /// 从点集创建包围球（使用Ritter算法）
        /// </summary>
        public static BoundingSphere FromPoints(params Vector3[] points)
        {
            if (points == null || points.Length == 0)
            {
                throw new ArgumentException("至少需要一个点");
            }

            // 找到最小和最大点来确定初始中心
            Vector3 min = points[0];
            Vector3 max = points[0];

            foreach (Vector3 point in points)
            {
                min = Vector3.ComponentMin(min, point);
                max = Vector3.ComponentMax(max, point);
            }

            Vector3 center = (min + max) * 0.5f;
            float radius = 0f;

            // 计算初始半径
            foreach (Vector3 point in points)
            {
                float distance = Vector3.Distance(center, point);
                if (distance > radius)
                {
                    radius = distance;
                }
            }

            // Ritter算法优化
            for (int i = 0; i < 2; i++)
            {
                foreach (Vector3 point in points)
                {
                    Vector3 direction = point - center;
                    float distance = direction.Length;

                    if (distance > radius)
                    {
                        // 扩展球体以包含该点
                        float half = (distance - radius) * 0.5f;
                        radius += half;
                        center += direction * (half / distance);
                    }
                }
            }

            return new BoundingSphere(center, radius);
        }

        /// <summary>
        /// 从包围盒创建包围球
        /// </summary>
        public static BoundingSphere FromBox(BoundingBox box)
        {
            return box.ToBoundingSphere();
        }

        /// <summary>
        /// 检查点是否在球内
        /// </summary>
        public bool Contains(Vector3 point)
        {
            return Vector3.DistanceSquared(this.Center, point) <= this.Radius * this.Radius;
        }

        /// <summary>
        /// 检查另一个包围球是否完全包含在内
        /// </summary>
        public bool Contains(BoundingSphere sphere)
        {
            float distance = Vector3.Distance(this.Center, sphere.Center);
            return distance + sphere.Radius <= this.Radius;
        }

        /// <summary>
        /// 检查是否与另一个包围球相交
        /// </summary>
        public bool Intersects(BoundingSphere sphere)
        {
            float distance = Vector3.Distance(this.Center, sphere.Center);
            return distance <= this.Radius + sphere.Radius;
        }

        /// <summary>
        /// 检查是否与包围盒相交
        /// </summary>
        public bool Intersects(BoundingBox box)
        {
            // 找到球心到盒子的最近点
            Vector3 closestPoint = new Vector3(
                Math.Max(box.Minimum.X, Math.Min(this.Center.X, box.Maximum.X)),
                Math.Max(box.Minimum.Y, Math.Min(this.Center.Y, box.Maximum.Y)),
                Math.Max(box.Minimum.Z, Math.Min(this.Center.Z, box.Maximum.Z))
            );

            float distanceSquared = Vector3.DistanceSquared(this.Center, closestPoint);
            return distanceSquared <= this.Radius * this.Radius;
        }

        /// <summary>
        /// 检查射线是否与包围球相交
        /// </summary>
        public bool Intersects(Ray ray, out float distance)
        {
            return ray.Intersects(this, out distance);
        }

        /// <summary>
        /// 合并两个包围球
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

            // 计算新的中心
            float newRadius = (distance + this.Radius + other.Radius) * 0.5f;
            Vector3 newCenter = this.Center + direction * ((newRadius - this.Radius) / distance);

            return new BoundingSphere(newCenter, newRadius);
        }

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
                this.Radius += half;
                this.Center += direction * (half / distance);
            }
        }

        /// <summary>
        /// 转换为BoundingBox
        /// </summary>
        public BoundingBox ToBoundingBox()
        {
            Vector3 extent = new Vector3(this.Radius);
            return new BoundingBox(this.Center - extent, this.Center + extent);
        }

        public bool Equals(BoundingSphere other)
        {
            return this.Center.Equals(other.Center) && this.Radius.Equals(other.Radius);
        }

        public override bool Equals(object obj)
        {
            return obj is BoundingSphere other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Center, this.Radius);
        }

        public static bool operator ==(BoundingSphere left, BoundingSphere right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BoundingSphere left, BoundingSphere right)
        {
            return !left.Equals(right);
        }
    }
}
