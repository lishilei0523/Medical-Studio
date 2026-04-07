using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MedicalSharp.Primitives.Maths
{
    /// <summary>
    /// 包围盒
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BoundingBox : IEquatable<BoundingBox>
    {
        #region # 字段及构造器

        /// <summary>
        /// 最小点
        /// </summary>
        private Vector3 _minimum;

        /// <summary>
        /// 最大点
        /// </summary>
        private Vector3 _maximum;

        /// <summary>
        /// 创建包围盒构造器
        /// </summary>
        /// <param name="minimum">最小点</param>
        /// <param name="maximum">最大点</param>
        public BoundingBox(Vector3 minimum, Vector3 maximum)
        {
            this._minimum = minimum;
            this._maximum = maximum;
            this.Corners = new Vector3[8];
            this.UpdateCorners();
        }

        #endregion

        #region # 属性

        #region 角点列表 —— Vector3[] Corners
        /// <summary>
        /// 角点列表
        /// </summary>
        /// <remarks>长度为8</remarks>
        public Vector3[] Corners { get; private set; }
        #endregion

        #region 只读属性 - 最小点 —— Vector3 Minimum
        /// <summary>
        /// 只读属性 - 最小点
        /// </summary>
        public Vector3 Minimum
        {
            get => this._minimum;
        }
        #endregion

        #region 只读属性 - 最大点 —— Vector3 Maximum
        /// <summary>
        /// 只读属性 - 最大点
        /// </summary>
        public Vector3 Maximum
        {
            get => this._maximum;
        }
        #endregion

        #region 只读属性 - 中心点 —— Vector3 Center
        /// <summary>
        /// 只读属性 - 中心点
        /// </summary>
        public Vector3 Center
        {
            get => (this._minimum + this._maximum) * 0.5f;
        }
        #endregion

        #region 只读属性 - 尺寸 —— Vector3 Size
        /// <summary>
        /// 只读属性 - 尺寸
        /// </summary>
        public Vector3 Size
        {
            get => this._maximum - this._minimum;
        }
        #endregion

        #region 只读属性 - 半径 —— float Radius
        /// <summary>
        /// 只读属性 - 半径
        /// </summary>
        public float Radius
        {
            get => Vector3.Distance(this.Center, this._maximum);
        }
        #endregion

        #endregion

        #region # 方法

        //Static

        #region 从点集创建包围盒 —— static BoundingBox FromPoints(IList<Vector3> points)
        /// <summary>
        /// 从点集创建包围盒
        /// </summary>
        public static BoundingBox FromPoints(IList<Vector3> points)
        {
            #region # 验证

            if (points == null || !points.Any())
            {
                throw new ArgumentException("至少需要一个点！");
            }

            #endregion

            Vector3 min = points[0];
            Vector3 max = points[0];
            foreach (Vector3 point in points)
            {
                min = Vector3.ComponentMin(min, point);
                max = Vector3.ComponentMax(max, point);
            }

            return new BoundingBox(min, max);
        }
        #endregion

        #region 从多个包围盒创建包围盒 —— static BoundingBox FromBoxes(IList<BoundingBox> boxes)
        /// <summary>
        /// 从多个包围盒创建包围盒
        /// </summary>
        public static BoundingBox FromBoxes(IList<BoundingBox> boxes)
        {
            #region # 验证

            if (boxes == null || !boxes.Any())
            {
                throw new ArgumentException("至少需要一个包围盒");
            }

            #endregion

            Vector3 min = boxes[0].Minimum;
            Vector3 max = boxes[0].Maximum;
            foreach (BoundingBox box in boxes)
            {
                min = Vector3.ComponentMin(min, box.Minimum);
                max = Vector3.ComponentMax(max, box.Maximum);
            }

            return new BoundingBox(min, max);
        }
        #endregion


        //Public

        #region 设置最小点 —— void SetMinimum(Vector3 minimum)
        /// <summary>
        /// 设置最小点
        /// </summary>
        /// <param name="minimum">最小点</param>
        public void SetMinimum(Vector3 minimum)
        {
            this._minimum = minimum;
            this.UpdateCorners();
        }
        #endregion

        #region 设置最大点 —— void SetMaximum(Vector3 maximum)
        /// <summary>
        /// 设置最大点
        /// </summary>
        /// <param name="maximum">最大点</param>
        public void SetMaximum(Vector3 maximum)
        {
            this._maximum = maximum;
            this.UpdateCorners();
        }
        #endregion

        #region 检查点是否在包围盒内 —— bool Contains(Vector3 point)
        /// <summary>
        /// 检查点是否在包围盒内
        /// </summary>
        public bool Contains(Vector3 point)
        {
            bool contains = point.X >= this._minimum.X && point.X <= this._maximum.X &&
                            point.Y >= this._minimum.Y && point.Y <= this._maximum.Y &&
                            point.Z >= this._minimum.Z && point.Z <= this._maximum.Z;

            return contains;
        }
        #endregion

        #region 检查另一个包围盒是否在包围盒内 —— bool Contains(BoundingBox other)
        /// <summary>
        /// 检查另一个包围盒是否在包围盒内
        /// </summary>
        public bool Contains(BoundingBox other)
        {
            bool contains = other._minimum.X >= this._minimum.X && other._maximum.X <= this._maximum.X &&
                            other._minimum.Y >= this._minimum.Y && other._maximum.Y <= this._maximum.Y &&
                            other._minimum.Z >= this._minimum.Z && other._maximum.Z <= this._maximum.Z;

            return contains;
        }
        #endregion

        #region 检查是否与另一个包围盒相交 —— bool Intersects(BoundingBox other)
        /// <summary>
        /// 检查是否与另一个包围盒相交
        /// </summary>
        public bool Intersects(BoundingBox other)
        {
            bool intersects = !(other._minimum.X > this._maximum.X || other._maximum.X < this._minimum.X ||
                                other._minimum.Y > this._maximum.Y || other._maximum.Y < this._minimum.Y ||
                                other._minimum.Z > this._maximum.Z || other._maximum.Z < this._minimum.Z);

            return intersects;
        }
        #endregion

        #region 检查是否与另一个包围盒相等 —— bool Equals(BoundingBox other)
        /// <summary>
        /// 检查是否与另一个包围盒相等
        /// </summary>
        /// <param name="other">其他包围盒</param>
        /// <returns>是否相等</returns>
        public bool Equals(BoundingBox other)
        {
            return this._minimum.Equals(other._minimum) && this._maximum.Equals(other._maximum);
        }
        #endregion

        #region 检查射线是否与包围盒相交 —— bool Intersects(Ray ray, out float distance)
        /// <summary>
        /// 检查射线是否与包围盒相交
        /// </summary>
        /// <param name="ray">射线</param>
        /// <param name="distance">距离</param>
        /// <returns>是否相交</returns>
        public bool Intersects(Ray ray, out float distance)
        {
            return ray.Intersects(this, out distance);
        }
        #endregion

        #region 合并包围盒 —— BoundingBox Union(BoundingBox other)
        /// <summary>
        /// 合并包围盒
        /// </summary>
        /// <param name="other">其他包围盒</param>
        /// <returns>新包围盒</returns>
        public BoundingBox Union(BoundingBox other)
        {
            Vector3 minimum = Vector3.ComponentMin(this._minimum, other._minimum);
            Vector3 maximum = Vector3.ComponentMax(this._maximum, other._maximum);
            BoundingBox boundingBox = new BoundingBox(minimum, maximum);

            return boundingBox;
        }
        #endregion

        #region 扩大包围盒以包含点 —— void Expand(Vector3 point)
        /// <summary>
        /// 扩大包围盒以包含点
        /// </summary>
        public void Expand(Vector3 point)
        {
            this._minimum = Vector3.ComponentMin(this._minimum, point);
            this._maximum = Vector3.ComponentMax(this._maximum, point);

            this.UpdateCorners();
        }
        #endregion

        #region 转换为包围球 —— BoundingSphere ToBoundingSphere()
        /// <summary>
        /// 转换为包围球
        /// </summary>
        public BoundingSphere ToBoundingSphere()
        {
            return new BoundingSphere(this.Center, this.Radius);
        }
        #endregion


        //IEquatable

        #region 是否相等 —— override bool Equals(object obj)
        /// <summary>
        /// 是否相等
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is BoundingBox other && this.Equals(other);
        }
        #endregion

        #region 获取哈希码 —— override int GetHashCode()
        /// <summary>
        /// 获取哈希码
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(this._minimum, this._maximum);
        }
        #endregion

        #region 相等运算符 —— static bool operator ==(BoundingBox left, BoundingBox right)
        /// <summary>
        /// 相等运算符
        /// </summary>
        public static bool operator ==(BoundingBox left, BoundingBox right)
        {
            return left.Equals(right);
        }
        #endregion

        #region 不等运算符 —— static bool operator !=(BoundingBox left, BoundingBox right)
        /// <summary>
        /// 不等运算符
        /// </summary>
        public static bool operator !=(BoundingBox left, BoundingBox right)
        {
            return !left.Equals(right);
        }
        #endregion


        //Private

        #region 更新角点列表 —— void UpdateCorners()
        /// <summary>
        /// 更新角点列表
        /// </summary>
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
        #endregion 

        #endregion
    }
}
