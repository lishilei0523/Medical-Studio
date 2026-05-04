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
    /// 椭圆形3D元素
    /// </summary>
    public class EllipseVisual3D : ShapeVisual3D, IVisual2DIn3D, ITranslatable, IRotatable, IResizable2D, IResizable3D, ICutVolume
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
        /// 细分数量依赖属性
        /// </summary>
        public static readonly StyledProperty<int> SegmentsProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static EllipseVisual3D()
        {
            WidthProperty = AvaloniaProperty.Register<EllipseVisual3D, float>(nameof(Width), 1.0f);
            HeightProperty = AvaloniaProperty.Register<EllipseVisual3D, float>(nameof(Height), 1.0f);
            CenterProperty = AvaloniaProperty.Register<EllipseVisual3D, Vector3D>(nameof(Center), new Vector3D(0, 0, 0));
            NormalProperty = AvaloniaProperty.Register<EllipseVisual3D, Vector3D>(nameof(Normal), new Vector3D(0, 0, 1));
            SegmentsProperty = AvaloniaProperty.Register<EllipseVisual3D, int>(nameof(Segments), 64);
        }


        /// <summary>
        /// 起始位置（UV空间）
        /// </summary>
        private Vector2 _startPos2D;

        /// <summary>
        /// 起始宽度
        /// </summary>
        private float _startWidth;

        /// <summary>
        /// 起始高度
        /// </summary>
        private float _startHeight;

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

        #region 依赖属性 - 宽度 —— new float Width
        /// <summary>
        /// 依赖属性 - 宽度
        /// </summary>
        public new float Width
        {
            get => this.GetValue(WidthProperty);
            set => this.SetValue(WidthProperty, value);
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

        #region 只读属性 - 平面上一点 —— Vector3D PointOnPlane
        /// <summary>
        /// 只读属性 - 平面上一点
        /// </summary>
        public Vector3D PointOnPlane
        {
            get => this.Center;
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
            Vector3 center = this.Center.ToVector3();
            Vector3 normal = this.Normal.ToVector3();
            MeshGeometry strokeMesh = MeshFactory.CreateEllipse(center, this.Width, this.Height, normal, this.Segments, GraphicPrimitiveType.Lines);
            MeshGeometry fillMesh = MeshFactory.CreateEllipse(center, this.Width, this.Height, normal, this.Segments, GraphicPrimitiveType.Triangles);
            if (this.Renderable == null)
            {
                WildframeRenderable renderable = new WildframeRenderable(strokeMesh, fillMesh);
                renderable.SetWildframe(this.Stroke.ToVector4(), this.StrokeThickness, this.Fill.ToVector4());
                this.Renderable = renderable;
            }
            else
            {
                WildframeRenderable renderable = (WildframeRenderable)this.Renderable;
                renderable.Update(strokeMesh, fillMesh);
                renderable.SetWildframe(this.Stroke.ToVector4(), this.StrokeThickness, this.Fill.ToVector4());
            }

            this.BuildBasis();
        }
        #endregion

        #region 克隆 —— override ShapeVisual3D Clone()
        /// <summary>
        /// 克隆
        /// </summary>
        /// <returns>形状副本</returns>
        public override ShapeVisual3D Clone()
        {
            EllipseVisual3D copy = new EllipseVisual3D
            {
                Id = this.Id,
                Stroke = this.Stroke,
                StrokeThickness = this.StrokeThickness,
                Fill = this.Fill,
                UAxis = this.UAxis,
                VAxis = this.VAxis,
                Width = this.Width,
                Height = this.Height,
                Center = this.Center,
                Normal = this.Normal
            };

            return copy;
        }
        #endregion

        #region 复制 —— override void Copy(ShapeVisual3D shapeVisual3D)
        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="shapeVisual3D">形状</param>
        public override void Copy(ShapeVisual3D shapeVisual3D)
        {
            if (shapeVisual3D is EllipseVisual3D shape)
            {
                this.Stroke = shape.Stroke;
                this.StrokeThickness = shape.StrokeThickness;
                this.Fill = shape.Fill;
                this.UAxis = shape.UAxis;
                this.VAxis = shape.VAxis;
                this.Width = shape.Width;
                this.Height = shape.Height;
                this.Center = shape.Center;
                this.Normal = shape.Normal;
                this.Transform.SetMatrix(shape.Transform.Matrix);
            }
        }
        #endregion

        #region 开始调整尺寸 —— void BeginResize(Vector2 startPos2D)
        /// <summary>
        /// 开始调整尺寸
        /// </summary>
        /// <param name="startPos2D">起始位置（UV空间）</param>
        public void BeginResize(Vector2 startPos2D)
        {
            this._startPos2D = startPos2D;
            this._startWidth = this.Width;
            this._startHeight = this.Height;
        }
        #endregion

        #region 适用调整尺寸 —— void ApplyResize(Vector2 currentPos2D)
        /// <summary>
        /// 适用调整尺寸
        /// </summary>
        /// <param name="currentPos2D">当前位置（UV空间）</param>
        public void ApplyResize(Vector2 currentPos2D)
        {
            float deltaX = currentPos2D.X - this._startPos2D.X;
            float deltaY = this._startPos2D.Y - currentPos2D.Y;

            //水平移动 -> 改变宽度
            float width = this._startWidth + deltaX;
            this.Width = Math.Max(width, 0.02f);

            //垂直移动 -> 改变高度
            float height = this._startHeight + deltaY;
            this.Height = Math.Max(height, 0.02f);
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
            float halfW = this.Width * 0.5f;
            float halfH = this.Height * 0.5f;

            (HitFace face, Vector3 point, Vector3 normal)[] planes =
            [
                (HitFace.Right,  center + this.UAxis * halfW,  this.UAxis),
                (HitFace.Left,   center - this.UAxis * halfW, -this.UAxis),
                (HitFace.Top,    center + this.VAxis * halfH,  this.VAxis),
                (HitFace.Bottom, center - this.VAxis * halfH, -this.VAxis)
            ];

            HitFace nearestFace = HitFace.None;
            float nearestDistance = float.MaxValue;

            foreach ((HitFace face, Vector3 point, Vector3 normal) in planes)
            {
                if (localRay.IntersectsPlane(point, normal, out Vector3 hitPoint, out float distance))
                {
                    Vector3 localHit = hitPoint - center;
                    float u = Vector3.Dot(localHit, this.UAxis);
                    float v = Vector3.Dot(localHit, this.VAxis);

                    bool inBounds = (face == HitFace.Right || face == HitFace.Left)
                        ? Math.Abs(v) <= Math.Max(this.Height * 0.5f, 0.5f) + 0.1f  // ✅ 用 Height 的一半
                        : Math.Abs(u) <= Math.Max(this.Width * 0.5f, 0.5f) + 0.1f;  // ✅ 用 Width 的一半
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

        #region 适用调整尺寸 —— void ApplyResize(ResizeContext3D resizeContext, Vector3 localHitPoint)
        /// <summary>
        /// 适用调整尺寸
        /// </summary>
        /// <param name="resizeContext">调整尺寸上下文</param>
        /// <param name="localHitPoint">命中点（局部空间）</param>
        public void ApplyResize(ResizeContext3D resizeContext, Vector3 localHitPoint)
        {
            Vector3 delta = localHitPoint - resizeContext.Anchor;
            float newHalf = Math.Abs(Vector3.Dot(delta, resizeContext.Axis));
            newHalf = Math.Max(newHalf, 0.01f);

            if (Vector3.Dot(resizeContext.Axis, this.UAxis) > 0.99f)
            {
                this.Width = newHalf * 2.0f;
            }
            else
            {
                this.Height = newHalf * 2.0f;
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
            Vector3 normal = this.Normal.ToVector3();
            Vector3 uAxis = this.UAxis;
            Vector3 vAxis = this.VAxis;
            Matrix4 localToWorld = this.Transform.Matrix;
            renderable.ApplyEllipseCut(this.Width, this.Height, center, normal, uAxis, vAxis, localToWorld, cutMode, markValue);
            renderable.SyncMarkDataFromGpu();
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
