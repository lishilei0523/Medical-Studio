using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Caliburn.Micro;
using IconPacks.Avalonia.MaterialDesign;
using MedicalSharp.Client.ViewModels.CameraContext;
using MedicalSharp.Client.ViewModels.ProtocolContext;
using MedicalSharp.Client.ViewModels.TissueContext;
using MedicalSharp.Client.Views.VolumeContext;
using MedicalSharp.Controls.Commands;
using MedicalSharp.Controls.Commands.Arguments;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.InputManagers;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Controls.Visual3Ds;
using MedicalSharp.Engine.Base;
using MedicalSharp.Engine.Managers;
using MedicalSharp.Presentation.Events;
using MedicalSharp.Presentation.Maps;
using MedicalSharp.Presentation.Models;
using MedicalSharp.Primitives;
using MedicalSharp.Primitives.Cameras;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Managers;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using SD.Infrastructure.Avalonia.Caliburn.Extensions;
using SD.Infrastructure.Avalonia.Commands;
using SD.Infrastructure.Avalonia.CustomControls;
using SD.Infrastructure.Avalonia.Enums;
using SD.IOC.Core.Mediators;
using SD.Toolkits.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MedicalSharp.Client.ViewModels.VolumeContext
{
    /// <summary>
    /// 体积渲染视图模型
    /// </summary>
    public class VolumeViewModel : ScreenBase, IHandle<ClearShapesEvent>, IHandle<MarkModeSwitchedEvent>, IHandle<MarkColorChangedEvent>, IHandle<SyncViewportEvent>, IHandle<MPRPlaneChangedEvent>
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗口管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 事件聚合器
        /// </summary>
        private readonly IEventAggregator _eventAggregator;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public VolumeViewModel(IWindowManager windowManager, IEventAggregator eventAggregator)
        {
            this._windowManager = windowManager;
            this._eventAggregator = eventAggregator;
            this._eventAggregator.SubscribeOnUIThread(this);

            //初始化相机
            Vector3 cameraPosition = new Vector3(0, 2.5f, 0);
            Vector3 targetPosition = Vector3.Zero;
            Vector3 worldUpDirection = Vector3.UnitZ;
            this.Camera = new OrbitPerspectiveCamera(cameraPosition, targetPosition, worldUpDirection);

            //初始化输入管理器
            this.InputManager = new OrbitInputManager(this.Camera);
            this.TranslateNormal();

            //初始化MPR平面
            this.InitMprPlanes();

            //初始化预设协议
            this.InitPresetProtocols();

            //默认值
            this.ViewEnabled = false;
            this.ToolbarConfig = new VolumeToolbar();
            this.Shapes = [];
            this.RaycastChecked = true;
            this.AxialPlaneVisible = false;
            this.CoronalPlaneVisible = false;
            this.SagittalPlaneVisible = false;
            this.ViewBoxVisible = true;
            this.AxisVisible = false;
            this.Brightness = 1.0f;
            this.DensityScale = 1.0f;
            this.StepSize = 0.0012f;
            this.MaxStepsCount = 1000;
            this.OpacityThreshold = 0.99f;
            this.InterpolationMode = InterpolationMode.Linear;
            this.TFControlPoints = new AvaloniaList<DensityControlPoint>(ProtocolManager.AnatomyControlPoints);
        }

        #endregion

        #region # 属性

        //属性

        #region 视图是否启用 —— bool ViewEnabled
        /// <summary>
        /// 视图是否启用
        /// </summary>
        [DependencyProperty]
        public bool ViewEnabled { get; set; }
        #endregion

        #region 工具栏配置 —— VolumeToolbar ToolbarConfig
        /// <summary>
        /// 工具栏配置
        /// </summary>
        [DependencyProperty]
        public VolumeToolbar ToolbarConfig { get; set; }
        #endregion

        #region Raycast渲染模式选中 —— bool RaycastChecked
        /// <summary>
        /// Raycast渲染模式选中
        /// </summary>
        public bool RaycastChecked
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                this.RenderMode = VolumeRenderMode.Raycast;
            }
        }
        #endregion

        #region AIP渲染模式选中 —— bool AIPChecked
        /// <summary>
        /// AIP渲染模式选中
        /// </summary>
        public bool AIPChecked
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                this.RenderMode = VolumeRenderMode.AIP;
            }
        }
        #endregion

        #region MIP渲染模式选中 —— bool MIPChecked
        /// <summary>
        /// MIP渲染模式选中
        /// </summary>
        public bool MIPChecked
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                this.RenderMode = VolumeRenderMode.MIP;
            }
        }
        #endregion

        #region MinIP渲染模式选中 —— bool MinIPChecked
        /// <summary>
        /// MinIP渲染模式选中
        /// </summary>
        public bool MinIPChecked
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                this.RenderMode = VolumeRenderMode.MinIP;
            }
        }
        #endregion

        #region 横断面是否可见 —— bool AxialPlaneVisible
        /// <summary>
        /// 横断面是否可见
        /// </summary>
        public bool AxialPlaneVisible
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                this.AxialPlane.IsVisible = value;
                this.FrameToken++;
            }
        }
        #endregion

        #region 冠状面是否可见 —— bool CoronalPlaneVisible
        /// <summary>
        /// 冠状面是否可见
        /// </summary>
        public bool CoronalPlaneVisible
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                this.CoronalPlane.IsVisible = value;
                this.FrameToken++;
            }
        }
        #endregion

        #region 矢状面是否可见 —— bool SagittalPlaneVisible
        /// <summary>
        /// 矢状面是否可见
        /// </summary>
        public bool SagittalPlaneVisible
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                this.SagittalPlane.IsVisible = value;
                this.FrameToken++;
            }
        }
        #endregion

        #region ViewBox是否可见 —— bool ViewBoxVisible
        /// <summary>
        /// ViewBox是否可见
        /// </summary>
        public bool ViewBoxVisible
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                this.FrameToken++;
            }
        }
        #endregion

        #region 坐标轴是否可见 —— bool AxisVisible
        /// <summary>
        /// 坐标轴是否可见
        /// </summary>
        public bool AxisVisible
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                this.FrameToken++;
            }
        }
        #endregion

        #region 帧令牌 —— int FrameToken
        /// <summary>
        /// 帧令牌
        /// </summary>
        [DependencyProperty]
        public int FrameToken { get; set; }
        #endregion

        #region 轨道相机 —— OrbitCamera Camera
        /// <summary>
        /// 轨道相机
        /// </summary>
        [DependencyProperty]
        public OrbitCamera Camera { get; set; }
        #endregion

        #region 输入管理器 —— OrbitInputManager InputManager
        /// <summary>
        /// 输入管理器
        /// </summary>
        [DependencyProperty]
        public OrbitInputManager InputManager { get; set; }
        #endregion

        #region 体积数据 —— VolumeData VolumeData
        /// <summary>
        /// 体积数据
        /// </summary>
        public VolumeData VolumeData
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                this.ResetMprPlanes(value);
                if (value == null)
                {
                    this.ViewEnabled = false;
                    this.Shapes.Clear();
                }
                else
                {
                    this.ViewEnabled = true;
                    this.WindowWidth = WindowLevelManager.Default.WindowWidth;
                    this.WindowCenter = WindowLevelManager.Default.WindowCenter;
                }
            }
        }
        #endregion

        #region 渲染模式 —— VolumeRenderMode RenderMode
        /// <summary>
        /// 渲染模式
        /// </summary>
        [DependencyProperty]
        public VolumeRenderMode RenderMode { get; set; }
        #endregion

        #region 窗宽 —— int WindowWidth
        /// <summary>
        /// 窗宽
        /// </summary>
        [DependencyProperty]
        public int WindowWidth { get; set; }
        #endregion

        #region 窗位 —— int WindowCenter
        /// <summary>
        /// 窗位
        /// </summary>
        [DependencyProperty]
        public int WindowCenter { get; set; }
        #endregion

        #region 亮度 —— float Brightness
        /// <summary>
        /// 亮度
        /// </summary>
        [DependencyProperty]
        public float Brightness { get; set; }
        #endregion

        #region 密度缩放 —— float DensityScale
        /// <summary>
        /// 密度缩放
        /// </summary>
        [DependencyProperty]
        public float DensityScale { get; set; }
        #endregion

        #region 步长 —— float StepSize
        /// <summary>
        /// 步长
        /// </summary>
        [DependencyProperty]
        public float StepSize { get; set; }
        #endregion

        #region 最大步数 —— int MaxStepsCount
        /// <summary>
        /// 最大步数
        /// </summary>
        [DependencyProperty]
        public int MaxStepsCount { get; set; }
        #endregion

        #region 透明度阈值 —— float OpacityThreshold
        /// <summary>
        /// 透明度阈值
        /// </summary>
        [DependencyProperty]
        public float OpacityThreshold { get; set; }
        #endregion

        #region 插值模式 —— InterpolationMode InterpolationMode
        /// <summary>
        /// 插值模式
        /// </summary>
        [DependencyProperty]
        public InterpolationMode InterpolationMode { get; set; }
        #endregion

        #region 传递函数控制点列表 —— AvaloniaList<DensityControlPoint> TFControlPoints
        /// <summary>
        /// 传递函数控制点列表
        /// </summary>
        [DependencyProperty]
        public AvaloniaList<DensityControlPoint> TFControlPoints { get; set; }
        #endregion

        #region 横断面 —— PlaneVisual3D AxialPlane
        /// <summary>
        /// 横断面
        /// </summary>
        [DependencyProperty]
        public MPRPlaneVisual3D AxialPlane { get; set; }
        #endregion

        #region 冠状面 —— PlaneVisual3D CoronalPlane
        /// <summary>
        /// 冠状面
        /// </summary>
        [DependencyProperty]
        public MPRPlaneVisual3D CoronalPlane { get; set; }
        #endregion

        #region 矢状面 —— PlaneVisual3D SagittalPlane
        /// <summary>
        /// 矢状面
        /// </summary>
        [DependencyProperty]
        public MPRPlaneVisual3D SagittalPlane { get; set; }
        #endregion

        #region 已选组织 —— TissueInfo SelectedTissue
        /// <summary>
        /// 已选组织
        /// </summary>
        [DependencyProperty]
        public TissueInfo SelectedTissue { get; set; }
        #endregion

        #region 组织列表 —— AvaloniaList<TissueInfo> Tissues
        /// <summary>
        /// 组织列表
        /// </summary>
        [DependencyProperty]
        public AvaloniaList<TissueInfo> Tissues { get; set; }
        #endregion

        #region 形状列表 —— AvaloniaList<ShapeVisual3D> Shapes
        /// <summary>
        /// 形状列表
        /// </summary>
        [DependencyProperty]
        public AvaloniaList<ShapeVisual3D> Shapes { get; set; }
        #endregion

        #region 已选协议 —— RaycastProtocol SelectedProtocol
        /// <summary>
        /// 已选协议
        /// </summary>
        public RaycastProtocol SelectedProtocol
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                if (value != null)
                {
                    //应用协议
                    this.WindowWidth = value.WindowWidth;
                    this.WindowCenter = value.WindowCenter;
                    this.Brightness = value.Brightness;
                    this.DensityScale = value.DensityScale;
                    this.StepSize = value.StepSize;
                    this.MaxStepsCount = value.MaxStepsCount;
                    this.OpacityThreshold = value.OpacityThreshold;
                    this.InterpolationMode = value.InterpolationMode;
                    this.TFControlPoints = new AvaloniaList<DensityControlPoint>(value.ControlPoints.Select(x => x.ToDensityControlPoint()));
                    this.FrameToken++;
                }
            }
        }
        #endregion

        #region 预设协议列表 —— AvaloniaList<RaycastProtocol> PresetProtocols
        /// <summary>
        /// 预设协议列表
        /// </summary>
        [DependencyProperty]
        public AvaloniaList<RaycastProtocol> PresetProtocols { get; set; }
        #endregion

        #region 只读属性 - 体积渲染视口 —— VolumeViewport VolumeViewport
        /// <summary>
        /// 只读属性 - 体积渲染视口
        /// </summary>
        public VolumeViewport VolumeViewport
        {
            get
            {
                VolumeView view = (VolumeView)this.GetView();
                return view?.VolumeViewport;
            }
        }
        #endregion


        //命令

        #region 复位MPR平面命令 —— ICommand ResetMprPlanesCommand
        /// <summary>
        /// 复位MPR平面命令
        /// </summary>
        public ICommand ResetMprPlanesCommand => new RelayCommand(_ =>
        {
            this.AxialPlane.Transform?.SetMatrix(Matrix4.Identity);
            this.CoronalPlane.Transform?.SetMatrix(Matrix4.Identity);
            this.SagittalPlane.Transform?.SetMatrix(Matrix4.Identity);
            this.FrameToken++;

            //发布事件
            MPRPlaneResetEvent messageAxial = new MPRPlaneResetEvent
            {
                Publisher = this
            };
            MPRPlaneResetEvent messageCoronal = new MPRPlaneResetEvent
            {
                Publisher = this
            };
            MPRPlaneResetEvent messageSagittal = new MPRPlaneResetEvent
            {
                Publisher = this
            };
            this._eventAggregator.PublishOnUIThreadAsync(messageAxial);
            this._eventAggregator.PublishOnUIThreadAsync(messageCoronal);
            this._eventAggregator.PublishOnUIThreadAsync(messageSagittal);
        }, _ => this.VolumeData != null);
        #endregion

        #region 调节相机命令 —— ICommand TuneCameraCommand
        /// <summary>
        /// 调节相机命令
        /// </summary>
        public ICommand TuneCameraCommand => new AsyncRelayCommand(async _ =>
        {
            TuneOrbitCameraViewModel viewModel = ResolveMediator.Resolve<TuneOrbitCameraViewModel>();
            viewModel.PanSpeed = this.Camera.PanSpeed;
            viewModel.RotateSpeed = this.Camera.RotateSpeed;
            viewModel.ZoomSpeed = this.Camera.ZoomSpeed;
            viewModel.MinDistance = this.Camera.MinDistance;
            viewModel.MaxDistance = this.Camera.MaxDistance;
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.Camera.SetSpeeds(viewModel.PanSpeed, viewModel.RotateSpeed, viewModel.ZoomSpeed);
                this.Camera.SetDistanceLimits(viewModel.MinDistance, viewModel.MaxDistance);
            }
        }, _ => this.VolumeData != null);
        #endregion

        #region 重置相机命令 —— ICommand ResetCameraCommand
        /// <summary>
        /// 重置相机命令
        /// </summary>
        public ICommand ResetCameraCommand => new AsyncRelayCommand(async _ =>
        {
            Vector3 cameraPosition = new Vector3(0, 2.5f, 0);
            Vector3 targetPosition = Vector3.Zero;
            this.Camera.SetPositions(cameraPosition, targetPosition);
            this.Camera.SetRotation(-90.0f, 0);
            this.FrameToken++;
        }, _ => this.VolumeData != null);
        #endregion

        #region 刷新协议命令 —— ICommand ReloadProtocolsCommand
        /// <summary>
        /// 刷新协议命令
        /// </summary>
        public ICommand ReloadProtocolsCommand => new AsyncRelayCommand(async _ =>
        {
            await this.InitPresetProtocols();
        }, _ => this.VolumeData != null);
        #endregion

        #region 删除协议命令 —— ICommand RemoveProtocolCommand
        /// <summary>
        /// 删除协议命令
        /// </summary>
        public ICommand RemoveProtocolCommand => new AsyncRelayCommand(async _ =>
        {
            RaycastProtocol protocol = this.SelectedProtocol;
            string path = $"{Constants.VRProtocolPath}/{protocol.Name}.json";
            if (File.Exists(path))
            {
                await Task.Run(() => File.Delete(path));
            }
            this.PresetProtocols.Remove(protocol);

        }, _ => this.VolumeData != null && this.SelectedProtocol != null);
        #endregion

        #region 调节协议命令 —— ICommand TuneProtocolCommand
        /// <summary>
        /// 调节协议命令
        /// </summary>
        public ICommand TuneProtocolCommand => new AsyncRelayCommand(async _ =>
        {
            VolumeProtocolViewModel viewModel = ResolveMediator.Resolve<VolumeProtocolViewModel>();
            viewModel.VolumeViewModel = this;
            await this._windowManager.ShowWindowAsync(viewModel);
        }, _ => this.VolumeData != null);
        #endregion

        #region 截屏命令 —— ICommand CaptureCommand
        /// <summary>
        /// 截屏命令
        /// </summary>
        public ICommand CaptureCommand => new AsyncRelayCommand(async _ =>
        {
            using SKBitmap bitmap = this.VolumeViewport.Capture();

            //保存文件对话框
            FilePickerSaveOptions saveOptions = new FilePickerSaveOptions
            {
                Title = "保存截图",
                DefaultExtension = "png",
                SuggestedFileName = $"截图_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}",
                FileTypeChoices =
                [
                    new FilePickerFileType("PNG图片")
                    {
                        Patterns = ["*.png"]
                    }
                ]
            };

            //保存截图
            IStorageFile storageFile = await this.SaveFilePickerAsync(saveOptions);
            if (storageFile != null)
            {
                await using Stream stream = await storageFile.OpenWriteAsync();
                bitmap.Encode(SKEncodedImageFormat.Png, 80).SaveTo(stream);
                await MessageBox.Show($"已保存至\"{storageFile.TryGetLocalPath()}\"");
            }
        }, _ => this.VolumeData != null);
        #endregion

        #endregion

        #region # 方法

        //Actions

        #region 拾取体素 —— void PickVoxel()
        /// <summary>
        /// 拾取体素
        /// </summary>
        public void PickVoxel()
        {
            Action<VoxelPickedEventArgs> picked = e =>
            {
                Vector2 mousePos2D = e.MousePos2D;
                Vector3? textureCoord = e.PickedTextureCoord;
                Vector3? worldPosition = e.PickedWorldPosition;
                Vector3i? voxelPostion = e.PickedVoxelPosition;
                short? voxelValue = e.PickedVoxelValue;
                byte? markValue = e.PickedMarkValue;
                if (textureCoord.HasValue)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine($"点击屏幕坐标: X:{mousePos2D.X}, Y:{mousePos2D.Y}");
                    builder.AppendLine($"点击纹理坐标: X:{textureCoord.Value.X}, Y:{textureCoord.Value.Y}, Z:{textureCoord.Value.Z}");
                    builder.AppendLine($"点击世界坐标: X:{worldPosition.Value.X}, Y:{worldPosition.Value.Y}, Z:{worldPosition.Value.Z}");
                    builder.AppendLine($"点击体素坐标: X:{voxelPostion.Value.X}, Y:{voxelPostion.Value.Y}, Z:{voxelPostion.Value.Z}");
                    builder.AppendLine($"点击体素HU值: {voxelValue}");
                    builder.AppendLine($"点击标记值: {markValue}");
                    MessageBox.Show(builder.ToString(), "成功", MessageBoxButton.OK, PackIconMaterialDesignKind.Info);
                }
                else
                {
                    MessageBox.Show("拾取失败！", "错误", MessageBoxButton.OK, PackIconMaterialDesignKind.Error);
                }
            };

            PickVoxelCommand command = new PickVoxelCommand
            {
                VisualPicked = this.OnVisualPicked,
                VisualRemoved = this.OnVisualRemoved,
                GetMarkValue = this.GetCurrentMarkValue,
                ShapeCut = this.OnShapeCutEnd,
                ShapeAnalysed = this.OnShapeAnalyseEnd,
                VoxelPicked = picked
            };
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 3D平移 —— void Translate3D()
        /// <summary>
        /// 3D平移
        /// </summary>
        public void Translate3D()
        {
            Action<ITranslatable3D> translated = _ =>
            {
                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };

            TranslateVisual3DCommand command = new TranslateVisual3DCommand
            {
                VisualPicked = this.OnVisualPicked,
                VisualRemoved = this.OnVisualRemoved,
                GetMarkValue = this.GetCurrentMarkValue,
                ShapeCut = this.OnShapeCutEnd,
                ShapeAnalysed = this.OnShapeAnalyseEnd,
                Translated = translated
            };
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 沿法向量平移 —— void TranslateNormal()
        /// <summary>
        /// 沿法向量平移
        /// </summary>
        public void TranslateNormal()
        {
            Action<ITranslatableNormal> translating = translatable =>
            {
                if (translatable is ShapeVisual3D shape)
                {
                    ShapeTranslatingEvent message = new ShapeTranslatingEvent
                    {
                        Publisher = this,
                        Shape = shape
                    };
                    this._eventAggregator.PublishOnUIThreadAsync(message);
                }
            };
            Action<ITranslatableNormal> translated = _ =>
            {
                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };

            TranslateVisualNormalCommand command = new TranslateVisualNormalCommand
            {
                VisualPicked = this.OnVisualPicked,
                VisualRemoved = this.OnVisualRemoved,
                GetMarkValue = this.GetCurrentMarkValue,
                ShapeCut = this.OnShapeCutEnd,
                ShapeAnalysed = this.OnShapeAnalyseEnd,
                Translating = translating,
                Translated = translated
            };
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region U轴旋转 —— void RotateU()
        /// <summary>
        /// U轴旋转
        /// </summary>
        public void RotateU()
        {
            Action<IRotatable> rotating = rotatable =>
            {
                if (rotatable is ShapeVisual3D shape)
                {
                    ShapeRotatingEvent message = new ShapeRotatingEvent
                    {
                        Publisher = this,
                        Shape = shape
                    };
                    this._eventAggregator.PublishOnUIThreadAsync(message);
                }
            };
            Action<IRotatable> rotated = _ =>
            {
                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };

            RotateVisualUCommand command = new RotateVisualUCommand
            {
                VisualPicked = this.OnVisualPicked,
                VisualRemoved = this.OnVisualRemoved,
                GetMarkValue = this.GetCurrentMarkValue,
                ShapeCut = this.OnShapeCutEnd,
                ShapeAnalysed = this.OnShapeAnalyseEnd,
                Rotating = rotating,
                Rotated = rotated
            };
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region V轴旋转 —— void RotateV()
        /// <summary>
        /// V轴旋转
        /// </summary>
        public void RotateV()
        {
            Action<IRotatable> rotating = rotatable =>
            {
                if (rotatable is ShapeVisual3D shape)
                {
                    ShapeRotatingEvent message = new ShapeRotatingEvent
                    {
                        Publisher = this,
                        Shape = shape
                    };
                    this._eventAggregator.PublishOnUIThreadAsync(message);
                }
            };
            Action<IRotatable> rotated = _ =>
            {
                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };

            RotateVisualVCommand command = new RotateVisualVCommand
            {
                VisualPicked = this.OnVisualPicked,
                VisualRemoved = this.OnVisualRemoved,
                GetMarkValue = this.GetCurrentMarkValue,
                ShapeCut = this.OnShapeCutEnd,
                ShapeAnalysed = this.OnShapeAnalyseEnd,
                Rotating = rotating,
                Rotated = rotated
            };
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 3D旋转 —— void Rotate3D()
        /// <summary>
        /// 3D旋转
        /// </summary>
        public void Rotate3D()
        {
            Action<IRotatable> rotating = rotatable =>
            {
                if (rotatable is ShapeVisual3D shape)
                {
                    ShapeRotatingEvent message = new ShapeRotatingEvent
                    {
                        Publisher = this,
                        Shape = shape
                    };
                    this._eventAggregator.PublishOnUIThreadAsync(message);
                }
            };
            Action<IRotatable> rotated = _ =>
            {
                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };

            RotateVisual3DCommand command = new RotateVisual3DCommand
            {
                VisualPicked = this.OnVisualPicked,
                VisualRemoved = this.OnVisualRemoved,
                GetMarkValue = this.GetCurrentMarkValue,
                ShapeCut = this.OnShapeCutEnd,
                ShapeAnalysed = this.OnShapeAnalyseEnd,
                Rotating = rotating,
                Rotated = rotated
            };
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 调整尺寸 —— void Resize()
        /// <summary>
        /// 调整尺寸
        /// </summary>
        public void Resize()
        {
            Action<IResizable3D> resized = _ =>
            {
                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };

            ResizeVisual3DCommand command = new ResizeVisual3DCommand
            {
                VisualPicked = this.OnVisualPicked,
                VisualRemoved = this.OnVisualRemoved,
                GetMarkValue = this.GetCurrentMarkValue,
                ShapeCut = this.OnShapeCutEnd,
                ShapeAnalysed = this.OnShapeAnalyseEnd,
                Resized = resized
            };
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 编辑顶点 —— void EditVertex()
        /// <summary>
        /// 编辑顶点
        /// </summary>
        public void EditVertex()
        {
            Action<IVertexEditable> vertexEdited = _ =>
            {
                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };

            EditVertexCommand command = new EditVertexCommand
            {
                VisualPicked = this.OnVisualPicked,
                VisualRemoved = this.OnVisualRemoved,
                GetMarkValue = this.GetCurrentMarkValue,
                ShapeCut = this.OnShapeCutEnd,
                ShapeAnalysed = this.OnShapeAnalyseEnd,
                VertexEdited = vertexEdited
            };
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制文本 —— void DrawText()
        /// <summary>
        /// 绘制文本
        /// </summary>
        public void DrawText()
        {
            //TODO 实现
        }
        #endregion

        #region 绘制点 —— void DrawPoint()
        /// <summary>
        /// 绘制点
        /// </summary>
        public void DrawPoint()
        {
            Action<PointVisual3D> drawEnd = shape =>
            {
                int count = this.Shapes.OfType<PointVisual3D>().Count();
                shape.DisplayName = $"点{count + 1}";
                this.Shapes.Add(shape);

                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };

            DrawPointCommand command = new DrawPointCommand
            {
                VisualPicked = this.OnVisualPicked,
                VisualRemoved = this.OnVisualRemoved,
                GetMarkValue = this.GetCurrentMarkValue,
                ShapeCut = this.OnShapeCutEnd,
                ShapeAnalysed = this.OnShapeAnalyseEnd,
                DrawEnd = drawEnd
            };
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制线段 —— void DrawLineSegment()
        /// <summary>
        /// 绘制线段
        /// </summary>
        public void DrawLineSegment()
        {
            Action<LineSegmentVisual3D> drawStart = shape =>
            {
                int count = this.Shapes.OfType<LineSegmentVisual3D>().Count();
                shape.DisplayName = $"线段{count + 1}";
                this.Shapes.Add(shape);
            };
            Action<LineSegmentVisual3D> drawEnd = _ =>
            {
                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };

            DrawLineSegmentCommand command = new DrawLineSegmentCommand
            {
                VisualPicked = this.OnVisualPicked,
                VisualRemoved = this.OnVisualRemoved,
                GetMarkValue = this.GetCurrentMarkValue,
                ShapeCut = this.OnShapeCutEnd,
                ShapeAnalysed = this.OnShapeAnalyseEnd,
                DrawStart = drawStart,
                DrawEnd = drawEnd
            };
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制矩形 —— void DrawRectangle()
        /// <summary>
        /// 绘制矩形
        /// </summary>
        public void DrawRectangle()
        {
            Func<Vector3D> getNormal = () => -this.Camera.LookDirection.ToVector3();
            Action<RectangleVisual3D> drawStart = shape =>
            {
                int count = this.Shapes.OfType<RectangleVisual3D>().Count();
                shape.DisplayName = $"矩形{count + 1}";
                this.Shapes.Add(shape);
            };
            Action<RectangleVisual3D> drawEnd = _ =>
            {
                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };

            DrawRectangleCommand command = new DrawRectangleCommand
            {
                VisualPicked = this.OnVisualPicked,
                VisualRemoved = this.OnVisualRemoved,
                GetMarkValue = this.GetCurrentMarkValue,
                ShapeCut = this.OnShapeCutEnd,
                ShapeAnalysed = this.OnShapeAnalyseEnd,
                GetNormal = getNormal,
                DrawStart = drawStart,
                DrawEnd = drawEnd
            };
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制圆形 —— void DrawCircle()
        /// <summary>
        /// 绘制圆形
        /// </summary>
        public void DrawCircle()
        {
            Func<Vector3D> getNormal = () => -this.Camera.LookDirection.ToVector3();
            Action<CircleVisual3D> drawStart = shape =>
            {
                int count = this.Shapes.OfType<CircleVisual3D>().Count();
                shape.DisplayName = $"圆{count + 1}";
                this.Shapes.Add(shape);
            };
            Action<CircleVisual3D> drawEnd = _ =>
            {
                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };

            DrawCircleCommand command = new DrawCircleCommand
            {
                VisualPicked = this.OnVisualPicked,
                VisualRemoved = this.OnVisualRemoved,
                GetMarkValue = this.GetCurrentMarkValue,
                ShapeCut = this.OnShapeCutEnd,
                ShapeAnalysed = this.OnShapeAnalyseEnd,
                GetNormal = getNormal,
                DrawStart = drawStart,
                DrawEnd = drawEnd
            };
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制椭圆形 —— void DrawEllipse()
        /// <summary>
        /// 绘制椭圆形
        /// </summary>
        public void DrawEllipse()
        {
            Func<Vector3D> getNormal = () => -this.Camera.LookDirection.ToVector3();
            Action<EllipseVisual3D> drawStart = shape =>
            {
                int count = this.Shapes.OfType<EllipseVisual3D>().Count();
                shape.DisplayName = $"椭圆{count + 1}";
                this.Shapes.Add(shape);
            };
            Action<EllipseVisual3D> drawEnd = _ =>
            {
                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };

            DrawEllipseCommand command = new DrawEllipseCommand
            {
                VisualPicked = this.OnVisualPicked,
                VisualRemoved = this.OnVisualRemoved,
                GetMarkValue = this.GetCurrentMarkValue,
                ShapeCut = this.OnShapeCutEnd,
                ShapeAnalysed = this.OnShapeAnalyseEnd,
                GetNormal = getNormal,
                DrawStart = drawStart,
                DrawEnd = drawEnd
            };
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制折线 —— void DrawPolyline()
        /// <summary>
        /// 绘制折线
        /// </summary>
        public void DrawPolyline()
        {
            Action<PolylineVisual3D> drawStart = shape =>
            {
                int count = this.Shapes.OfType<PolylineVisual3D>().Count(x => !x.Closed);
                shape.DisplayName = $"折线{count + 1}";
                this.Shapes.Add(shape);
            };
            Action<PolylineVisual3D> drawEnd = _ =>
            {
                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };
            Action<PolylineVisual3D> drawCancelled = shape => this.Shapes.Remove(shape);

            DrawPolylineCommand command = new DrawPolylineCommand(false)
            {
                VisualPicked = this.OnVisualPicked,
                VisualRemoved = this.OnVisualRemoved,
                GetMarkValue = this.GetCurrentMarkValue,
                ShapeCut = this.OnShapeCutEnd,
                ShapeAnalysed = this.OnShapeAnalyseEnd,
                DrawStart = drawStart,
                DrawEnd = drawEnd,
                DrawCancelled = drawCancelled
            };
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制曲线 —— void DrawCurve()
        /// <summary>
        /// 绘制曲线
        /// </summary>
        public void DrawCurve()
        {
            Action<CurveVisual3D> drawStart = shape =>
            {
                int count = this.Shapes.OfType<CurveVisual3D>().Count(x => !x.Closed);
                shape.DisplayName = $"曲线{count + 1}";
                this.Shapes.Add(shape);
            };
            Action<CurveVisual3D> drawEnd = _ =>
            {
                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };
            Action<CurveVisual3D> drawCancelled = shape => this.Shapes.Remove(shape);

            DrawCurveCommand command = new DrawCurveCommand(false)
            {
                VisualPicked = this.OnVisualPicked,
                VisualRemoved = this.OnVisualRemoved,
                GetMarkValue = this.GetCurrentMarkValue,
                ShapeCut = this.OnShapeCutEnd,
                ShapeAnalysed = this.OnShapeAnalyseEnd,
                DrawStart = drawStart,
                DrawEnd = drawEnd,
                DrawCancelled = drawCancelled
            };
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制多边形 —— void DrawPolygon()
        /// <summary>
        /// 绘制多边形
        /// </summary>
        public void DrawPolygon()
        {
            Action<PolylineVisual3D> drawStart = shape =>
            {
                int count = this.Shapes.OfType<PolylineVisual3D>().Count(x => x.Closed);
                shape.DisplayName = $"多边形{count + 1}";
                this.Shapes.Add(shape);
            };
            Action<PolylineVisual3D> drawEnd = _ =>
            {
                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };
            Action<PolylineVisual3D> drawCancelled = shape => this.Shapes.Remove(shape);

            DrawPolylineCommand command = new DrawPolylineCommand(true)
            {
                VisualPicked = this.OnVisualPicked,
                VisualRemoved = this.OnVisualRemoved,
                GetMarkValue = this.GetCurrentMarkValue,
                ShapeCut = this.OnShapeCutEnd,
                ShapeAnalysed = this.OnShapeAnalyseEnd,
                DrawStart = drawStart,
                DrawEnd = drawEnd,
                DrawCancelled = drawCancelled
            };
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制闭合曲线 —— void DrawClosedCurve()
        /// <summary>
        /// 绘制闭合曲线
        /// </summary>
        public void DrawClosedCurve()
        {
            Action<CurveVisual3D> drawStart = shape =>
            {
                int count = this.Shapes.OfType<CurveVisual3D>().Count(x => x.Closed);
                shape.DisplayName = $"闭合曲线{count + 1}";
                this.Shapes.Add(shape);
            };
            Action<CurveVisual3D> drawEnd = _ =>
            {
                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };
            Action<CurveVisual3D> drawCancelled = shape => this.Shapes.Remove(shape);

            DrawCurveCommand command = new DrawCurveCommand(true)
            {
                VisualPicked = this.OnVisualPicked,
                VisualRemoved = this.OnVisualRemoved,
                GetMarkValue = this.GetCurrentMarkValue,
                ShapeCut = this.OnShapeCutEnd,
                ShapeAnalysed = this.OnShapeAnalyseEnd,
                DrawStart = drawStart,
                DrawEnd = drawEnd,
                DrawCancelled = drawCancelled
            };
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制立方体 —— void DrawBox()
        /// <summary>
        /// 绘制立方体
        /// </summary>
        public void DrawBox()
        {
            Action<BoundingBoxVisual3D> drawStart = shape =>
            {
                int count = this.Shapes.OfType<BoundingBoxVisual3D>().Count();
                shape.DisplayName = $"立方体{count + 1}";
                this.Shapes.Add(shape);
            };
            Action<BoundingBoxVisual3D> drawEnd = _ =>
            {
                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };

            DrawBoundingBoxCommand command = new DrawBoundingBoxCommand
            {
                VisualPicked = this.OnVisualPicked,
                VisualRemoved = this.OnVisualRemoved,
                GetMarkValue = this.GetCurrentMarkValue,
                ShapeCut = this.OnShapeCutEnd,
                ShapeAnalysed = this.OnShapeAnalyseEnd,
                DrawStart = drawStart,
                DrawEnd = drawEnd
            };
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制球体 —— void DrawSphere()
        /// <summary>
        /// 绘制球体
        /// </summary>
        public void DrawSphere()
        {
            Action<BoundingSphereVisual3D> drawStart = shape =>
            {
                int count = this.Shapes.OfType<BoundingSphereVisual3D>().Count();
                shape.DisplayName = $"球体{count + 1}";
                this.Shapes.Add(shape);
            };
            Action<BoundingSphereVisual3D> drawEnd = _ =>
            {
                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };

            DrawBoundingSphereCommand command = new DrawBoundingSphereCommand
            {
                VisualPicked = this.OnVisualPicked,
                VisualRemoved = this.OnVisualRemoved,
                GetMarkValue = this.GetCurrentMarkValue,
                ShapeCut = this.OnShapeCutEnd,
                ShapeAnalysed = this.OnShapeAnalyseEnd,
                DrawStart = drawStart,
                DrawEnd = drawEnd
            };
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制圆柱体 —— void DrawCylinder()
        /// <summary>
        /// 绘制圆柱体
        /// </summary>
        public void DrawCylinder()
        {
            Action<CylinderVisual3D> drawStart = shape =>
            {
                int count = this.Shapes.OfType<CylinderVisual3D>().Count();
                shape.DisplayName = $"圆柱体{count + 1}";
                this.Shapes.Add(shape);
            };
            Action<CylinderVisual3D> drawEnd = _ =>
            {
                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };

            DrawCylinderCommand command = new DrawCylinderCommand
            {
                VisualPicked = this.OnVisualPicked,
                VisualRemoved = this.OnVisualRemoved,
                GetMarkValue = this.GetCurrentMarkValue,
                ShapeCut = this.OnShapeCutEnd,
                ShapeAnalysed = this.OnShapeAnalyseEnd,
                DrawStart = drawStart,
                DrawEnd = drawEnd
            };
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制凸多面体 —— void DrawConvexPolyhedron()
        /// <summary>
        /// 绘制凸多面体
        /// </summary>
        public void DrawConvexPolyhedron()
        {
            Action<ConvexPolyhedronVisual3D> drawStart = shape =>
            {
                int count = this.Shapes.OfType<ConvexPolyhedronVisual3D>().Count();
                shape.DisplayName = $"多面体{count + 1}";
                this.Shapes.Add(shape);
            };
            Action<ConvexPolyhedronVisual3D> drawEnd = _ =>
            {
                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };
            Action<ConvexPolyhedronVisual3D> drawCancelled = shape => this.Shapes.Remove(shape);

            DrawConvexPolyhedronCommand command = new DrawConvexPolyhedronCommand
            {
                VisualPicked = this.OnVisualPicked,
                VisualRemoved = this.OnVisualRemoved,
                GetMarkValue = this.GetCurrentMarkValue,
                ShapeCut = this.OnShapeCutEnd,
                ShapeAnalysed = this.OnShapeAnalyseEnd,
                DrawStart = drawStart,
                DrawEnd = drawEnd,
                DrawCancelled = drawCancelled
            };
            this.InputManager.SwitchCommand(command);
        }
        #endregion


        //Events

        #region 3D元素已拾取事件 —— void OnVisualPicked(Visual3D visual3D)
        /// <summary>
        /// 3D元素已拾取事件
        /// </summary>
        /// <param name="visual3D">3D元素</param>
        private void OnVisualPicked(Visual3D visual3D)
        {
            foreach (ShapeVisual3D shape in this.Shapes)
            {
                shape.IsSelected = false;
            }
            if (visual3D is ShapeVisual3D shapeVisual3D && this.Shapes.Contains(shapeVisual3D))
            {
                shapeVisual3D.IsSelected = true;
            }

            //发布消息
            SyncViewportEvent message = new SyncViewportEvent
            {
                Publisher = this
            };
            this._eventAggregator.PublishOnUIThreadAsync(message);
        }
        #endregion

        #region 3D元素已删除事件 —— void OnVisualRemoved(Visual3D visual3D)
        /// <summary>
        /// 3D元素已删除事件
        /// </summary>
        /// <param name="visual3D">3D元素</param>
        private void OnVisualRemoved(Visual3D visual3D)
        {
            if (visual3D is ShapeVisual3D shapeVisual3D && this.Shapes.Contains(shapeVisual3D))
            {
                this.Shapes.Remove(shapeVisual3D);
            }
        }
        #endregion

        #region 获取当前标记值 —— Task<byte> GetCurrentMarkValue()
        /// <summary>
        /// 获取当前标记值
        /// </summary>
        /// <returns>标记值</returns>
        private async Task<byte> GetCurrentMarkValue()
        {
            SelectViewModel viewModel = ResolveMediator.Resolve<SelectViewModel>();
            viewModel.Tissues = this.Tissues;
            viewModel.SelectedTissue = this.SelectedTissue;

            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                return viewModel.SelectedTissue.MarkValue;
            }

            return 0;
        }
        #endregion

        #region 形状切割结束事件 —— void OnShapeCutEnd()
        /// <summary>
        /// 形状切割结束事件
        /// </summary>
        public void OnShapeCutEnd()
        {
            SyncViewportEvent message = new SyncViewportEvent
            {
                Publisher = this
            };
            this._eventAggregator.PublishOnUIThreadAsync(message);
        }
        #endregion

        #region 形状统计结束事件 —— void OnShapeAnalyseEnd(StatisticResult result)
        /// <summary>
        /// 形状统计结束事件
        /// </summary>
        /// <param name="result">统计结果</param>
        public void OnShapeAnalyseEnd(StatisticResult result)
        {
            StatisticFinishedEvent message = new StatisticFinishedEvent
            {
                Publisher = this,
                StatisticResult = result
            };
            this._eventAggregator.PublishOnUIThreadAsync(message);
        }
        #endregion

        #region 处理清空形状事件 —— Task HandleAsync(ClearShapesEvent message...
        /// <summary>
        /// 处理清空形状事件
        /// </summary>
        public Task HandleAsync(ClearShapesEvent message, CancellationToken cancellationToken)
        {
            this.Shapes.Clear();
            this.FrameToken++;

            return Task.CompletedTask;
        }
        #endregion

        #region 处理标记模式切换事件 —— Task HandleAsync(MarkModeSwitchedEvent message...
        /// <summary>
        /// 处理标记模式切换事件
        /// </summary>
        public Task HandleAsync(MarkModeSwitchedEvent message, CancellationToken cancellationToken)
        {
            if (this.VolumeData != null)
            {
                VolumeSession session = SessionManager.VolumeSessions[this.VolumeData.Metadata.Id];
                session.MarkStrategy.SwitchMarkMode(message.MarkValue, message.MarkMode);
                this.FrameToken++;
            }

            return Task.CompletedTask;
        }
        #endregion

        #region 处理标记颜色改变事件 —— Task HandleAsync(MarkColorChangedEvent message...
        /// <summary>
        /// 处理标记颜色改变事件
        /// </summary>
        public Task HandleAsync(MarkColorChangedEvent message, CancellationToken cancellationToken)
        {
            if (this.VolumeData != null)
            {
                VolumeSession session = SessionManager.VolumeSessions[this.VolumeData.Metadata.Id];
                session.MarkStrategy.SetMarkColor(message.MarkValue, message.Color.ToVector4());
                this.FrameToken++;
            }

            return Task.CompletedTask;
        }
        #endregion

        #region 处理同步视口事件 —— Task HandleAsync(SyncViewportEvent message...
        /// <summary>
        /// 处理同步视口事件
        /// </summary>
        public Task HandleAsync(SyncViewportEvent message, CancellationToken cancellationToken)
        {
            #region # 验证

            if (message.Publisher == this)
            {
                return Task.CompletedTask;
            }

            #endregion

            this.FrameToken++;

            return Task.CompletedTask;
        }
        #endregion

        #region 处理MPR平面变化事件 —— Task HandleAsync(MPRPlaneChangedEvent message...
        /// <summary>
        /// 处理MPR平面变化事件
        /// </summary>
        public Task HandleAsync(MPRPlaneChangedEvent message, CancellationToken cancellationToken)
        {
            #region # 验证

            if (message.Publisher == this)
            {
                return Task.CompletedTask;
            }
            if (message.Plane == null)
            {
                return Task.CompletedTask;
            }
            if (message.SkipVolumeSync)
            {
                return Task.CompletedTask;
            }

            #endregion

            MPRPlaneVisual3D targetPlane = message.Plane.OriginalPlaneType switch
            {
                MPRPlaneType.Axial => this.AxialPlane,
                MPRPlaneType.Coronal => this.CoronalPlane,
                MPRPlaneType.Sagittal => this.SagittalPlane,
                _ => null
            };

            #region # 验证

            if (targetPlane == null || targetPlane.Transform == null)
            {
                return Task.CompletedTask;
            }

            #endregion

            //同步位置
            targetPlane.Transform.SetPosition(message.Plane.WorldCenter);
            this.FrameToken++;

            return Task.CompletedTask;
        }
        #endregion

        #region 失活事件 —— override Task OnDeactivateAsync(bool close...
        /// <summary>
        /// 失活事件
        /// </summary>
        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (close)
            {
                this._eventAggregator.Unsubscribe(this);
            }

            return base.OnDeactivateAsync(close, cancellationToken);
        }
        #endregion


        //Methods

        #region 初始化预设协议 —— async Task InitPresetProtocols()
        /// <summary>
        /// 初始化预设协议
        /// </summary>
        public async Task InitPresetProtocols()
        {
            string[] protocolFiles = Directory.GetFiles(Constants.VRProtocolPath);
            List<RaycastProtocol> protocols = [];
            foreach (string protocolFile in protocolFiles)
            {
                string json = await File.ReadAllTextAsync(protocolFile);
                RaycastProtocol protocol = json.AsJsonTo<RaycastProtocol>();
                protocols.Add(protocol);
            }
            this.PresetProtocols = new AvaloniaList<RaycastProtocol>(protocols);
        }
        #endregion

        #region 初始化MPR三平面 —— void InitMprPlanes()
        /// <summary>
        /// 初始化MPR三平面
        /// </summary>
        private void InitMprPlanes()
        {
            this.AxialPlane = new MPRPlaneVisual3D
            {
                Stroke = Colors.LimeGreen,
                StrokeThickness = 1,
                Fill = Color.Parse("#2032CD32"),
                Width = 1,
                Height = 1,
                Center = new Vector3D(0, 0, 0),
                UAxis = new Vector3D(1, 0, 0),
                VAxis = new Vector3D(0, -1, 0),
                Normal = new Vector3D(0, 0, 1),
                PlaneType = MPRPlaneType.Axial
            };
            this.CoronalPlane = new MPRPlaneVisual3D
            {
                Stroke = Colors.Red,
                StrokeThickness = 1,
                Fill = Color.Parse("#20FF0000"),
                Width = 1,
                Height = 1,
                Center = new Vector3D(0, 0, 0),
                UAxis = new Vector3D(1, 0, 0),
                VAxis = new Vector3D(0, 0, 1),
                Normal = new Vector3D(0, 1, 0),
                PlaneType = MPRPlaneType.Coronal
            };
            this.SagittalPlane = new MPRPlaneVisual3D
            {
                Stroke = Colors.DeepSkyBlue,
                StrokeThickness = 1,
                Fill = Color.Parse("#2000BFFF"),
                Width = 1,
                Height = 1,
                Center = new Vector3D(0, 0, 0),
                UAxis = new Vector3D(0, 1, 0),
                VAxis = new Vector3D(0, 0, 1),
                Normal = new Vector3D(-1, 0, 0),
                PlaneType = MPRPlaneType.Sagittal
            };
        }
        #endregion

        #region 重置MPR三平面 —— void ResetMprPlanes(VolumeData volumeData)
        /// <summary>
        /// 重置MPR三平面
        /// </summary>
        private void ResetMprPlanes(VolumeData volumeData)
        {
            #region # 验证

            if (volumeData == null)
            {
                return;
            }

            #endregion

            this.AxialPlane.Transform?.SetMatrix(Matrix4.Identity);
            this.CoronalPlane.Transform?.SetMatrix(Matrix4.Identity);
            this.SagittalPlane.Transform?.SetMatrix(Matrix4.Identity);
        }
        #endregion

        #endregion
    }
}
