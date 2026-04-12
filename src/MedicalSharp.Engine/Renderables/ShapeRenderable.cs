using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Maths;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Engine.Renderables
{
    /// <summary>
    /// 形状渲染对象
    /// </summary>
    public abstract class ShapeRenderable : Renderable, IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 释放标识
        /// </summary>
        protected bool _disposed;

        /// <summary>
        /// 默认构造器
        /// </summary>
        protected ShapeRenderable()
        {
            this._disposed = false;
        }

        #endregion

        #region # 属性

        //

        #endregion

        #region # 方法

        #region 渲染 —— abstract void Render(ShaderProgram program)
        /// <summary>
        /// 渲染
        /// </summary>
        /// <param name="program">Shader程序</param>
        public abstract void Render(ShaderProgram program);
        #endregion

        #region 检测射线相交 —— virtual bool IntersectsRay(Ray ray, out float distance...
        /// <summary>
        /// 检测射线相交
        /// </summary>
        /// <param name="ray">射线（世界空间）</param>
        /// <param name="distance">相交距离</param>
        /// <param name="hitPoint">命中点坐标</param>
        /// <param name="hitNormal">命中点法向量</param>
        /// <param name="hitTriangleIndex">命中三角形索引</param>
        /// <returns>是否相交</returns>
        public virtual bool IntersectsRay(Ray ray, out float distance, out Vector3 hitPoint, out Vector3 hitNormal, out int hitTriangleIndex)
        {
            distance = float.MaxValue;
            hitPoint = Vector3.Zero;
            hitNormal = Vector3.Zero;
            hitTriangleIndex = -1;

            //将射线变换到局部空间
            Matrix4 worldToLocal = Matrix4.Invert(this.ModelMatrix);
            Ray localRay = ray.Transform(worldToLocal);

            //快速剔除：先检测包围盒
            if (!this.BoundingBox.Intersects(localRay, out float boxDistance))
            {
                return false;
            }

            //三角形检测
            if (this is IHasTriangles hasTriangles)
            {
                //遍历所有三角形
                for (int index = 0; index < hasTriangles.Triangles.Count; index++)
                {
                    Triangle triangle = hasTriangles.Triangles[index];

                    //三角形相交检测
                    if (triangle.IntersectRay(localRay, out float t, out float u, out float v))
                    {
                        if (t > 0 && t < distance)
                        {
                            distance = t;
                            hitTriangleIndex = index;

                            //计算局部交点
                            Vector3 localHitPoint = triangle.GetPoint(u, v);

                            //转换到世界空间
                            hitPoint = Vector3.TransformPosition(localHitPoint, this.ModelMatrix);

                            //计算世界空间法线（考虑变换）
                            hitNormal = Vector3.TransformNormal(triangle.Normal, this.ModelMatrix);
                            hitNormal = Vector3.Normalize(hitNormal);
                        }
                    }
                }

                bool hit = hitTriangleIndex >= 0;

                //如果没有命中任何三角形，返回包围盒距离
                if (!hit && boxDistance < float.MaxValue)
                {
                    distance = boxDistance;
                }

                return hit;
            }

            return false;
        }
        #endregion

        #region 释放资源 —— abstract void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public abstract void Dispose();
        #endregion 

        #endregion
    }
}
