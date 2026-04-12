using OpenTK.Mathematics;
using System;
using System.Runtime.InteropServices;

namespace MedicalSharp.Primitives.Maths
{
    /// <summary>
    /// 射线
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Ray : IEquatable<Ray>
    {
        #region # 字段及构造器

        /// <summary>
        /// 起点
        /// </summary>
        private Vector3 _position;

        /// <summary>
        /// 方向
        /// </summary>
        private Vector3 _direction;

        /// <summary>
        /// 创建射线构造器
        /// </summary>
        /// <param name="position">起点</param>
        /// <param name="direction">方向</param>
        public Ray(Vector3 position, Vector3 direction)
        {
            this._position = position;
            this._direction = direction.Normalized();
        }

        #endregion

        #region # 属性

        #region 只读属性 - 起点 —— Vector3 Position
        /// <summary>
        /// 只读属性 - 起点
        /// </summary>
        public Vector3 Position
        {
            get => this._position;
        }
        #endregion

        #region 只读属性 - 方向 —— Vector3 Direction
        /// <summary>
        /// 只读属性 - 方向
        /// </summary>
        public Vector3 Direction
        {
            get => this._direction;
        }
        #endregion

        #endregion

        #region # 方法

        //Static

        #region 反投影创建射线 —— static Ray UnProject(Vector2 mousePos2D, Vector3 cameraPosition...
        /// <summary>
        /// 反投影创建射线
        /// </summary>
        /// <param name="mousePos2D">鼠标2D位置</param>
        /// <param name="cameraPosition">相机位置</param>
        /// <param name="viewportSize">视口尺寸</param>
        /// <param name="projectionMatrix">投影矩阵</param>
        /// <param name="viewMatrix">视图矩阵</param>
        /// <returns>射线</returns>
        public static Ray UnProject(Vector2 mousePos2D, Vector3 cameraPosition, Vector2 viewportSize, Matrix4 projectionMatrix, Matrix4 viewMatrix)
        {
            //将屏幕坐标转换到NDC（标准化设备坐标）
            float ndcX = (2.0f * mousePos2D.X) / viewportSize.X - 1.0f;
            float ndcY = 1.0f - (2.0f * mousePos2D.Y) / viewportSize.Y;

            //创建近平面和远平面上的点
            Vector4 rayStartNDC = new Vector4(ndcX, ndcY, -1.0f, 1.0f);
            Vector4 rayEndNDC = new Vector4(ndcX, ndcY, 1.0f, 1.0f);

            //转换到相机空间
            Matrix4 invProjection = Matrix4.Invert(projectionMatrix);
            Vector4 rayStartCamera = rayStartNDC * invProjection;
            Vector4 rayEndCamera = rayEndNDC * invProjection;

            //透视除法
            rayStartCamera /= rayStartCamera.W;
            rayEndCamera /= rayEndCamera.W;

            //转换到世界空间
            Matrix4 invView = Matrix4.Invert(viewMatrix);
            Vector3 rayStartWorld = Vector3.TransformPosition(rayStartCamera.Xyz, invView);
            Vector3 rayEndWorld = Vector3.TransformPosition(rayEndCamera.Xyz, invView);

            //计算方向
            Vector3 direction = Vector3.Normalize(rayEndWorld - rayStartWorld);

            //创建射线（使用相机位置作为起点）
            return new Ray(cameraPosition, direction);
        }
        #endregion


        //Public

        #region 获取射线上的点 —— Vector3 GetPoint(float distance)
        /// <summary>
        /// 获取射线上的点
        /// </summary>
        public Vector3 GetPoint(float distance)
        {
            return this._position + this._direction * distance;
        }
        #endregion

        #region 检查是否与包围盒相交 —— bool Intersects(BoundingBox box, out float distance)
        /// <summary>
        /// 检查是否与包围盒相交
        /// </summary>
        public bool Intersects(BoundingBox box, out float distance)
        {
            distance = 0f;

            //使用slab方法进行射线-AABB相交检测
            float tMin = 0f;
            float tMax = float.MaxValue;

            for (int i = 0; i < 3; i++)
            {
                float origin = this._position[i];
                float direction = this._direction[i];
                float min = box.Minimum[i];
                float max = box.Maximum[i];

                if (Math.Abs(direction) < float.Epsilon)
                {
                    //射线平行于该轴
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
        #endregion

        #region 检查是否与包围球相交 —— bool Intersects(BoundingSphere sphere, out float distance)
        /// <summary>
        /// 检查是否与包围球相交
        /// </summary>
        public bool Intersects(BoundingSphere sphere, out float distance)
        {
            distance = 0f;

            Vector3 toSphere = sphere.Center - this._position;
            float rayLength = Vector3.Dot(this._direction, toSphere);
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
        #endregion

        #region 检查是否与三角形相交 —— bool IntersectsTriangle(Vector3 pointA, Vector3 pointB...
        /// <summary>
        /// 检查是否与三角形相交
        /// </summary>
        /// <remarks>Möller–Trumbore算法</remarks>
        public bool IntersectsTriangle(Vector3 pointA, Vector3 pointB, Vector3 pointC, out float distance)
        {
            distance = 0f;
            const float epsilon = 0.0000001f;

            Vector3 edge1 = pointB - pointA;
            Vector3 edge2 = pointC - pointA;
            Vector3 h = Vector3.Cross(this._direction, edge2);
            float a = Vector3.Dot(edge1, h);

            if (a is > -epsilon and < epsilon)
            {
                return false;
            }

            float f = 1.0f / a;
            Vector3 s = this._position - pointA;
            float u = f * Vector3.Dot(s, h);

            if (u < 0.0f || u > 1.0f)
            {
                return false;
            }

            Vector3 q = Vector3.Cross(s, edge1);
            float v = f * Vector3.Dot(this._direction, q);

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
        #endregion

        #region 检查是否与平面相交 —— bool IntersectsPlane(Vector3 planeNormal, float planeDistance...
        /// <summary>
        /// 检查是否与平面相交
        /// </summary>
        public bool IntersectsPlane(Vector3 planeNormal, float planeDistance, out float distance)
        {
            distance = 0f;

            float denominator = Vector3.Dot(planeNormal, this._direction);

            if (Math.Abs(denominator) < float.Epsilon)
            {
                return false;
            }

            float t = (planeDistance - Vector3.Dot(planeNormal, this._position)) / denominator;

            if (t >= 0)
            {
                distance = t;
                return true;
            }

            return false;
        }
        #endregion

        #region 检查是否与平面相交 —— bool IntersectsPlane(Vector3 planePoint, Vector3 planeNormal...
        /// <summary>
        /// 检查是否与平面相交
        /// </summary>
        /// <param name="planePoint">平面上的一点</param>
        /// <param name="planeNormal">平面法向量</param>
        /// <param name="hitPoint">交点坐标</param>
        /// <returns>是否相交</returns>
        public bool IntersectsPlane(Vector3 planePoint, Vector3 planeNormal, out Vector3 hitPoint)
        {
            hitPoint = Vector3.Zero;

            //确保法向量已归一化
            Vector3 normal = planeNormal;
            float normalLength = normal.Length;
            if (normalLength < float.Epsilon)
            {
                return false; // 无效的法向量
            }
            normal /= normalLength;

            //计算射线方向与法向量的点积
            float dotND = Vector3.Dot(normal, this._direction);

            //如果射线与平面平行，则无交点或无穷多个交点
            if (Math.Abs(dotND) < 1e-7f)
            {
                return false;
            }

            //计算从射线起点到平面上点的向量
            Vector3 planeToRayOrigin = planePoint - this._position;

            //计算射线起点到平面的垂直距离（带符号）
            float signedDistance = Vector3.Dot(planeToRayOrigin, normal);

            //计算交点参数 t
            float t = signedDistance / dotND;

            //使用小的容差处理浮点精度问题
            const float tolerance = 1e-5f;
            if (t >= -tolerance)
            {
                //如果t为负数但在容差范围内，则取0
                t = Math.Max(0, t);
                hitPoint = this._position + this._direction * t;

                return true;
            }

            return false;
        }
        #endregion

        #region 检查是否与另一条射线相等 —— bool Equals(Ray other)
        /// <summary>
        /// 检查是否与另一条射线相等
        /// </summary>
        public bool Equals(Ray other)
        {
            return this._position.Equals(other._position) && this._direction.Equals(other._direction);
        }
        #endregion

        #region 计算点到射线的最短距离 —— float CalculateDistanceToPoint(Vector3 point)
        /// <summary>
        /// 计算点到射线的最短距离
        /// </summary>
        public float CalculateDistanceToPoint(Vector3 point)
        {
            Vector3 toPoint = point - this._position;
            float projection = Vector3.Dot(toPoint, this._direction);

            if (projection < 0)
            {
                return toPoint.Length;
            }

            Vector3 projectedPoint = this._position + this._direction * projection;
            return Vector3.Distance(point, projectedPoint);
        }
        #endregion

        #region 计算两条射线的最接近点 —— bool CalculateClosestPoints(Ray other, out Vector3 pointOnThis...
        /// <summary>
        /// 计算两条射线的最接近点
        /// </summary>
        public bool CalculateClosestPoints(Ray other, out Vector3 pointOnThis, out Vector3 pointOnOther)
        {
            pointOnThis = this._position;
            pointOnOther = other._position;

            Vector3 d1 = this._direction;
            Vector3 d2 = other._direction;
            Vector3 r = this._position - other._position;

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
        #endregion

        #region 变换射线 —— Ray Transform(Matrix4 transform)
        /// <summary>
        /// 变换射线
        /// </summary>
        public Ray Transform(Matrix4 transform)
        {
            Vector3 newPosition = Vector3.TransformPosition(this._position, transform);
            Vector3 newDirection = Vector3.TransformNormal(this._direction, transform).Normalized();
            Ray ray = new Ray(newPosition, newDirection);

            return ray;
        }
        #endregion


        //IEquatable

        #region 是否相等 —— override bool Equals(object obj)
        /// <summary>
        /// 是否相等
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is Ray other && this.Equals(other);
        }
        #endregion

        #region 获取哈希码 —— override int GetHashCode()
        /// <summary>
        /// 获取哈希码
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(this._position, this._direction);
        }
        #endregion

        #region 相等运算符 —— static bool operator ==(Ray left, Ray right)
        /// <summary>
        /// 相等运算符
        /// </summary>
        public static bool operator ==(Ray left, Ray right)
        {
            return left.Equals(right);
        }
        #endregion

        #region 不等运算符 —— static bool operator !=(Ray left, Ray right)
        /// <summary>
        /// 不等运算符
        /// </summary>
        public static bool operator !=(Ray left, Ray right)
        {
            return !left.Equals(right);
        }
        #endregion 

        #endregion
    }
}
