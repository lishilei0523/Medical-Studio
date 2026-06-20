using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalSharp.Engine.Renderables
{
    /// <summary>
    /// 形状组渲染对象
    /// </summary>
    public class ShapeGroupRenderable : ShapeRenderable
    {
        #region # 字段及构造器

        /// <summary>
        /// 形状列表
        /// </summary>
        private readonly HashSet<ShapeRenderable> _items;

        /// <summary>
        /// 默认构造器
        /// </summary>
        public ShapeGroupRenderable()
        {
            this._items = [];
        }

        #endregion

        #region # 属性

        #region 只读属性 - 形状列表 —— IReadOnlySet<ShapeRenderable> Items
        /// <summary>
        /// 只读属性 - 形状列表
        /// </summary>
        public IReadOnlySet<ShapeRenderable> Items
        {
            get => this._items;
        }
        #endregion

        #endregion

        #region # 方法

        #region 追加形状 —— void AppendItem(ShapeRenderable renderable)
        /// <summary>
        /// 追加形状
        /// </summary>
        /// <param name="renderable">渲染对象</param>
        public void AppendItem(ShapeRenderable renderable)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable), "形状渲染对象不可为空！");
            }

            this._items.Add(renderable);
        }
        #endregion

        #region 删除形状 —— void RemoveItem(ShapeRenderable renderable)
        /// <summary>
        /// 删除形状
        /// </summary>
        /// <param name="renderable">形状</param>
        public void RemoveItem(ShapeRenderable renderable)
        {
            if (renderable == null)
            {
                return;
            }

            this._items.Remove(renderable);
        }
        #endregion

        #region 清空形状 —— void ClearItems()
        /// <summary>
        /// 清空形状
        /// </summary>
        public void ClearItems()
        {
            this._items.Clear();
        }
        #endregion

        #region 渲染 —— override void Render(ShaderProgram program, RenderContext3D context)
        /// <summary>
        /// 渲染
        /// </summary>
        /// <param name="program">Shader程序</param>
        /// <param name="context">渲染上下文</param>
        public override void Render(ShaderProgram program, RenderContext3D context)
        {
            foreach (ShapeRenderable item in this.Items)
            {
                item.Transform.SetMatrix(this.Transform.Matrix);
                item.Render(program, context);
            }
        }
        #endregion

        #region 检测射线相交 —— override bool IntersectsRay(Ray ray, out float distance)
        /// <summary>
        /// 检测射线相交
        /// </summary>
        /// <param name="ray">射线（世界空间）</param>
        /// <param name="distance">相交距离</param>
        /// <returns>是否相交</returns>
        /// <remarks>世界空间</remarks>
        public override bool IntersectsRay(Ray ray, out float distance)
        {
            bool hasHit = false;
            float nearestDistance = float.MaxValue;
            foreach (ShapeRenderable item in this.Items)
            {
                if (item.IntersectsRay(ray, out float itemDistance))
                {
                    hasHit = true;
                    if (itemDistance < nearestDistance)
                    {
                        nearestDistance = itemDistance;
                    }
                }
            }

            distance = nearestDistance;

            return hasHit;
        }
        #endregion

        #region 检测射线相交 —— override bool IntersectsRay(Ray ray, out float distance...
        /// <summary>
        /// 检测射线相交
        /// </summary>
        /// <param name="ray">射线（世界空间）</param>
        /// <param name="distance">相交距离</param>
        /// <param name="hitPoint">命中点坐标</param>
        /// <param name="hitNormal">命中点法向量</param>
        /// <param name="hitTriangleIndex">命中三角形索引</param>
        /// <returns>是否相交</returns>
        public override bool IntersectsRay(Ray ray, out float distance, out Vector3 hitPoint, out Vector3 hitNormal, out int hitTriangleIndex)
        {
            float nearestDistance = float.MaxValue;
            Vector3 nearestHitPoint = Vector3.Zero;
            Vector3 nearestHitNormal = Vector3.Zero;
            int nearestHitTriangleIndex = -1;
            bool hasHit = false;
            foreach (ShapeRenderable item in this.Items)
            {
                if (item.IntersectsRay(ray, out float itemDistance, out Vector3 itemHitPoint, out Vector3 itemHitNormal, out int itemHitTriangleIndex))
                {
                    hasHit = true;
                    if (itemDistance < nearestDistance)
                    {
                        nearestDistance = itemDistance;
                        nearestHitPoint = itemHitPoint;
                        nearestHitNormal = itemHitNormal;
                        nearestHitTriangleIndex = itemHitTriangleIndex;
                    }
                }
            }

            distance = nearestDistance;
            hitPoint = nearestHitPoint;
            hitNormal = nearestHitNormal;
            hitTriangleIndex = nearestHitTriangleIndex;

            return hasHit;
        }
        #endregion

        #region 选中 —— override void Select()
        /// <summary>
        /// 选中
        /// </summary>
        public override void Select()
        {
            foreach (ShapeRenderable item in this.Items)
            {
                item.IsSelected = true;
            }
        }
        #endregion

        #region 取消选中 —— override void Unselect()
        /// <summary>
        /// 取消选中
        /// </summary>
        public override void Unselect()
        {
            foreach (ShapeRenderable item in this.Items)
            {
                item.IsSelected = false;
            }
        }
        #endregion

        #region 释放资源 —— override void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            foreach (ShapeRenderable item in this.Items)
            {
                item.Dispose();
            }

            this._disposed = true;
        }
        #endregion


        //Protected

        #region 计算包围盒 —— override BoundingBox CalculateBoundingBox()
        /// <summary>
        /// 计算包围盒
        /// </summary>
        protected override BoundingBox CalculateBoundingBox()
        {
            #region # 验证

            if (!this.Items.Any())
            {
                return new BoundingBox(Vector3.Zero, Vector3.Zero);
            }

            #endregion

            BoundingBox[] itemsBoxes = this.Items.Select(x => x.BoundingBox).ToArray();
            BoundingBox boundingBox = BoundingBox.FromBoxes(itemsBoxes);

            return boundingBox;
        }
        #endregion

        #endregion
    }
}
