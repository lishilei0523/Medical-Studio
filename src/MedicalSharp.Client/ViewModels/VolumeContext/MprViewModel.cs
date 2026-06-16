using Avalonia;
using Avalonia.Collections;
using Avalonia.Platform.Storage;
using Caliburn.Micro;
using IconPacks.Avalonia.MaterialDesign;
using MedicalSharp.Client.ViewModels.ProtocolContext;
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
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using MedicalSharp.Primitives.Models.Arguments;
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
    /// MPR视图模型
    /// </summary>
    public class MprViewModel : ScreenBase, IHandle<ClearShapesEvent>, IHandle<TissueSelectedEvent>, IHandle<MarkModeSwitchedEvent>, IHandle<MarkColorChangedEvent>, IHandle<SyncViewportEvent>, IHandle<ShapeTranslatingEvent>, IHandle<ShapeRotatingEvent>, IHandle<MPRPlaneChangedEvent>, IHandle<MPRPlaneResetEvent>
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
        public MprViewModel(IWindowManager windowManager, IEventAggregator eventAggregator, string title, MPRCamera camera, MPRInputManager inputManager)
        {
            this._windowManager = windowManager;
            this._eventAggregator = eventAggregator;
            this._eventAggregator.SubscribeOnUIThread(this);
            this.Title = title;
            this.Camera = camera;
            this.InputManager = inputManager;

            //初始化预设协议
            this.InitPresetProtocols();

            //默认值
            this.ViewEnabled = false;
            this.ToolbarConfig = new MprToolbar();
            this.Shapes = [];
            this.GrayModeChecked = true;
            this.Crosshair = new CrosshairVisual3D();
            this.CrosshairVisible = true;
            this.DirectionVisible = true;
            this.WindowLevelVisible = true;
            this.Brightness = 1.0f;
            this.Contrast = 1.0f;
            this.InterpolationMode = InterpolationMode.Linear;
            this.TFControlPoints = new AvaloniaList<HUControlPoint>(ProtocolManager.SolidRainbowControlPoints);
            this.Translate3D();
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

        #region 工具栏配置 —— MprToolbar ToolbarConfig
        /// <summary>
        /// 工具栏配置
        /// </summary>
        [DependencyProperty]
        public MprToolbar ToolbarConfig { get; set; }
        #endregion

        #region 已选组织 —— TissueInfo SelectedTissue
        /// <summary>
        /// 已选组织
        /// </summary>
        public TissueInfo SelectedTissue { get; set; }
        #endregion

        #region 灰度渲染模式选中 —— bool GrayModeChecked
        /// <summary>
        /// 灰度渲染模式选中
        /// </summary>
        public bool GrayModeChecked
        {
            get => field;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                this.RenderMode = MPRRenderMode.Gray;
            }
        }
        #endregion

        #region 伪彩渲染模式选中 —— bool PseudoColorChecked
        /// <summary>
        /// 伪彩渲染模式选中
        /// </summary>
        public bool PseudoColorChecked
        {
            get => field;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                this.RenderMode = MPRRenderMode.PseudoColor;
            }
        }
        #endregion

        #region 十字线是否可见 —— bool CrosshairVisible
        /// <summary>
        /// 十字线是否可见
        /// </summary>
        public bool CrosshairVisible
        {
            get => field;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                this.Crosshair.IsVisible = value;
                this.FrameToken++;
            }
        }
        #endregion

        #region 方向标识是否可见 —— bool DirectionVisible
        /// <summary>
        /// 方向标识是否可见
        /// </summary>
        [DependencyProperty]
        public bool DirectionVisible { get; set; }
        #endregion

        #region 窗宽/窗位是否可见 —— bool WindowLevelVisible
        /// <summary>
        /// 窗宽/窗位是否可见
        /// </summary>
        [DependencyProperty]
        public bool WindowLevelVisible { get; set; }
        #endregion

        #region 帧令牌 —— int FrameToken
        /// <summary>
        /// 帧令牌
        /// </summary>
        [DependencyProperty]
        public int FrameToken { get; set; }
        #endregion

        #region 标题 —— string Title
        /// <summary>
        /// 标题
        /// </summary>
        [DependencyProperty]
        public string Title { get; set; }
        #endregion

        #region MPR平面 —— MPRPlane Plane
        /// <summary>
        /// MPR平面
        /// </summary>
        public MPRPlane Plane
        {
            get;
            set
            {
                if (field != null)
                {
                    field.PlaneChangedEvent -= this.OnMPRPlaneChanged;
                }

                if (value != null)
                {
                    value.PlaneChangedEvent += this.OnMPRPlaneChanged;

                    //初始化十字线方向和位置
                    this.Crosshair.UAxis = value.WorldUAxis.Normalized().ToVector3();
                    this.Crosshair.VAxis = value.WorldVAxis.Normalized().ToVector3();
                    this.Crosshair.Center = value.WorldCenter.ToVector3();
                    this.Crosshair.Transform?.SetPosition(value.WorldCenter);

                    //更新方向
                    this.FourDirection = value.DirectionIndicator.ToDirectionInfo();
                }

                field = value;
                this.NotifyOfPropertyChange();
            }
        }
        #endregion

        #region MPR相机 —— MPRCamera Camera
        /// <summary>
        /// MPR相机
        /// </summary>
        [DependencyProperty]
        public MPRCamera Camera { get; set; }
        #endregion

        #region 输入管理器 —— MPRInputManager InputManager
        /// <summary>
        /// 输入管理器
        /// </summary>
        [DependencyProperty]
        public MPRInputManager InputManager { get; set; }
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

        #region 渲染模式 —— MPRRenderMode RenderMode
        /// <summary>
        /// 渲染模式
        /// </summary>
        [DependencyProperty]
        public MPRRenderMode RenderMode { get; set; }
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

        #region 对比度 —— float Contrast
        /// <summary>
        /// 对比度
        /// </summary>
        [DependencyProperty]
        public float Contrast { get; set; }
        #endregion

        #region 插值模式 —— InterpolationMode InterpolationMode
        /// <summary>
        /// 插值模式
        /// </summary>
        [DependencyProperty]
        public InterpolationMode InterpolationMode { get; set; }
        #endregion

        #region 传递函数控制点列表 —— AvaloniaList<HUControlPoint> TFControlPoints
        /// <summary>
        /// 传递函数控制点列表
        /// </summary>
        [DependencyProperty]
        public AvaloniaList<HUControlPoint> TFControlPoints { get; set; }
        #endregion

        #region 十字线 —— CrosshairVisual3D Crosshair
        /// <summary>
        /// 十字线
        /// </summary>
        [DependencyProperty]
        public CrosshairVisual3D Crosshair { get; set; }
        #endregion

        #region 四方向标识 —— FourDirectionInfo FourDirection
        /// <summary>
        /// 四方向标识
        /// </summary>
        [DependencyProperty]
        public FourDirectionInfo FourDirection { get; set; }
        #endregion

        #region 形状列表 —— AvaloniaList<ShapeVisual3D> Shapes
        /// <summary>
        /// 形状列表
        /// </summary>
        [DependencyProperty]
        public AvaloniaList<ShapeVisual3D> Shapes { get; set; }
        #endregion

        #region 已选协议 —— MprProtocol SelectedProtocol
        /// <summary>
        /// 已选协议
        /// </summary>
        public MprProtocol SelectedProtocol
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
                    this.Contrast = value.Contrast;
                    this.InterpolationMode = value.InterpolationMode;
                    this.TFControlPoints = new AvaloniaList<HUControlPoint>(value.ControlPoints.Select(x => x.ToHUControlPoint()));
                    this.FrameToken++;
                }
            }
        }
        #endregion

        #region 预设协议列表 —— AvaloniaList<MprProtocol> PresetProtocols
        /// <summary>
        /// 预设协议列表
        /// </summary>
        [DependencyProperty]
        public AvaloniaList<MprProtocol> PresetProtocols { get; set; }
        #endregion

        #region 只读属性 - MPR渲染视口 —— MPRViewport MPRViewport
        /// <summary>
        /// 只读属性 - MPR渲染视口
        /// </summary>
        public MPRViewport MPRViewport
        {
            get
            {
                MprView view = (MprView)this.GetView();
                return view?.MPRViewport;
            }
        }
        #endregion


        //命令

        #region 复位十字线命令 —— ICommand ResetCrosshairCommand
        /// <summary>
        /// 复位十字线命令
        /// </summary>
        public ICommand ResetCrosshairCommand => new RelayCommand(_ =>
        {
            this.Crosshair.UAxis = this.Plane.WorldUAxis.ToVector3();
            this.Crosshair.VAxis = this.Plane.WorldVAxis.ToVector3();
            this.Crosshair.Center = this.Plane.WorldCenter.ToVector3();
            this.Crosshair.Transform.SetMatrix(Matrix4.Identity);
            this.Crosshair.Transform.SetPosition(this.Plane.WorldCenter);
            this.Plane.ResetToStandard();
            this.FrameToken++;

            //发布事件
            MPRPlaneResetEvent message = new MPRPlaneResetEvent
            {
                Publisher = this
            };
            this._eventAggregator.PublishOnUIThreadAsync(message);
        }, _ => this.VolumeData != null);
        #endregion

        #region 重置相机命令 —— ICommand ResetCameraCommand
        /// <summary>
        /// 重置相机命令
        /// </summary>
        public ICommand ResetCameraCommand => new AsyncRelayCommand(async _ =>
        {
            this.Camera.Reset();
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
            MprProtocol protocol = this.SelectedProtocol;
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
            MprProtocolViewModel viewModel = ResolveMediator.Resolve<MprProtocolViewModel>();
            viewModel.MprViewModel = this;
            await this._windowManager.ShowWindowAsync(viewModel);
        }, _ => this.VolumeData != null);
        #endregion

        #region 截屏命令 —— ICommand CaptureCommand
        /// <summary>
        /// 截屏命令
        /// </summary>
        public ICommand CaptureCommand => new AsyncRelayCommand(async _ =>
        {
            using SKBitmap bitmap = this.MPRViewport.Capture();

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

            PickVoxelCommand command = new PickVoxelCommand();
            command.VoxelPicked = picked;
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 拾取形状 —— void PickShape()
        /// <summary>
        /// 拾取形状
        /// </summary>
        public void PickShape()
        {
            PickVisual3DCommand command = new PickVisual3DCommand();
            command.VisualPicked = this.OnVisualPicked;
            command.VisualRemoved = this.OnVisualRemoved;
            command.GetMarkValue = () => this.SelectedTissue.MarkValue;
            command.ShapeCut = this.OnShapeCutEnd;
            command.ShapeAnalysed = this.OnShapeAnalyseEnd;
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 3D平移 —— void Translate3D()
        /// <summary>
        /// 3D平移
        /// </summary>
        public void Translate3D()
        {
            Action<ITranslatable3D> translating = translatable =>
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
            Action<ITranslatable3D> translated = _ =>
            {
                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };

            TranslateVisual3DCommand command = new TranslateVisual3DCommand();
            command.Translating = translating;
            command.Translated = translated;
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 2D旋转 —— void Rotate2D()
        /// <summary>
        /// 2D旋转
        /// </summary>
        public void Rotate2D()
        {
            Action<IRotatable> rotateEnd = _ =>
            {
                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };

            RotateVisual2DCommand command = new RotateVisual2DCommand();
            command.RotateEnd = rotateEnd;
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 调整尺寸 —— void Resize()
        /// <summary>
        /// 调整尺寸
        /// </summary>
        public void Resize()
        {
            Action<IResizable2D> resizeEnd = _ =>
            {
                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };

            ResizeVisual2DCommand command = new ResizeVisual2DCommand();
            command.ResizeEnd = resizeEnd;
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 编辑顶点 —— void EditVertex()
        /// <summary>
        /// 编辑顶点
        /// </summary>
        public void EditVertex()
        {
            Action<IVertexEditable> editVertexEnd = _ =>
            {
                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };

            EditVertexCommand command = new EditVertexCommand();
            command.EditVertexEnd = editVertexEnd;
            this.InputManager.SwitchCommand(command);
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

            DrawPointCommand command = new DrawPointCommand();
            command.DrawEnd = drawEnd;
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

            DrawLineSegmentCommand command = new DrawLineSegmentCommand();
            command.DrawStart = drawStart;
            command.DrawEnd = drawEnd;
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制矩形 —— void DrawRectangle()
        /// <summary>
        /// 绘制矩形
        /// </summary>
        public void DrawRectangle()
        {
            Func<Vector3D> getNormal = () => this.Plane.Normal.ToVector3();
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

            DrawRectangleCommand command = new DrawRectangleCommand();
            command.GetNormal = getNormal;
            command.DrawStart = drawStart;
            command.DrawEnd = drawEnd;
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制圆形 —— void DrawCircle()
        /// <summary>
        /// 绘制圆形
        /// </summary>
        public void DrawCircle()
        {
            Func<Vector3D> getNormal = () => this.Plane.Normal.ToVector3();
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

            DrawCircleCommand command = new DrawCircleCommand();
            command.GetNormal = getNormal;
            command.DrawStart = drawStart;
            command.DrawEnd = drawEnd;
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制椭圆形 —— void DrawEllipse()
        /// <summary>
        /// 绘制椭圆形
        /// </summary>
        public void DrawEllipse()
        {
            Func<Vector3D> getNormal = () => this.Plane.Normal.ToVector3();
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

            DrawEllipseCommand command = new DrawEllipseCommand();
            command.GetNormal = getNormal;
            command.DrawStart = drawStart;
            command.DrawEnd = drawEnd;
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
            Action<PolylineVisual3D> drawEnd = shape =>
            {
                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };
            Action<PolylineVisual3D> drawCancelled = shape => this.Shapes.Remove(shape);

            DrawPolylineCommand command = new DrawPolylineCommand(false);
            command.DrawStart = drawStart;
            command.DrawEnd = drawEnd;
            command.DrawCancelled = drawCancelled;
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

            DrawCurveCommand command = new DrawCurveCommand(false);
            command.DrawStart = drawStart;
            command.DrawEnd = drawEnd;
            command.DrawCancelled = drawCancelled;
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
            Action<PolylineVisual3D> drawEnd = shape =>
            {
                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };
            Action<PolylineVisual3D> drawCancelled = shape => this.Shapes.Remove(shape);

            DrawPolylineCommand command = new DrawPolylineCommand(true);
            command.DrawStart = drawStart;
            command.DrawEnd = drawEnd;
            command.DrawCancelled = drawCancelled;
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

            DrawCurveCommand command = new DrawCurveCommand(true);
            command.DrawStart = drawStart;
            command.DrawEnd = drawEnd;
            command.DrawCancelled = drawCancelled;
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
            if (visual3D is ShapeVisual3D shapeVisual3D)
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
            if (visual3D is ShapeVisual3D shapeVisual3D)
            {
                this.Shapes.Remove(shapeVisual3D);
            }
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

        #region MPR平面变化事件 —— void OnMPRPlaneChanged(object sender...
        /// <summary>
        /// MPR平面变化事件
        /// </summary>
        private void OnMPRPlaneChanged(object sender, MPRPlaneChangedEventArgs eventArgs)
        {
            MPRPlane plane = (MPRPlane)sender;
            if (eventArgs.TriggerSource == MPRPlaneChangeSource.SliceScroll)
            {
                this.Crosshair.UAxis = plane.WorldUAxis.ToVector3();
                this.Crosshair.VAxis = plane.WorldVAxis.ToVector3();
                this.Crosshair.Center = plane.WorldCenter.ToVector3();

                //平移世界步长
                this.Crosshair.Transform?.Translate(plane.WorldSliceStep);
            }
            if (eventArgs.TriggerSource == MPRPlaneChangeSource.ExternalSync)
            {
                this.Crosshair.UAxis = plane.WorldUAxis.ToVector3();
                this.Crosshair.VAxis = plane.WorldVAxis.ToVector3();
                this.Crosshair.Center = plane.WorldCenter.ToVector3();

                //设置世界位置
                this.Crosshair.Transform?.SetPosition(plane.WorldCenter);
            }

            //更新方向
            this.FourDirection = plane.DirectionIndicator.ToDirectionInfo();

            //发布消息
            MPRPlaneChangedEvent message = new MPRPlaneChangedEvent
            {
                Publisher = this,
                Plane = plane,
                TriggerSource = eventArgs.TriggerSource,
                Crosshair = this.Crosshair,
                SkipVolumeSync = eventArgs.TriggerSource == MPRPlaneChangeSource.ExternalSync
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

        #region 处理组织选中事件 —— Task HandleAsync(TissueSelectedEvent message...
        /// <summary>
        /// 处理组织选中事件
        /// </summary>
        public Task HandleAsync(TissueSelectedEvent message, CancellationToken cancellationToken)
        {
            this.SelectedTissue = message.TissueInfo;

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

        #region 处理形状平移中事件 —— Task HandleAsync(ShapeTranslatingEvent message...
        /// <summary>
        /// 处理形状平移中事件
        /// </summary>
        public Task HandleAsync(ShapeTranslatingEvent message, CancellationToken cancellationToken)
        {
            #region # 验证

            if (message.Publisher == this)
            {
                return Task.CompletedTask;
            }
            if (message.Shape == null)
            {
                return Task.CompletedTask;
            }

            #endregion

            //十字线平移
            if (message.Shape is CrosshairVisual3D crosshair)
            {
                this.Crosshair.Transform?.SetPosition(crosshair.Transform.Position);
                this.Plane.Relocate(crosshair.Transform.Position);
                this.FrameToken++;
            }

            //面平移
            if (message.Shape is MPRPlaneVisual3D mprPlane && mprPlane.PlaneType == this.Plane.OriginalPlaneType)
            {
                this.Plane.Relocate(mprPlane.WorldUAxis, mprPlane.WorldVAxis, mprPlane.WorldCenter, mprPlane.WorldNormal);
                this.FrameToken++;
            }

            return Task.CompletedTask;
        }
        #endregion

        #region 处理形状旋转中事件 —— Task HandleAsync(ShapeRotatingEvent message...
        /// <summary>
        /// 处理形状旋转中事件
        /// </summary>
        public Task HandleAsync(ShapeRotatingEvent message, CancellationToken cancellationToken)
        {
            #region # 验证

            if (message.Publisher == this)
            {
                return Task.CompletedTask;
            }
            if (message.Shape == null)
            {
                return Task.CompletedTask;
            }

            #endregion

            //面旋转
            if (message.Shape is MPRPlaneVisual3D mprPlane && mprPlane.PlaneType == this.Plane.OriginalPlaneType)
            {
                this.Plane.Relocate(mprPlane.WorldUAxis, mprPlane.WorldVAxis, mprPlane.WorldCenter, mprPlane.WorldNormal);
                this.FrameToken++;
            }

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
            if (message.Crosshair == null)
            {
                return Task.CompletedTask;
            }

            #endregion

            if (message.TriggerSource is MPRPlaneChangeSource.SliceScroll or MPRPlaneChangeSource.ExternalSync)
            {
                this.Crosshair.Transform?.SetPosition(message.Crosshair.Transform.Position);
                this.FrameToken++;
            }

            return Task.CompletedTask;
        }
        #endregion

        #region 处理MPR平面重置事件 —— Task HandleAsync(MPRPlaneResetEvent message...
        /// <summary>
        /// 处理MPR平面重置事件
        /// </summary>
        public Task HandleAsync(MPRPlaneResetEvent message, CancellationToken cancellationToken)
        {
            #region # 验证

            if (message.Publisher == this)
            {
                return Task.CompletedTask;
            }

            #endregion

            this.Plane.ResetToStandard();
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
            string[] protocolFiles = Directory.GetFiles(Constants.MPRProtocolPath);
            List<MprProtocol> protocols = [];
            foreach (string protocolFile in protocolFiles)
            {
                string json = await File.ReadAllTextAsync(protocolFile);
                MprProtocol protocol = json.AsJsonTo<MprProtocol>();
                protocols.Add(protocol);
            }
            this.PresetProtocols = new AvaloniaList<MprProtocol>(protocols);
        }
        #endregion

        #endregion
    }
}
