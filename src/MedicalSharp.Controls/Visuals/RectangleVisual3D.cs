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
    /// 矩形3D元素
    /// </summary>
    public class RectangleVisual3D : ShapeVisual3D, IResizable
    {
        #region # 字段及构造器

        /// <summary>
        /// 宽度依赖属性
        /// </summary>
        public new static readonly StyledProperty<float> WidthProperty;

        /// <summary>
        /// 高度依赖属性
        /// </summary>
        public new static readonly StyledProperty<float> HeightProperty;

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
        static RectangleVisual3D()
        {
            WidthProperty = AvaloniaProperty.Register<RectangleVisual3D, float>(nameof(Width), 1.0f);
            HeightProperty = AvaloniaProperty.Register<RectangleVisual3D, float>(nameof(Height), 1.0f);
            CenterProperty = AvaloniaProperty.Register<RectangleVisual3D, Vector3D>(nameof(Center), new Vector3D(0, 0, 0));
            NormalProperty = AvaloniaProperty.Register<RectangleVisual3D, Vector3D>(nameof(Normal), new Vector3D(0, 0, 1));

            //属性改变事件
            WidthProperty.Changed.AddClassHandler<RectangleVisual3D, float>(OnWidthChanged);
            HeightProperty.Changed.AddClassHandler<RectangleVisual3D, float>(OnHeightChanged);
            CenterProperty.Changed.AddClassHandler<RectangleVisual3D, Vector3D>(OnCenterChanged);
            NormalProperty.Changed.AddClassHandler<RectangleVisual3D, Vector3D>(OnNormalChanged);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public RectangleVisual3D()
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

        #region 依赖属性 - 宽度 —— float Width
        /// <summary>
        /// 依赖属性 - 宽度
        /// </summary>
        public new float Width
        {
            get => this.GetValue(WidthProperty);
            set => this.SetValue(WidthProperty, value);
        }
        #endregion

        #region 依赖属性 - 高度 —— float Height
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
                MeshGeometry strokeMesh = MeshFactory.CreateRectangle(this.Center.ToVector3(), this.Width, this.Height, this.Normal.ToVector3(), GraphicPrimitiveType.Lines);
                MeshGeometry fillMesh = MeshFactory.CreateRectangle(this.Center.ToVector3(), this.Width, this.Height, this.Normal.ToVector3(), GraphicPrimitiveType.Triangles);

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
                MeshGeometry strokeMesh = MeshFactory.CreateRectangle(this.Center.ToVector3(), this.Width, this.Height, this.Normal.ToVector3(), GraphicPrimitiveType.Lines);
                MeshGeometry fillMesh = MeshFactory.CreateRectangle(this.Center.ToVector3(), this.Width, this.Height, this.Normal.ToVector3(), GraphicPrimitiveType.Triangles);

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

            float halfW = this.Width * 0.5f;
            float halfH = this.Height * 0.5f;
            Vector3 center = this.Center.ToVector3();

            //构造四个边所在的平面（垂直于U或V，经过边的中点）
            (HitFace face, Vector3 point, Vector3 normal)[] planes =
            [
                (face: HitFace.Right,  point: center + this.UAxis * halfW, normal:  this.UAxis),
                (face: HitFace.Left,   point: center - this.UAxis * halfW, normal: -this.UAxis),
                (face: HitFace.Top,    point: center + this.VAxis * halfH, normal:  this.VAxis),
                (face: HitFace.Bottom, point: center - this.VAxis * halfH, normal: -this.VAxis)
            ];

            HitFace nearestFace = HitFace.None;
            float nearestDistance = float.MaxValue;
            foreach ((HitFace face, Vector3 point, Vector3 normal) in planes)
            {
                if (localRay.IntersectsPlane(point, normal, out Vector3 hitPoint, out float distance))
                {
                    //检查交点是否在矩形面的范围内
                    Vector3 localHit = hitPoint - center;
                    float u = Vector3.Dot(localHit, this.UAxis);
                    float v = Vector3.Dot(localHit, this.VAxis);

                    bool inBounds = (face == HitFace.Right || face == HitFace.Left)
                        ? Math.Abs(v) <= halfH + 0.1f
                        : Math.Abs(u) <= halfW + 0.1f;
                    if (inBounds && distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestFace = face;
                    }
                }
            }

            //根据命中面构造ResizeContext
            switch (nearestFace)
            {
                case HitFace.Right:
                    resizeContext.Anchor = center;
                    resizeContext.Axis = this.UAxis;
                    resizeContext.CurrentValue = halfW;
                    return true;

                case HitFace.Left:
                    resizeContext.Anchor = center;
                    resizeContext.Axis = -this.UAxis;
                    resizeContext.CurrentValue = halfW;
                    return true;

                case HitFace.Top:
                    resizeContext.Anchor = center;
                    resizeContext.Axis = this.VAxis;
                    resizeContext.CurrentValue = halfH;
                    return true;

                case HitFace.Bottom:
                    resizeContext.Anchor = center;
                    resizeContext.Axis = -this.VAxis;
                    resizeContext.CurrentValue = halfH;
                    return true;
            }

            return false;
        }
        #endregion

        #region 适用调整尺寸 —— void ApplyResize(ResizeContext resizeContext, Vector3 hitPoint)
        /// <summary>
        /// 适用调整尺寸
        /// </summary>
        /// <param name="resizeContext">调整尺寸上下文</param>
        /// <param name="hitPoint">命中点</param>
        public void ApplyResize(ResizeContext resizeContext, Vector3 hitPoint)
        {
            Vector3 delta = hitPoint - resizeContext.Anchor;
            float newHalf = Math.Abs(Vector3.Dot(delta, resizeContext.Axis));
            newHalf = Math.Max(newHalf, 0.01f);

            float dotU = Math.Abs(Vector3.Dot(resizeContext.Axis, this.UAxis));
            float dotV = Math.Abs(Vector3.Dot(resizeContext.Axis, this.VAxis));
            if (dotU > 0.99f)
            {
                this.Width = newHalf * 2f;
            }
            else if (dotV > 0.99f)
            {
                this.Height = newHalf * 2f;
            }
        }
        #endregion


        //Events

        #region 宽度改变事件 —— static void OnWidthChanged(RectangleVisual3D visual3D...
        /// <summary>
        /// 宽度改变事件
        /// </summary>
        private static void OnWidthChanged(RectangleVisual3D visual3D, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 高度改变事件 —— static void OnHeightChanged(RectangleVisual3D visual3D...
        /// <summary>
        /// 高度改变事件
        /// </summary>
        private static void OnHeightChanged(RectangleVisual3D visual3D, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 中心位置改变事件 —— static void OnCenterChanged(RectangleVisual3D visual3D...
        /// <summary>
        /// 中心位置改变事件
        /// </summary>
        private static void OnCenterChanged(RectangleVisual3D visual3D, AvaloniaPropertyChangedEventArgs<Vector3D> eventArgs)
        {
            visual3D.UpdateRenderable();
        }
        #endregion

        #region 法向量改变事件 —— static void OnNormalChanged(RectangleVisual3D visual3D...
        /// <summary>
        /// 法向量改变事件
        /// </summary>
        private static void OnNormalChanged(RectangleVisual3D visual3D, AvaloniaPropertyChangedEventArgs<Vector3D> eventArgs)
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
