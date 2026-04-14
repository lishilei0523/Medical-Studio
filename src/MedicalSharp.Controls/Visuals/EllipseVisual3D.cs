using Avalonia;
using MedicalSharp.Controls.Extensions;
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
    /// 椭圆形3D元素
    /// </summary>
    public class EllipseVisual3D : ShapeVisual3D, IResizable
    {
        #region # 字段及构造器

        /// <summary>
        /// 长轴依赖属性
        /// </summary>
        public static readonly StyledProperty<float> RadiusXProperty;

        /// <summary>
        /// 短轴依赖属性
        /// </summary>
        public static readonly StyledProperty<float> RadiusYProperty;

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
        static EllipseVisual3D()
        {
            RadiusXProperty = AvaloniaProperty.Register<EllipseVisual3D, float>(nameof(RadiusX), 1.0f);
            RadiusYProperty = AvaloniaProperty.Register<EllipseVisual3D, float>(nameof(EllipseVisual3D.RadiusY), 1.0f);
            CenterProperty = AvaloniaProperty.Register<EllipseVisual3D, Vector3D>(nameof(Center), new Vector3D(0, 0, 0));
            NormalProperty = AvaloniaProperty.Register<EllipseVisual3D, Vector3D>(nameof(Normal), new Vector3D(0, 0, 1));

            //属性改变事件
            RadiusXProperty.Changed.AddClassHandler<EllipseVisual3D, float>(EllipseVisual3D.OnRadiusXChanged);
            RadiusYProperty.Changed.AddClassHandler<EllipseVisual3D, float>(EllipseVisual3D.OnRadiusYChanged);
            CenterProperty.Changed.AddClassHandler<EllipseVisual3D, Vector3D>(OnCenterChanged);
            NormalProperty.Changed.AddClassHandler<EllipseVisual3D, Vector3D>(OnNormalChanged);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public EllipseVisual3D()
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

        #region 依赖属性 - 长轴 —— float RadiusX
        /// <summary>
        /// 依赖属性 - 长轴
        /// </summary>
        public float RadiusX
        {
            get => this.GetValue(EllipseVisual3D.RadiusXProperty);
            set => this.SetValue(EllipseVisual3D.RadiusXProperty, value);
        }
        #endregion

        #region 依赖属性 - 短轴 —— float RadiusY
        /// <summary>
        /// 依赖属性 - 短轴
        /// </summary>
        public float RadiusY
        {
            get => this.GetValue(EllipseVisual3D.RadiusYProperty);
            set => this.SetValue(EllipseVisual3D.RadiusYProperty, value);
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
                MeshGeometry strokeMesh = MeshFactory.CreateEllipse(this.Center.ToVector3(), this.RadiusX, this.RadiusY, this.Normal.ToVector3(), 64, GraphicPrimitiveType.Lines);
                MeshGeometry fillMesh = MeshFactory.CreateEllipse(this.Center.ToVector3(), this.RadiusX, this.RadiusY, this.Normal.ToVector3(), 64, GraphicPrimitiveType.Triangles);

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
                MeshGeometry strokeMesh = MeshFactory.CreateEllipse(this.Center.ToVector3(), this.RadiusX, this.RadiusY, this.Normal.ToVector3(), 64, GraphicPrimitiveType.Lines);
                MeshGeometry fillMesh = MeshFactory.CreateEllipse(this.Center.ToVector3(), this.RadiusX, this.RadiusY, this.Normal.ToVector3(), 64, GraphicPrimitiveType.Triangles);

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

            (HitFace face, Vector3 point, Vector3 normal)[] planes =
            [
                (HitFace.Right,  center + this.UAxis * this.RadiusX,  this.UAxis),
                (HitFace.Left,   center - this.UAxis * this.RadiusX, -this.UAxis),
                (HitFace.Top,    center + this.VAxis * this.RadiusY,  this.VAxis),
                (HitFace.Bottom, center - this.VAxis * this.RadiusY, -this.VAxis)
            ];

            HitFace nearestFace = HitFace.None;
            float nearestDistance = float.MaxValue;
            foreach ((HitFace face, Vector3 point, Vector3 normal) in planes)
            {
                //过滤背面
                if (Vector3.Dot(localRay.Direction, normal) >= 0)
                {
                    continue;
                }

                if (localRay.IntersectsPlane(point, normal, out Vector3 hitPoint, out float distance))
                {
                    Vector3 localHit = hitPoint - center;
                    float u = Vector3.Dot(localHit, this.UAxis);
                    float v = Vector3.Dot(localHit, this.VAxis);

                    bool inBounds = (face == HitFace.Right || face == HitFace.Left)
                        ? Math.Abs(v) <= this.RadiusY + 0.1f
                        : Math.Abs(u) <= this.RadiusX + 0.1f;
                    if (inBounds && distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestFace = face;
                    }
                }
            }

            switch (nearestFace)
            {
                case HitFace.Right:
                    resizeContext.Anchor = center;
                    resizeContext.Axis = this.UAxis;
                    resizeContext.CurrentValue = this.RadiusX;
                    return true;
                case HitFace.Left:
                    resizeContext.Anchor = center;
                    resizeContext.Axis = -this.UAxis;
                    resizeContext.CurrentValue = this.RadiusX;
                    return true;
                case HitFace.Top:
                    resizeContext.Anchor = center;
                    resizeContext.Axis = this.VAxis;
                    resizeContext.CurrentValue = this.RadiusY;
                    return true;
                case HitFace.Bottom:
                    resizeContext.Anchor = center;
                    resizeContext.Axis = -this.VAxis;
                    resizeContext.CurrentValue = this.RadiusY;
                    return true;
            }

            return false;
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
            Vector3 delta = localHitPoint - resizeContext.Anchor;
            float newValue = Math.Abs(Vector3.Dot(delta, resizeContext.Axis));
            newValue = Math.Max(newValue, 0.01f);

            if (Vector3.Dot(resizeContext.Axis, this.UAxis) > 0.99f)
            {
                this.RadiusX = newValue * 2;
            }
            else
            {
                this.RadiusY = newValue * 2;
            }
        }
        #endregion


        //Events

        #region 长轴改变事件 —— static void OnRadiusXChanged(EllipseVisual3D visual3D...
        /// <summary>
        /// 长轴改变事件
        /// </summary>
        private static void OnRadiusXChanged(EllipseVisual3D visual3D, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 短轴改变事件 —— static void OnRadiusYChanged(EllipseVisual3D visual3D...
        /// <summary>
        /// 短轴改变事件
        /// </summary>
        private static void OnRadiusYChanged(EllipseVisual3D visual3D, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 中心位置改变事件 —— static void OnCenterChanged(EllipseVisual3D visual3D...
        /// <summary>
        /// 中心位置改变事件
        /// </summary>
        private static void OnCenterChanged(EllipseVisual3D visual3D, AvaloniaPropertyChangedEventArgs<Vector3D> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 法向量改变事件 —— static void OnNormalChanged(EllipseVisual3D visual3D...
        /// <summary>
        /// 法向量改变事件
        /// </summary>
        private static void OnNormalChanged(EllipseVisual3D visual3D, AvaloniaPropertyChangedEventArgs<Vector3D> eventArgs)
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

            //Z-up下，默认法线是+Z，所以U = X, V = Y
            if (Math.Abs(Vector3.Dot(normal, Vector3.UnitZ)) > 0.99f)
            {
                this.UAxis = Vector3.UnitX;
                this.VAxis = Vector3.UnitY;
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
