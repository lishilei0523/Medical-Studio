using Avalonia;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.InputManagers;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Engine.Managers;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Renderers;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Cameras;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace MedicalSharp.Controls.Viewports
{
    /// <summary>
    /// MPR渲染视口
    /// </summary>
    public class MPRViewport : BasicViewport, IPickVoxel
    {
        #region # 字段及构造器

        /// <summary>
        /// 平面依赖属性
        /// </summary>
        public static readonly StyledProperty<MPRPlane> PlaneProperty;

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
        static MPRViewport()
        {
            PlaneProperty = AvaloniaProperty.Register<MPRViewport, MPRPlane>(nameof(Plane));
            WindowWidthProperty = AvaloniaProperty.Register<MPRViewport, float>(nameof(WindowWidth), 400.0f);
            WindowCenterProperty = AvaloniaProperty.Register<MPRViewport, float>(nameof(WindowCenter), 40.0f);
            BrightnessProperty = AvaloniaProperty.Register<MPRViewport, float>(nameof(Brightness), 1.0f);
            ContrastProperty = AvaloniaProperty.Register<MPRViewport, float>(nameof(Contrast), 1.0f);
            VolumeDataProperty = AvaloniaProperty.Register<MPRViewport, VolumeData>(nameof(VolumeData));

            //属性改变事件
            PlaneProperty.Changed.AddClassHandler<MPRViewport, MPRPlane>(OnPlaneChanged);
            WindowWidthProperty.Changed.AddClassHandler<MPRViewport, float>(OnWindowWidthChanged);
            WindowCenterProperty.Changed.AddClassHandler<MPRViewport, float>(OnWindowCenterChanged);
            BrightnessProperty.Changed.AddClassHandler<MPRViewport, float>(OnBrightnessChanged);
            ContrastProperty.Changed.AddClassHandler<MPRViewport, float>(OnContrastChanged);
            VolumeDataProperty.Changed.AddClassHandler<MPRViewport, VolumeData>(OnVolumeDataChanged);
        }

        /// <summary>
        /// 体积渲染对象
        /// </summary>
        private VolumeRenderable _volumeRenderable;

        /// <summary>
        /// MPR渲染器
        /// </summary>
        private MPRRenderer _mprRenderer;

        /// <summary>
        /// 默认构造器
        /// </summary>
        public MPRViewport()
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

        #region 只读属性 - 体积渲染对象 —— VolumeRenderable VolumeRenderable
        /// <summary>
        /// 只读属性 - 体积渲染对象
        /// </summary>
        public VolumeRenderable VolumeRenderable
        {
            get => this._volumeRenderable;
        }
        #endregion

        #region 只读属性 - MPR相机 —— MPRCamera MPRCamera
        /// <summary>
        /// 只读属性 - MPR相机
        /// </summary>
        public MPRCamera MPRCamera
        {
            get => (MPRCamera)this.Camera;
        }
        #endregion

        #region 只读属性 - MPR渲染器 —— MPRRenderer MPRRenderer
        /// <summary>
        /// 只读属性 - MPR渲染器
        /// </summary>
        public MPRRenderer MPRRenderer
        {
            get => this._mprRenderer;
        }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 反投影 —— override Ray UnProject(Vector2 screenPos2D)
        /// <summary>
        /// 反投影
        /// </summary>
        /// <param name="screenPos2D">屏幕2D位置</param>
        /// <returns>射线</returns>
        public override Ray UnProject(Vector2 screenPos2D)
        {
            Vector2? planeUV = this._mprRenderer.Plane.ScreenToPlaneUV(screenPos2D, this.Camera.LookDirection, this._viewportSize.ToVector2(), this.Camera.ProjectionMatrix, this.Camera.ViewMatrix, out Ray ray);
            if (planeUV.HasValue)
            {
                return ray;
            }

            return base.UnProject(screenPos2D);
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
            this.GlContext.MakeCurrent();

            textureCoord = Vector3.Zero;
            worldPosition = Vector3.Zero;
            voxelPosition = Vector3i.Zero;
            voxelValue = -1;
            markValue = 0;

            Vector2? planeUV = this._mprRenderer.Plane.ScreenToPlaneUV(position, this.Camera.LookDirection, this._viewportSize.ToVector2(), this.Camera.ProjectionMatrix, this.Camera.ViewMatrix, out ray);
            if (planeUV.HasValue)
            {
                voxelPosition = this._mprRenderer.Plane.GetVoxelPosition(planeUV.Value.X, planeUV.Value.Y, out Vector3 texCoord);
                textureCoord = texCoord;
                worldPosition = (textureCoord - new Vector3(0.5f)) * this.VolumeData.Metadata.VolumeScale;
                voxelValue = this.VolumeData[voxelPosition.X, voxelPosition.Y, voxelPosition.Z];
                markValue = this.VolumeData.GetMarkValue(voxelPosition.X, voxelPosition.Y, voxelPosition.Z);

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
            //InputManger默认值
            if (this.InputManager == null)
            {
                this.InputManager = new MPRInputManager(this.MPRCamera);
            }

            //初始化形状、文本渲染器
            this._shapeRenderer = new ShapeRenderer(this.MPRCamera);
            this._textRenderer = new TextRenderer(this.MPRCamera);

            //初始化MPR渲染器
            this._mprRenderer = new MPRRenderer(this.MPRCamera);
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
                //禁用面剔除
                GL.Disable(EnableCap.CullFace);

                //启用混合
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                //关闭深度
                GL.DepthMask(false);

                //MPR渲染
                this._mprRenderer.BindPlane(this.Plane);
                this._mprRenderer.SetRenderable(this._volumeRenderable);
                this._mprRenderer.RenderFrame(viewportSize.Width, viewportSize.Height);

                //形状、文本渲染
                base.OnOpenTKRender(viewportSize);
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
            this._mprRenderer?.Dispose();
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
                if (shapeVisual3D.IsOnPlane(this.Plane))
                {
                    if (shapeVisual3D is IPureVisual3D pureVisual3D)
                    {
                        PolylineVisual3D polygon = pureVisual3D.CreateSectionPolygon(this.Plane);
                        if (polygon != null)
                        {
                            polygon.EnsureRenderable();
                            shapeVisual3Ds.Add(polygon);
                        }
                    }
                    else
                    {
                        shapeVisual3Ds.Add(shapeVisual3D);
                    }
                }
            }

            return shapeVisual3Ds;
        }
        #endregion

        #region 获取文本3D元素列表 —— override List<TextVisual3D> GetTextVisual3Ds()
        /// <summary>
        /// 获取文本3D元素列表
        /// </summary>
        /// <returns>文本3D元素列表</returns>
        protected override List<TextVisual3D> GetTextVisual3Ds()
        {
            List<TextVisual3D> textVisual3Ds = [];
            foreach (TextVisual3D textVisual3D in base.GetTextVisual3Ds())
            {
                if (textVisual3D.IsOnPlane(this.Plane))
                {
                    textVisual3Ds.Add(textVisual3D);
                }
            }

            return textVisual3Ds;
        }
        #endregion


        //Events

        #region 平面改变事件 —— static void OnPlaneChanged(MPRViewport viewport...
        /// <summary>
        /// 平面改变事件
        /// </summary>
        private static void OnPlaneChanged(MPRViewport viewport, AvaloniaPropertyChangedEventArgs<MPRPlane> eventArgs)
        {
            viewport._mprRenderer?.BindPlane(eventArgs.NewValue.Value);
        }
        #endregion

        #region 窗宽改变事件 —— static void OnWindowWidthChanged(MPRViewport viewport...
        /// <summary>
        /// 窗宽改变事件
        /// </summary>
        private static void OnWindowWidthChanged(MPRViewport viewport, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            viewport._mprRenderer?.SetWindowLevel(eventArgs.NewValue.Value, viewport.WindowWidth);
        }
        #endregion

        #region 窗位改变事件 —— static void OnWindowCenterChanged(MPRViewport viewport...
        /// <summary>
        /// 窗位改变事件
        /// </summary>
        private static void OnWindowCenterChanged(MPRViewport viewport, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            viewport._mprRenderer?.SetWindowLevel(viewport.WindowCenter, eventArgs.NewValue.Value);
        }
        #endregion

        #region 亮度改变事件 —— static void OnBrightnessChanged(MPRViewport viewport...
        /// <summary>
        /// 亮度改变事件
        /// </summary>
        private static void OnBrightnessChanged(MPRViewport viewport, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            viewport._mprRenderer?.SetMaterialOptions(eventArgs.NewValue.Value, viewport.Contrast);
        }
        #endregion

        #region 对比度改变事件 —— static void OnContrastChanged(MPRViewport viewport...
        /// <summary>
        /// 对比度改变事件
        /// </summary>
        private static void OnContrastChanged(MPRViewport viewport, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            viewport._mprRenderer?.SetMaterialOptions(viewport.Brightness, eventArgs.NewValue.Value);
        }
        #endregion

        #region 体积数据改变事件 —— static void OnVolumeDataChanged(MPRViewport viewport...
        /// <summary>
        /// 体积数据改变事件
        /// </summary>
        private static void OnVolumeDataChanged(MPRViewport viewport, AvaloniaPropertyChangedEventArgs<VolumeData> eventArgs)
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
            }
            else
            {
                volumeSession = SessionManager.VolumeSessions[volumeData.Metadata.Id];
            }

            viewport._volumeRenderable = new VolumeRenderable(volumeSession.VolumeTexture, volumeSession.MarkTexture, volumeData);
            viewport._mprRenderer.SetTransferFunction(volumeSession.MPRTransferFunction);
            viewport._mprRenderer.SetMarkStrategy(volumeSession.MarkStrategy);
            //viewport._mprRenderer.TransferFunction.InitFromControlPoints(viewport.TFControlPoints); TODO 伪彩
            if (volumeData.Metadata.WindowWidth.HasValue)
            {
                viewport.WindowWidth = volumeData.Metadata.WindowWidth.Value;
            }
            if (volumeData.Metadata.WindowCenter.HasValue)
            {
                viewport.WindowCenter = volumeData.Metadata.WindowCenter.Value;
            }

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #endregion
    }
}
