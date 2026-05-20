using Avalonia;
using Avalonia.Collections;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Engine.Managers;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Renderers;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Managers;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Specialized;

namespace MedicalSharp.Controls.Viewports
{
    /// <summary>
    /// 体积渲染视口
    /// </summary>
    public class VolumeViewport : BasicViewport, IPickVoxel
    {
        #region # 字段及构造器

        /// <summary>
        /// 预览模式依赖属性
        /// </summary>
        public static readonly StyledProperty<PreviewMode> PreviewModeProperty;

        /// <summary>
        /// 渲染模式依赖属性
        /// </summary>
        public static readonly StyledProperty<VolumeRenderMode> RenderModeProperty;

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
        /// 传递函数控制点列表依赖属性
        /// </summary>
        public static readonly StyledProperty<AvaloniaList<DensityControlPoint>> TFControlPointsProperty;

        /// <summary>
        /// 是否开启深度写入依赖属性
        /// </summary>
        public static readonly StyledProperty<bool> DepthMaskEnabledProperty;

        /// <summary>
        /// 体积数据依赖属性
        /// </summary>
        public static readonly StyledProperty<VolumeData> VolumeDataProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static VolumeViewport()
        {
            PreviewModeProperty = AvaloniaProperty.Register<VolumeViewport, PreviewMode>(nameof(PreviewMode));
            RenderModeProperty = AvaloniaProperty.Register<VolumeViewport, VolumeRenderMode>(nameof(RenderMode));
            WindowWidthProperty = AvaloniaProperty.Register<VolumeViewport, int>(nameof(WindowWidth), 400);
            WindowCenterProperty = AvaloniaProperty.Register<VolumeViewport, int>(nameof(WindowCenter), 40);
            BrightnessProperty = AvaloniaProperty.Register<VolumeViewport, float>(nameof(Brightness), 1.0f);
            DensityScaleProperty = AvaloniaProperty.Register<VolumeViewport, float>(nameof(DensityScale), 1.0f);
            StepSizeProperty = AvaloniaProperty.Register<VolumeViewport, float>(nameof(StepSize), 0.0012f);
            MaxStepsCountProperty = AvaloniaProperty.Register<VolumeViewport, int>(nameof(MaxStepsCount), 1000);
            OpacityThresholdProperty = AvaloniaProperty.Register<VolumeViewport, float>(nameof(OpacityThreshold), 0.99f);
            TFControlPointsProperty = AvaloniaProperty.Register<VolumeViewport, AvaloniaList<DensityControlPoint>>(nameof(TFControlPoints));
            DepthMaskEnabledProperty = AvaloniaProperty.Register<VolumeViewport, bool>(nameof(DepthMaskEnabled), false);
            VolumeDataProperty = AvaloniaProperty.Register<VolumeViewport, VolumeData>(nameof(VolumeData));

            //属性改变事件
            PreviewModeProperty.Changed.AddClassHandler<VolumeViewport, PreviewMode>(OnPreviewModeChanged);
            RenderModeProperty.Changed.AddClassHandler<VolumeViewport, VolumeRenderMode>(OnRenderModeChanged);
            WindowWidthProperty.Changed.AddClassHandler<VolumeViewport, int>(OnWindowWidthChanged);
            WindowCenterProperty.Changed.AddClassHandler<VolumeViewport, int>(OnWindowCenterChanged);
            BrightnessProperty.Changed.AddClassHandler<VolumeViewport, float>(OnBrightnessChanged);
            DensityScaleProperty.Changed.AddClassHandler<VolumeViewport, float>(OnDensityScaleChanged);
            StepSizeProperty.Changed.AddClassHandler<VolumeViewport, float>(OnStepSizeChanged);
            MaxStepsCountProperty.Changed.AddClassHandler<VolumeViewport, int>(OnMaxStepsCountChanged);
            OpacityThresholdProperty.Changed.AddClassHandler<VolumeViewport, float>(OnOpacityThresholdChanged);
            TFControlPointsProperty.Changed.AddClassHandler<VolumeViewport, AvaloniaList<DensityControlPoint>>(OnTFControlPointsChanged);
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
        /// 默认构造器
        /// </summary>
        public VolumeViewport()
        {

        }

        #endregion

        #region # 属性

        #region 依赖属性 - 预览模式 —— PreviewMode PreviewMode
        /// <summary>
        /// 依赖属性 - 预览模式
        /// </summary>
        public PreviewMode PreviewMode
        {
            get => this.GetValue(PreviewModeProperty);
            set => this.SetValue(PreviewModeProperty, value);
        }
        #endregion

        #region 依赖属性 - 渲染模式 —— VolumeRenderMode RenderMode
        /// <summary>
        /// 依赖属性 - 渲染模式
        /// </summary>
        public VolumeRenderMode RenderMode
        {
            get => this.GetValue(RenderModeProperty);
            set => this.SetValue(RenderModeProperty, value);
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

        #region 依赖属性 - 传递函数控制点列表 —— AvaloniaList<DensityControlPoint> TFControlPoints
        /// <summary>
        /// 依赖属性 - 传递函数控制点列表
        /// </summary>
        public AvaloniaList<DensityControlPoint> TFControlPoints
        {
            get => this.GetValue(TFControlPointsProperty);
            set => this.SetValue(TFControlPointsProperty, value);
        }
        #endregion

        #region 依赖属性 - 是否开启深度写入 —— bool DepthMaskEnabled
        /// <summary>
        /// 依赖属性 - 是否开启深度写入
        /// </summary>
        public bool DepthMaskEnabled
        {
            get => this.GetValue(DepthMaskEnabledProperty);
            set => this.SetValue(DepthMaskEnabledProperty, value);
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

        #region 只读属性 - 体积渲染器 —— VolumeRenderer VolumeRenderer
        /// <summary>
        /// 只读属性 - 体积渲染器
        /// </summary>
        public VolumeRenderer VolumeRenderer
        {
            get => this._volumeRenderer;
        }
        #endregion

        #endregion

        #region # 方法

        //Public

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

            if (this.VolumeData == null)
            {
                return false;
            }

            #endregion

            this.GlContext.MakeCurrent();
            ray = Ray.UnProject(position, this.Camera.CameraPosition, this._viewportSize.ToVector2(), this.Camera.ProjectionMatrix, this.Camera.ViewMatrix);
            Vector3i? pickedVoxelPosition = this._volumeRenderer.PickVoxel(this.GlContextHandle, ray, this._viewportSize.Width, this._viewportSize.Height, out Vector3? texCoord);
            if (pickedVoxelPosition.HasValue)
            {
                textureCoord = texCoord!.Value;
                worldPosition = (textureCoord - new Vector3(0.5f)) * this.VolumeData.Metadata.VolumeScale;
                voxelPosition = pickedVoxelPosition.Value;
                voxelValue = this.VolumeData.GetOriginalValue(voxelPosition);
                markValue = this.VolumeData.GetMarkValue(voxelPosition);

                return true;
            }

            return false;
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
            base.OnOpenTKInit();

            //初始化体积渲染器
            this._volumeRenderer = new VolumeRenderer(this.Camera);
            this._volumeRenderer.SetWindowLevel(this.WindowWidth, this.WindowCenter);
            this._volumeRenderer.SetMaterialOptions(this.Brightness, this.DensityScale);
            this._volumeRenderer.SetSamplingOptions(this.StepSize, this.MaxStepsCount, this.OpacityThreshold);
        }
        #endregion

        #region OpenTK渲染事件 —— override void OnOpenTKRender(PixelSize viewportSize)
        /// <summary>
        /// OpenTK渲染事件
        /// </summary>
        /// <param name="viewportSize">视口尺寸</param>
        protected override void OnOpenTKRender(PixelSize viewportSize)
        {
            if (this._volumeRenderable != null)
            {
                //禁用面剔除
                GL.Disable(EnableCap.CullFace);

                //启用混合
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                //开启/关闭深度写入
                GL.DepthMask(this.DepthMaskEnabled);

                //体积渲染
                this._volumeRenderer.SetRenderable(this._volumeRenderable);
                this._volumeRenderer.RenderFrame(viewportSize.Width, viewportSize.Height, this.GlContextHandle);

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
            this._volumeRenderer?.Dispose();
        }
        #endregion


        //Events

        #region 预览模式改变事件 —— static void OnPreviewModeChanged(VolumeViewport viewport...
        /// <summary>
        /// 预览模式改变事件
        /// </summary>
        private static void OnPreviewModeChanged(VolumeViewport viewport, AvaloniaPropertyChangedEventArgs<PreviewMode> eventArgs)
        {
            viewport._volumeRenderer?.SwitchPreviewMode(eventArgs.NewValue.Value);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 渲染模式改变事件 —— static void OnRenderModeChanged(VolumeViewport viewport...
        /// <summary>
        /// 渲染模式改变事件
        /// </summary>
        private static void OnRenderModeChanged(VolumeViewport viewport, AvaloniaPropertyChangedEventArgs<VolumeRenderMode> eventArgs)
        {
            viewport._volumeRenderer?.SwitchRenderMode(eventArgs.NewValue.Value);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 窗宽改变事件 —— static void OnWindowWidthChanged(VolumeViewport viewport...
        /// <summary>
        /// 窗宽改变事件
        /// </summary>
        private static void OnWindowWidthChanged(VolumeViewport viewport, AvaloniaPropertyChangedEventArgs<int> eventArgs)
        {
            viewport._volumeRenderer?.SetWindowLevel(eventArgs.NewValue.Value, viewport.WindowCenter);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 窗位改变事件 —— static void OnWindowCenterChanged(VolumeViewport viewport...
        /// <summary>
        /// 窗位改变事件
        /// </summary>
        private static void OnWindowCenterChanged(VolumeViewport viewport, AvaloniaPropertyChangedEventArgs<int> eventArgs)
        {
            viewport._volumeRenderer?.SetWindowLevel(viewport.WindowWidth, eventArgs.NewValue.Value);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 亮度改变事件 —— static void OnBrightnessChanged(VolumeViewport viewport...
        /// <summary>
        /// 亮度改变事件
        /// </summary>
        private static void OnBrightnessChanged(VolumeViewport viewport, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            viewport._volumeRenderer?.SetMaterialOptions(eventArgs.NewValue.Value, viewport.DensityScale);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 密度缩放改变事件 —— static void OnDensityScaleChanged(VolumeViewport viewport...
        /// <summary>
        /// 密度缩放改变事件
        /// </summary>
        private static void OnDensityScaleChanged(VolumeViewport viewport, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            viewport._volumeRenderer?.SetMaterialOptions(viewport.Brightness, eventArgs.NewValue.Value);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 步长改变事件 —— static void OnStepSizeChanged(VolumeViewport viewport...
        /// <summary>
        /// 步长改变事件
        /// </summary>
        private static void OnStepSizeChanged(VolumeViewport viewport, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            viewport._volumeRenderer?.SetSamplingOptions(eventArgs.NewValue.Value, viewport.MaxStepsCount, viewport.OpacityThreshold);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 最大步数改变事件 —— static void OnMaxStepsCountChanged(VolumeViewport viewport...
        /// <summary>
        /// 最大步数改变事件
        /// </summary>
        private static void OnMaxStepsCountChanged(VolumeViewport viewport, AvaloniaPropertyChangedEventArgs<int> eventArgs)
        {
            viewport._volumeRenderer?.SetSamplingOptions(viewport.StepSize, eventArgs.NewValue.Value, viewport.OpacityThreshold);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 透明度阈值改变事件 —— static void OnOpacityThresholdChanged(VolumeViewport viewport...
        /// <summary>
        /// 透明度阈值改变事件
        /// </summary>
        private static void OnOpacityThresholdChanged(VolumeViewport viewport, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            viewport._volumeRenderer?.SetSamplingOptions(viewport.StepSize, viewport.MaxStepsCount, eventArgs.NewValue.Value);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 传递函数控制点列表改变事件 —— static void OnTFControlPointsChanged(VolumeViewport viewport...
        /// <summary>
        /// 传递函数控制点列表改变事件
        /// </summary>
        private static void OnTFControlPointsChanged(VolumeViewport viewport, AvaloniaPropertyChangedEventArgs<AvaloniaList<DensityControlPoint>> eventArgs)
        {
            if (eventArgs.OldValue.Value != null)
            {
                //清除旧元素
                foreach (DensityControlPoint controlPoint in eventArgs.OldValue.Value)
                {
                    viewport._volumeRenderer?.TransferFunction.RemoveControlPoint(controlPoint);
                }
                eventArgs.OldValue.Value.CollectionChanged -= viewport.OnTFControlPointsItemChanged;
            }
            if (eventArgs.NewValue.Value != null)
            {
                //添加新元素
                foreach (DensityControlPoint controlPoint in eventArgs.NewValue.Value)
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
                //清除旧元素
                foreach (DensityControlPoint controlPoint in eventArgs.OldItems)
                {
                    this._volumeRenderer?.TransferFunction.RemoveControlPoint(controlPoint);
                }
            }
            if (eventArgs.NewItems != null)
            {
                //添加新元素
                foreach (DensityControlPoint controlPoint in eventArgs.NewItems)
                {
                    this._volumeRenderer?.TransferFunction.AddControlPoint(controlPoint);
                }
            }
            if (eventArgs.Action == NotifyCollectionChangedAction.Reset)
            {
                //清空元素
                this._volumeRenderer?.TransferFunction.ClearControlPoints();
            }

            //请求下一帧
            this.RequestNextFrameRendering();
        }
        #endregion

        #region 体积数据改变事件 —— static void OnVolumeDataChanged(VolumeViewport viewport...
        /// <summary>
        /// 体积数据改变事件
        /// </summary>
        private static void OnVolumeDataChanged(VolumeViewport viewport, AvaloniaPropertyChangedEventArgs<VolumeData> eventArgs)
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

            viewport._volumeRenderable = new VolumeRenderable(volumeSession.OriginalTexture, volumeSession.PreviewTexture, volumeSession.MarkTexture, volumeData);

            //初始化传递函数、标记策略
            viewport._volumeRenderer.SetTransferFunction(volumeSession.VRTransferFunction);
            viewport._volumeRenderer.SetMarkStrategy(volumeSession.MarkStrategy);
            viewport._volumeRenderer.TransferFunction.InitFromControlPoints(viewport.TFControlPoints);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #endregion
    }
}
