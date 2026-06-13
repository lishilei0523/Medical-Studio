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
    /// 圆柱体3D元素
    /// </summary>
    public class CylinderVisual3D : ShapeVisual3D, IPureVisual3D, ITranslatable3D, IRotatable, IResizable3D, IHasSurfaceArea, IHasVolume, ICutVolume, IAnalyseVolume3D
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
        /// 可否旋转依赖属性
        /// </summary>
        public static readonly StyledProperty<bool> CanRotateProperty;

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
            CanRotateProperty = AvaloniaProperty.Register<CylinderVisual3D, bool>(nameof(CanRotate), true);
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
            MeshGeometry strokeMesh = MeshFactory.CreateCylinder(this.Radius, this.Height, this.Center.ToVector3(), this.Segments, GraphicPrimitiveType.Lines, this.WithCaps);
            MeshGeometry fillMesh = MeshFactory.CreateCylinder(this.Radius, this.Height, this.Center.ToVector3(), this.Segments, GraphicPrimitiveType.Triangles, this.WithCaps);
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
            float halfHeight = this.Height * 0.5f;
            int segments = Math.Max(this.Segments, 16);  //最低16段

            List<Vector3> localHull = new List<Vector3>(segments * 2);
            for (int segment = 0; segment < segments; segment++)
            {
                float angle = 2.0f * MathHelper.Pi * segment / segments;
                float x = radius * MathF.Cos(angle);
                float y = radius * MathF.Sin(angle);

                //上圆环顶点
                localHull.Add(new Vector3(center.X + x, center.Y + y, center.Z + halfHeight));

                //下圆环顶点
                localHull.Add(new Vector3(center.X + x, center.Y + y, center.Z - halfHeight));
            }

            //转换到世界空间（注意：Transform 包含旋转，圆柱轴可能不是Z轴）
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

        #region 适用调整尺寸 —— void ApplyResize(ResizeContext3D resizeContext, Vector3 localHitPoint)
        /// <summary>
        /// 适用调整尺寸
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

        #region 计算表面积 —— float CalculateSurfaceArea(VolumeMetadata metadata)
        /// <summary>
        /// 计算表面积
        /// </summary>
        /// <param name="metadata">体积元数据</param>
        /// <returns>表面积（mm²）</returns>
        public float CalculateSurfaceArea(VolumeMetadata metadata)
        {
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
            IReadOnlyList<Vector3> hull = GetConvexHullPositions();
            if (hull.Count < 4)
            {
                return 0;
            }

            //转换到毫米空间
            Vector3[] mmHull = hull.Select(p => p.ToMillimeterPosition(metadata)).ToArray();

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
            volumeData.ApplyCylinderCut(markTexture, this.Radius, this.Height, center, localToWorld, cutMode, markValue);
        }
        #endregion

        #region 适用统计体积 —— async Task<StatisticResult> ApplyAnalyseVolume(VolumeData volumeData...
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

            //计算圆柱体世界坐标参数
            Vector3 localCenter = this.Center.ToVector3();
            Vector3 worldCenter = Vector3.TransformPosition(localCenter, this.Transform.Matrix);

            //圆柱轴方向（局部空间中沿Z轴，变换到世界空间）
            Vector3 localAxis = Vector3.UnitZ;
            Vector3 worldAxis = Vector3.TransformNormal(localAxis, this.Transform.Matrix).Normalized();

            float worldRadius = this.Radius;
            float worldHeight = this.Height;
            StatisticResult result = await Task.Run(() => volumeData.ApplyCylinderAnalyse(worldCenter, worldAxis, worldRadius, worldHeight, markValue));
            result.SurfaceArea = this.CalculateSurfaceArea(volumeData.Metadata);
            result.Volume = this.CalculateVolume(volumeData.Metadata);

            return result;
        }
        #endregion

        #endregion
    }
}
