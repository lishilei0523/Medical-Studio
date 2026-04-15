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
    /// 折线3D元素
    /// </summary>
    public class PolylineVisual3D : ShapeVisual3D
    {
        #region # 字段及构造器

        /// <summary>
        /// 位置列表依赖属性
        /// </summary>
        public static readonly StyledProperty<AvaloniaList<Vector3D>> PositionsProperty;

        /// <summary>
        /// 点尺寸依赖属性
        /// </summary>
        public static readonly StyledProperty<bool> ClosedProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static PolylineVisual3D()
        {
            PositionsProperty = AvaloniaProperty.Register<PolylineVisual3D, AvaloniaList<Vector3D>>(nameof(Positions), []);
            ClosedProperty = AvaloniaProperty.Register<PolylineVisual3D, bool>(nameof(Closed), false);

            //属性改变事件
            PositionsProperty.Changed.AddClassHandler<PolylineVisual3D, AvaloniaList<Vector3D>>(OnPositionsChanged);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public PolylineVisual3D()
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

        #region 依赖属性 - 是否闭合 —— bool Closed
        /// <summary>
        /// 依赖属性 - 是否闭合
        /// </summary>
        public bool Closed
        {
            get => this.GetValue(ClosedProperty);
            set => this.SetValue(ClosedProperty, value);
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
                PolylineRenderable renderable = new PolylineRenderable([.. positions], this.Closed);
                renderable.SetStroke(this.Stroke.ToVector4(), this.StrokeThickness);

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
                PolylineRenderable renderable = (PolylineRenderable)this.Renderable;
                renderable.Update([.. positions]);
                renderable.SetStroke(this.Stroke.ToVector4(), this.StrokeThickness);
            }
        }
        #endregion

        #region 位置列表改变事件 —— static void OnPositionsChanged(PolylineVisual3D visual3D...
        /// <summary>
        /// 位置列表改变事件
        /// </summary>
        private static void OnPositionsChanged(PolylineVisual3D visual3D, AvaloniaPropertyChangedEventArgs<AvaloniaList<Vector3D>> eventArgs)
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
