using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Caliburn.Micro;
using IconPacks.Avalonia.MaterialDesign;
using MedicalSharp.Client.ViewModels.CommonContext;
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
using MedicalSharp.Presentation.Models;
using MedicalSharp.Primitives.Cameras;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Managers;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using SD.Infrastructure.Avalonia.Caliburn.Extensions;
using SD.Infrastructure.Avalonia.Commands;
using SD.Infrastructure.Avalonia.CustomControls;
using SD.Infrastructure.Avalonia.Enums;
using SD.IOC.Core.Mediators;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MedicalSharp.Client.ViewModels.VolumeContext
{
    /// <summary>
    /// CPR视图模型
    /// </summary>
    public class CprViewModel : ScreenBase, IHandle<SwitchViewportCommandEvent>, IHandle<RestoreViewportCommandEvent>, IHandle<MarkModeSwitchedEvent>, IHandle<MarkColorChangedEvent>, IHandle<SyncViewportEvent>
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
        public CprViewModel(IWindowManager windowManager, IEventAggregator eventAggregator, string title, CPRCamera camera, CPRInputManager inputManager)
        {
            this._windowManager = windowManager;
            this._eventAggregator = eventAggregator;
            this._eventAggregator.SubscribeOnUIThread(this);
            base.DisplayName = title;
            this.Camera = camera;
            this.InputManager = inputManager;

            //初始化工具栏
            this.InitToolbarCommands();

            //初始化曲线引导线
            this.CurveGuide = new CurveGuideVisual3D
            {
                Stroke = Colors.GreenYellow,
                StrokeThickness = 1.5f
            };

            //默认值
            this.ViewEnabled = false;
            this.Shapes = [];
            this.GrayModeChecked = true;
            this.Brightness = 1.0f;
            this.Contrast = 1.0f;
            this.InterpolationMode = InterpolationMode.Linear;
            this.TFControlPoints = new AvaloniaList<HUControlPoint>(ProtocolManager.SolidRainbowControlPoints);
            this.RadialWidth = 0.1f;
            this.RotationAngle = 0f;
            this.ProjectionThickness = 0.05f;
            this.MaxStepsCount = 100;
            this.ProjectionMode = IntensityProjectionMode.MIP;
            this.ProjectionDirection = CPRProjectionDirection.Tangent;
            this.StraightenDirection = CPRStraightenDirection.Horizontal;
            this.ArcPosition = 0.5f;
            this.CrossSectionSize = 0.1f;
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

        #region 灰度渲染模式选中 —— bool GrayModeChecked
        /// <summary>
        /// 灰度渲染模式选中
        /// </summary>
        public bool GrayModeChecked
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                this.RenderMode = CPRRenderMode.Gray;
            }
        }
        #endregion

        #region 伪彩渲染模式选中 —— bool PseudoColorChecked
        /// <summary>
        /// 伪彩渲染模式选中
        /// </summary>
        public bool PseudoColorChecked
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                this.RenderMode = CPRRenderMode.PseudoColor;
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

        #region 曲线3D元素 —— CurveVisual3D CurveVisual3D
        /// <summary>
        /// 曲线3D元素
        /// </summary>
        public CurveVisual3D CurveVisual3D
        {
            get;
            set
            {
                //注销旧订阅
                if (field != null)
                {
                    field.PropertyChanged -= this.OnCurveVisual3DPropertyChanged;
                }

                field = value;
                this.NotifyOfPropertyChange();

                //注册属性变化
                if (value != null)
                {
                    value.PropertyChanged += this.OnCurveVisual3DPropertyChanged;
                    this.CurveGuide.Curve = value.Curve;
                }
            }
        }
        #endregion

        #region CPR相机 —— CPRCamera Camera
        /// <summary>
        /// CPR相机
        /// </summary>
        [DependencyProperty]
        public CPRCamera Camera { get; set; }
        #endregion

        #region 输入管理器 —— CPRInputManager InputManager
        /// <summary>
        /// 输入管理器
        /// </summary>
        [DependencyProperty]
        public CPRInputManager InputManager { get; set; }
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

        #region 渲染模式 —— CPRRenderMode RenderMode
        /// <summary>
        /// 渲染模式
        /// </summary>
        [DependencyProperty]
        public CPRRenderMode RenderMode { get; set; }
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

        #region 曲线引导线 —— CurveGuideVisual3D CurveGuide
        /// <summary>
        /// 曲线引导线
        /// </summary>
        [DependencyProperty]
        public CurveGuideVisual3D CurveGuide { get; set; }
        #endregion

        #region CPR模式 —— CPRMode CPRMode
        /// <summary>
        /// CPR模式
        /// </summary>
        [DependencyProperty]
        public CPRMode CPRMode { get; set; }
        #endregion

        #region 径向宽度 —— float RadialWidth
        /// <summary>
        /// 径向宽度
        /// </summary>
        [DependencyProperty]
        public float RadialWidth { get; set; }
        #endregion

        #region 旋转角度 —— float RotationAngle
        /// <summary>
        /// 旋转角度
        /// </summary>
        [DependencyProperty]
        public float RotationAngle { get; set; }
        #endregion

        #region 投影厚度 —— float ProjectionThickness
        /// <summary>
        /// 投影厚度
        /// </summary>
        [DependencyProperty]
        public float ProjectionThickness { get; set; }
        #endregion

        #region 最大步数 —— int MaxStepsCount
        /// <summary>
        /// 最大步数
        /// </summary>
        [DependencyProperty]
        public int MaxStepsCount { get; set; }
        #endregion

        #region 投影模式 —— IntensityProjectionMode ProjectionMode
        /// <summary>
        /// 投影模式
        /// </summary>
        [DependencyProperty]
        public IntensityProjectionMode ProjectionMode { get; set; }
        #endregion

        #region 投影方向 —— CPRProjectionDirection ProjectionDirection
        /// <summary>
        /// 投影方向
        /// </summary>
        [DependencyProperty]
        public CPRProjectionDirection ProjectionDirection { get; set; }
        #endregion

        #region 拉直方向 —— CPRStraightenDirection StraightenDirection
        /// <summary>
        /// 拉直方向
        /// </summary>
        [DependencyProperty]
        public CPRStraightenDirection StraightenDirection { get; set; }
        #endregion

        #region 弧长位置 —— float ArcPosition
        /// <summary>
        /// 弧长位置
        /// </summary>
        [DependencyProperty]
        public float ArcPosition { get; set; }
        #endregion

        #region 剖面尺寸 —— float CrossSectionSize
        /// <summary>
        /// 剖面尺寸
        /// </summary>
        [DependencyProperty]
        public float CrossSectionSize { get; set; }
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

        #region 工具栏命令列表 —— AvaloniaList<ToolbarCommand> ToolbarCommands
        /// <summary>
        /// 工具栏命令列表
        /// </summary>
        [DependencyProperty]
        public AvaloniaList<ToolbarCommand> ToolbarCommands { get; set; }
        #endregion

        #region 只读属性 - CPR渲染视口 —— CPRViewport CPRViewport
        /// <summary>
        /// 只读属性 - CPR渲染视口
        /// </summary>
        public CPRViewport CPRViewport
        {
            get
            {
                CprView view = (CprView)this.GetView();
                return view?.CPRViewport;
            }
        }
        #endregion


        //命令

        #region 拾取体素命令 —— ICommand PickVoxelCommand
        /// <summary>
        /// 拾取体素命令
        /// </summary>
        public ICommand PickVoxelCommand => new RelayCommand(_ =>
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
        });
        #endregion

        #region 3D平移命令 —— ICommand Translate3DCommand
        /// <summary>
        /// 3D平移命令
        /// </summary>
        public ICommand Translate3DCommand => new RelayCommand(_ =>
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
        });
        #endregion

        #region 编辑顶点命令 —— ICommand EditVertexCommand
        /// <summary>
        /// 编辑顶点命令
        /// </summary>
        public ICommand EditVertexCommand => new RelayCommand(_ =>
        {
            Action<IVertexEditable> vertexEdited = _ =>
            {
                this.FrameToken++;

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
        });
        #endregion

        #region 拖拽引导线命令 —— ICommand DragCurveGuideCommand
        /// <summary>
        /// 拖拽引导线命令
        /// </summary>
        public ICommand DragGuideCommand => new RelayCommand(_ =>
        {
            Action<IDraggableAlongCurve> dragging = _ =>
            {
                //拖拽中：同步ArcPosition到CurveGuide
                this.ArcPosition = this.CurveGuide.ArcPosition;
            };
            Action<IDraggableAlongCurve> dragged = _ =>
            {
                this.ArcPosition = this.CurveGuide.ArcPosition;
            };

            DragCurveGuideCommand command = new DragCurveGuideCommand
            {
                Dragging = dragging,
                Dragged = dragged
            };
            this.InputManager.SwitchCommand(command);
        });
        #endregion

        #region 绘制文本命令 —— ICommand DrawTextCommand
        /// <summary>
        /// 绘制文本命令
        /// </summary>
        public ICommand DrawTextCommand => new RelayCommand(_ =>
        {
            Func<Vector3D> getNormal = () => new Vector3D(0, 0, 1);
            Func<TextVisual3D, Task<string>> drawStart = async shape =>
            {
                int count = this.Shapes.OfType<LineSegmentVisual3D>().Count();
                shape.DisplayName = $"文本{count + 1}";
                this.Shapes.Add(shape);

                TextViewModel viewModel = ResolveMediator.Resolve<TextViewModel>();
                bool? result = await this._windowManager.ShowDialogAsync(viewModel);
                if (result == true)
                {
                    return viewModel.Content;
                }

                return null;
            };
            Action<TextVisual3D> drawEnd = _ =>
            {
                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };
            Action<TextVisual3D> drawCancelled = shape => this.Shapes.Remove(shape);

            DrawTextCommand command = new DrawTextCommand
            {
                VisualPicked = this.OnVisualPicked,
                VisualRemoved = this.OnVisualRemoved,
                GetMarkValue = this.GetCurrentMarkValue,
                ShapeCut = this.OnShapeCutEnd,
                ShapeAnalysed = this.OnShapeAnalyseEnd,
                GetNormal = getNormal,
                DrawStart = drawStart,
                DrawEnd = drawEnd,
                DrawCancelled = drawCancelled
            };
            this.InputManager.SwitchCommand(command);
        });
        #endregion

        #region 绘制点命令 —— ICommand DrawPointCommand
        /// <summary>
        /// 绘制点命令
        /// </summary>
        public ICommand DrawPointCommand => new RelayCommand(_ =>
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
        });
        #endregion

        #region 绘制线段命令 —— ICommand DrawLineSegmentCommand
        /// <summary>
        /// 绘制线段命令
        /// </summary>
        public ICommand DrawLineSegmentCommand => new RelayCommand(_ =>
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
        });
        #endregion

        #region 绘制折线命令 —— ICommand DrawPolylineCommand
        /// <summary>
        /// 绘制折线命令
        /// </summary>
        public ICommand DrawPolylineCommand => new RelayCommand(_ =>
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
        });
        #endregion

        #region 绘制曲线命令 —— ICommand DrawCurveCommand
        /// <summary>
        /// 绘制曲线命令
        /// </summary>
        public ICommand DrawCurveCommand => new RelayCommand(_ =>
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
        });
        #endregion

        #region 绘制矩形命令 —— ICommand DrawRectangleCommand
        /// <summary>
        /// 绘制矩形命令
        /// </summary>
        public ICommand DrawRectangleCommand => new RelayCommand(_ =>
        {
            Func<Vector3D> getNormal = () => new Vector3D(0, 0, 1);
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
        });
        #endregion

        #region 绘制圆形命令 —— ICommand DrawCircleCommand
        /// <summary>
        /// 绘制圆形命令
        /// </summary>
        public ICommand DrawCircleCommand => new RelayCommand(_ =>
        {
            Func<Vector3D> getNormal = () => new Vector3D(0, 0, 1);
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
        });
        #endregion

        #region 绘制椭圆形命令 —— ICommand DrawEllipseCommand
        /// <summary>
        /// 绘制椭圆形命令
        /// </summary>
        public ICommand DrawEllipseCommand => new RelayCommand(_ =>
        {
            Func<Vector3D> getNormal = () => new Vector3D(0, 0, 1);
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
        });
        #endregion

        #region 绘制多边形命令 —— ICommand DrawPolygonCommand
        /// <summary>
        /// 绘制多边形命令
        /// </summary>
        public ICommand DrawPolygonCommand => new RelayCommand(_ =>
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
        });
        #endregion

        #region 绘制闭合曲线命令 —— ICommand DrawClosedCurveCommand
        /// <summary>
        /// 绘制闭合曲线命令
        /// </summary>
        public ICommand DrawClosedCurveCommand => new RelayCommand(_ =>
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
        });
        #endregion

        #region 复位参考线命令 —— ICommand ResetGuideCommand
        /// <summary>
        /// 复位参考线命令
        /// </summary>
        public ICommand ResetGuideCommand => new RelayCommand(_ =>
        {
            this.ArcPosition = 0.5f;
            this.RotationAngle = 0f;
            this.FrameToken++;
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

        #region 截屏命令 —— ICommand CaptureCommand
        /// <summary>
        /// 截屏命令
        /// </summary>
        public ICommand CaptureCommand => new AsyncRelayCommand(async _ =>
        {
            using SKBitmap bitmap = this.CPRViewport.Capture();

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

        #region 曲线3D元素属性变化事件 —— void OnCurveVisual3DPropertyChanged(object sender...
        /// <summary>
        /// 曲线3D元素属性变化事件
        /// </summary>
        private void OnCurveVisual3DPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.Property == CurveVisual3D.CurveProperty)
            {
                this.CurveGuide.Curve = (Curve)eventArgs.NewValue;
                this.FrameToken++;
            }
        }
        #endregion

        #region 处理切换视口命令事件 —— Task HandleAsync(SwitchViewportCommandEvent message...
        /// <summary>
        /// 处理切换视口命令事件
        /// </summary>
        public Task HandleAsync(SwitchViewportCommandEvent message, CancellationToken cancellationToken)
        {
            #region # 验证

            if (message.Publisher == this)
            {
                return Task.CompletedTask;
            }

            #endregion

            this.InputManager.SwitchCommand(message.Command);
            foreach (ToolbarCommand toolbarCommand in this.ToolbarCommands)
            {
                toolbarCommand.IsChecked = false;
            }

            return Task.CompletedTask;
        }
        #endregion

        #region 处理恢复视口命令事件 —— Task HandleAsync(RestoreViewportCommandEvent message...
        /// <summary>
        /// 处理恢复视口命令事件
        /// </summary>
        public Task HandleAsync(RestoreViewportCommandEvent message, CancellationToken cancellationToken)
        {
            #region # 验证

            if (message.Publisher == this)
            {
                return Task.CompletedTask;
            }

            #endregion

            if (this.ToolbarCommands.All(x => !x.IsChecked))
            {
                ToolbarCommand defaultCommand = this.ToolbarCommands.Single(x => x.IsDefault);
                defaultCommand.IsChecked = true;
            }

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

        #region 初始化工具栏命令 —— void InitToolbarCommands()
        /// <summary>
        /// 初始化工具栏命令
        /// </summary>
        private void InitToolbarCommands()
        {
            this.ToolbarCommands =
            [
                new ToolbarCommand("拾取体素", "Icon-PickVoxel", this.PickVoxelCommand),
                new ToolbarCommand("平移", "Icon-Translate3D", this.Translate3DCommand, true, true, true),
                new ToolbarCommand("编辑顶点", "Icon-EditVertex", this.EditVertexCommand),
                new ToolbarCommand("拖拽引导线", "Icon-DragGuide", this.DragGuideCommand),
                new ToolbarCommand("绘制文本", "Icon-Text", this.DrawTextCommand),
                new ToolbarCommand("绘制点", "Icon-Point", this.DrawPointCommand),
                new ToolbarCommand("绘制线段", "Icon-LineSegment", this.DrawLineSegmentCommand),
                new ToolbarCommand("绘制折线", "Icon-Polyline", this.DrawPolylineCommand),
                new ToolbarCommand("绘制曲线", "Icon-Curve", this.DrawCurveCommand),
                new ToolbarCommand("绘制矩形", "Icon-Rectangle", this.DrawRectangleCommand),
                new ToolbarCommand("绘制圆形", "Icon-Circle", this.DrawCircleCommand),
                new ToolbarCommand("绘制椭圆形", "Icon-Ellipse", this.DrawEllipseCommand),
                new ToolbarCommand("绘制多边形", "Icon-Polygon", this.DrawPolygonCommand),
                new ToolbarCommand("绘制闭合曲线", "Icon-ClosedCurve", this.DrawClosedCurveCommand)
            ];
        }
        #endregion

        #endregion
    }
}
