using Avalonia;
using Avalonia.Collections;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.InputManagers;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Controls.Visual3Ds;
using MedicalSharp.Engine.Base;
using MedicalSharp.Engine.Managers;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Renderers;
using MedicalSharp.Primitives.Algorithms;
using MedicalSharp.Primitives.Cameras;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Managers;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace MedicalSharp.Controls.Viewports
{
    /// <summary>
    /// CPR渲染视口
    /// </summary>
    public class CPRViewport : BasicViewport, IPickVoxel
    {
        #region # 字段及构造器

        /// <summary>
        /// 渲染模式依赖属性
        /// </summary>
        public static readonly StyledProperty<CPRRenderMode> RenderModeProperty;

        /// <summary>
        /// 曲线依赖属性
        /// </summary>
        public static readonly StyledProperty<Curve> CurveProperty;

        /// <summary>
        /// 窗宽依赖属性
        /// </summary>
        public static readonly StyledProperty<int> WindowWidthProperty;

        /// <summary>
        /// 窗位依赖属性
        /// </summary>
        public static readonly StyledProperty<int> WindowCenterProperty;

        /// <summary>
        /// 亮度依赖属性
        /// </summary>
        public static readonly StyledProperty<float> BrightnessProperty;

        /// <summary>
        /// 对比度依赖属性
        /// </summary>
        public static readonly StyledProperty<float> ContrastProperty;

        /// <summary>
        /// 插值模式依赖属性
        /// </summary>
        public static readonly StyledProperty<InterpolationMode> InterpolationModeProperty;

        /// <summary>
        /// 传递函数控制点列表依赖属性
        /// </summary>
        public static readonly StyledProperty<AvaloniaList<HUControlPoint>> TFControlPointsProperty;

        /// <summary>
        /// CPR模式依赖属性
        /// </summary>
        public static readonly StyledProperty<CPRMode> CPRModeProperty;

        /// <summary>
        /// 径向宽度依赖属性
        /// </summary>
        public static readonly StyledProperty<float> RadialWidthProperty;

        /// <summary>
        /// 旋转角度依赖属性
        /// </summary>
        public static readonly StyledProperty<float> RotationAngleProperty;

        /// <summary>
        /// 投影厚度依赖属性
        /// </summary>
        public static readonly StyledProperty<float> ProjectionThicknessProperty;

        /// <summary>
        /// 最大步数依赖属性
        /// </summary>
        public static readonly StyledProperty<int> MaxStepsCountProperty;

        /// <summary>
        /// 投影模式依赖属性
        /// </summary>
        public static readonly StyledProperty<IntensityProjectionMode> ProjectionModeProperty;

        /// <summary>
        /// 投影方向依赖属性
        /// </summary>
        public static readonly StyledProperty<CPRProjectionDirection> ProjectionDirectionProperty;

        /// <summary>
        /// 弧长位置依赖属性
        /// </summary>
        public static readonly StyledProperty<float> ArcPositionProperty;

        /// <summary>
        /// 剖面尺寸依赖属性
        /// </summary>
        public static readonly StyledProperty<float> CrossSectionSizeProperty;

        /// <summary>
        /// 体积数据依赖属性
        /// </summary>
        public static readonly StyledProperty<VolumeData> VolumeDataProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static CPRViewport()
        {
            RenderModeProperty = AvaloniaProperty.Register<CPRViewport, CPRRenderMode>(nameof(RenderMode), CPRRenderMode.Gray);
            CurveProperty = AvaloniaProperty.Register<CPRViewport, Curve>(nameof(Curve));
            WindowWidthProperty = AvaloniaProperty.Register<CPRViewport, int>(nameof(WindowWidth), 400);
            WindowCenterProperty = AvaloniaProperty.Register<CPRViewport, int>(nameof(WindowCenter), 40);
            BrightnessProperty = AvaloniaProperty.Register<CPRViewport, float>(nameof(Brightness), 1.0f);
            ContrastProperty = AvaloniaProperty.Register<CPRViewport, float>(nameof(Contrast), 1.0f);
            InterpolationModeProperty = AvaloniaProperty.Register<CPRViewport, InterpolationMode>(nameof(InterpolationMode), InterpolationMode.Linear);
            TFControlPointsProperty = AvaloniaProperty.Register<CPRViewport, AvaloniaList<HUControlPoint>>(nameof(TFControlPoints));
            CPRModeProperty = AvaloniaProperty.Register<CPRViewport, CPRMode>(nameof(CPRMode), CPRMode.Straightened);
            RadialWidthProperty = AvaloniaProperty.Register<CPRViewport, float>(nameof(RadialWidth), 0.1f);
            RotationAngleProperty = AvaloniaProperty.Register<CPRViewport, float>(nameof(RotationAngle), 0f);
            ProjectionThicknessProperty = AvaloniaProperty.Register<CPRViewport, float>(nameof(ProjectionThickness), 0.05f);
            MaxStepsCountProperty = AvaloniaProperty.Register<CPRViewport, int>(nameof(MaxStepsCount), 100);
            ProjectionModeProperty = AvaloniaProperty.Register<CPRViewport, IntensityProjectionMode>(nameof(ProjectionMode), IntensityProjectionMode.MIP);
            ProjectionDirectionProperty = AvaloniaProperty.Register<CPRViewport, CPRProjectionDirection>(nameof(ProjectionDirection), CPRProjectionDirection.Tangent);
            ArcPositionProperty = AvaloniaProperty.Register<CPRViewport, float>(nameof(ArcPosition), 0.5f);
            CrossSectionSizeProperty = AvaloniaProperty.Register<CPRViewport, float>(nameof(CrossSectionSize), 0.1f);
            VolumeDataProperty = AvaloniaProperty.Register<CPRViewport, VolumeData>(nameof(VolumeData));

            //属性改变事件
            RenderModeProperty.Changed.AddClassHandler<CPRViewport, CPRRenderMode>(OnRenderModeChanged);
            CurveProperty.Changed.AddClassHandler<CPRViewport, Curve>(OnCurveChanged);
            WindowWidthProperty.Changed.AddClassHandler<CPRViewport, int>(OnWindowWidthChanged);
            WindowCenterProperty.Changed.AddClassHandler<CPRViewport, int>(OnWindowCenterChanged);
            BrightnessProperty.Changed.AddClassHandler<CPRViewport, float>(OnBrightnessChanged);
            ContrastProperty.Changed.AddClassHandler<CPRViewport, float>(OnContrastChanged);
            InterpolationModeProperty.Changed.AddClassHandler<CPRViewport, InterpolationMode>(OnInterpolationModeChanged);
            TFControlPointsProperty.Changed.AddClassHandler<CPRViewport, AvaloniaList<HUControlPoint>>(OnTFControlPointsChanged);
            CPRModeProperty.Changed.AddClassHandler<CPRViewport, CPRMode>(OnCPRModeChanged);
            RadialWidthProperty.Changed.AddClassHandler<CPRViewport, float>(OnRadialWidthChanged);
            RotationAngleProperty.Changed.AddClassHandler<CPRViewport, float>(OnRotationAngleChanged);
            ProjectionThicknessProperty.Changed.AddClassHandler<CPRViewport, float>(OnProjectionThicknessChanged);
            MaxStepsCountProperty.Changed.AddClassHandler<CPRViewport, int>(OnMaxStepsCountChanged);
            ProjectionModeProperty.Changed.AddClassHandler<CPRViewport, IntensityProjectionMode>(OnProjectionModeChanged);
            ProjectionDirectionProperty.Changed.AddClassHandler<CPRViewport, CPRProjectionDirection>(OnProjectionDirectionChanged);
            ArcPositionProperty.Changed.AddClassHandler<CPRViewport, float>(OnArcPositionChanged);
            CrossSectionSizeProperty.Changed.AddClassHandler<CPRViewport, float>(OnCrossSectionSizeChanged);
            VolumeDataProperty.Changed.AddClassHandler<CPRViewport, VolumeData>(OnVolumeDataChanged);
        }


        /// <summary>
        /// 体积渲染对象
        /// </summary>
        private VolumeRenderable _volumeRenderable;

        /// <summary>
        /// CPR渲染器
        /// </summary>
        private CPRRenderer _cprRenderer;

        /// <summary>
        /// 默认构造器
        /// </summary>
        public CPRViewport()
        {

        }

        #endregion

        #region # 属性

        #region 依赖属性 - 渲染模式 —— CPRRenderMode RenderMode
        /// <summary>
        /// 依赖属性 - 渲染模式
        /// </summary>
        public CPRRenderMode RenderMode
        {
            get => this.GetValue(RenderModeProperty);
            set => this.SetValue(RenderModeProperty, value);
        }
        #endregion

        #region 依赖属性 - 曲线 —— Curve Curve
        /// <summary>
        /// 依赖属性 - 曲线
        /// </summary>
        public Curve Curve
        {
            get => this.GetValue(CurveProperty);
            set => this.SetValue(CurveProperty, value);
        }
        #endregion

        #region 依赖属性 - 窗宽 —— int WindowWidth
        /// <summary>
        /// 依赖属性 - 窗宽
        /// </summary>
        public int WindowWidth
        {
            get => this.GetValue(WindowWidthProperty);
            set => this.SetValue(WindowWidthProperty, value);
        }
        #endregion

        #region 依赖属性 - 窗位 —— int WindowCenter
        /// <summary>
        /// 依赖属性 - 窗位
        /// </summary>
        public int WindowCenter
        {
            get => this.GetValue(WindowCenterProperty);
            set => this.SetValue(WindowCenterProperty, value);
        }
        #endregion

        #region 依赖属性 - 亮度 —— float Brightness
        /// <summary>
        /// 依赖属性 - 亮度
        /// </summary>
        public float Brightness
        {
            get => this.GetValue(BrightnessProperty);
            set => this.SetValue(BrightnessProperty, value);
        }
        #endregion

        #region 依赖属性 - 对比度 —— float Contrast
        /// <summary>
        /// 依赖属性 - 对比度
        /// </summary>
        public float Contrast
        {
            get => this.GetValue(ContrastProperty);
            set => this.SetValue(ContrastProperty, value);
        }
        #endregion

        #region 依赖属性 - 插值模式 —— InterpolationMode InterpolationMode
        /// <summary>
        /// 依赖属性 - 插值模式
        /// </summary>
        public InterpolationMode InterpolationMode
        {
            get => this.GetValue(InterpolationModeProperty);
            set => this.SetValue(InterpolationModeProperty, value);
        }
        #endregion

        #region 依赖属性 - 传递函数控制点列表 —— AvaloniaList<HUControlPoint> TFControlPoints
        /// <summary>
        /// 依赖属性 - 传递函数控制点列表
        /// </summary>
        public AvaloniaList<HUControlPoint> TFControlPoints
        {
            get => this.GetValue(TFControlPointsProperty);
            set => this.SetValue(TFControlPointsProperty, value);
        }
        #endregion

        #region 依赖属性 - CPR模式 —— CPRMode CPRMode
        /// <summary>
        /// 依赖属性 - CPR模式
        /// </summary>
        public CPRMode CPRMode
        {
            get => this.GetValue(CPRModeProperty);
            set => this.SetValue(CPRModeProperty, value);
        }
        #endregion

        #region 依赖属性 - 径向宽度 —— float RadialWidth
        /// <summary>
        /// 依赖属性 - 径向宽度
        /// </summary>
        public float RadialWidth
        {
            get => this.GetValue(RadialWidthProperty);
            set => this.SetValue(RadialWidthProperty, value);
        }
        #endregion

        #region 依赖属性 - 旋转角度 —— float RotationAngle
        /// <summary>
        /// 依赖属性 - 旋转角度
        /// </summary>
        public float RotationAngle
        {
            get => this.GetValue(RotationAngleProperty);
            set => this.SetValue(RotationAngleProperty, value);
        }
        #endregion

        #region 依赖属性 - 投影厚度 —— float ProjectionThickness
        /// <summary>
        /// 依赖属性 - 投影厚度
        /// </summary>
        public float ProjectionThickness
        {
            get => this.GetValue(ProjectionThicknessProperty);
            set => this.SetValue(ProjectionThicknessProperty, value);
        }
        #endregion

        #region 依赖属性 - 最大步数 —— int MaxStepsCount
        /// <summary>
        /// 依赖属性 - 最大步数
        /// </summary>
        public int MaxStepsCount
        {
            get => this.GetValue(MaxStepsCountProperty);
            set => this.SetValue(MaxStepsCountProperty, value);
        }
        #endregion

        #region 依赖属性 - 投影模式 —— IntensityProjectionMode ProjectionMode
        /// <summary>
        /// 依赖属性 - 投影模式
        /// </summary>
        public IntensityProjectionMode ProjectionMode
        {
            get => this.GetValue(ProjectionModeProperty);
            set => this.SetValue(ProjectionModeProperty, value);
        }
        #endregion

        #region 依赖属性 - 投影方向 —— CPRProjectionDirection ProjectionDirection
        /// <summary>
        /// 依赖属性 - 投影方向
        /// </summary>
        public CPRProjectionDirection ProjectionDirection
        {
            get => this.GetValue(ProjectionDirectionProperty);
            set => this.SetValue(ProjectionDirectionProperty, value);
        }
        #endregion

        #region 依赖属性 - 弧长位置 —— float ArcPosition
        /// <summary>
        /// 依赖属性 - 弧长位置
        /// </summary>
        public float ArcPosition
        {
            get => this.GetValue(ArcPositionProperty);
            set => this.SetValue(ArcPositionProperty, value);
        }
        #endregion

        #region 依赖属性 - 剖面尺寸 —— float CrossSectionSize
        /// <summary>
        /// 依赖属性 - 剖面尺寸
        /// </summary>
        public float CrossSectionSize
        {
            get => this.GetValue(CrossSectionSizeProperty);
            set => this.SetValue(CrossSectionSizeProperty, value);
        }
        #endregion

        #region 依赖属性 - 体积数据 —— VolumeData VolumeData
        /// <summary>
        /// 依赖属性 - 体积数据
        /// </summary>
        public VolumeData VolumeData
        {
            get => this.GetValue(VolumeDataProperty);
            set => this.SetValue(VolumeDataProperty, value);
        }
        #endregion

        #region 只读属性 - 体积渲染对象 —— VolumeRenderable VolumeRenderable
        /// <summary>
        /// 只读属性 - 体积渲染对象
        /// </summary>
        public VolumeRenderable VolumeRenderable
        {
            get => this._volumeRenderable;
        }
        #endregion

        #region 只读属性 - CPR相机 —— CPRCamera CPRCamera
        /// <summary>
        /// 只读属性 - CPR相机
        /// </summary>
        public CPRCamera CPRCamera
        {
            get => (CPRCamera)this.Camera;
        }
        #endregion

        #region 只读属性 - CPR渲染器 —— CPRRenderer CPRRenderer
        /// <summary>
        /// 只读属性 - CPR渲染器
        /// </summary>
        public CPRRenderer CPRRenderer
        {
            get => this._cprRenderer;
        }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 反投影 —— override Ray UnProject(Vector2 screenPixelPos2D)
        /// <summary>
        /// 反投影
        /// </summary>
        /// <param name="screenPixelPos2D">屏幕像素2D位置</param>
        /// <returns>射线</returns>
        public override Ray UnProject(Vector2 screenPixelPos2D)
        {
            #region # 验证

            if (this.Curve == null || this.CPRCamera == null || this._viewportSize.Width == 0 || this._viewportSize.Height == 0)
            {
                return default;
            }

            #endregion

            //屏幕坐标 -> UnitPlane UV（和顶点着色器一致：UV = aPos.xy + 0.5）
            float scaling = (float)this.GetRenderScaling();
            float ndcX = (2.0f * screenPixelPos2D.X * scaling) / this._viewportSize.Width - 1.0f;
            float ndcY = 1.0f - (2.0f * screenPixelPos2D.Y * scaling) / this._viewportSize.Height;
            float uvX = ndcX * 0.5f + 0.5f;
            float uvY = ndcY * 0.5f + 0.5f;

            //UV -> 弧长 + 径向偏移
            float arcLength = uvX * this.Curve.TotalArcLength;
            float radialOffset = (uvY - 0.5f) * this.RadialWidth;

            //获取FrenetFrame
            FrenetFrame frame = this.Curve.GetFrameAtArcLength(arcLength);

            //绕Tangent旋转Normal
            float cosA = MathF.Cos(this.RotationAngle);
            float sinA = MathF.Sin(this.RotationAngle);
            Vector3 rotatedNormal = frame.Normal * cosA +
                                    Vector3.Cross(frame.Tangent, frame.Normal) * sinA +
                                    frame.Tangent * Vector3.Dot(frame.Tangent, frame.Normal) * (1.0f - cosA);

            //计算采样位置（世界空间）
            Vector3 samplePosition = frame.Position + rotatedNormal * radialOffset;

            //构造射线
            Ray ray = new Ray(samplePosition, this.CPRCamera.LookDirection);

            return ray;
        }
        #endregion

        #region 查找最近体素 —— bool FindNearestVoxel(Vector2 position, out Vector3 textureCoord...
        /// <summary>
        /// 查找最近体素
        /// </summary>
        /// <param name="position">2D位置</param>
        /// <param name="textureCoord">纹理坐标</param>
        /// <param name="worldPosition">世界坐标</param>
        /// <param name="voxelPosition">体素坐标</param>
        /// <param name="voxelValue">体素HU值</param>
        /// <param name="markValue">标记值</param>
        /// <param name="ray">射线</param>
        /// <returns>是否成功</returns>
        public bool FindNearestVoxel(Vector2 position, out Vector3 textureCoord, out Vector3 worldPosition, out Vector3i voxelPosition, out short voxelValue, out byte markValue, out Ray ray)
        {
            textureCoord = Vector3.Zero;
            worldPosition = Vector3.Zero;
            voxelPosition = Vector3i.Zero;
            voxelValue = -1;
            markValue = 0;
            ray = default;

            #region # 验证

            if (this.VolumeData == null || this.Curve == null)
            {
                return false;
            }

            #endregion

            this.GlContext.MakeCurrent();

            ray = this.UnProject(position);

            //射线起点 -> 纹理坐标
            worldPosition = ray.Origin;
            textureCoord = worldPosition.ToTextureCoord(this.VolumeData.Metadata);
            voxelPosition = worldPosition.ToVoxelPosition(this.VolumeData.Metadata);

            //边界检查
            if (textureCoord.X < 0 || textureCoord.X > 1 ||
                textureCoord.Y < 0 || textureCoord.Y > 1 ||
                textureCoord.Z < 0 || textureCoord.Z > 1)
            {
                return false;
            }

            voxelValue = this.VolumeData.GetPreviewValue(voxelPosition);
            markValue = this.VolumeData.GetMarkValue(voxelPosition);

            return true;
        }
        #endregion

        #region 查找最近位置 —— override Vector3? FindNearestPosition(Vector2 position)
        /// <summary>
        /// 查找最近位置
        /// </summary>
        /// <param name="position">2D位置</param>
        /// <returns>3D位置</returns>
        public override Vector3? FindNearestPosition(Vector2 position)
        {
            if (this.FindNearestVoxel(position, out _, out Vector3 worldPosition, out _, out _, out _, out _))
            {
                return worldPosition;
            }

            return null;
        }
        #endregion


        //Protected

        #region OpenTK初始化事件 —— override void OnOpenTKInit()
        /// <summary>
        /// OpenTK初始化事件
        /// </summary>
        protected override void OnOpenTKInit()
        {
            //InputManger默认值
            this.InputManager ??= new CPRInputManager(this.CPRCamera);

            //初始化形状渲染器
            this._shapeRenderer = new ShapeRenderer(this.CPRCamera);
            this._overlayRenderer = new OverlayRenderer(this.CPRCamera);

            //初始化CPR渲染器
            this._cprRenderer = new CPRRenderer(this.CPRCamera);
            this._cprRenderer.SetWindowLevel(this.WindowWidth, this.WindowCenter);
            this._cprRenderer.SetMaterialOptions(this.Brightness, this.Contrast);
        }
        #endregion

        #region OpenTK渲染事件 —— override void OnOpenTKRender(PixelSize viewportSize)
        /// <summary>
        /// OpenTK渲染事件
        /// </summary>
        /// <param name="viewportSize">视口尺寸</param>
        protected override void OnOpenTKRender(PixelSize viewportSize)
        {
            if (this._volumeRenderable != null && this.Curve != null)
            {
                //禁用面剔除
                GL.Disable(EnableCap.CullFace);

                //启用混合
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                //关闭深度
                GL.DepthMask(false);

                //CPR渲染
                this._cprRenderer.BindCurve(this.Curve);
                this._cprRenderer.SetRenderable(this._volumeRenderable);
                this._cprRenderer.RenderFrame(viewportSize.Width, viewportSize.Height, this.GlContextHandle);

                //形状渲染
                base.OnOpenTKRender(viewportSize);
            }
            else
            {
                this._shapeVisual3Ds.Clear();
            }
        }
        #endregion

        #region OpenTK卸载事件 —— override void OnOpenTKDeinit()
        /// <summary>
        /// OpenTK卸载事件
        /// </summary>
        protected override void OnOpenTKDeinit()
        {
            base.OnOpenTKDeinit();
            this._cprRenderer?.Dispose();
        }
        #endregion

        #region 获取形状3D元素列表 —— override List<ShapeVisual3D> GetShapeVisual3Ds()
        /// <summary>
        /// 获取形状3D元素列表
        /// </summary>
        /// <returns>形状3D元素列表</returns>
        protected override List<ShapeVisual3D> GetShapeVisual3Ds()
        {
            List<ShapeVisual3D> shapeVisual3Ds = [];
            foreach (ShapeVisual3D shapeVisual3D in base.GetShapeVisual3Ds())
            {
                if (shapeVisual3D is IVisual2DIn3D)
                {
                    shapeVisual3Ds.Add(shapeVisual3D);
                }
            }

            return shapeVisual3Ds;
        }
        #endregion


        //Events

        #region 渲染模式改变事件 —— static void OnRenderModeChanged(CPRViewport viewport...
        /// <summary>
        /// 渲染模式改变事件
        /// </summary>
        private static void OnRenderModeChanged(CPRViewport viewport, AvaloniaPropertyChangedEventArgs<CPRRenderMode> eventArgs)
        {
            viewport._cprRenderer?.SwitchRenderMode(eventArgs.NewValue.Value);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 曲线改变事件 —— static void OnCurveChanged(CPRViewport viewport...
        /// <summary>
        /// 曲线改变事件
        /// </summary>
        private static void OnCurveChanged(CPRViewport viewport, AvaloniaPropertyChangedEventArgs<Curve> eventArgs)
        {
            viewport._cprRenderer?.BindCurve(eventArgs.NewValue.Value);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 窗宽改变事件 —— static void OnWindowWidthChanged(CPRViewport viewport...
        /// <summary>
        /// 窗宽改变事件
        /// </summary>
        private static void OnWindowWidthChanged(CPRViewport viewport, AvaloniaPropertyChangedEventArgs<int> eventArgs)
        {
            viewport._cprRenderer?.SetWindowLevel(eventArgs.NewValue.Value, viewport.WindowCenter);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 窗位改变事件 —— static void OnWindowCenterChanged(CPRViewport viewport...
        /// <summary>
        /// 窗位改变事件
        /// </summary>
        private static void OnWindowCenterChanged(CPRViewport viewport, AvaloniaPropertyChangedEventArgs<int> eventArgs)
        {
            viewport._cprRenderer?.SetWindowLevel(viewport.WindowWidth, eventArgs.NewValue.Value);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 亮度改变事件 —— static void OnBrightnessChanged(CPRViewport viewport...
        /// <summary>
        /// 亮度改变事件
        /// </summary>
        private static void OnBrightnessChanged(CPRViewport viewport, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            viewport._cprRenderer?.SetMaterialOptions(eventArgs.NewValue.Value, viewport.Contrast);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 对比度改变事件 —— static void OnContrastChanged(CPRViewport viewport...
        /// <summary>
        /// 对比度改变事件
        /// </summary>
        private static void OnContrastChanged(CPRViewport viewport, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            viewport._cprRenderer?.SetMaterialOptions(viewport.Brightness, eventArgs.NewValue.Value);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 插值模式改变事件 —— static void OnInterpolationModeChanged(CPRViewport viewport...
        /// <summary>
        /// 插值模式改变事件
        /// </summary>
        private static void OnInterpolationModeChanged(CPRViewport viewport, AvaloniaPropertyChangedEventArgs<InterpolationMode> eventArgs)
        {
            viewport._cprRenderer?.TransferFunction.SwitchInterpolationMode(eventArgs.NewValue.Value);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 传递函数控制点列表改变事件 —— static void OnTFControlPointsChanged(CPRViewport viewport...
        /// <summary>
        /// 传递函数控制点列表改变事件
        /// </summary>
        private static void OnTFControlPointsChanged(CPRViewport viewport, AvaloniaPropertyChangedEventArgs<AvaloniaList<HUControlPoint>> eventArgs)
        {
            if (eventArgs.OldValue.Value != null)
            {
                eventArgs.OldValue.Value.CollectionChanged -= viewport.OnTFControlPointsItemChanged;
            }
            if (eventArgs.NewValue.Value != null)
            {
                viewport._cprRenderer?.TransferFunction.InitFromControlPoints(eventArgs.NewValue.Value);
                eventArgs.NewValue.Value.CollectionChanged += viewport.OnTFControlPointsItemChanged;
            }
            if (eventArgs.NewValue.Value == null)
            {
                viewport._cprRenderer?.TransferFunction.ClearControlPoints();
            }

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 传递函数控制点列表元素改变事件 —— void OnTFControlPointsItemChanged(object sender...
        /// <summary>
        /// 传递函数控制点列表元素改变事件
        /// </summary>
        private void OnTFControlPointsItemChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            if (eventArgs.OldItems != null)
            {
                foreach (HUControlPoint controlPoint in eventArgs.OldItems)
                {
                    this._cprRenderer?.TransferFunction.RemoveControlPoint(controlPoint);
                }
            }
            if (eventArgs.NewItems != null)
            {
                foreach (HUControlPoint controlPoint in eventArgs.NewItems)
                {
                    this._cprRenderer?.TransferFunction.AddControlPoint(controlPoint);
                }
            }
            if (eventArgs.Action == NotifyCollectionChangedAction.Reset)
            {
                this._cprRenderer?.TransferFunction.ClearControlPoints();
            }

            //请求下一帧
            this.RequestNextFrameRendering();
        }
        #endregion

        #region CPR模式改变事件 —— static void OnCPRModeChanged(CPRViewport viewport...
        /// <summary>
        /// CPR模式改变事件
        /// </summary>
        private static void OnCPRModeChanged(CPRViewport viewport, AvaloniaPropertyChangedEventArgs<CPRMode> eventArgs)
        {
            viewport._cprRenderer?.SwitchCPRMode(eventArgs.NewValue.Value);
            viewport.ApplyCPROptions(viewport._cprRenderer);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 径向宽度改变事件 —— static void OnRadialWidthChanged(CPRViewport viewport...
        /// <summary>
        /// 径向宽度改变事件
        /// </summary>
        private static void OnRadialWidthChanged(CPRViewport viewport, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            viewport.ApplyCPROptions(viewport._cprRenderer);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 旋转角度改变事件 —— static void OnRotationAngleChanged(CPRViewport viewport...
        /// <summary>
        /// 旋转角度改变事件
        /// </summary>
        private static void OnRotationAngleChanged(CPRViewport viewport, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            viewport.ApplyCPROptions(viewport._cprRenderer);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 投影厚度改变事件 —— static void OnProjectionThicknessChanged(CPRViewport viewport...
        /// <summary>
        /// 投影厚度改变事件
        /// </summary>
        private static void OnProjectionThicknessChanged(CPRViewport viewport, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            viewport.ApplyCPROptions(viewport._cprRenderer);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 最大步数改变事件 —— static void OnMaxStepsCountChanged(CPRViewport viewport...
        /// <summary>
        /// 最大步数改变事件
        /// </summary>
        private static void OnMaxStepsCountChanged(CPRViewport viewport, AvaloniaPropertyChangedEventArgs<int> eventArgs)
        {
            viewport.ApplyCPROptions(viewport._cprRenderer);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 投影模式改变事件 —— static void OnProjectionModeChanged(CPRViewport viewport...
        /// <summary>
        /// 投影模式改变事件
        /// </summary>
        private static void OnProjectionModeChanged(CPRViewport viewport, AvaloniaPropertyChangedEventArgs<IntensityProjectionMode> eventArgs)
        {
            viewport._cprRenderer?.SwitchProjectionMode(eventArgs.NewValue.Value);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 投影方向改变事件 —— static void OnProjectionDirectionChanged(CPRViewport viewport...
        /// <summary>
        /// 投影方向改变事件
        /// </summary>
        private static void OnProjectionDirectionChanged(CPRViewport viewport, AvaloniaPropertyChangedEventArgs<CPRProjectionDirection> eventArgs)
        {
            viewport._cprRenderer?.SwitchProjectionDirection(eventArgs.NewValue.Value);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 弧长位置改变事件 —— static void OnArcPositionChanged(CPRViewport viewport...
        /// <summary>
        /// 弧长位置改变事件
        /// </summary>
        private static void OnArcPositionChanged(CPRViewport viewport, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            viewport.ApplyCPROptions(viewport._cprRenderer);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 剖面尺寸改变事件 —— static void OnCrossSectionSizeChanged(CPRViewport viewport...
        /// <summary>
        /// 剖面尺寸改变事件
        /// </summary>
        private static void OnCrossSectionSizeChanged(CPRViewport viewport, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            viewport.ApplyCPROptions(viewport._cprRenderer);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 体积数据改变事件 —— static void OnVolumeDataChanged(CPRViewport viewport...
        /// <summary>
        /// 体积数据改变事件
        /// </summary>
        private static void OnVolumeDataChanged(CPRViewport viewport, AvaloniaPropertyChangedEventArgs<VolumeData> eventArgs)
        {
            #region # 验证

            if (eventArgs.NewValue.Value == null)
            {
                viewport._volumeRenderable = null;
                viewport.RequestNextFrameRendering();
                return;
            }

            #endregion

            VolumeData volumeData = eventArgs.NewValue.Value;
            VolumeSession volumeSession;
            if (!SessionManager.VolumeSessions.ContainsKey(volumeData.Metadata.Id))
            {
                viewport.GlContext.MakeCurrent();
                volumeSession = new VolumeSession(volumeData);
                SessionManager.AddVolumeSession(volumeSession.Id, volumeSession);

                //设置默认窗
                WindowLevelManager.SetDefault(volumeData.Metadata.WindowWidth, volumeData.Metadata.WindowCenter);
            }
            else
            {
                volumeSession = SessionManager.VolumeSessions[volumeData.Metadata.Id];
            }

            viewport._volumeRenderable = new VolumeRenderable(volumeSession.PreviewTexture, volumeSession.MarkTexture, volumeData);

            //初始化传递函数、标记策略
            viewport._cprRenderer.SetTransferFunction(volumeSession.MPRTransferFunction);
            viewport._cprRenderer.SetMarkStrategy(volumeSession.MarkStrategy);
            viewport._cprRenderer.TransferFunction.SetHURange(volumeData.Metadata.MinHU, volumeData.Metadata.MaxHU);
            viewport._cprRenderer.TransferFunction.InitFromControlPoints(viewport.TFControlPoints);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion


        //Private

        #region 应用CPR选项 —— void ApplyCPROptions(CPRRenderer renderer)
        /// <summary>
        /// 应用CPR选项
        /// </summary>
        /// <param name="renderer">CPR渲染器</param>
        private void ApplyCPROptions(CPRRenderer renderer)
        {
            #region # 验证

            if (renderer == null)
            {
                return;
            }

            #endregion

            switch (this.CPRMode)
            {
                case CPRMode.Straightened:
                    renderer.SetStraightenedOptions(this.RadialWidth, this.RotationAngle);
                    break;
                case CPRMode.Projected:
                    renderer.SetProjectedOptions(this.RadialWidth, this.RotationAngle, this.ProjectionThickness, this.MaxStepsCount);
                    break;
                case CPRMode.CrossSectional:
                    renderer.SetCrossSectionalOptions(this.ArcPosition, this.CrossSectionSize);
                    break;
            }
        }
        #endregion

        #endregion
    }
}
