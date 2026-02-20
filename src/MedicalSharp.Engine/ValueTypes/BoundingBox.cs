using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Engine.ValueTypes
{
    /// <summary>
    /// 包围盒
    /// </summary>
    public struct BoundingBox : IEquatable<BoundingBox>
    {
        private Vector3 _minimum;
        private Vector3 _maximum;

        /// <summary>
        /// 获取或设置最小点
        /// </summary>
        public Vector3 Minimum
        {
            get => this._minimum;
            set
            {
                this._minimum = value;
                this.UpdateCorners();
            }
        }

        /// <summary>
        /// 获取或设置最大点
        /// </summary>
        public Vector3 Maximum
        {
            get => this._maximum;
            set
            {
                this._maximum = value;
                this.UpdateCorners();
            }
        }

        /// <summary>
        /// 获取包围盒的中心点
        /// </summary>
        public Vector3 Center => (this._minimum + this._maximum) * 0.5f;

        /// <summary>
        /// 获取包围盒的尺寸
        /// </summary>
        public Vector3 Size => this._maximum - this._minimum;

        /// <summary>
        /// 获取包围盒的八个角点
        /// </summary>
        public Vector3[] Corners { get; private set; }

        public BoundingBox(Vector3 minimum, Vector3 maximum)
        {
            this._minimum = minimum;
            this._maximum = maximum;
            this.Corners = new Vector3[8];
            this.UpdateCorners();
        }

        /// <summary>
        /// 创建包围盒从点集
        /// </summary>
        public static BoundingBox FromPoints(params Vector3[] points)
        {
            if (points == null || points.Length == 0)
            {
                throw new ArgumentException("至少需要一个点");
            }

            Vector3 min = points[0];
            Vector3 max = points[0];

            foreach (Vector3 point in points)
            {
                min = Vector3.ComponentMin(min, point);
                max = Vector3.ComponentMax(max, point);
            }

            return new BoundingBox(min, max);
        }

        /// <summary>
        /// 创建包围盒从多个包围盒
        /// </summary>
        public static BoundingBox FromBoxes(params BoundingBox[] boxes)
        {
            if (boxes == null || boxes.Length == 0)
            {
                throw new ArgumentException("至少需要一个包围盒");
            }

            Vector3 min = boxes[0].Minimum;
            Vector3 max = boxes[0].Maximum;

            foreach (BoundingBox box in boxes)
            {
                min = Vector3.ComponentMin(min, box.Minimum);
                max = Vector3.ComponentMax(max, box.Maximum);
            }

            return new BoundingBox(min, max);
        }

        private void UpdateCorners()
        {
            this.Corners ??= new Vector3[8];
            this.Corners[0] = new Vector3(this._minimum.X, this._minimum.Y, this._minimum.Z);
            this.Corners[1] = new Vector3(this._maximum.X, this._minimum.Y, this._minimum.Z);
            this.Corners[2] = new Vector3(this._maximum.X, this._maximum.Y, this._minimum.Z);
            this.Corners[3] = new Vector3(this._minimum.X, this._maximum.Y, this._minimum.Z);
            this.Corners[4] = new Vector3(this._minimum.X, this._minimum.Y, this._maximum.Z);
            this.Corners[5] = new Vector3(this._maximum.X, this._minimum.Y, this._maximum.Z);
            this.Corners[6] = new Vector3(this._maximum.X, this._maximum.Y, this._maximum.Z);
            this.Corners[7] = new Vector3(this._minimum.X, this._maximum.Y, this._maximum.Z);
        }

        /// <summary>
        /// 检查点是否在包围盒内
        /// </summary>
        public bool Contains(Vector3 point)
        {
            return point.X >= this._minimum.X && point.X <= this._maximum.X &&
                   point.Y >= this._minimum.Y && point.Y <= this._maximum.Y &&
                   point.Z >= this._minimum.Z && point.Z <= this._maximum.Z;
        }

        /// <summary>
        /// 检查另一个包围盒是否完全包含在内
        /// </summary>
        public bool Contains(BoundingBox box)
        {
            return box._minimum.X >= this._minimum.X && box._maximum.X <= this._maximum.X &&
                   box._minimum.Y >= this._minimum.Y && box._maximum.Y <= this._maximum.Y &&
                   box._minimum.Z >= this._minimum.Z && box._maximum.Z <= this._maximum.Z;
        }

        /// <summary>
        /// 检查是否与另一个包围盒相交
        /// </summary>
        public bool Intersects(BoundingBox box)
        {
            return !(box._minimum.X > this._maximum.X || box._maximum.X < this._minimum.X ||
                     box._minimum.Y > this._maximum.Y || box._maximum.Y < this._minimum.Y ||
                     box._minimum.Z > this._maximum.Z || box._maximum.Z < this._minimum.Z);
        }

        /// <summary>
        /// 检查射线是否与包围盒相交
        /// </summary>
        public bool Intersects(Ray ray, out float distance)
        {
            return ray.Intersects(this, out distance);
        }

        /// <summary>
        /// 合并两个包围盒
        /// </summary>
        public BoundingBox Union(BoundingBox other)
        {
            return new BoundingBox(
                Vector3.ComponentMin(this._minimum, other._minimum),
                Vector3.ComponentMax(this._maximum, other._maximum)
            );
        }

        /// <summary>
        /// 扩大包围盒以包含点
        /// </summary>
        public void Expand(Vector3 point)
        {
            this._minimum = Vector3.ComponentMin(this._minimum, point);
            this._maximum = Vector3.ComponentMax(this._maximum, point);
            this.UpdateCorners();
        }

        /// <summary>
        /// 获取包围盒的半径（从中心到角点的距离）
        /// </summary>
        public float Radius()
        {
            return Vector3.Distance(this.Center, this._maximum);
        }

        /// <summary>
        /// 转换为BoundingSphere
        /// </summary>
        public BoundingSphere ToBoundingSphere()
        {
            return new BoundingSphere(this.Center, this.Radius());
        }

        public bool Equals(BoundingBox other)
        {
            return this._minimum.Equals(other._minimum) && this._maximum.Equals(other._maximum);
        }

        public override bool Equals(object obj)
        {
            return obj is BoundingBox other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this._minimum, this._maximum);
        }

        public static bool operator ==(BoundingBox left, BoundingBox right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BoundingBox left, BoundingBox right)
        {
            return !left.Equals(right);
        }
    }
}
