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
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedicalSharp.Controls.Visual3Ds
{
    /// <summary>
    /// 包围盒3D元素
    /// </summary>
    public class BoundingBoxVisual3D : ShapeVisual3D, IPureVisual3D, ITranslatable3D, IRotatable, IResizable3D, IHasSurfaceArea, IHasVolume, ICutVolume, IAnalyseVolume3D
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
        /// 深度依赖属性
        /// </summary>
        public static readonly StyledProperty<float> DepthProperty;

        /// <summary>
        /// 中心位置依赖属性
        /// </summary>
        public static readonly StyledProperty<Vector3D> CenterProperty;

        /// <summary>
        /// 可否旋转依赖属性
        /// </summary>
        public static readonly StyledProperty<bool> CanRotateProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static BoundingBoxVisual3D()
        {
            WidthProperty = AvaloniaProperty.Register<BoundingBoxVisual3D, float>(nameof(Width), 1.0f);
            HeightProperty = AvaloniaProperty.Register<BoundingBoxVisual3D, float>(nameof(Height), 1.0f);
            DepthProperty = AvaloniaProperty.Register<BoundingBoxVisual3D, float>(nameof(Depth), 1.0f);
            CenterProperty = AvaloniaProperty.Register<BoundingBoxVisual3D, Vector3D>(nameof(Center), new Vector3D(0, 0, 0));
            CanRotateProperty = AvaloniaProperty.Register<BoundingBoxVisual3D, bool>(nameof(CanRotate), true);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public BoundingBoxVisual3D()
        {

        }

        #endregion

        #region # 属性

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

        #region 依赖属性 - 深度 —— float Depth
        /// <summary>
        /// 依赖属性 - 深度
        /// </summary>
        public float Depth
        {
            get => this.GetValue(DepthProperty);
            set => this.SetValue(DepthProperty, value);
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

        #region 只读属性 - 最小点 —— Vector3 Minimum
        /// <summary>
        /// 只读属性 - 最小点
        /// </summary>
        /// <remarks>局部空间</remarks>
        public Vector3 Minimum
        {
            get
            {
                Vector3 center = this.Center.ToVector3();
                return new Vector3(
                    center.X - this.Width * 0.5f,
                    center.Y - this.Depth * 0.5f,
                    center.Z - this.Height * 0.5f);
            }
        }
        #endregion

        #region 只读属性 - 最大点 —— Vector3 Maximum
        /// <summary>
        /// 只读属性 - 最大点
        /// </summary>
        /// <remarks>局部空间</remarks>
        public Vector3 Maximum
        {
            get
            {
                Vector3 center = this.Center.ToVector3();
                return new Vector3(
                    center.X + this.Width * 0.5f,
                    center.Y + this.Depth * 0.5f,
                    center.Z + this.Height * 0.5f
                );
            }
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
            MeshGeometry strokeMesh = MeshFactory.CreateBoundingBox(this.Width, this.Height, this.Depth, this.Center.ToVector3(), (GraphicPrimitiveType)PrimitiveType.Lines);
            MeshGeometry fillMesh = MeshFactory.CreateBoundingBox(this.Width, this.Height, this.Depth, this.Center.ToVector3(), (GraphicPrimitiveType)PrimitiveType.Triangles);
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
            Vector3 minimum = this.Minimum;
            Vector3 maximum = this.Maximum;
            List<Vector3> localHull =
            [
                new Vector3(minimum.X, minimum.Y, minimum.Z),
                new Vector3(minimum.X, minimum.Y, maximum.Z),
                new Vector3(minimum.X, maximum.Y, minimum.Z),
                new Vector3(minimum.X, maximum.Y, maximum.Z),
                new Vector3(maximum.X, minimum.Y, minimum.Z),
                new Vector3(maximum.X, minimum.Y, maximum.Z),
                new Vector3(maximum.X, maximum.Y, minimum.Z),
                new Vector3(maximum.X, maximum.Y, maximum.Z)
            ];

            //转换到世界空间
            Matrix4 localToWorld = this.Transform.Matrix;
            List<Vector3> convexHullPositions = new List<Vector3>(8);
            for (int index = 0; index < 8; index++)
            {
                Vector3 worldPosition = Vector3.TransformPosition(localHull[index], localToWorld);
                convexHullPositions.Add(worldPosition);
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

            Vector3 minimum = this.Bounds.Minimum;
            Vector3 maximum = this.Bounds.Maximum;
            Vector3 center = this.Bounds.Center;
            float halfX = (maximum.X - minimum.X) * 0.5f;  //Width
            float halfY = (maximum.Y - minimum.Y) * 0.5f;  //Depth （前后）
            float halfZ = (maximum.Z - minimum.Z) * 0.5f;  //Height（上下）

            //射线与包围盒求交
            if (!localRay.Intersects(this.Bounds, out float distance))
            {
                return false;
            }

            Vector3 hitPoint = localRay.GetPoint(distance);
            Vector3 localHitPoint = hitPoint - center;

            //根据交点判断是哪个面（Z-up映射）
            float distX = halfX - Math.Abs(localHitPoint.X);  //Width  方向
            float distY = halfY - Math.Abs(localHitPoint.Y);  //Depth  方向（前后）
            float distZ = halfZ - Math.Abs(localHitPoint.Z);  //Height 方向（上下）

            Vector3 axis;
            float currentValue;
            if (distX < distY && distX < distZ)
            {
                //X轴方向：Right / Left
                axis = (localHitPoint.X > 0) ? Vector3.UnitX : -Vector3.UnitX;
                currentValue = halfX;
            }
            else if (distY < distZ)
            {
                //Y轴方向：Front / Back（Z-up下Y是前后）
                axis = (localHitPoint.Y > 0) ? Vector3.UnitY : -Vector3.UnitY;
                currentValue = halfY;
            }
            else
            {
                //Z轴方向：Top / Bottom（Z-up下Z是上下）
                axis = (localHitPoint.Z > 0) ? Vector3.UnitZ : -Vector3.UnitZ;
                currentValue = halfZ;
            }

            resizeContext.Anchor = center;
            resizeContext.Axis = axis;
            resizeContext.CurrentValue = currentValue;

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
            Vector3 delta = localHitPoint - resizeContext.Anchor;
            float newHalf = Math.Abs(Vector3.Dot(delta, resizeContext.Axis));
            newHalf = Math.Max(newHalf, 0.01f);

            float dotX = Math.Abs(Vector3.Dot(resizeContext.Axis, Vector3.UnitX));
            float dotY = Math.Abs(Vector3.Dot(resizeContext.Axis, Vector3.UnitY));
            float dotZ = Math.Abs(Vector3.Dot(resizeContext.Axis, Vector3.UnitZ));
            if (dotX > 0.99f)
            {
                this.Width = newHalf * 2.0f;
            }
            else if (dotY > 0.99f)
            {
                this.Depth = newHalf * 2.0f;   //Y轴对应Depth
            }
            else if (dotZ > 0.99f)
            {
                this.Height = newHalf * 2.0f;  //Z轴对应Height
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
            for (int i = 1; i < mmHull.Length - 1; i++)
            {
                volume += Vector3.Dot(mmHull[i] - origin, Vector3.Cross(mmHull[i + 1] - origin, mmHull[i + 2] - origin));
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
            Matrix4 localToWorld = this.Transform.Matrix;
            volumeData.ApplyBoxCut(markTexture, this.Minimum, this.Maximum, localToWorld, cutMode, markValue);
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

            Vector3 minimum = this.Minimum;
            Vector3 maximum = this.Maximum;
            Matrix4 localToWorld = this.Transform.Matrix;
            StatisticResult result = await Task.Run(() => volumeData.ApplyBoxAnalyse(minimum, maximum, localToWorld, markValue));
            result.SurfaceArea = this.CalculateSurfaceArea(volumeData.Metadata);
            result.Volume = this.CalculateVolume(volumeData.Metadata);

            return result;
        }
        #endregion

        #endregion
    }
}
