using Avalonia;
using Avalonia.Collections;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Engine.Renderables;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace MedicalSharp.Controls.Visuals
{
    /// <summary>
    /// 点云3D元素
    /// </summary>
    public class PointCloudVisual3D : ShapeVisual3D
    {
        #region # 字段及构造器

        /// <summary>
        /// 位置列表依赖属性
        /// </summary>
        public static readonly StyledProperty<AvaloniaList<Vector3D>> PositionsProperty;

        /// <summary>
        /// 点尺寸依赖属性
        /// </summary>
        public static readonly StyledProperty<float> PointSizeProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static PointCloudVisual3D()
        {
            PositionsProperty = AvaloniaProperty.Register<PointCloudVisual3D, AvaloniaList<Vector3D>>(nameof(Positions), []);
            PointSizeProperty = AvaloniaProperty.Register<PointCloudVisual3D, float>(nameof(PointSize), 2.0f);

            //属性改变事件
            PositionsProperty.Changed.AddClassHandler<PointCloudVisual3D, AvaloniaList<Vector3D>>(OnPositionsChanged);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public PointCloudVisual3D()
        {

        }

        #endregion

        #region # 属性

        #region 依赖属性 - 位置列表 —— AvaloniaList<Vector3D> Positions
        /// <summary>
        /// 依赖属性 - 位置列表
        /// </summary>
        public AvaloniaList<Vector3D> Positions
        {
            get => this.GetValue(PositionsProperty);
            set => this.SetValue(PositionsProperty, value);
        }
        #endregion

        #region 依赖属性 - 点尺寸 —— float PointSize
        /// <summary>
        /// 依赖属性 - 点尺寸
        /// </summary>
        public float PointSize
        {
            get => this.GetValue(PointSizeProperty);
            set => this.SetValue(PointSizeProperty, value);
        }
        #endregion

        #endregion

        #region # 方法

        #region 确保渲染对象 —— override void EnsureRenderable()
        /// <summary>
        /// 确保渲染对象
        /// </summary>
        internal override void EnsureRenderable()
        {
            if (this.Renderable == null && this.Positions != null)
            {
                IEnumerable<Vector3> positions = this.Positions.Select(x => x.ToVector3());
                PointCloudRenderable renderable = new PointCloudRenderable([.. positions]);
                renderable.SetFill(this.Fill.ToVector4(), this.PointSize);

                this.Renderable = renderable;
            }
        }
        #endregion

        #region 更新渲染对象 —— void UpdateRenderable()
        /// <summary>
        /// 更新渲染对象
        /// </summary>
        private void UpdateRenderable()
        {
            if (this.Renderable != null && this.Positions != null)
            {
                IEnumerable<Vector3> positions = this.Positions.Select(x => x.ToVector3());
                PointCloudRenderable renderable = (PointCloudRenderable)this.Renderable;
                renderable.Update([.. positions]);
                renderable.SetFill(this.Fill.ToVector4(), this.PointSize);
            }
        }
        #endregion

        #region 位置列表改变事件 —— static void OnPositionsChanged(PointCloudVisual3D visual3D...
        /// <summary>
        /// 位置列表改变事件
        /// </summary>
        private static void OnPositionsChanged(PointCloudVisual3D visual3D, AvaloniaPropertyChangedEventArgs<AvaloniaList<Vector3D>> eventArgs)
        {
            visual3D.UpdateRenderable();
            if (eventArgs.OldValue.Value != null)
            {
                eventArgs.OldValue.Value.CollectionChanged -= visual3D.OnPositionsItemChanged;
            }
            if (eventArgs.NewValue.Value != null)
            {
                eventArgs.NewValue.Value.CollectionChanged += visual3D.OnPositionsItemChanged;
            }

        }
        #endregion

        #region 位置列表元素改变事件 —— void OnPositionsItemChanged(object sender...
        /// <summary>
        /// 位置列表元素改变事件
        /// </summary>
        private void OnPositionsItemChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            this.UpdateRenderable();
        }
        #endregion

        #endregion
    }
}
