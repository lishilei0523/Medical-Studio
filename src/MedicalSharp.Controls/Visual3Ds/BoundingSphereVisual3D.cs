using Avalonia;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedicalSharp.Controls.Visual3Ds
{
    /// <summary>
    /// 包围球3D元素
    /// </summary>
    public class BoundingSphereVisual3D : ShapeVisual3D, IPureVisual3D, ITranslatable3D, IRotatable, IResizable3D, IHasSurfaceArea, IHasVolume, ICutVolume, IAnalyseVolume3D
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
        /// 可否旋转依赖属性
        /// </summary>
        public static readonly StyledProperty<bool> CanRotateProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static BoundingSphereVisual3D()
        {
            RadiusProperty = AvaloniaProperty.Register<BoundingSphereVisual3D, float>(nameof(Radius), 1.0f);
            CenterProperty = AvaloniaProperty.Register<BoundingSphereVisual3D, Vector3D>(nameof(Center), new Vector3D(0, 0, 0));
            SegmentsProperty = AvaloniaProperty.Register<BoundingSphereVisual3D, int>(nameof(Segments), 32);
            RingsProperty = AvaloniaProperty.Register<BoundingSphereVisual3D, int>(nameof(Rings), 16);
            CanRotateProperty = AvaloniaProperty.Register<BoundingSphereVisual3D, bool>(nameof(CanRotate), true);
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

        #endregion

        #region # 方法

        #region 确保渲染对象 —— override void EnsureRenderable()
        /// <summary>
        /// 确保渲染对象
        /// </summary>
        internal override void EnsureRenderable()
        {
            Vector3 center = this.Center.ToVector3();
            MeshGeometry strokeMesh = MeshFactory.CreateSphere(this.Radius, center, this.Segments, this.Rings);
            MeshGeometry fillMesh = MeshFactory.CreateSphere(this.Radius, center, this.Segments, this.Rings);
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
        }
        #endregion

        #region 获取凸包位置列表 —— IReadOnlyList<Vector3> GetConvexHullPositions()
        /// <summary>
        /// 获取凸包位置列表
        /// </summary>
        /// <returns>位置列表（世界空间）</returns>
        public IReadOnlyList<Vector3> GetConvexHullPositions()
        {
            Vector3 center = this.Center.ToVector3();
            float radius = this.Radius;
            int segments = Math.Max(this.Segments, 12);  // 最低12段
            int rings = Math.Max(this.Rings, 6);         // 最低6环

            List<Vector3> localHull = [];

            //上极点
            localHull.Add(new Vector3(center.X, center.Y, center.Z + radius));

            //中间环
            for (int ring = 1; ring < rings; ring++)
            {
                float phi = MathHelper.Pi * ring / rings;
                float z = radius * MathF.Cos(phi);
                float r = radius * MathF.Sin(phi);
                for (int segment = 0; segment < segments; segment++)
                {
                    float theta = 2.0f * MathHelper.Pi * segment / segments;
                    float x = r * MathF.Cos(theta);
                    float y = r * MathF.Sin(theta);
                    localHull.Add(new Vector3(center.X + x, center.Y + y, center.Z + z));
                }
            }

            //下极点
            localHull.Add(new Vector3(center.X, center.Y, center.Z - radius));

            //转换到世界空间
            Matrix4 localToWorld = this.Transform.Matrix;
            Vector3[] convexHullPositions = new Vector3[localHull.Count];
            for (int index = 0; index < localHull.Count; index++)
            {
                Vector3 localPosition = localHull[index];
                Vector3 worldPosition = Vector3.TransformPosition(localPosition, localToWorld);
                convexHullPositions[index] = worldPosition;
            }

            return convexHullPositions;
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

        #region 计算表面积 —— float CalculateSurfaceArea(VolumeMetadata metadata)
        /// <summary>
        /// 计算表面积
        /// </summary>
        /// <param name="metadata">体积元数据</param>
        /// <returns>表面积（mm²）</returns>
        public float CalculateSurfaceArea(VolumeMetadata metadata)
        {
            //获取世界空间的凸包顶点
            IReadOnlyList<Vector3> hull = this.GetConvexHullPositions();

            #region # 验证

            if (hull.Count < 4)
            {
                return 0;
            }

            #endregion

            //获取三角形面
            WildframeRenderable renderable = (WildframeRenderable)this.Renderable;
            if (renderable?.Triangles == null)
            {
                return 0;
            }

            float area = 0;
            Matrix4 localToWorld = this.Transform.Matrix;
            foreach (Triangle triangle in renderable.Triangles)
            {
                //局部 -> 世界 -> 毫米
                Vector3 mmA = Vector3.TransformPosition(triangle.PointA, localToWorld).ToMillimeterPosition(metadata);
                Vector3 mmB = Vector3.TransformPosition(triangle.PointB, localToWorld).ToMillimeterPosition(metadata);
                Vector3 mmC = Vector3.TransformPosition(triangle.PointC, localToWorld).ToMillimeterPosition(metadata);

                Vector3 ab = mmB - mmA;
                Vector3 ac = mmC - mmA;
                area += Vector3.Cross(ab, ac).Length;
            }

            area /= 2.0f;

            return area;
        }
        #endregion

        #region 计算体积 —— float CalculateVolume(VolumeMetadata metadata)
        /// <summary>
        /// 计算体积
        /// </summary>
        /// <param name="metadata">体积元数据</param>
        /// <returns>体积（mm³）</returns>
        public float CalculateVolume(VolumeMetadata metadata)
        {
            //获取世界空间的凸包顶点
            IReadOnlyList<Vector3> hull = this.GetConvexHullPositions();

            #region # 验证

            if (hull.Count < 4)
            {
                return 0;
            }

            #endregion

            //转换到毫米空间
            Vector3[] mmHull = hull.Select(position => position.ToMillimeterPosition(metadata)).ToArray();

            //四面体分解法
            float volume = 0;
            Vector3 origin = mmHull[0];
            for (int index = 1; index < mmHull.Length - 1; index++)
            {
                volume += Vector3.Dot(mmHull[index] - origin, Vector3.Cross(mmHull[index + 1] - origin, mmHull[index + 2] - origin));
            }
            volume = Math.Abs(volume) / 6.0f;

            return volume;
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
            Matrix4 localToWorld = this.Transform.Matrix;
            volumeData.ApplySphereCut(markTexture, this.Radius, center, localToWorld, cutMode, markValue);
        }
        #endregion

        #region 适用统计体积 —— async Task<StatisticResult> ApplySphereAnalyse(VolumeData volumeData...
        /// <summary>
        /// 适用统计体积
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="markValue">标记值</param>
        /// <returns>统计结果</returns>
        public async Task<StatisticResult> ApplyAnalyseVolume(VolumeData volumeData, byte? markValue)
        {
            #region # 验证

            if (volumeData == null)
            {
                return default;
            }

            #endregion

            Vector3 localCenter = this.Center.ToVector3();
            Vector3 worldCenter = Vector3.TransformPosition(localCenter, this.Transform.Matrix);
            float worldRadius = this.Radius;

            StatisticResult result = await Task.Run(() => volumeData.ApplySphereAnalyse(worldCenter, worldRadius, markValue));

            return result;
        }
        #endregion

        #endregion
    }
}
