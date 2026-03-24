using Avalonia;
using Avalonia.Collections;
using Avalonia.Metadata;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Inputs;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Dicoms.Models;
using MedicalSharp.Engine.Cameras;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Renderers;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Engine.ValueTypes;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Specialized;

namespace MedicalSharp.Controls.Viewports
{
    /// <summary>
    /// 体积渲染视口
    /// </summary>
    public class VolumeViewport : OpenTKViewport
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗宽依赖属性
        /// </summary>
        public static readonly StyledProperty<float> WindowWidthProperty;

        /// <summary>
        /// 窗位依赖属性
        /// </summary>
        public static readonly StyledProperty<float> WindowCenterProperty;

        /// <summary>
        /// 亮度依赖属性
        /// </summary>
        public static readonly StyledProperty<float> BrightnessProperty;

        /// <summary>
        /// 密度缩放依赖属性
        /// </summary>
        public static readonly StyledProperty<float> DensityScaleProperty;

        /// <summary>
        /// 步长依赖属性
        /// </summary>
        public static readonly StyledProperty<float> StepSizeProperty;

        /// <summary>
        /// 最大步数依赖属性
        /// </summary>
        public static readonly StyledProperty<int> MaxStepsCountProperty;

        /// <summary>
        /// 透明度阈值依赖属性
        /// </summary>
        public static readonly StyledProperty<float> OpacityThresholdProperty;

        /// <summary>
        /// 传输函数控制列表依赖属性
        /// </summary>
        public static readonly StyledProperty<AvaloniaList<TFControlPoint>> TFControlPointsProperty;

        /// <summary>
        /// 体积数据依赖属性
        /// </summary>
        public static readonly StyledProperty<VolumeData> VolumeDataProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static VolumeViewport()
        {
            WindowWidthProperty = AvaloniaProperty.Register<VolumeViewport, float>(nameof(WindowWidth), 400.0f);
            WindowCenterProperty = AvaloniaProperty.Register<VolumeViewport, float>(nameof(WindowCenter), 40.0f);
            BrightnessProperty = AvaloniaProperty.Register<VolumeViewport, float>(nameof(Brightness), 1.0f);
            DensityScaleProperty = AvaloniaProperty.Register<VolumeViewport, float>(nameof(DensityScale), 1.5f);
            StepSizeProperty = AvaloniaProperty.Register<VolumeViewport, float>(nameof(StepSize), 0.0012f);
            MaxStepsCountProperty = AvaloniaProperty.Register<VolumeViewport, int>(nameof(MaxStepsCount), 1000);
            OpacityThresholdProperty = AvaloniaProperty.Register<VolumeViewport, float>(nameof(OpacityThreshold), 0.99f);
            TFControlPointsProperty = AvaloniaProperty.Register<VolumeViewport, AvaloniaList<TFControlPoint>>(nameof(TFControlPoints));
            VolumeDataProperty = AvaloniaProperty.Register<VolumeViewport, VolumeData>(nameof(VolumeData));

            //属性改变事件
            WindowWidthProperty.Changed.AddClassHandler<VolumeViewport, float>(OnWindowWidthChanged);
            WindowCenterProperty.Changed.AddClassHandler<VolumeViewport, float>(OnWindowCenterChanged);
            BrightnessProperty.Changed.AddClassHandler<VolumeViewport, float>(OnBrightnessChanged);
            DensityScaleProperty.Changed.AddClassHandler<VolumeViewport, float>(OnDensityScaleChanged);
            StepSizeProperty.Changed.AddClassHandler<VolumeViewport, float>(OnStepSizeChanged);
            MaxStepsCountProperty.Changed.AddClassHandler<VolumeViewport, int>(OnMaxStepsCountChanged);
            OpacityThresholdProperty.Changed.AddClassHandler<VolumeViewport, float>(OnOpacityThresholdChanged);
            TFControlPointsProperty.Changed.AddClassHandler<VolumeViewport, AvaloniaList<TFControlPoint>>(OnTFControlPointsChanged);
            VolumeDataProperty.Changed.AddClassHandler<VolumeViewport, VolumeData>(OnVolumeDataChanged);
        }


        /// <summary>
        /// 体积渲染对象
        /// </summary>
        private VolumeRenderable _volumeRenderable;

        /// <summary>
        /// 体积渲染器
        /// </summary>
        private VolumeRenderer _volumeRenderer;

