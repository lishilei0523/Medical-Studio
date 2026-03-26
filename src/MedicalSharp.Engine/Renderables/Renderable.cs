using MedicalSharp.Engine.Resources;
using MedicalSharp.Engine.ValueTypes;
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
        /// 体积标记是否脏
        /// </summary>
        protected bool _isBoundingVolumesDirty;

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
                if (this._isBoundingVolumesDirty || !this._boundingBox.HasValue)
                {
                    this._boundingBox = this.CalculateLocalBoundingBox();
                    this._isBoundingVolumesDirty = false;
                }

                return this._boundingBox.Value;
            }
        }
        #endregion

        #region 只读属性 - 世界包围盒 —— BoundingBox WorldBoundingBox
        /// <summary>
        /// 只读属性 - 世界包围盒
        /// </summary>
        public BoundingBox WorldBoundingBox
        {
            get => this.CalculateWorldBoundingBox();
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
                if (this._isBoundingVolumesDirty || !this._boundingSphere.HasValue)
                {
                    this._boundingSphere = this.CalculateLocalBoundingSphere();
                }

                return this._boundingSphere.Value;
            }
        }
        #endregion

        #region 只读属性 - 世界包围球 —— BoundingSphere WorldBoundingSphere
        /// <summary>
        /// 只读属性 - 世界包围球
        /// </summary>
        public BoundingSphere WorldBoundingSphere
        {
            get => this.CalculateWolrdBoundingSphere();
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

        #region 计算局部包围盒 —— BoundingBox CalculateLocalBoundingBox()
        /// <summary>
        /// 计算局部包围盒
        /// </summary>
        protected abstract BoundingBox CalculateLocalBoundingBox();
        #endregion

        #region 计算局部包围球 —— BoundingSphere CalculateLocalBoundingSphere()
        /// <summary>
        /// 计算局部包围球
        /// </summary>
        protected virtual BoundingSphere CalculateLocalBoundingSphere()
        {
            //默认从包围盒生成包围球
            return this.BoundingBox.ToBoundingSphere();
        }
        #endregion

        #region 计算世界包围盒 —— BoundingBox CalculateWorldBoundingBox()
        /// <summary>
        /// 计算世界包围盒
        /// </summary>
        protected BoundingBox CalculateWorldBoundingBox()
        {
            //变换所有8个角点到世界空间
            Vector3[] worldCorners = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                worldCorners[i] = Vector3.TransformPosition(this.BoundingBox.Corners[i], this.ModelMatrix);
            }

            //从世界空间角点重新计算包围盒
            return BoundingBox.FromPoints(worldCorners);
        }
        #endregion

        #region 计算世界包围球 —— BoundingSphere CalculateWolrdBoundingSphere()
        /// <summary>
        /// 计算世界包围球
        /// </summary>
        protected BoundingSphere CalculateWolrdBoundingSphere()
        {
            //变换球心到世界空间
            Vector3 worldCenter = Vector3.TransformPosition(this.BoundingSphere.Center, this.ModelMatrix);

            //计算世界空间半径（考虑非均匀缩放）
            Vector3 scale = this.Transform.Matrix.ExtractScale();
            float maxScale = Math.Max(Math.Max(scale.X, scale.Y), scale.Z);
            float worldRadius = this.BoundingSphere.Radius * maxScale;

            return new BoundingSphere(worldCenter, worldRadius);
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

            //先快速检测包围球
            if (!this.WorldBoundingSphere.Intersects(ray, out float sphereDistance))
            {
                return false;
            }

            //再检测包围盒（更精确）
            if (!this.WorldBoundingBox.Intersects(ray, out float boxDistance))
            {
                return false;
            }

            distance = Math.Min(sphereDistance, boxDistance);

            return true;
        }
        #endregion

        #region 使体积标记脏 —— void InvalidateBoundingVolumes()
        /// <summary>
        /// 使体积标记脏
        /// </summary>
        /// <remarks>当对象几何改变时调用</remarks>
        protected void InvalidateBoundingVolumes()
        {
            this._isBoundingVolumesDirty = true;
            this._boundingBox = null;
            this._boundingSphere = null;
        }
        #endregion

        #endregion
    }
}
