using Avalonia;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Primitives.Builders;
using MedicalSharp.Primitives.Models;

namespace MedicalSharp.Controls.Visuals
{
    /// <summary>
    /// 球体3D元素
    /// </summary>
    public class SphereVisual3D : ShapeVisual3D
    {
        #region # 字段及构造器

        /// <summary>
        /// 半径依赖属性
        /// </summary>
        public static readonly StyledProperty<float> RadiusProperty;

        /// <summary>
        /// 中心位置依赖属性
        /// </summary>
        public static readonly StyledProperty<Vector3D> CenterProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static SphereVisual3D()
        {
            RadiusProperty = AvaloniaProperty.Register<SphereVisual3D, float>(nameof(Radius), 1.0f);
            CenterProperty = AvaloniaProperty.Register<SphereVisual3D, Vector3D>(nameof(Center), new Vector3D(0, 0, 0));

            //属性改变事件
            RadiusProperty.Changed.AddClassHandler<SphereVisual3D, float>(OnRadiusChanged);
            CenterProperty.Changed.AddClassHandler<SphereVisual3D, Vector3D>(OnCenterChanged);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public SphereVisual3D()
        {

        }

        #endregion

        #region # 属性

        #region 依赖属性 - 半径 —— float Radius
        /// <summary>
        /// 依赖属性 - 半径
        /// </summary>
        public float Radius
        {
            get => this.GetValue(RadiusProperty);
            set => this.SetValue(RadiusProperty, value);
        }
        #endregion

        #region 依赖属性 - 中心位置 —— Vector3D Center
        /// <summary>
        /// 依赖属性 - 中心位置
        /// </summary>
        public Vector3D Center
        {
            get => this.GetValue(CenterProperty);
            set => this.SetValue(CenterProperty, value);
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
                MeshGeometry strokeMesh = MeshFactory.CreateSphere(this.Radius, this.Center.ToVector3());
                MeshGeometry fillMesh = MeshFactory.CreateSphere(this.Radius, this.Center.ToVector3());
                this.Renderable = ShapeRenderable.CreateFull(strokeMesh, fillMesh);
                this.Renderable.SetStroke(this.Stroke.ToVector4(), this.StrokeThickness);
                this.Renderable.SetFill(this.Fill.ToVector4());
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
                MeshGeometry strokeMesh = MeshFactory.CreateSphere(this.Radius, this.Center.ToVector3());
                MeshGeometry fillMesh = MeshFactory.CreateSphere(this.Radius, this.Center.ToVector3());
                this.Renderable.UpdateFull(strokeMesh, fillMesh);
            }
        }
        #endregion

        #region 半径改变事件 —— static void OnRadiusChanged(SphereVisual3D visual3D...
        /// <summary>
        /// 半径改变事件
        /// </summary>
        private static void OnRadiusChanged(SphereVisual3D visual3D, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 中心位置改变事件 —— static void OnCenterChanged(SphereVisual3D visual3D...
        /// <summary>
        /// 中心位置改变事件
        /// </summary>
        private static void OnCenterChanged(SphereVisual3D visual3D, AvaloniaPropertyChangedEventArgs<Vector3D> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #endregion
    }
}
