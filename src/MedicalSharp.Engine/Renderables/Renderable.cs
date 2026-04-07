using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Engine.Renderables
{
    /// <summary>
    /// 渲染对象
    /// </summary>
    public abstract class Renderable
    {
        #region # 字段及构造器

        /// <summary>
        /// 渲染事件
        /// </summary>
        public event EventHandler<RenderContext> RenderEvent;

        /// <summary>
        /// 包围盒
        /// </summary>
        private BoundingBox? _boundingBox;

        /// <summary>
        /// 包围球
        /// </summary>
        private BoundingSphere? _boundingSphere;

        /// <summary>
        /// 包围盒/包围球是否脏
        /// </summary>
        protected bool _boundingsDirty;

        /// <summary>
        /// 创建渲染对象构造器
        /// </summary>
        protected Renderable()
        {
            this.Transform = new Transform();
        }

        #endregion

        #region # 属性

        #region 变换 —— Transform Transform
        /// <summary>
        /// 变换
        /// </summary>
        public Transform Transform { get; private set; }
        #endregion

        #region 只读属性 - 模型矩阵 —— Matrix4 ModelMatrix
        /// <summary>
        /// 只读属性 - 模型矩阵
        /// </summary>
        public Matrix4 ModelMatrix
        {
            get => this.Transform?.Matrix ?? Matrix4.Identity;
        }
        #endregion

        #region 只读属性 - 包围盒 —— BoundingBox BoundingBox
        /// <summary>
        /// 只读属性 - 包围盒
        /// </summary>
        /// <remarks>局部坐标</remarks>
        public BoundingBox BoundingBox
        {
            get
            {
                if (this._boundingsDirty || !this._boundingBox.HasValue)
                {
                    this._boundingBox = this.CalculateBoundingBox();
                    this._boundingsDirty = false;
                }

                return this._boundingBox.Value;
            }
        }
        #endregion

        #region 只读属性 - 包围球 —— BoundingSphere BoundingSphere
        /// <summary>
        /// 只读属性 - 包围球
        /// </summary>
        /// <remarks>局部坐标</remarks>
        public BoundingSphere BoundingSphere
        {
            get
            {
                if (this._boundingsDirty || !this._boundingSphere.HasValue)
                {
                    this._boundingSphere = this.CalculateBoundingSphere();
                }

                return this._boundingSphere.Value;
            }
        }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 设置变换 —— void SetTransform(Transform transform)
        /// <summary>
        /// 设置变换
        /// </summary>
        public void SetTransform(Transform transform)
        {
            if (transform == null)
            {
                throw new ArgumentNullException(nameof(transform), "变换不可为空！");
            }

            this.Transform = transform;
        }
        #endregion

        #region 渲染事件 —— void OnRender(RenderContext renderContext)
        /// <summary>
        /// 渲染事件
        /// </summary>
        /// <param name="renderContext">渲染上下文</param>
        protected internal void OnRender(RenderContext renderContext)
        {
            this.RenderEvent?.Invoke(this, renderContext);
        }
        #endregion


        //Protected

        #region 计算包围盒 —— abstract BoundingBox CalculateBoundingBox()
        /// <summary>
        /// 计算包围盒
        /// </summary>
        protected abstract BoundingBox CalculateBoundingBox();
        #endregion

        #region 计算包围球 —— virtual BoundingSphere CalculateBoundingSphere()
        /// <summary>
        /// 计算包围球
        /// </summary>
        protected virtual BoundingSphere CalculateBoundingSphere()
        {
            //默认从包围盒生成包围球
            return this.BoundingBox.ToBoundingSphere();
        }
        #endregion

        #region 检测射线相交 —— bool IntersectsRay(Ray ray, out float distance)
        /// <summary>
        /// 检测射线相交
        /// </summary>
        /// <param name="ray">射线（世界空间）</param>
        /// <param name="distance">相交距离</param>
        /// <returns>是否相交</returns>
        /// <remarks>世界空间</remarks>
        public bool IntersectsRay(Ray ray, out float distance)
        {
            distance = 0f;

            //将射线变换到局部空间
            Matrix4 worldToLocal = Matrix4.Invert(this.ModelMatrix);
            Ray localRay = ray.Transform(worldToLocal);

            //先快速检测包围球
            if (!this.BoundingSphere.Intersects(localRay, out float sphereDistance))
            {
                return false;
            }

            //再检测包围盒（更精确）
            if (!this.BoundingBox.Intersects(localRay, out float boxDistance))
            {
                return false;
            }

            distance = Math.Min(sphereDistance, boxDistance);

            return true;
        }
        #endregion

        #region 标记包围盒/包围球为脏 —— void InvalidateBoundings()
        /// <summary>
        /// 标记包围盒/包围球为脏
        /// </summary>
        /// <remarks>当对象几何改变时调用</remarks>
        protected void InvalidateBoundings()
        {
            this._boundingsDirty = true;
            this._boundingBox = null;
            this._boundingSphere = null;
        }
        #endregion

        #endregion
    }
}
