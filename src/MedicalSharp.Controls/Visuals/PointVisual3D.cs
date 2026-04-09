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
    public class PointVisual3D : BoundingVisual3D
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
        /// 线框渲染对象
        /// </summary>
        private WireframeRenderable _renderable;

        /// <summary>
        /// 默认构造器
        /// </summary>
        public PointVisual3D()
        {
            this._renderable = null;
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

        #region 只读属性 - 线框渲染对象 —— override WireframeRenderable Renderable
        /// <summary>
        /// 只读属性 - 线框渲染对象
        /// </summary>
        public override WireframeRenderable Renderable
        {
            get
            {
                if (this._renderable == null)
                {
                    MeshGeometry strokeMesh = MeshFactory.CreatePoint(this.Position.ToVector3());
                    MeshGeometry fillMesh = MeshFactory.CreatePoint(this.Position.ToVector3());
                    this._renderable = new WireframeRenderable(strokeMesh, fillMesh);
                    this._renderable.SetColor(this.Stroke.ToVector4(), this.StrokeThickness, this.Fill.ToVector4());
                }

                return this._renderable;
            }
        }
        #endregion

        #endregion

        #region # 方法

        #region 更新渲染对象 —— void UpdateRenderable()
        /// <summary>
        /// 更新渲染对象
        /// </summary>
        private void UpdateRenderable()
        {
            if (this._renderable != null)
            {
                MeshGeometry strokeMesh = MeshFactory.CreatePoint(this.Position.ToVector3());
                MeshGeometry fillMesh = MeshFactory.CreatePoint(this.Position.ToVector3());
                this._renderable.Update(strokeMesh, fillMesh);
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
