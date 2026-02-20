using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Engine.ValueTypes
{
    /// <summary>
    /// 射线
    /// </summary>
    public struct Ray : IEquatable<Ray>
    {
        /// <summary>
        /// 射线起点
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// 射线方向
        /// </summary>
        public Vector3 Direction { get; set; }

        public Ray(Vector3 position, Vector3 direction)
        {
            this.Position = position;
            this.Direction = direction.Normalized();
        }

        /// <summary>
        /// 获取射线上的点
        /// </summary>
        public Vector3 GetPoint(float distance)
        {
            return this.Position + this.Direction * distance;
        }

        /// <summary>
        /// 检查是否与BoundingBox相交
        /// </summary>
        public bool Intersects(BoundingBox box, out float distance)
        {
            distance = 0f;

            // 使用slab方法进行射线-AABB相交检测
            float tMin = 0f;
            float tMax = float.MaxValue;

            for (int i = 0; i < 3; i++)
            {
                float origin = this.Position[i];
                float direction = this.Direction[i];
                float min = box.Minimum[i];
                float max = box.Maximum[i];

                if (Math.Abs(direction) < float.Epsilon)
                {
                    // 射线平行于该轴
                    if (origin < min || origin > max)
                    {
                        return false;
                    }
                }
                else
                {
                    float invD = 1.0f / direction;
                    float t1 = (min - origin) * invD;
                    float t2 = (max - origin) * invD;

                    if (t1 > t2)
                    {
                        (t1, t2) = (t2, t1);
                    }

                    tMin = Math.Max(tMin, t1);
                    tMax = Math.Min(tMax, t2);

                    if (tMin > tMax || tMax < 0)
                    {
                        return false;
                    }
                }
            }

            distance = tMin;
            return true;
        }

        /// <summary>
        /// 检查是否与BoundingSphere相交
        /// </summary>
        public bool Intersects(BoundingSphere sphere, out float distance)
        {
            distance = 0f;

            Vector3 toSphere = sphere.Center - this.Position;
            float rayLength = Vector3.Dot(this.Direction, toSphere);
            float closestDistanceSquared = Vector3.Dot(toSphere, toSphere) - rayLength * rayLength;

            float radiusSquared = sphere.Radius * sphere.Radius;

            if (closestDistanceSquared > radiusSquared)
            {
                return false;
            }

            float intersectionDistance = (float)Math.Sqrt(radiusSquared - closestDistanceSquared);
            distance = rayLength - intersectionDistance;

            if (distance < 0)
            {
                distance = rayLength + intersectionDistance;
                if (distance < 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 检查是否与三角形相交 (Möller–Trumbore算法)
        /// </summary>
        public bool IntersectsTriangle(Vector3 v0, Vector3 v1, Vector3 v2, out float distance)
        {
            distance = 0f;
            const float epsilon = 0.0000001f;

            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;
            Vector3 h = Vector3.Cross(this.Direction, edge2);
            float a = Vector3.Dot(edge1, h);

            if (a is > -epsilon and < epsilon)
            {
                return false;
            }

            float f = 1.0f / a;
            Vector3 s = this.Position - v0;
            float u = f * Vector3.Dot(s, h);

            if (u < 0.0f || u > 1.0f)
            {
                return false;
            }

            Vector3 q = Vector3.Cross(s, edge1);
            float v = f * Vector3.Dot(this.Direction, q);

            if (v < 0.0f || u + v > 1.0f)
            {
                return false;
            }

            float t = f * Vector3.Dot(edge2, q);

            if (t > epsilon)
            {
                distance = t;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 检查是否与平面相交
        /// </summary>
        public bool IntersectsPlane(Vector3 planeNormal, float planeDistance, out float distance)
        {
            distance = 0f;

            float denominator = Vector3.Dot(planeNormal, this.Direction);

            if (Math.Abs(denominator) < float.Epsilon)
            {
                return false;
            }

            float t = (planeDistance - Vector3.Dot(planeNormal, this.Position)) / denominator;

            if (t >= 0)
            {
                distance = t;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 计算点到射线的最短距离
        /// </summary>
        public float DistanceToPoint(Vector3 point)
        {
            Vector3 toPoint = point - this.Position;
            float projection = Vector3.Dot(toPoint, this.Direction);

            if (projection < 0)
            {
                return toPoint.Length;
            }

            Vector3 projectedPoint = this.Position + this.Direction * projection;
            return Vector3.Distance(point, projectedPoint);
        }

        /// <summary>
        /// 计算两条射线的最接近点
        /// </summary>
        public bool ClosestPoints(Ray other, out Vector3 pointOnThis, out Vector3 pointOnOther)
        {
            pointOnThis = this.Position;
            pointOnOther = other.Position;

            Vector3 d1 = this.Direction;
            Vector3 d2 = other.Direction;
            Vector3 r = this.Position - other.Position;

            float a = Vector3.Dot(d1, d1);
            float b = Vector3.Dot(d1, d2);
            float c = Vector3.Dot(d2, d2);
            float d = Vector3.Dot(d1, r);
            float e = Vector3.Dot(d2, r);
            float denom = a * c - b * b;

            if (Math.Abs(denom) < float.Epsilon)
            {
                return false; // 射线平行
            }

            float t1 = (b * e - c * d) / denom;
            float t2 = (a * e - b * d) / denom;

            pointOnThis = this.GetPoint(t1);
            pointOnOther = other.GetPoint(t2);

            return true;
        }

        /// <summary>
        /// 变换射线
        /// </summary>
        public Ray Transform(Matrix4 transform)
        {
            Vector3 newPosition = Vector3.TransformPosition(this.Position, transform);
            Vector3 newDirection = Vector3.TransformNormal(this.Direction, transform).Normalized();
            return new Ray(newPosition, newDirection);
        }

        public bool Equals(Ray other)
        {
            return this.Position.Equals(other.Position) && this.Direction.Equals(other.Direction);
        }

        public override bool Equals(object obj)
        {
            return obj is Ray other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Position, this.Direction);
        }

        public static bool operator ==(Ray left, Ray right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Ray left, Ray right)
        {
            return !left.Equals(right);
        }
    }
}
