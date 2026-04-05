using Avalonia;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Inputs;
using MedicalSharp.Dicoms.Models;
using MedicalSharp.Engine.Cameras;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Renderers;
using MedicalSharp.Engine.Resources;
using OpenTK.Graphics.OpenGL4;

namespace MedicalSharp.Controls.Viewports
{
    /// <summary>
    /// MPR渲染视口
    /// </summary>
    public class MPRViewport2 : OpenTKViewport
    {
        #region # 字段及构造器

        /// <summary>
        /// 平面依赖属性
        /// </summary>
        public new static readonly StyledProperty<MPRPlane> PlaneProperty;

        /// <summary>
        /// 相机依赖属性
        /// </summary>
        public new static readonly StyledProperty<MPRCamera2> CameraProperty;

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
        /// 对比度依赖属性
        /// </summary>
        public static readonly StyledProperty<float> ContrastProperty;

        /// <summary>
        /// 体积数据依赖属性
        /// </summary>
        public static readonly StyledProperty<VolumeData> VolumeDataProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static MPRViewport2()
        {
            PlaneProperty = AvaloniaProperty.Register<MPRViewport2, MPRPlane>(nameof(Plane));
            CameraProperty = AvaloniaProperty.Register<MPRViewport2, MPRCamera2>(nameof(Camera));
            WindowWidthProperty = AvaloniaProperty.Register<MPRViewport2, float>(nameof(WindowWidth), 400.0f);
            WindowCenterProperty = AvaloniaProperty.Register<MPRViewport2, float>(nameof(WindowCenter), 40.0f);
            BrightnessProperty = AvaloniaProperty.Register<MPRViewport2, float>(nameof(Brightness), 1.0f);
            ContrastProperty = AvaloniaProperty.Register<MPRViewport2, float>(nameof(Contrast), 1.0f);
            VolumeDataProperty = AvaloniaProperty.Register<MPRViewport2, VolumeData>(nameof(VolumeData));

            //属性改变事件
            WindowWidthProperty.Changed.AddClassHandler<MPRViewport2, float>(OnWindowWidthChanged);
            WindowCenterProperty.Changed.AddClassHandler<MPRViewport2, float>(OnWindowCenterChanged);
            BrightnessProperty.Changed.AddClassHandler<MPRViewport2, float>(OnBrightnessChanged);
            ContrastProperty.Changed.AddClassHandler<MPRViewport2, float>(OnContrastChanged);
            VolumeDataProperty.Changed.AddClassHandler<MPRViewport2, VolumeData>(OnVolumeDataChanged);
        }

        /// <summary>
        /// 体积渲染对象
        /// </summary>
        private VolumeRenderable _volumeRenderable;

        /// <summary>
        /// MPR渲染器
        /// </summary>
        private MPRRenderer2 _mprRenderer;

        /// <summary>
        /// 默认构造器
        /// </summary>
        public MPRViewport2()
        {

        }

        #endregion

        #region # 属性

        #region 依赖属性 - 平面 —— MPRPlane Plane
        /// <summary>
        /// 依赖属性 - 平面
        /// </summary>
        public MPRPlane Plane
        {
            get => this.GetValue(PlaneProperty);
            set => this.SetValue(PlaneProperty, value);
        }
        #endregion

        #region 依赖属性 - 相机 —— new MPRCamera2 Camera
        /// <summary>
        /// 依赖属性 - 相机
        /// </summary>
        public new MPRCamera2 Camera
        {
            get => this.GetValue(CameraProperty);
            set => this.SetValue(CameraProperty, value);
        }
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

        #region 只读属性 - MPR渲染器 —— MPRRenderer2 MPRRenderer
        /// <summary>
        /// 只读属性 - MPR渲染器
        /// </summary>
        public MPRRenderer2 MPRRenderer
        {
            get => this._mprRenderer;
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
            if (this.InputManager == null)
            {
                this.InputManager = new MPRInputManager2(this.Camera);
            }

            //初始化体积渲染器
            this._mprRenderer = new MPRRenderer2(this.Camera);
            this._mprRenderer.BindPlane(this.Plane);
            this._mprRenderer.SetWindowLevel(this.WindowWidth, this.WindowCenter);
            this._mprRenderer.SetMaterialOptions(this.Brightness, this.Contrast);
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
                //开启深度测试
                GL.Enable(EnableCap.DepthTest);

                //关闭混合
                GL.Disable(EnableCap.Blend);

                this._mprRenderer.SetRenderable(this._volumeRenderable);
                this._mprRenderer.RenderFrame(viewportSize.Width, viewportSize.Height);
            }
        }
        #endregion