        /// <summary>
        /// 线框渲染器
        /// </summary>
        private WireframeRenderer _wireframeRenderer;

        /// <summary>
        /// 默认构造器
        /// </summary>
        public VolumeViewport()
        {
            this.Children = new AvaloniaList<BoundingVisual3D>();
        }

        #endregion

        #region # 属性

        #region 子元素列表 —— AvaloniaList<BoundingVisual3D> Children
        /// <summary>
        /// 子元素列表
        /// </summary>
        [Content]
        public AvaloniaList<BoundingVisual3D> Children { get; private set; }
        #endregion

        #region 依赖属性 - 窗宽 —— float WindowWidth
        /// <summary>
        /// 依赖属性 - 窗宽
        /// </summary>
        public float WindowWidth
        {
            get => this.GetValue(WindowWidthProperty);
            set => this.SetValue(WindowWidthProperty, value);
        }
        #endregion

        #region 依赖属性 - 窗位 —— float WindowCenter
        /// <summary>
        /// 依赖属性 - 窗位
        /// </summary>
        public float WindowCenter
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

        #region 依赖属性 - 密度缩放 —— float DensityScale
        /// <summary>
        /// 依赖属性 - 密度缩放
        /// </summary>
        public float DensityScale
        {
            get => this.GetValue(DensityScaleProperty);
            set => this.SetValue(DensityScaleProperty, value);
        }
        #endregion

