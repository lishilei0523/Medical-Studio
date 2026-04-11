using Avalonia;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Engine.Renderables;

namespace MedicalSharp.Controls.Visuals
{
    /// <summary>
    /// 线段3D元素
    /// </summary>
    public class LineSegmentVisual3D : ShapeVisual3D
    {
        #region # 字段及构造器

        /// <summary>
        /// 起始点依赖属性
        /// </summary>
        public static readonly StyledProperty<Vector3D> StartPointProperty;

        /// <summary>
        /// 终止点依赖属性
        /// </summary>
        public static readonly StyledProperty<Vector3D> EndPointProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static LineSegmentVisual3D()
        {
            StartPointProperty = AvaloniaProperty.Register<LineSegmentVisual3D, Vector3D>(nameof(StartPoint));
            EndPointProperty = AvaloniaProperty.Register<LineSegmentVisual3D, Vector3D>(nameof(EndPoint));

            //属性改变事件
            StartPointProperty.Changed.AddClassHandler<LineSegmentVisual3D, Vector3D>(OnStartPointChanged);
            EndPointProperty.Changed.AddClassHandler<LineSegmentVisual3D, Vector3D>(OnEndPointChanged);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public LineSegmentVisual3D()
        {

        }

        #endregion

        #region # 属性

        #region 依赖属性 - 起始点 —— Vector3D StartPoint
        /// <summary>
        /// 依赖属性 - 起始点
        /// </summary>
        public Vector3D StartPoint
        {
            get => this.GetValue(StartPointProperty);
            set => this.SetValue(StartPointProperty, value);
        }
        #endregion

        #region 依赖属性 - 终止点 —— Vector3D EndPoint
        /// <summary>
        /// 依赖属性 - 终止点
        /// </summary>
        public Vector3D EndPoint
        {
            get => this.GetValue(EndPointProperty);
            set => this.SetValue(EndPointProperty, value);
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
            if (this.Renderable == null)
            {
                LineSegmentRenderable renderable = new LineSegmentRenderable(this.StartPoint.ToVector3(), this.EndPoint.ToVector3());
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
            if (this.Renderable != null)
            {
                LineSegmentRenderable renderable = (LineSegmentRenderable)this.Renderable;
                renderable.Update(this.StartPoint.ToVector3(), this.EndPoint.ToVector3());
                renderable.SetStroke(this.Stroke.ToVector4(), this.StrokeThickness);
            }
        }
        #endregion

        #region 起始点改变事件 —— static void OnStartPointChanged(LineSegmentVisual3D visual3D...
        /// <summary>
        /// 起始点改变事件
        /// </summary>
        private static void OnStartPointChanged(LineSegmentVisual3D visual3D, AvaloniaPropertyChangedEventArgs<Vector3D> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 终止点改变事件 —— static void OnEndPointChanged(LineSegmentVisual3D visual3D...
        /// <summary>
        /// 终止点改变事件
        /// </summary>
        private static void OnEndPointChanged(LineSegmentVisual3D visual3D, AvaloniaPropertyChangedEventArgs<Vector3D> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #endregion
    }
}