        #region OpenTK卸载事件 —— override void OnOpenTKDeinit()
        /// <summary>
        /// OpenTK卸载事件
        /// </summary>
        protected override void OnOpenTKDeinit()
        {
            this._mprRenderer?.Dispose();
        }
        #endregion 

        #region 窗宽改变事件 —— static void OnWindowWidthChanged(MPRViewport2 viewport...
        /// <summary>
        /// 窗宽改变事件
        /// </summary>
        private static void OnWindowWidthChanged(MPRViewport2 viewport, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            viewport._mprRenderer?.SetWindowLevel(eventArgs.NewValue.Value, viewport.WindowWidth);
        }
        #endregion

        #region 窗位改变事件 —— static void OnWindowCenterChanged(MPRViewport2 viewport...
        /// <summary>
        /// 窗位改变事件
        /// </summary>
        private static void OnWindowCenterChanged(MPRViewport2 viewport, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            viewport._mprRenderer?.SetWindowLevel(viewport.WindowCenter, eventArgs.NewValue.Value);
        }
        #endregion

        #region 亮度改变事件 —— static void OnBrightnessChanged(MPRViewport2 viewport...
        /// <summary>
        /// 亮度改变事件
        /// </summary>
        private static void OnBrightnessChanged(MPRViewport2 viewport, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            viewport._mprRenderer?.SetMaterialOptions(eventArgs.NewValue.Value, viewport.Contrast);
        }
        #endregion

        #region 对比度改变事件 —— static void OnContrastChanged(MPRViewport2 viewport...
        /// <summary>
        /// 对比度改变事件
        /// </summary>
        private static void OnContrastChanged(MPRViewport2 viewport, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            viewport._mprRenderer?.SetMaterialOptions(viewport.Brightness, eventArgs.NewValue.Value);
        }
        #endregion

        #region 体积数据改变事件 —— static void OnVolumeDataChanged(MPRViewport2 viewport...
        /// <summary>
        /// 体积数据改变事件
        /// </summary>
        private static void OnVolumeDataChanged(MPRViewport2 viewport, AvaloniaPropertyChangedEventArgs<VolumeData> eventArgs)
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
            if (!ResourceManager.Texture3Ds.ContainsKey(volumeData.Id))
            {
                viewport.GlContext.MakeCurrent();
                Texture3D volumeTexture = Texture3D.CreateFromVolume(volumeData.VolumeSize.Width, volumeData.VolumeSize.Height, volumeData.VolumeSize.Depth, volumeData.OriginalData);
                viewport._volumeRenderable = new VolumeRenderable(volumeTexture, volumeData.VolumeSize.ToGlmVector3(), volumeData.Spacing.ToGlmVector3(), volumeData.PhysicalSize.ToGlmVector3(), volumeData.VolumeScale.ToGlmVector3(), volumeData.RescaleSlope, volumeData.RescaleIntercept, volumeData.Origin.ToGlmVector3(), volumeData.RowDirection.ToGlmVector3(), volumeData.ColDirection.ToGlmVector3(), volumeData.SliceDirection.ToGlmVector3());
                viewport.WindowWidth = volumeData.WindowWidth;
                viewport.WindowCenter = volumeData.WindowCenter;

                ResourceManager.AddTexture3D(volumeData.Id, volumeTexture);
            }
            else
            {
                Texture3D volumeTexture = ResourceManager.Texture3Ds[volumeData.Id];
                viewport._volumeRenderable = new VolumeRenderable(volumeTexture, volumeData.VolumeSize.ToGlmVector3(), volumeData.Spacing.ToGlmVector3(), volumeData.PhysicalSize.ToGlmVector3(), volumeData.VolumeScale.ToGlmVector3(), volumeData.RescaleSlope, volumeData.RescaleIntercept, volumeData.Origin.ToGlmVector3(), volumeData.RowDirection.ToGlmVector3(), volumeData.ColDirection.ToGlmVector3(), volumeData.SliceDirection.ToGlmVector3());
                viewport.WindowWidth = volumeData.WindowWidth;
                viewport.WindowCenter = volumeData.WindowCenter;
            }
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #endregion
    }
}
