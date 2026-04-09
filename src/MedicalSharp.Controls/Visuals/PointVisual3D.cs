using Avalonia;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Primitives.Builders;
using MedicalSharp.Primitives.Models;

namespace MedicalSharp.Controls.Visuals
{
    /// <summary>
    /// 点3D元素
    /// </summary>
    public class PointVisual3D : ShapeVisual3D
    {
        #region # 字段及构造器

        /// <summary>
        /// 位置依赖属性
        /// </summary>
        public static readonly StyledProperty<Vector3D> PositionProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static PointVisual3D()
        {
            PositionProperty = AvaloniaProperty.Register<PointVisual3D, Vector3D>(nameof(Position), new Vector3D(0, 0, 0));

            //属性改变事件
            PositionProperty.Changed.AddClassHandler<PointVisual3D, Vector3D>(OnPositionChanged);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public PointVisual3D()
        {

        }

        #endregion

        #region # 属性

        #region 依赖属性 - 位置 —— Vector3D Position
        /// <summary>
        /// 依赖属性 - 位置
        /// </summary>
        public Vector3D Position
        {
            get => this.GetValue(PositionProperty);
            set => this.SetValue(PositionProperty, value);
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
                MeshGeometry fillMesh = MeshFactory.CreatePoint(this.Position.ToVector3());
                this.Renderable = ShapeRenderable.CreateFill(fillMesh);
                this.Renderable.SetColor(this.Stroke.ToVector4(), this.StrokeThickness, this.Fill.ToVector4());
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
                MeshGeometry strokeMesh = MeshFactory.CreatePoint(this.Position.ToVector3());
                MeshGeometry fillMesh = MeshFactory.CreatePoint(this.Position.ToVector3());
                this.Renderable.UpdateFull(strokeMesh, fillMesh);
            }
        }
        #endregion

        #region 位置改变事件 —— static void OnPositionChanged(PointVisual3D visual3D...
        /// <summary>
        /// 位置改变事件
        /// </summary>
        private static void OnPositionChanged(PointVisual3D visual3D, AvaloniaPropertyChangedEventArgs<Vector3D> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #endregion
    }
}
