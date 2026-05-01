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
    /// 圆形3D元素
    /// </summary>
    public class CircleVisual3D : ShapeVisual3D, IVisual2DIn3D, ITranslatable, IRotatable, IResizable, ICutVolume
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
        /// 法向量依赖属性
        /// </summary>
        public static readonly StyledProperty<Vector3D> NormalProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static CircleVisual3D()
        {
            RadiusProperty = AvaloniaProperty.Register<CircleVisual3D, float>(nameof(Radius), 1.0f);
            CenterProperty = AvaloniaProperty.Register<CircleVisual3D, Vector3D>(nameof(Center), new Vector3D(0, 0, 0));
            NormalProperty = AvaloniaProperty.Register<CircleVisual3D, Vector3D>(nameof(Normal), new Vector3D(0, 0, 1));

            //属性改变事件
            RadiusProperty.Changed.AddClassHandler<CircleVisual3D, float>(OnRadiusChanged);
            CenterProperty.Changed.AddClassHandler<CircleVisual3D, Vector3D>(OnCenterChanged);
            NormalProperty.Changed.AddClassHandler<CircleVisual3D, Vector3D>(OnNormalChanged);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public CircleVisual3D()
        {

        }

        #endregion

        #region # 属性

        #region U轴 —— Vector3 UAxis
        /// <summary>
        /// U轴
        /// </summary>
        public Vector3 UAxis { get; private set; }
        #endregion

        #region V轴 —— Vector3 VAxis
        /// <summary>
        /// V轴
        /// </summary>
        public Vector3 VAxis { get; private set; }
        #endregion

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

        #region 依赖属性 - 法向量 —— Vector3D Normal
        /// <summary>
        /// 依赖属性 - 法向量
        /// </summary>
        public Vector3D Normal
        {
            get => this.GetValue(NormalProperty);
            set => this.SetValue(NormalProperty, value);
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
                MeshGeometry strokeMesh = MeshFactory.CreateEllipse(this.Center.ToVector3(), this.Radius * 2, this.Radius * 2, this.Normal.ToVector3(), 64, GraphicPrimitiveType.Lines);
                MeshGeometry fillMesh = MeshFactory.CreateEllipse(this.Center.ToVector3(), this.Radius * 2, this.Radius * 2, this.Normal.ToVector3(), 64, GraphicPrimitiveType.Triangles);

                WildframeRenderable renderable = new WildframeRenderable(strokeMesh, fillMesh);
                renderable.SetWildframe(this.Stroke.ToVector4(), this.StrokeThickness, this.Fill.ToVector4());

                this.Renderable = renderable;
                this.BuildBasis();
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
                MeshGeometry strokeMesh = MeshFactory.CreateEllipse(this.Center.ToVector3(), this.Radius * 2, this.Radius * 2, this.Normal.ToVector3(), 64, GraphicPrimitiveType.Lines);
                MeshGeometry fillMesh = MeshFactory.CreateEllipse(this.Center.ToVector3(), this.Radius * 2, this.Radius * 2, this.Normal.ToVector3(), 64, GraphicPrimitiveType.Triangles);

                WildframeRenderable renderable = (WildframeRenderable)this.Renderable;
                renderable.Update(strokeMesh, fillMesh);
                renderable.SetWildframe(this.Stroke.ToVector4(), this.StrokeThickness, this.Fill.ToVector4());
                this.BuildBasis();
            }
        }
        #endregion

        #region 尝试获取伸缩方向 —— bool TryGetResizeAxis(Ray localRay, out ResizeContext resizeContext)
        /// <summary>
        /// 尝试获取伸缩方向
        /// </summary>
        /// <param name="localRay">射线（局部空间）</param>
        /// <param name="resizeContext">调整尺寸上下文</param>
        /// <returns>是否成功</returns>
        public bool TryGetResizeAxis(Ray localRay, out ResizeContext resizeContext)
        {
            resizeContext = default;
            Vector3 center = this.Center.ToVector3();
            Vector3 normal = Vector3.Cross(this.UAxis, this.VAxis).Normalized();

            //射线与圆所在平面求交
            if (!localRay.IntersectsPlane(center, normal, out Vector3 hitPoint, out _))
            {
                return false;
            }

            //检查是否在圆周附近
            float dist = Vector3.Distance(hitPoint, center);
            if (Math.Abs(dist - this.Radius) > Math.Max(this.Radius * 0.3f, 0.3f))
            {
                return false;
            }

            //计算伸缩方向（径向）
            Vector3 radialDir = Vector3.Normalize(hitPoint - center);

            resizeContext.Anchor = center;
            resizeContext.Axis = radialDir;
            resizeContext.CurrentValue = this.Radius;

            return true;
        }
        #endregion

        #region 适用调整尺寸 —— void ApplyResize(ResizeContext resizeContext, Vector3 localHitPoint)
        /// <summary>
        /// 适用调整尺寸
        /// </summary>
        /// <param name="resizeContext">调整尺寸上下文</param>
        /// <param name="localHitPoint">命中点（局部空间）</param>
        public void ApplyResize(ResizeContext resizeContext, Vector3 localHitPoint)
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
            Vector3 normal = this.Normal.ToVector3();
            Vector3 uAxis = this.UAxis;
            Vector3 vAxis = this.VAxis;
            Matrix4 localToWorld = this.Transform.Matrix;
            renderable.ApplyCircleCut(this.Radius, center, normal, uAxis, vAxis, localToWorld, cutMode, markValue);
            renderable.SyncMarkDataFromGpu();
        }
        #endregion


        //Events

        #region 半径改变事件 —— static void OnRadiusChanged(CircleVisual3D visual3D...
        /// <summary>
        /// 半径改变事件
        /// </summary>
        private static void OnRadiusChanged(CircleVisual3D visual3D, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 中心位置改变事件 —— static void OnCenterChanged(CircleVisual3D visual3D...
        /// <summary>
        /// 中心位置改变事件
        /// </summary>
        private static void OnCenterChanged(CircleVisual3D visual3D, AvaloniaPropertyChangedEventArgs<Vector3D> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 法向量改变事件 —— static void OnNormalChanged(CircleVisual3D visual3D...
        /// <summary>
        /// 法向量改变事件
        /// </summary>
        private static void OnNormalChanged(CircleVisual3D visual3D, AvaloniaPropertyChangedEventArgs<Vector3D> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion


        //Private

        #region 构建UV正交基 —— void BuildBasis()
        /// <summary>
        /// 构建UV正交基
        /// </summary>
        private void BuildBasis()
        {
            Vector3 normal = this.Normal.ToVector3();

            //法向量接近Z轴
            if (Math.Abs(Vector3.Dot(normal, Vector3.UnitZ)) > 0.99f)
            {
                this.UAxis = Vector3.UnitX;
                this.VAxis = Vector3.UnitY;
            }
            //法向量接近Y轴
            else if (Math.Abs(Vector3.Dot(normal, Vector3.UnitY)) > 0.99f)
            {
                this.UAxis = Vector3.UnitX;
                this.VAxis = Vector3.UnitZ;
            }
            //法向量接近X轴
            else if (Math.Abs(Vector3.Dot(normal, Vector3.UnitX)) > 0.99f)
            {
                this.UAxis = Vector3.UnitY;
                this.VAxis = Vector3.UnitZ;
            }
            else
            {
                //如果法线被旋转过，重新构造正交基（保证U在XY平面内优先）
                this.UAxis = Vector3.Normalize(Vector3.Cross(Vector3.UnitZ, normal));
                this.VAxis = Vector3.Normalize(Vector3.Cross(normal, this.UAxis));
            }
        }
        #endregion

        #endregion
    }
}