        #region 依赖属性 - 步长 —— float StepSize
        /// <summary>
        /// 依赖属性 - 步长
        /// </summary>
        public float StepSize
        {
            get => this.GetValue(StepSizeProperty);
            set => this.SetValue(StepSizeProperty, value);
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

        #region 依赖属性 - 透明度阈值 —— float OpacityThreshold
        /// <summary>
        /// 依赖属性 - 透明度阈值
        /// </summary>
        public float OpacityThreshold
        {
            get => this.GetValue(OpacityThresholdProperty);
            set => this.SetValue(OpacityThresholdProperty, value);
        }
        #endregion

        #region 依赖属性 - 传输函数控制列表 —— AvaloniaList<TFControlPoint> TFControlPoints
        /// <summary>
        /// 依赖属性 - 传输函数控制列表
        /// </summary>
        public AvaloniaList<TFControlPoint> TFControlPoints
        {
            get => this.GetValue(TFControlPointsProperty);
            set => this.SetValue(TFControlPointsProperty, value);
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

        #region 只读属性 - 体积渲染器 —— VolumeRenderer VolumeRenderer
        /// <summary>
        /// 只读属性 - 体积渲染器
        /// </summary>
        public VolumeRenderer VolumeRenderer
        {
            get => this._volumeRenderer;
        }
        #endregion

        #region 只读属性 - 线框渲染器 —— WireframeRenderer WireframeRenderer
        /// <summary>
        /// 只读属性 - 线框渲染器
        /// </summary>
        public WireframeRenderer WireframeRenderer
        {
            get => this._wireframeRenderer;
        }
        #endregion

        #endregion

        #region # 方法

        #region OpenTK初始化事件 —— override void OnOpenTKInit()
        /// <summary>
        /// OpenTK初始化事件
        /// </summary>
        protected override void OnOpenTKInit()
        {
            //InputManger默认值
            if (this.InputManager == null && this.Camera is OrbitCamera orbitCamera)
            {
                this.InputManager = new OrbitInputManager(orbitCamera);
            }

            //初始化体积渲染器
            this._volumeRenderer = new VolumeRenderer(this.Camera);
            this._volumeRenderer.SetWindowLevel(this.WindowWidth, this.WindowCenter);
            this._volumeRenderer.SetMaterialOptions(this.Brightness, this.DensityScale);
            this._volumeRenderer.SetSamplingOptions(this.StepSize, this.MaxStepsCount, this.OpacityThreshold);
            this._volumeRenderer.TransferFunction.InitFromControlPoints(this.TFControlPoints);

            //初始化线框渲染器
            this._wireframeRenderer = new WireframeRenderer(this.Camera);
            foreach (BoundingVisual3D visual3D in this.Children)
            {
                this._wireframeRenderer.AppendItem(visual3D.Renderable);
            }
        }
        #endregion

        #region OpenTK渲染事件 —— override void OnOpenTKRender(PixelSize viewportSize)
        /// <summary>
        /// OpenTK渲染事件
        /// </summary>
        /// <param name="viewportSize">视口尺寸</param>
        protected override void OnOpenTKRender(PixelSize viewportSize)
        {
            //开启深度测试
            GL.Enable(EnableCap.DepthTest);

            //启用混合
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            //禁用面剔除
            GL.Disable(EnableCap.CullFace);

            if (this._volumeRenderable != null)
            {
                this._volumeRenderer.SetRenderable(this._volumeRenderable);
                this._volumeRenderer.RenderFrame(viewportSize.Width, viewportSize.Height);
            }

            //this._wireframeRenderer.RenderFrame(viewportSize.Width, viewportSize.Height);
        }
        #endregion

        #region OpenTK卸载事件 —— override void OnOpenTKDeinit()
        /// <summary>
        /// OpenTK卸载事件
        /// </summary>
        protected override void OnOpenTKDeinit()
        {
            this._volumeRenderer?.Dispose();
            this._wireframeRenderer?.Dispose();
        }
        #endregion 

        #region 窗宽改变事件 —— static void OnWindowWidthChanged(VolumeViewport viewport...
        /// <summary>
        /// 窗宽改变事件
        /// </summary>
        private static void OnWindowWidthChanged(VolumeViewport viewport, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            viewport._volumeRenderer?.SetWindowLevel(eventArgs.NewValue.Value, viewport.WindowWidth);
        }
        #endregion

        #region 窗位改变事件 —— static void OnWindowCenterChanged(VolumeViewport viewport...
        /// <summary>
        /// 窗位改变事件
        /// </summary>
        private static void OnWindowCenterChanged(VolumeViewport viewport, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            viewport._volumeRenderer?.SetWindowLevel(viewport.WindowCenter, eventArgs.NewValue.Value);
        }
        #endregion

        #region 亮度改变事件 —— static void OnBrightnessChanged(VolumeViewport viewport...
        /// <summary>
        /// 亮度改变事件
        /// </summary>
        private static void OnBrightnessChanged(VolumeViewport viewport, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            viewport._volumeRenderer?.SetMaterialOptions(eventArgs.NewValue.Value, viewport.DensityScale);
        }
        #endregion

        #region 密度缩放改变事件 —— static void OnDensityScaleChanged(VolumeViewport viewport...
        /// <summary>
        /// 密度缩放改变事件
        /// </summary>
        private static void OnDensityScaleChanged(VolumeViewport viewport, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            viewport._volumeRenderer?.SetMaterialOptions(viewport.Brightness, eventArgs.NewValue.Value);
        }
        #endregion

        #region 步长改变事件 —— static void OnStepSizeChanged(VolumeViewport viewport...
        /// <summary>
        /// 步长改变事件
        /// </summary>
        private static void OnStepSizeChanged(VolumeViewport viewport, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            viewport._volumeRenderer?.SetSamplingOptions(eventArgs.NewValue.Value, viewport.MaxStepsCount, viewport.OpacityThreshold);
        }
        #endregion

        #region 最大步数改变事件 —— static void OnMaxStepsCountChanged(VolumeViewport viewport...
        /// <summary>
        /// 最大步数改变事件
        /// </summary>
        private static void OnMaxStepsCountChanged(VolumeViewport viewport, AvaloniaPropertyChangedEventArgs<int> eventArgs)
        {
            viewport._volumeRenderer?.SetSamplingOptions(viewport.StepSize, eventArgs.NewValue.Value, viewport.OpacityThreshold);
        }
        #endregion

        #region 透明度阈值改变事件 —— static void OnOpacityThresholdChanged(VolumeViewport viewport...
        /// <summary>
        /// 透明度阈值改变事件
        /// </summary>
        private static void OnOpacityThresholdChanged(VolumeViewport viewport, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            viewport._volumeRenderer?.SetSamplingOptions(viewport.StepSize, viewport.MaxStepsCount, eventArgs.NewValue.Value);
        }
        #endregion

        #region 传输函数控制点列表改变事件 —— static void OnTFControlPointsChanged(VolumeViewport viewport...
        /// <summary>
        /// 传输函数控制点列表改变事件
        /// </summary>
        private static void OnTFControlPointsChanged(VolumeViewport viewport, AvaloniaPropertyChangedEventArgs<AvaloniaList<TFControlPoint>> eventArgs)
        {
            if (eventArgs.OldValue.Value != null)
            {
                //清除旧元素
                foreach (TFControlPoint controlPoint in eventArgs.OldValue.Value)
                {
                    viewport._volumeRenderer?.TransferFunction.RemoveControlPoint(controlPoint);
                }
                eventArgs.OldValue.Value.CollectionChanged -= viewport.OnTFControlPointsItemChanged;
            }
            if (eventArgs.NewValue.Value != null)
            {
                //添加新元素
                foreach (TFControlPoint controlPoint in eventArgs.NewValue.Value)
                {
                    viewport._volumeRenderer?.TransferFunction.AddControlPoint(controlPoint);
                }

                eventArgs.NewValue.Value.CollectionChanged += viewport.OnTFControlPointsItemChanged;
            }
            if (eventArgs.NewValue.Value == null)
            {
                //清空旧元素
                viewport._volumeRenderer?.TransferFunction.ClearControlPoints();
            }
        }
        #endregion

        #region 传输函数控制点列表元素改变事件 —— void OnTFControlPointsItemChanged(object sender...
        /// <summary>
        /// 传输函数控制点列表元素改变事件
        /// </summary>
        private void OnTFControlPointsItemChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            if (eventArgs.OldItems != null)
            {
                //清除旧元素
                foreach (TFControlPoint controlPoint in eventArgs.OldItems)
                {
                    this._volumeRenderer?.TransferFunction.RemoveControlPoint(controlPoint);
                }
            }
            if (eventArgs.NewItems != null)
            {
                //添加新元素
                foreach (TFControlPoint controlPoint in eventArgs.NewItems)
                {
                    this._volumeRenderer?.TransferFunction.AddControlPoint(controlPoint);
                }
            }
            if (eventArgs.Action == NotifyCollectionChangedAction.Reset)
            {
                //清空元素
                this._volumeRenderer?.TransferFunction.ClearControlPoints();
            }
        }
        #endregion

        #region 体积数据改变事件 —— static void OnVolumeDataChanged(VolumeViewport viewport...
        /// <summary>
        /// 体积数据改变事件
        /// </summary>
        private static void OnVolumeDataChanged(VolumeViewport viewport, AvaloniaPropertyChangedEventArgs<VolumeData> eventArgs)
        {
            #region # 验证

            if (eventArgs.NewValue == null)
            {
                //TODO 卸载数据，黑屏处理，停止渲染
                return;
            }

            #endregion

            VolumeData volumeData = eventArgs.NewValue.Value;
            if (!ResourceManager.Texture3Ds.ContainsKey(volumeData.Id))
            {
                viewport.GlContext.MakeCurrent();
                Texture3D volumeTexture = Texture3D.CreateFromVolume(volumeData.VoxelSize.Width, volumeData.VoxelSize.Height, volumeData.VoxelSize.Depth, volumeData.OriginalData);
                viewport._volumeRenderable = new VolumeRenderable(volumeTexture, volumeData.VoxelSize.ToGlmVector3(), volumeData.Spacing.ToGlmVector3(), volumeData.ActualSize.ToGlmVector3(), volumeData.VolumeScale.ToGlmVector3(), volumeData.RescaleSlope, volumeData.RescaleIntercept, volumeData.Origin.ToGlmVector3(), volumeData.RowDirection.ToGlmVector3(), volumeData.ColDirection.ToGlmVector3(), volumeData.SliceDirection.ToGlmVector3());
                viewport.WindowWidth = volumeData.WindowWidth;
                viewport.WindowCenter = volumeData.WindowCenter;

                ResourceManager.AddTexture3D(volumeData.Id, volumeTexture);
            }
            else
            {
                Texture3D volumeTexture = ResourceManager.Texture3Ds[volumeData.Id];
                viewport._volumeRenderable = new VolumeRenderable(volumeTexture, volumeData.VoxelSize.ToGlmVector3(), volumeData.Spacing.ToGlmVector3(), volumeData.ActualSize.ToGlmVector3(), volumeData.VolumeScale.ToGlmVector3(), volumeData.RescaleSlope, volumeData.RescaleIntercept, volumeData.Origin.ToGlmVector3(), volumeData.RowDirection.ToGlmVector3(), volumeData.ColDirection.ToGlmVector3(), volumeData.SliceDirection.ToGlmVector3());
                viewport.WindowWidth = volumeData.WindowWidth;
                viewport.WindowCenter = volumeData.WindowCenter;
            }
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #endregion
    }
}
