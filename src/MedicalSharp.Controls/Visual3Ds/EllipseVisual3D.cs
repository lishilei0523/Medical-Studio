using Avalonia;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Engine.Algorithms;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Builders;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Controls.Visual3Ds
{
    /// <summary>
    /// 椭圆形3D元素
    /// </summary>
    public class EllipseVisual3D : ShapeVisual3D, IVisual2DIn3D, ITranslatable3D, IRotatable, IResizable2D, IResizable3D, IHasPerimeter, IHasSurfaceArea, ICutVolume, IAnalyseVolume2D
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
        /// 可否旋转依赖属性
        /// </summary>
        public static readonly StyledProperty<bool> CanRotateProperty;

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
            CanRotateProperty = AvaloniaProperty.Register<EllipseVisual3D, bool>(nameof(CanRotate), true);
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

        #region U轴 —— Vector3D UAxis
        /// <summary>
        /// U轴
        /// </summary>
        public Vector3D UAxis { get; private set; }
        #endregion

        #region V轴 —— Vector3D VAxis
        /// <summary>
        /// V轴
        /// </summary>
        public Vector3D VAxis { get; private set; }
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

        #region 依赖属性 - 可否旋转 —— bool CanRotate
        /// <summary>
        /// 依赖属性 - 可否旋转
        /// </summary>
        public bool CanRotate
        {
            get => this.GetValue(CanRotateProperty);
            set => this.SetValue(CanRotateProperty, value);
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
            Vector3 uAxis = this.UAxis.ToVector3();
            Vector3 vAxis = this.VAxis.ToVector3();
            float halfW = this.Width * 0.5f;
            float halfH = this.Height * 0.5f;

            (HitFace face, Vector3 point, Vector3 normal)[] planes =
            [
                (HitFace.Right,  center + uAxis * halfW,  uAxis),
                (HitFace.Left,   center - uAxis * halfW, -uAxis),
                (HitFace.Top,    center + vAxis * halfH,  vAxis),
                (HitFace.Bottom, center - vAxis * halfH, -vAxis)
            ];

            HitFace nearestFace = HitFace.None;
            float nearestDistance = float.MaxValue;

            foreach ((HitFace face, Vector3 point, Vector3 normal) in planes)
            {
                if (localRay.IntersectsPlane(point, normal, out Vector3 hitPoint, out float distance))
                {
                    Vector3 localHit = hitPoint - center;
                    float u = Vector3.Dot(localHit, uAxis);
                    float v = Vector3.Dot(localHit, vAxis);

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
                    resizeContext.Axis = uAxis;
                    resizeContext.CurrentValue = halfW;
                    return true;
                case HitFace.Left:
                    resizeContext.Anchor = center;
                    resizeContext.Axis = -uAxis;
                    resizeContext.CurrentValue = halfW;
                    return true;
                case HitFace.Top:
                    resizeContext.Anchor = center;
                    resizeContext.Axis = vAxis;
                    resizeContext.CurrentValue = halfH;
                    return true;
                case HitFace.Bottom:
                    resizeContext.Anchor = center;
                    resizeContext.Axis = -vAxis;
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

            if (Vector3.Dot(resizeContext.Axis, this.UAxis.ToVector3()) > 0.99f)
            {
                this.Width = newHalf * 2.0f;
            }
            else
            {
                this.Height = newHalf * 2.0f;
            }
        }
        #endregion

        #region 计算周长 —— float CalculatePerimeter(VolumeMetadata metadata)
        /// <summary>
        /// 计算周长
        /// </summary>
        /// <param name="metadata">体积元数据</param>
        /// <returns>周长（mm）</returns>
        public float CalculatePerimeter(VolumeMetadata metadata)
        {
            (float a, float b) = this.GetSemiAxesInMillimeters(metadata);
            float perimeter = MathF.PI * (3 * (a + b) - MathF.Sqrt((3 * a + b) * (a + 3 * b)));

            return perimeter;
        }
        #endregion

        #region 计算表面积 —— float CalculateSurfaceArea(VolumeMetadata metadata)
        /// <summary>
        /// 计算表面积
        /// </summary>
        /// <param name="metadata">体积元数据</param>
        /// <returns>表面积（mm²）</returns>
        public float CalculateSurfaceArea(VolumeMetadata metadata)
        {
            (float a, float b) = this.GetSemiAxesInMillimeters(metadata);
            float area = MathF.PI * a * b;

            return area;
        }
        #endregion

        #region 适用切割体积 —— void ApplyCutVolume(VolumeData volumeData...
        /// <summary>
        /// 适用切割体积
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="markTexture">标记纹理</param>
        /// <param name="cutMode">切割模式</param>
        /// <param name="markValue">标记值</param>
        public void ApplyCutVolume(VolumeData volumeData, Texture3D markTexture, CutMode cutMode, byte markValue)
        {
            Vector3 center = this.Center.ToVector3();
            Vector3 normal = this.Normal.ToVector3();
            Vector3 uAxis = this.UAxis.ToVector3();
            Vector3 vAxis = this.VAxis.ToVector3();
            Matrix4 localToWorld = this.Transform.Matrix;
            volumeData.ApplyEllipseCut(markTexture, this.Width, this.Height, center, normal, uAxis, vAxis, localToWorld, cutMode, markValue);
        }
        #endregion

        #region 适用统计体积 —— StatisticResult ApplyAnalyseVolume(MPRViewport viewport...
        /// <summary>
        /// 适用统计体积
        /// </summary>
        /// <param name="viewport">MPR渲染视口</param>
        /// <param name="markValue">标记值</param>
        /// <returns>统计结果</returns>
        public StatisticResult ApplyAnalyseVolume(MPRViewport viewport, byte? markValue)
        {
            #region # 验证

            if (viewport.VolumeData == null)
            {
                return default;
            }
            if (viewport.MPRRenderer == null)
            {
                return default;
            }
            if (viewport.MPRCamera == null)
            {
                return default;
            }
            if (viewport.Plane == null)
            {
                return default;
            }

            #endregion

            //计算世界坐标
            Vector3 localCenter = this.Center.ToVector3();
            Vector3 worldCenter = Vector3.TransformPosition(localCenter, this.Transform.Matrix);
            Vector3 worldUAxis = Vector3.TransformNormal(this.UAxis.ToVector3(), this.Transform.Matrix);
            Vector3 worldVAxis = Vector3.TransformNormal(this.VAxis.ToVector3(), this.Transform.Matrix);

            //投影到屏幕坐标
            Vector2 screenCenter = viewport.Project(worldCenter);

            //计算屏幕上的半轴长度
            Vector3 worldUEdgePos = worldCenter + worldUAxis * this.Width / 2;
            Vector3 worldUEdgeNeg = worldCenter - worldUAxis * this.Width / 2;
            Vector3 worldVEdgePos = worldCenter + worldVAxis * this.Height / 2;
            Vector3 worldVEdgeNeg = worldCenter - worldVAxis * this.Height / 2;
            Vector2 screenUEdgePos = viewport.Project(worldUEdgePos);
            Vector2 screenUEdgeNeg = viewport.Project(worldUEdgeNeg);
            Vector2 screenVEdgePos = viewport.Project(worldVEdgePos);
            Vector2 screenVEdgeNeg = viewport.Project(worldVEdgeNeg);
            float screenHalfWidth = Vector2.Distance(screenUEdgePos, screenUEdgeNeg) / 2;
            float screenHalfHeight = Vector2.Distance(screenVEdgePos, screenVEdgeNeg) / 2;

            int viewportWidth = viewport.ViewportSize.Width;
            int viewportHeight = viewport.ViewportSize.Height;
            byte[] pixels = viewport.MPRRenderer.RenderStatistic(viewportWidth, viewportHeight, viewport.GlContextHandle);
            StatisticResult result = viewport.VolumeData.ApplyEllipseAnalyse(screenCenter, screenHalfWidth, screenHalfHeight, viewportWidth, viewportHeight, viewport.MPRCamera.ZoomFactor, pixels, markValue);
            result.Perimeter = this.CalculatePerimeter(viewport.VolumeData.Metadata);
            result.SurfaceArea = this.CalculateSurfaceArea(viewport.VolumeData.Metadata);

            return result;
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
                this.UAxis = Vector3.UnitX.ToVector3();
                this.VAxis = Vector3.UnitY.ToVector3();
            }
            //法向量接近Y轴
            else if (Math.Abs(Vector3.Dot(normal, Vector3.UnitY)) > 0.99f)
            {
                this.UAxis = Vector3.UnitX.ToVector3();
                this.VAxis = Vector3.UnitZ.ToVector3();
            }
            //法向量接近X轴
            else if (Math.Abs(Vector3.Dot(normal, Vector3.UnitX)) > 0.99f)
            {
                this.UAxis = Vector3.UnitY.ToVector3();
                this.VAxis = Vector3.UnitZ.ToVector3();
            }
            else
            {
                //如果法线被旋转过，重新构造正交基（保证U在XY平面内优先）
                this.UAxis = Vector3.Normalize(Vector3.Cross(Vector3.UnitZ, normal)).ToVector3();
                this.VAxis = Vector3.Normalize(Vector3.Cross(normal, this.UAxis.ToVector3())).ToVector3();
            }
        }
        #endregion

        #region 获取毫米空间半轴长度 —— (float a, float b) GetSemiAxesInMillimeters(VolumeMetadata...
        /// <summary>
        /// 获取毫米空间半轴长度
        /// </summary>
        /// <param name="metadata">体积元数据</param>
        /// <returns>半轴长度</returns>
        /// <remarks>
        /// a为长半轴，b为短半轴，单位mm
        /// 计算流程：
        /// 1、将椭圆中心从局部空间转换到毫米空间；
        /// 2、沿 U 轴（宽度方向）和 V 轴（高度方向）计算边界点；
        /// 3、计算中心到边界点的距离，得到半轴长度；
        /// 4、返回时确保 a ≥ b（长半轴在前）；
        /// </remarks>
        private (float a, float b) GetSemiAxesInMillimeters(VolumeMetadata metadata)
        {
            Matrix4 localToWorld = this.Transform.Matrix;

            Vector3 localCenter = this.Center.ToVector3();
            Vector3 mmCenter = Vector3.TransformPosition(localCenter, localToWorld).ToMillimeterPosition(metadata);

            //U轴方向（宽度）
            Vector3 localUEdge = localCenter + this.UAxis.ToVector3() * this.Width / 2;
            Vector3 mmUEdge = Vector3.TransformPosition(localUEdge, localToWorld).ToMillimeterPosition(metadata);

            //V轴方向（高度）
            Vector3 localVEdge = localCenter + this.VAxis.ToVector3() * this.Height / 2;
            Vector3 mmVEdge = Vector3.TransformPosition(localVEdge, localToWorld).ToMillimeterPosition(metadata);

            //计算半轴长度（中心到边界的距离）
            float a = Vector3.Distance(mmCenter, mmUEdge);
            float b = Vector3.Distance(mmCenter, mmVEdge);

            //确保长半轴在前（拉马努金周长公式要求 a ≥ b）
            (float a, float b) ab = a >= b ? (a, b) : (b, a);

            return ab;
        }
        #endregion

        #endregion
    }
}
