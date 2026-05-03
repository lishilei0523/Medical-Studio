using Avalonia;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Engine.Algorithms;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Primitives.Builders;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Controls.Visuals
{
    /// <summary>
    /// 包围球3D元素
    /// </summary>
    public class BoundingSphereVisual3D : ShapeVisual3D, IPureVisual3D, ITranslatable, IRotatable, IResizable3D, ICutVolume
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
        /// 经线数量依赖属性
        /// </summary>
        public static readonly StyledProperty<int> SegmentsProperty;

        /// <summary>
        /// 纬线数量依赖属性
        /// </summary>
        public static readonly StyledProperty<int> RingsProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static BoundingSphereVisual3D()
        {
            RadiusProperty = AvaloniaProperty.Register<BoundingSphereVisual3D, float>(nameof(Radius), 1.0f);
            CenterProperty = AvaloniaProperty.Register<BoundingSphereVisual3D, Vector3D>(nameof(Center), new Vector3D(0, 0, 0));
            SegmentsProperty = AvaloniaProperty.Register<BoundingSphereVisual3D, int>(nameof(Segments), 32);
            RingsProperty = AvaloniaProperty.Register<BoundingSphereVisual3D, int>(nameof(Rings), 16);

            //属性改变事件
            RadiusProperty.Changed.AddClassHandler<BoundingSphereVisual3D, float>(OnRadiusChanged);
            CenterProperty.Changed.AddClassHandler<BoundingSphereVisual3D, Vector3D>(OnCenterChanged);
            SegmentsProperty.Changed.AddClassHandler<BoundingSphereVisual3D, int>(OnSegmentsChanged);
            RingsProperty.Changed.AddClassHandler<BoundingSphereVisual3D, int>(OnRingsChanged);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public BoundingSphereVisual3D()
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

        #region 依赖属性 - 经线数量 —— int Segments
        /// <summary>
        /// 依赖属性 - 经线数量
        /// </summary>
        public int Segments
        {
            get => this.GetValue(SegmentsProperty);
            set => this.SetValue(SegmentsProperty, value);
        }
        #endregion

        #region 依赖属性 - 纬线数量 —— int Rings
        /// <summary>
        /// 依赖属性 - 纬线数量
        /// </summary>
        public int Rings
        {
            get => this.GetValue(RingsProperty);
            set => this.SetValue(RingsProperty, value);
        }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 确保渲染对象 —— override void EnsureRenderable()
        /// <summary>
        /// 确保渲染对象
        /// </summary>
        internal override void EnsureRenderable()
        {
            if (this.Renderable == null)
            {
                Vector3 center = this.Center.ToVector3();
                MeshGeometry strokeMesh = MeshFactory.CreateSphere(this.Radius, center, this.Segments, this.Rings);
                MeshGeometry fillMesh = MeshFactory.CreateSphere(this.Radius, center, this.Segments, this.Rings);

                WildframeRenderable renderable = new WildframeRenderable(strokeMesh, fillMesh);
                renderable.SetWildframe(this.Stroke.ToVector4(), this.StrokeThickness, this.Fill.ToVector4());

                this.Renderable = renderable;
            }
        }
        #endregion

        #region 更新渲染对象 —— override void UpdateRenderable()
        /// <summary>
        /// 更新渲染对象
        /// </summary>
        internal override void UpdateRenderable()
        {
            if (this.Renderable != null)
            {
                Vector3 center = this.Center.ToVector3();
                MeshGeometry strokeMesh = MeshFactory.CreateSphere(this.Radius, center, this.Segments, this.Rings);
                MeshGeometry fillMesh = MeshFactory.CreateSphere(this.Radius, center, this.Segments, this.Rings);

                WildframeRenderable renderable = (WildframeRenderable)this.Renderable;
                renderable.Update(strokeMesh, fillMesh);
                renderable.SetWildframe(this.Stroke.ToVector4(), this.StrokeThickness, this.Fill.ToVector4());
            }
        }
        #endregion

        #region 尝试获取伸缩方向 —— bool TryGetResizeAxis(Ray localRay, out ResizeContext3D resizeContext)
        /// <summary>
        /// 尝试获取伸缩方向
        /// </summary>
        /// <param name="localRay">射线（局部空间）</param>
        /// <param name="resizeContext">调整尺寸上下文</param>
        /// <returns>是否成功</returns>
        public bool TryGetResizeAxis(Ray localRay, out ResizeContext3D resizeContext)
        {
            resizeContext = default;
            Vector3 center = this.Center.ToVector3();

            //射线与球面求交
            BoundingSphere sphere = new BoundingSphere(center, this.Radius);
            if (!localRay.Intersects(sphere, out Vector3 hitPoint, out float distance))
            {
                return false;
            }

            //过滤背面：球面法线与射线方向夹角
            Vector3 normal = Vector3.Normalize(hitPoint - center);
            if (Vector3.Dot(localRay.Direction, normal) >= 0)
            {
                return false;
            }

            resizeContext.Anchor = center;
            resizeContext.Axis = normal;
            resizeContext.CurrentValue = this.Radius;

            return true;
        }
        #endregion

        #region 适用调整尺寸 —— void ApplyResize(ResizeContext3D resizeContext, Vector3 localHitPoint)
        /// <summary>
        /// 适用调整尺寸
        /// </summary>
        /// <param name="resizeContext">调整尺寸上下文</param>
        /// <param name="localHitPoint">命中点（局部空间）</param>
        public void ApplyResize(ResizeContext3D resizeContext, Vector3 localHitPoint)
        {
            float newRadius = Vector3.Distance(resizeContext.Anchor, localHitPoint);
            this.Radius = Math.Max(newRadius, 0.01f);
        }
        #endregion

        #region 适用切割体积 —— void ApplyCutVolume(VolumeRenderable renderable...
        /// <summary>
        /// 适用切割体积
        /// </summary>
        /// <param name="renderable">体积渲染对象</param>
        /// <param name="cutMode">切割模式</param>
        /// <param name="markValue">标记值</param>
        public void ApplyCutVolume(VolumeRenderable renderable, CutMode cutMode, byte markValue)
        {
            Vector3 center = this.Center.ToVector3();
            Matrix4 localToWorld = this.Transform.Matrix;
            renderable.ApplySphereCut(this.Radius, center, localToWorld, cutMode, markValue);
            renderable.SyncMarkDataFromGpu();
        }
        #endregion


        //Events

        #region 半径改变事件 —— static void OnRadiusChanged(BoundingSphereVisual3D visual3D...
        /// <summary>
        /// 半径改变事件
        /// </summary>
        private static void OnRadiusChanged(BoundingSphereVisual3D visual3D, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 中心位置改变事件 —— static void OnCenterChanged(BoundingSphereVisual3D visual3D...
        /// <summary>
        /// 中心位置改变事件
        /// </summary>
        private static void OnCenterChanged(BoundingSphereVisual3D visual3D, AvaloniaPropertyChangedEventArgs<Vector3D> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 经线数量改变事件 —— static void OnSegmentsChanged(BoundingSphereVisual3D visual3D...
        /// <summary>
        /// 经线数量改变事件
        /// </summary>
        private static void OnSegmentsChanged(BoundingSphereVisual3D visual3D, AvaloniaPropertyChangedEventArgs<int> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 纬线数量改变事件 —— static void OnRingsChanged(BoundingSphereVisual3D visual3D...
        /// <summary>
        /// 纬线数量改变事件
        /// </summary>
        private static void OnRingsChanged(BoundingSphereVisual3D visual3D, AvaloniaPropertyChangedEventArgs<int> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #endregion
    }
}
