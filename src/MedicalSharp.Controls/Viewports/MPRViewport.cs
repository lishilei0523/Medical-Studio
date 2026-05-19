using Avalonia;
using Avalonia.Collections;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.InputManagers;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Engine.Managers;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Renderers;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Cameras;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Managers;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace MedicalSharp.Controls.Viewports
{
    /// <summary>
    /// MPR渲染视口
    /// </summary>
    public class MPRViewport : BasicViewport, IPickVoxel
    {
        #region # 字段及构造器

        /// <summary>
        /// 预览模式依赖属性
        /// </summary>
        public static readonly StyledProperty<PreviewMode> PreviewModeProperty;

        /// <summary>
        /// 渲染模式依赖属性
        /// </summary>
        public static readonly StyledProperty<MPRRenderMode> RenderModeProperty;

        /// <summary>
        /// MPR平面依赖属性
        /// </summary>
        public static readonly StyledProperty<MPRPlane> PlaneProperty;

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
        /// 传递函数控制点列表依赖属性
        /// </summary>
        public static readonly StyledProperty<AvaloniaList<HUControlPoint>> TFControlPointsProperty;

        /// <summary>
        /// 体积数据依赖属性
        /// </summary>
        public static readonly StyledProperty<VolumeData> VolumeDataProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static MPRViewport()
        {
            PreviewModeProperty = AvaloniaProperty.Register<MPRViewport, PreviewMode>(nameof(PreviewMode));
            RenderModeProperty = AvaloniaProperty.Register<MPRViewport, MPRRenderMode>(nameof(RenderMode));
            PlaneProperty = AvaloniaProperty.Register<MPRViewport, MPRPlane>(nameof(Plane));
            WindowWidthProperty = AvaloniaProperty.Register<MPRViewport, int>(nameof(WindowWidth), 400);
            WindowCenterProperty = AvaloniaProperty.Register<MPRViewport, int>(nameof(WindowCenter), 40);
            BrightnessProperty = AvaloniaProperty.Register<MPRViewport, float>(nameof(Brightness), 1.0f);
            ContrastProperty = AvaloniaProperty.Register<MPRViewport, float>(nameof(Contrast), 1.0f);
            TFControlPointsProperty = AvaloniaProperty.Register<MPRViewport, AvaloniaList<HUControlPoint>>(nameof(TFControlPoints));
            VolumeDataProperty = AvaloniaProperty.Register<MPRViewport, VolumeData>(nameof(VolumeData));

            //属性改变事件
            PreviewModeProperty.Changed.AddClassHandler<MPRViewport, PreviewMode>(OnPreviewModeChanged);
            RenderModeProperty.Changed.AddClassHandler<MPRViewport, MPRRenderMode>(OnRenderModeChanged);
            PlaneProperty.Changed.AddClassHandler<MPRViewport, MPRPlane>(OnPlaneChanged);
            WindowWidthProperty.Changed.AddClassHandler<MPRViewport, int>(OnWindowWidthChanged);
            WindowCenterProperty.Changed.AddClassHandler<MPRViewport, int>(OnWindowCenterChanged);
            BrightnessProperty.Changed.AddClassHandler<MPRViewport, float>(OnBrightnessChanged);
            ContrastProperty.Changed.AddClassHandler<MPRViewport, float>(OnContrastChanged);
            TFControlPointsProperty.Changed.AddClassHandler<MPRViewport, AvaloniaList<HUControlPoint>>(OnTFControlPointsChanged);
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

        #region 依赖属性 - 渲染模式 —— MPRRenderMode RenderMode
        /// <summary>
        /// 依赖属性 - 渲染模式
        /// </summary>
        public MPRRenderMode RenderMode
        {
            get => this.GetValue(RenderModeProperty);
            set => this.SetValue(RenderModeProperty, value);
        }
        #endregion

        #region 依赖属性 - MPR平面 —— MPRPlane Plane
        /// <summary>
        /// 依赖属性 - MPR平面
        /// </summary>
        public MPRPlane Plane
        {
            get => this.GetValue(PlaneProperty);
            set => this.SetValue(PlaneProperty, value);
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
            #region # 验证

            if (this.Plane == null)
            {
                return default;
            }

            #endregion

            Vector2? planeUV = this._mprRenderer.Plane.ScreenToPlaneUV(screenPos2D, this._viewportSize.ToVector2(), this.Camera.ProjectionMatrix, this.Camera.ViewMatrix, out Ray ray);
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
            textureCoord = Vector3.Zero;
            worldPosition = Vector3.Zero;
            voxelPosition = Vector3i.Zero;
            voxelValue = -1;
            markValue = 0;
            ray = default;
            if (this.Plane == null)
            {
                return false;
            }

            this.GlContext.MakeCurrent();
            Vector2? planeUV = this._mprRenderer.Plane.ScreenToPlaneUV(position, this._viewportSize.ToVector2(), this.Camera.ProjectionMatrix, this.Camera.ViewMatrix, out ray);
            if (planeUV.HasValue)
            {
                voxelPosition = this._mprRenderer.Plane.GetVoxelPosition(planeUV.Value.X, planeUV.Value.Y, out Vector3 texCoord, out Vector3 worldPoint);
                textureCoord = texCoord;
                worldPosition = worldPoint;
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
            //InputManger默认值
            this.InputManager ??= new MPRInputManager(this.MPRCamera);

            //初始化形状渲染器
            this._shapeRenderer = new ShapeRenderer(this.MPRCamera);

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
                this._mprRenderer.RenderFrame(viewportSize.Width, viewportSize.Height, this.GlContextHandle);

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
                            polygon.Id = shapeVisual3D.Id;
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


        //Events

        #region 预览模式改变事件 —— static void OnPreviewModeChanged(MPRViewport viewport...
        /// <summary>
        /// 预览模式改变事件
        /// </summary>
        private static void OnPreviewModeChanged(MPRViewport viewport, AvaloniaPropertyChangedEventArgs<PreviewMode> eventArgs)
        {
            viewport._mprRenderer?.SwitchPreviewMode(eventArgs.NewValue.Value);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 渲染模式改变事件 —— static void OnRenderModeChanged(MPRViewport viewport...
        /// <summary>
        /// 渲染模式改变事件
        /// </summary>
        private static void OnRenderModeChanged(MPRViewport viewport, AvaloniaPropertyChangedEventArgs<MPRRenderMode> eventArgs)
        {
            viewport._mprRenderer?.SwitchRenderMode(eventArgs.NewValue.Value);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region MPR平面改变事件 —— static void OnPlaneChanged(MPRViewport viewport...
        /// <summary>
        /// MPR平面改变事件
        /// </summary>
        private static void OnPlaneChanged(MPRViewport viewport, AvaloniaPropertyChangedEventArgs<MPRPlane> eventArgs)
        {
            viewport._mprRenderer?.BindPlane(eventArgs.NewValue.Value);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 窗宽改变事件 —— static void OnWindowWidthChanged(MPRViewport viewport...
        /// <summary>
        /// 窗宽改变事件
        /// </summary>
        private static void OnWindowWidthChanged(MPRViewport viewport, AvaloniaPropertyChangedEventArgs<int> eventArgs)
        {
            viewport._mprRenderer?.SetWindowLevel(eventArgs.NewValue.Value, viewport.WindowCenter);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 窗位改变事件 —— static void OnWindowCenterChanged(MPRViewport viewport...
        /// <summary>
        /// 窗位改变事件
        /// </summary>
        private static void OnWindowCenterChanged(MPRViewport viewport, AvaloniaPropertyChangedEventArgs<int> eventArgs)
        {
            viewport._mprRenderer?.SetWindowLevel(viewport.WindowWidth, eventArgs.NewValue.Value);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 亮度改变事件 —— static void OnBrightnessChanged(MPRViewport viewport...
        /// <summary>
        /// 亮度改变事件
        /// </summary>
        private static void OnBrightnessChanged(MPRViewport viewport, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            viewport._mprRenderer?.SetMaterialOptions(eventArgs.NewValue.Value, viewport.Contrast);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 对比度改变事件 —— static void OnContrastChanged(MPRViewport viewport...
        /// <summary>
        /// 对比度改变事件
        /// </summary>
        private static void OnContrastChanged(MPRViewport viewport, AvaloniaPropertyChangedEventArgs<float> eventArgs)
        {
            viewport._mprRenderer?.SetMaterialOptions(viewport.Brightness, eventArgs.NewValue.Value);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 传递函数控制点列表改变事件 —— static void OnTFControlPointsChanged(MPRViewport viewport...
        /// <summary>
        /// 传递函数控制点列表改变事件
        /// </summary>
        private static void OnTFControlPointsChanged(MPRViewport viewport, AvaloniaPropertyChangedEventArgs<AvaloniaList<HUControlPoint>> eventArgs)
        {
            if (eventArgs.OldValue.Value != null)
            {
                //清除旧元素
                foreach (HUControlPoint controlPoint in eventArgs.OldValue.Value)
                {
                    viewport._mprRenderer?.TransferFunction.RemoveControlPoint(controlPoint);
                }
                eventArgs.OldValue.Value.CollectionChanged -= viewport.OnTFControlPointsItemChanged;
            }
            if (eventArgs.NewValue.Value != null)
            {
                //添加新元素
                foreach (HUControlPoint controlPoint in eventArgs.NewValue.Value)
                {
                    viewport._mprRenderer?.TransferFunction.AddControlPoint(controlPoint);
                }

                eventArgs.NewValue.Value.CollectionChanged += viewport.OnTFControlPointsItemChanged;
            }
            if (eventArgs.NewValue.Value == null)
            {
                //清空旧元素
                viewport._mprRenderer?.TransferFunction.ClearControlPoints();
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
                foreach (HUControlPoint controlPoint in eventArgs.OldItems)
                {
                    this._mprRenderer?.TransferFunction.RemoveControlPoint(controlPoint);
                }
            }
            if (eventArgs.NewItems != null)
            {
                //添加新元素
                foreach (HUControlPoint controlPoint in eventArgs.NewItems)
                {
                    this._mprRenderer?.TransferFunction.AddControlPoint(controlPoint);
                }
            }
            if (eventArgs.Action == NotifyCollectionChangedAction.Reset)
            {
                //清空元素
                this._mprRenderer?.TransferFunction.ClearControlPoints();
            }

            //请求下一帧
            this.RequestNextFrameRendering();
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

                //设置默认窗
                WindowLevelManager.SetDefault(volumeData.Metadata.WindowWidth, volumeData.Metadata.WindowCenter);
            }
            else
            {
                volumeSession = SessionManager.VolumeSessions[volumeData.Metadata.Id];
            }

            viewport._volumeRenderable = new VolumeRenderable(volumeSession.OriginalTexture, volumeSession.PreviewTexture, volumeSession.MarkTexture, volumeData);

            //初始化传递函数、标记策略
            viewport._mprRenderer.SetTransferFunction(volumeSession.MPRTransferFunction);
            viewport._mprRenderer.SetMarkStrategy(volumeSession.MarkStrategy);
            viewport._mprRenderer.TransferFunction.SetHURange(volumeData.Metadata.MinHU, volumeData.Metadata.MaxHU);
            viewport._mprRenderer.TransferFunction.InitFromControlPoints(viewport.TFControlPoints);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #endregion
    }
}
