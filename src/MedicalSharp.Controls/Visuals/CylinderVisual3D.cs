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
    /// 圆柱体3D元素
    /// </summary>
    public class CylinderVisual3D : ShapeVisual3D, IPureVisual3D, ITranslatable, IRotatable, IResizable3D, ICutVolume
    {
        #region # 字段及构造器

        /// <summary>
        /// 半径依赖属性
        /// </summary>
        public static readonly StyledProperty<float> RadiusProperty;

        /// <summary>
        /// 高度依赖属性
        /// </summary>
        public new static readonly StyledProperty<float> HeightProperty;

        /// <summary>
        /// 中心位置依赖属性
        /// </summary>
        public static readonly StyledProperty<Vector3D> CenterProperty;

        /// <summary>
        /// 细分数量依赖属性
        /// </summary>
        public static readonly StyledProperty<int> SegmentsProperty;

        /// <summary>
        /// 是否封闭顶底盖
        /// </summary>
        public static readonly StyledProperty<bool> WithCapsProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static CylinderVisual3D()
        {
            RadiusProperty = AvaloniaProperty.Register<CylinderVisual3D, float>(nameof(Radius), 0.5f);
            HeightProperty = AvaloniaProperty.Register<CylinderVisual3D, float>(nameof(Height), 1.0f);
            CenterProperty = AvaloniaProperty.Register<CylinderVisual3D, Vector3D>(nameof(Center), new Vector3D(0, 0, 0));
            SegmentsProperty = AvaloniaProperty.Register<CylinderVisual3D, int>(nameof(Segments), 32);
            WithCapsProperty = AvaloniaProperty.Register<CylinderVisual3D, bool>(nameof(WithCaps), true);

            //属性改变事件
            RadiusProperty.Changed.AddClassHandler<CylinderVisual3D, float>(OnRadiusChanged);
            HeightProperty.Changed.AddClassHandler<CylinderVisual3D, float>(OnHeightChanged);
            CenterProperty.Changed.AddClassHandler<CylinderVisual3D, Vector3D>(OnCenterChanged);
            SegmentsProperty.Changed.AddClassHandler<CylinderVisual3D, int>(OnSegmentsChanged);
            WithCapsProperty.Changed.AddClassHandler<CylinderVisual3D, bool>(OnWithCapsChanged);
        }

        /// <summary>
        /// 默认构造器
        /// </summary>
        public CylinderVisual3D()
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

        #region 依赖属性 - 高度 —— new float Height
        /// <summary>
        /// 依赖属性 - 高度
        /// </summary>
        public new float Height
        {
            get => this.GetValue(HeightProperty);
            set => this.SetValue(HeightProperty, value);
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

        #region 依赖属性 - 细分数量 —— int Segments
        /// <summary>
        /// 依赖属性 - 细分数量
        /// </summary>
        /// <remarks>圆周分段数</remarks>
        public int Segments
        {
            get => this.GetValue(SegmentsProperty);
            set => this.SetValue(SegmentsProperty, value);
        }
        #endregion

        #region 依赖属性 - 是否封闭顶底盖 —— bool WithCaps
        /// <summary>
        /// 依赖属性 - 是否封闭顶底盖
        /// </summary>
        public bool WithCaps
        {
            get => this.GetValue(WithCapsProperty);
            set => this.SetValue(WithCapsProperty, value);
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
                MeshGeometry strokeMesh = MeshFactory.CreateCylinder(this.Radius, this.Height, this.Center.ToVector3(), this.Segments, GraphicPrimitiveType.Lines, this.WithCaps);
                MeshGeometry fillMesh = MeshFactory.CreateCylinder(this.Radius, this.Height, this.Center.ToVector3(), this.Segments, GraphicPrimitiveType.Triangles, this.WithCaps);

                WildframeRenderable renderable = new WildframeRenderable(strokeMesh, fillMesh);
                renderable.SetWildframe(this.Stroke.ToVector4(), this.StrokeThickness, this.Fill.ToVector4());

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
                MeshGeometry strokeMesh = MeshFactory.CreateCylinder(this.Radius, this.Height, this.Center.ToVector3(), this.Segments, GraphicPrimitiveType.Lines, this.WithCaps);
                MeshGeometry fillMesh = MeshFactory.CreateCylinder(this.Radius, this.Height, this.Center.ToVector3(), this.Segments, GraphicPrimitiveType.Triangles, this.WithCaps);

                WildframeRenderable renderable = (WildframeRenderable)this.Renderable;
                renderable.Update(strokeMesh, fillMesh);
                renderable.SetWildframe(this.Stroke.ToVector4(), this.StrokeThickness, this.Fill.ToVector4());
            }
        }
        #endregion

        #region 尝试获取伸缩方向 —— bool TryGetResizeAxis(Ray localRay, out ResizeContext resizeContext)
        /// <summary>
        /// 尝试获取伸缩方向
        /// </summary>
        public bool TryGetResizeAxis(Ray localRay, out ResizeContext3D resizeContext)
        {
            resizeContext = default;
            Vector3 center = this.Center.ToVector3();
            float radius = this.Radius;
            float halfH = this.Height * 0.5f;

            //射线与包围盒求交
            if (!localRay.Intersects(this.Bounds, out float distance))
            {
                return false;
            }

            Vector3 hitPoint = localRay.GetPoint(distance);
            Vector3 localHitPoint = hitPoint - center;

            //判断命中位置：侧面、顶面、底面
            bool onSide = Math.Abs(localHitPoint.Z) < halfH - 0.01f;
            bool onTop = Math.Abs(localHitPoint.Z - halfH) < 0.1f;
            bool onBottom = Math.Abs(localHitPoint.Z + halfH) < 0.1f;

            if (onSide)
            {
                //侧面：沿径向方向缩放半径
                Vector3 radialDir = new Vector3(localHitPoint.X, localHitPoint.Y, 0).Normalized();
                resizeContext.Anchor = center;
                resizeContext.Axis = radialDir;
                resizeContext.CurrentValue = radius;

                return true;
            }
            if (onTop)
            {
                //顶面：沿Z轴正向缩放高度
                resizeContext.Anchor = center;
                resizeContext.Axis = Vector3.UnitZ;
                resizeContext.CurrentValue = halfH;

                return true;
            }
            if (onBottom)
            {
                //底面：沿Z轴负向缩放高度
                resizeContext.Anchor = center;
                resizeContext.Axis = -Vector3.UnitZ;
                resizeContext.CurrentValue = halfH;

                return true;
            }

            return false;
        }
        #endregion

        #region 应用调整尺寸 —— void ApplyResize(ResizeContext resizeContext, Vector3 localHitPoint)
        /// <summary>
        /// 应用调整尺寸
        /// </summary>
        public void ApplyResize(ResizeContext3D resizeContext, Vector3 localHitPoint)
        {
            Vector3 delta = localHitPoint - resizeContext.Anchor;
            float dotX = Math.Abs(Vector3.Dot(resizeContext.Axis, Vector3.UnitX));
            float dotY = Math.Abs(Vector3.Dot(resizeContext.Axis, Vector3.UnitY));
            float dotZ = Math.Abs(Vector3.Dot(resizeContext.Axis, Vector3.UnitZ));

            if (dotX > 0.5f || dotY > 0.5f)
            {
                //半径调整
                float newRadius = Vector3.Distance(
                    new Vector3(resizeContext.Anchor.X, resizeContext.Anchor.Y, 0),
                    new Vector3(localHitPoint.X, localHitPoint.Y, 0)
                );
                this.Radius = Math.Max(newRadius, 0.01f);
            }
            else if (dotZ > 0.5f)
            {
                //高度调整
                float newHalfH = Math.Abs(Vector3.Dot(delta, resizeContext.Axis));
                float newHeight = newHalfH * 2.0f;
                this.Height = Math.Max(newHeight, 0.01f);
            }
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
            renderable.ApplyCylinderCut(this.Radius, this.Height, center, localToWorld, cutMode, markValue);
            renderable.SyncMarkDataFromGpu();
        }
        #endregion


        //Events

        #region 半径改变事件 —— static void OnRadiusChanged(CylinderVisual3D visual3D...
        /// <summary>
        /// 半径改变事件
        /// </summary>
        private static void OnRadiusChanged(CylinderVisual3D visual3D, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 高度改变事件 —— static void OnHeightChanged(CylinderVisual3D visual3D...
        /// <summary>
        /// 高度改变事件
        /// </summary>
        private static void OnHeightChanged(CylinderVisual3D visual3D, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 中心位置改变事件 —— static void OnCenterChanged(CylinderVisual3D visual3D...
        /// <summary>
        /// 中心位置改变事件
        /// </summary>
        private static void OnCenterChanged(CylinderVisual3D visual3D, AvaloniaPropertyChangedEventArgs<Vector3D> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 细分数量改变事件 —— static void OnSegmentsChanged(CylinderVisual3D visual3D...
        /// <summary>
        /// 细分数量改变事件
        /// </summary>
        private static void OnSegmentsChanged(CylinderVisual3D visual3D, AvaloniaPropertyChangedEventArgs<int> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 是否封闭顶底盖改变事件 —— static void OnWithCapsChanged(CylinderVisual3D visual3D...
        /// <summary>
        /// 是否封闭顶底盖改变事件
        /// </summary>
        private static void OnWithCapsChanged(CylinderVisual3D visual3D, AvaloniaPropertyChangedEventArgs<bool> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #endregion
    }
}
