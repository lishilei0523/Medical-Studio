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
    /// 圆形3D元素
    /// </summary>
    public class CircleVisual3D : ShapeVisual3D, IVisual2DIn3D, ITranslatable3D, IRotatable, IResizable2D, IResizable3D, IHasPerimeter, IHasSurfaceArea, ICutVolume, IAnalyseVolume2D
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
        static CircleVisual3D()
        {
            RadiusProperty = AvaloniaProperty.Register<CircleVisual3D, float>(nameof(Radius), 1.0f);
            CenterProperty = AvaloniaProperty.Register<CircleVisual3D, Vector3D>(nameof(Center), new Vector3D(0, 0, 0));
            NormalProperty = AvaloniaProperty.Register<CircleVisual3D, Vector3D>(nameof(Normal), new Vector3D(0, 0, 1));
            SegmentsProperty = AvaloniaProperty.Register<CircleVisual3D, int>(nameof(Segments), 64);
            CanRotateProperty = AvaloniaProperty.Register<CircleVisual3D, bool>(nameof(CanRotate), true);
        }


        /// <summary>
        /// 起始位置（UV空间）
        /// </summary>
        private Vector2 _startPos2D;

        /// <summary>
        /// 起始半径
        /// </summary>
        private float _startRadius;

        /// <summary>
        /// 默认构造器
        /// </summary>
        public CircleVisual3D()
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
            MeshGeometry strokeMesh = MeshFactory.CreateEllipse(this.Center.ToVector3(), this.Radius * 2, this.Radius * 2, this.Normal.ToVector3(), this.Segments, GraphicPrimitiveType.Lines);
            MeshGeometry fillMesh = MeshFactory.CreateEllipse(this.Center.ToVector3(), this.Radius * 2, this.Radius * 2, this.Normal.ToVector3(), this.Segments, GraphicPrimitiveType.Triangles);
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
            this._startRadius = this.Radius;
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

            //取水平或垂直移动的较大值作为半径变化量
            float delta = Math.Max(Math.Abs(deltaX), Math.Abs(deltaY));
            float direction = (deltaX + deltaY) > 0 ? 1 : -1;

            float radius = this._startRadius + delta * direction;
            this.Radius = Math.Max(radius, 0.01f);
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
            Vector3 normal = Vector3.Cross(this.UAxis.ToVector3(), this.VAxis.ToVector3()).Normalized();

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

        #region 计算周长 —— float CalculatePerimeter(VolumeMetadata metadata)
        /// <summary>
        /// 计算周长
        /// </summary>
        /// <param name="metadata">体积元数据</param>
        /// <returns>周长（mm）</returns>
        public float CalculatePerimeter(VolumeMetadata metadata)
        {
            Matrix4 localToWorld = this.Transform.Matrix;

            //圆心
            Vector3 localCenter = this.Center.ToVector3();
            Vector3 worldCenter = Vector3.TransformPosition(localCenter, localToWorld);
            Vector3 mmCenter = worldCenter.ToMillimeterPosition(metadata);

            //圆上一点（沿U轴方向）
            Vector3 localEdge = localCenter + this.UAxis.ToVector3() * this.Radius;
            Vector3 worldEdge = Vector3.TransformPosition(localEdge, localToWorld);
            Vector3 mmEdge = worldEdge.ToMillimeterPosition(metadata);

            //半径 = 圆心到圆上点的距离
            float mmRadius = Vector3.Distance(mmCenter, mmEdge);

            //圆周长 = 2πr
            float perimeter = 2 * MathF.PI * mmRadius;

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
            Matrix4 localToWorld = this.Transform.Matrix;

            //圆心
            Vector3 localCenter = this.Center.ToVector3();
            Vector3 worldCenter = Vector3.TransformPosition(localCenter, localToWorld);
            Vector3 mmCenter = worldCenter.ToMillimeterPosition(metadata);

            //圆上一点（沿U轴方向）
            Vector3 localEdge = localCenter + this.UAxis.ToVector3() * this.Radius;
            Vector3 worldEdge = Vector3.TransformPosition(localEdge, localToWorld);
            Vector3 mmEdge = worldEdge.ToMillimeterPosition(metadata);

            //半径 = 圆心到圆上点的距离
            float mmRadius = Vector3.Distance(mmCenter, mmEdge);

            //圆面积 = πr²
            float area = MathF.PI * mmRadius * mmRadius;

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
            volumeData.ApplyCircleCut(markTexture, this.Radius, center, normal, uAxis, vAxis, localToWorld, cutMode, markValue);
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

            //圆心世界坐标
            Vector3 localCenter = this.Center.ToVector3();
            Vector3 worldCenter = Vector3.TransformPosition(localCenter, this.Transform.Matrix);

            //圆上一点（沿U轴方向）
            Vector3 worldEdge = worldCenter + this.UAxis.ToVector3() * this.Radius;

            //投影到屏幕
            Vector2 screenCenter = viewport.Project(worldCenter);
            Vector2 screenEdge = viewport.Project(worldEdge);

            //屏幕半径
            float screenRadius = Vector2.Distance(screenCenter, screenEdge);

            int viewportWidth = viewport.ViewportSize.Width;
            int viewportHeight = viewport.ViewportSize.Height;
            byte[] layerPixels = viewport.MPRRenderer.RenderStatistic(viewportWidth, viewportHeight, viewport.GlContextHandle);
            StatisticResult result = viewport.VolumeData.ApplyCircleAnalyse(screenCenter, screenRadius, viewportWidth, viewportHeight, viewport.MPRCamera.ZoomFactor, layerPixels, markValue);
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

        #endregion
    }
}
