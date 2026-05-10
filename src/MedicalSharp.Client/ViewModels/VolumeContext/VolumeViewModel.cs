using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using Caliburn.Micro;
using IconPacks.Avalonia.MaterialDesign;
using MedicalSharp.Client.Events;
using MedicalSharp.Client.Views.VolumeContext;
using MedicalSharp.Controls.Commands;
using MedicalSharp.Controls.Commands.Arguments;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.InputManagers;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Primitives.Cameras;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Managers;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using SD.Infrastructure.Avalonia.Commands;
using SD.Infrastructure.Avalonia.CustomControls;
using SD.Infrastructure.Avalonia.Enums;
using System;
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
    public class VolumeViewModel : ScreenBase, IHandle<SyncViewportEvent>, IHandle<ShapeDrawnEvent>, IHandle<ShapeSyncEvent>, IHandle<ShapeRemovedEvent>, IHandle<MPRPlaneChangedEvent>
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
            Vector3 cameraPosition = new Vector3(0, 2, 0);
            Vector3 targetPosition = new Vector3(0.0f);
            Vector3 upDirection = new Vector3(0, 0, 1);
            this.Camera = new OrbitPerspectiveCamera(cameraPosition, targetPosition, upDirection);
            this.TFControlPoints = new AvaloniaList<TFControlPoint>(ResourceManager.GrayControlPoints);

            //初始化输入管理器
            this.InputManager = new OrbitInputManager(this.Camera);
            this.TranslateNormal();

            //初始化MPR平面
            this.InitMprPlanes();

            //默认值
            this.Shapes = [];
            this.RaycastChecked = true;
            this.AxialPlaneVisible = true;
            this.CoronalPlaneVisible = true;
            this.SagittalPlaneVisible = true;
        }

        #endregion

        #region # 属性

        //属性

        #region Raycast渲染模式选中 —— bool RaycastChecked
        /// <summary>
        /// Raycast渲染模式选中
        /// </summary>
        public bool RaycastChecked
        {
            get => field;
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
            get => field;
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
            get => field;
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
            get => field;
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
            get => field;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                this.AxialPlane.IsVisible = value;
                this.VolumeViewport?.RequestNextFrameRendering();
            }
        }
        #endregion

        #region 冠状面是否可见 —— bool CoronalPlaneVisible
        /// <summary>
        /// 冠状面是否可见
        /// </summary>
        public bool CoronalPlaneVisible
        {
            get => field;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                this.CoronalPlane.IsVisible = value;
                this.VolumeViewport?.RequestNextFrameRendering();
            }
        }
        #endregion

        #region 矢状面是否可见 —— bool SagittalPlaneVisible
        /// <summary>
        /// 矢状面是否可见
        /// </summary>
        public bool SagittalPlaneVisible
        {
            get => field;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                this.SagittalPlane.IsVisible = value;
                this.VolumeViewport?.RequestNextFrameRendering();
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
                    this.SelectedShape = null;
                    this.Shapes.Clear();
                }
            }
        }
        #endregion

        #region 传递函数控制点列表 —— AvaloniaList<TFControlPoint> TFControlPoints
        /// <summary>
        /// 传递函数控制点列表
        /// </summary>
        [DependencyProperty]
        public AvaloniaList<TFControlPoint> TFControlPoints { get; set; }
        #endregion

        #region 体积渲染模式 —— VolumeRenderMode RenderMode
        /// <summary>
        /// 体积渲染模式
        /// </summary>
        [DependencyProperty]
        public VolumeRenderMode RenderMode { get; set; }
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

        #region 选中的形状 —— ShapeVisual3D SelectedShape
        /// <summary>
        /// 选中的形状
        /// </summary>
        [DependencyProperty]
        public ShapeVisual3D SelectedShape { get; set; }
        #endregion

        #region 形状列表 —— AvaloniaList<ShapeVisual3D> Shapes
        /// <summary>
        /// 形状列表
        /// </summary>
        [DependencyProperty]
        public AvaloniaList<ShapeVisual3D> Shapes { get; set; }
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
            this.AxialPlane.Transform.SetMatrix(Matrix4.Identity);
            this.CoronalPlane.Transform.SetMatrix(Matrix4.Identity);
            this.SagittalPlane.Transform.SetMatrix(Matrix4.Identity);
            this.VolumeViewport.RequestNextFrameRendering();

            //发布事件
            ShapeTranslatingEvent messageAxial = new ShapeTranslatingEvent
            {
                Publisher = this,
                Shape = this.AxialPlane
            };
            ShapeTranslatingEvent messageCoronal = new ShapeTranslatingEvent
            {
                Publisher = this,
                Shape = this.SagittalPlane
            };
            ShapeTranslatingEvent messageSagittal = new ShapeTranslatingEvent
            {
                Publisher = this,
                Shape = this.SagittalPlane
            };
            this._eventAggregator.PublishOnUIThreadAsync(messageAxial);
            this._eventAggregator.PublishOnUIThreadAsync(messageCoronal);
            this._eventAggregator.PublishOnUIThreadAsync(messageSagittal);
        });
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
                    builder.AppendLine($"点击2D坐标: X:{mousePos2D.X}, Y:{mousePos2D.Y}");
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

            PickVoxelCommand command = new PickVoxelCommand(picked);
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 拾取形状 —— void PickShape()
        /// <summary>
        /// 拾取形状
        /// </summary>
        public void PickShape()
        {
            Action<Visual3DPickedEventArgs> picked = e =>
            {
                if (e.PickedVisual is ShapeVisual3D shapeVisual3D)
                {
                    this.SelectedShape = shapeVisual3D;
                }
            };
            Action<Visual3D> removed = visual =>
            {
                if (visual is ShapeVisual3D shapeVisual3D)
                {
                    this.SelectedShape = null;
                    this.Shapes.Remove(shapeVisual3D);

                    ShapeRemovedEvent message = new ShapeRemovedEvent
                    {
                        Publisher = this,
                        Shape = shapeVisual3D
                    };
                    this._eventAggregator.PublishOnUIThreadAsync(message);
                }
            };

            PickVisual3DCommand command = new PickVisual3DCommand(picked, removed);
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 3D平移 —— void Translate3D()
        /// <summary>
        /// 3D平移
        /// </summary>
        public void Translate3D()
        {
            Action<ITranslatable3D> translateEnd = translatable =>
            {
                if (translatable is ShapeVisual3D shape)
                {
                    ShapeSyncEvent message = new ShapeSyncEvent
                    {
                        Publisher = this,
                        Shape = shape
                    };
                    this._eventAggregator.PublishOnUIThreadAsync(message);
                }
            };

            TranslateVisual3DCommand command = new TranslateVisual3DCommand(translateEnd);
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
            Action<ITranslatableNormal> translateEnd = translatable =>
            {
                if (translatable is ShapeVisual3D shape)
                {
                    ShapeSyncEvent message = new ShapeSyncEvent
                    {
                        Publisher = this,
                        Shape = shape
                    };
                    this._eventAggregator.PublishOnUIThreadAsync(message);
                }
            };

            TranslateVisualNormalCommand command = new TranslateVisualNormalCommand(translateEnd);
            command.TranslatingEvent = translating;
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
            Action<IRotatable> rotateEnd = rotatable =>
            {
                if (rotatable is ShapeVisual3D shape)
                {
                    ShapeSyncEvent message = new ShapeSyncEvent
                    {
                        Publisher = this,
                        Shape = shape
                    };
                    this._eventAggregator.PublishOnUIThreadAsync(message);
                }
            };

            RotateVisualUCommand command = new RotateVisualUCommand(rotateEnd);
            command.RotatingEvent = rotating;
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
            Action<IRotatable> rotateEnd = rotatable =>
            {
                if (rotatable is ShapeVisual3D shape)
                {
                    ShapeSyncEvent message = new ShapeSyncEvent
                    {
                        Publisher = this,
                        Shape = shape
                    };
                    this._eventAggregator.PublishOnUIThreadAsync(message);
                }
            };

            RotateVisualVCommand command = new RotateVisualVCommand(rotateEnd);
            command.RotatingEvent = rotating;
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
            Action<IRotatable> rotateEnd = rotatable =>
            {
                if (rotatable is ShapeVisual3D shape)
                {
                    ShapeSyncEvent message = new ShapeSyncEvent
                    {
                        Publisher = this,
                        Shape = shape
                    };
                    this._eventAggregator.PublishOnUIThreadAsync(message);
                }
            };

            RotateVisual3DCommand command = new RotateVisual3DCommand(rotateEnd);
            command.RotatingEvent = rotating;
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 调整尺寸 —— void Resize()
        /// <summary>
        /// 调整尺寸
        /// </summary>
        public void Resize()
        {
            Action<IResizable3D> resizeEnd = resizable =>
            {
                if (resizable is ShapeVisual3D shape)
                {
                    ShapeSyncEvent message = new ShapeSyncEvent
                    {
                        Publisher = this,
                        Shape = shape
                    };
                    this._eventAggregator.PublishOnUIThreadAsync(message);
                }
            };

            ResizeVisual3DCommand command = new ResizeVisual3DCommand(resizeEnd);
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 编辑顶点 —— void EditVertex()
        /// <summary>
        /// 编辑顶点
        /// </summary>
        public void EditVertex()
        {
            Action<IVertexEditable> vertexEditEnd = vertexEditable =>
            {
                if (vertexEditable is ShapeVisual3D shape)
                {
                    ShapeSyncEvent message = new ShapeSyncEvent
                    {
                        Publisher = this,
                        Shape = shape
                    };
                    this._eventAggregator.PublishOnUIThreadAsync(message);
                }
            };

            EditVertexCommand command = new EditVertexCommand(vertexEditEnd);
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
                this.Shapes.Add(shape);
                ShapeDrawnEvent message = new ShapeDrawnEvent
                {
                    Publisher = this,
                    Shape = shape
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };

            DrawPointCommand command = new DrawPointCommand(drawEnd);
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制线段 —— void DrawLineSegment()
        /// <summary>
        /// 绘制线段
        /// </summary>
        public void DrawLineSegment()
        {
            Action<LineSegmentVisual3D> drawStart = shape => this.Shapes.Add(shape);
            Action<LineSegmentVisual3D> drawEnd = shape =>
            {
                ShapeDrawnEvent message = new ShapeDrawnEvent
                {
                    Publisher = this,
                    Shape = shape
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };

            DrawLineSegmentCommand command = new DrawLineSegmentCommand(drawStart, drawEnd);
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
            Action<RectangleVisual3D> drawStart = shape => this.Shapes.Add(shape);
            Action<RectangleVisual3D> drawEnd = null;//不同步

            DrawRectangleCommand command = new DrawRectangleCommand(drawStart, drawEnd, getNormal);
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
            Action<CircleVisual3D> drawStart = shape => this.Shapes.Add(shape);
            Action<CircleVisual3D> drawEnd = null;//不同步

            DrawCircleCommand command = new DrawCircleCommand(drawStart, drawEnd, getNormal);
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
            Action<EllipseVisual3D> drawStart = shape => this.Shapes.Add(shape);
            Action<EllipseVisual3D> drawEnd = null;//不同步

            DrawEllipseCommand command = new DrawEllipseCommand(drawStart, drawEnd, getNormal);
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制折线 —— void DrawPolyline()
        /// <summary>
        /// 绘制折线
        /// </summary>
        public void DrawPolyline()
        {
            Action<PolylineVisual3D> drawStart = shape => this.Shapes.Add(shape);
            Action<PolylineVisual3D> drawEnd = shape =>
            {
                ShapeDrawnEvent message = new ShapeDrawnEvent
                {
                    Publisher = this,
                    Shape = shape
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };
            Action<PolylineVisual3D> drawCancel = shape => this.Shapes.Remove(shape);

            DrawPolylineCommand command = new DrawPolylineCommand(drawStart, drawEnd, drawCancel, false);
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制曲线 —— void DrawCurve()
        /// <summary>
        /// 绘制曲线
        /// </summary>
        public void DrawCurve()
        {
            Action<CurveVisual3D> drawStart = shape => this.Shapes.Add(shape);
            Action<CurveVisual3D> drawEnd = shape =>
            {
                ShapeDrawnEvent message = new ShapeDrawnEvent
                {
                    Publisher = this,
                    Shape = shape
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };
            Action<CurveVisual3D> drawCancel = shape => this.Shapes.Remove(shape);

            DrawCurveCommand command = new DrawCurveCommand(drawStart, drawEnd, drawCancel, false);
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制多边形 —— void DrawPolyline()
        /// <summary>
        /// 绘制多边形
        /// </summary>
        public void DrawPolygon()
        {
            Action<PolylineVisual3D> drawStart = shape => this.Shapes.Add(shape);
            Action<PolylineVisual3D> drawEnd = shape =>
            {
                ShapeDrawnEvent message = new ShapeDrawnEvent
                {
                    Publisher = this,
                    Shape = shape
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };
            Action<PolylineVisual3D> drawCancel = shape => this.Shapes.Remove(shape);

            DrawPolylineCommand command = new DrawPolylineCommand(drawStart, drawEnd, drawCancel, true);
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制闭合曲线 —— void DrawClosedCurve()
        /// <summary>
        /// 绘制闭合曲线
        /// </summary>
        public void DrawClosedCurve()
        {
            Action<CurveVisual3D> drawStart = shape => this.Shapes.Add(shape);
            Action<CurveVisual3D> drawEnd = shape =>
            {
                ShapeDrawnEvent message = new ShapeDrawnEvent
                {
                    Publisher = this,
                    Shape = shape
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };
            Action<CurveVisual3D> drawCancel = shape => this.Shapes.Remove(shape);

            DrawCurveCommand command = new DrawCurveCommand(drawStart, drawEnd, drawCancel, true);
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制立方体 —— void DrawBox()
        /// <summary>
        /// 绘制立方体
        /// </summary>
        public void DrawBox()
        {
            Action<BoundingBoxVisual3D> drawStart = shape => this.Shapes.Add(shape);
            Action<BoundingBoxVisual3D> drawEnd = shape =>
            {
                ShapeDrawnEvent message = new ShapeDrawnEvent
                {
                    Publisher = this,
                    Shape = shape
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };

            DrawBoundingBoxCommand command = new DrawBoundingBoxCommand(drawStart, drawEnd);
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制球体 —— void DrawSphere()
        /// <summary>
        /// 绘制球体
        /// </summary>
        public void DrawSphere()
        {
            Action<BoundingSphereVisual3D> drawStart = shape => this.Shapes.Add(shape);
            Action<BoundingSphereVisual3D> drawEnd = shape =>
            {
                ShapeDrawnEvent message = new ShapeDrawnEvent
                {
                    Publisher = this,
                    Shape = shape
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };

            DrawBoundingSphereCommand command = new DrawBoundingSphereCommand(drawStart, drawEnd);
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制圆柱体 —— void DrawCylinder()
        /// <summary>
        /// 绘制圆柱体
        /// </summary>
        public void DrawCylinder()
        {
            Action<CylinderVisual3D> drawStart = shape => this.Shapes.Add(shape);
            Action<CylinderVisual3D> drawEnd = shape =>
            {
                ShapeDrawnEvent message = new ShapeDrawnEvent
                {
                    Publisher = this,
                    Shape = shape
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };

            DrawCylinderCommand command = new DrawCylinderCommand(drawStart, drawEnd);
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制凸多面体 —— void DrawConvexPolyhedron()
        /// <summary>
        /// 绘制凸多面体
        /// </summary>
        public void DrawConvexPolyhedron()
        {
            Action<ConvexPolyhedronVisual3D> drawStart = shape => this.Shapes.Add(shape);
            Action<ConvexPolyhedronVisual3D> drawEnd = shape =>
            {
                ShapeDrawnEvent message = new ShapeDrawnEvent
                {
                    Publisher = this,
                    Shape = shape
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };
            Action<ConvexPolyhedronVisual3D> drawCancel = shape => this.Shapes.Remove(shape);

            DrawConvexPolyhedronCommand command = new DrawConvexPolyhedronCommand(drawStart, drawEnd, drawCancel);
            this.InputManager.SwitchCommand(command);
        }
        #endregion


        //Events

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

        #region 处理形状绘制结束事件 —— Task HandleAsync(ShapeDrawnEvent message...
        /// <summary>
        /// 处理形状绘制结束事件
        /// </summary>
        public Task HandleAsync(ShapeDrawnEvent message, CancellationToken cancellationToken)
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

            this.Shapes.Add(message.Shape.Clone());
            this.FrameToken++;

            return Task.CompletedTask;
        }
        #endregion

        #region 处理形状同步事件 —— Task HandleAsync(ShapeSyncEvent message...
        /// <summary>
        /// 处理形状同步事件
        /// </summary>
        public Task HandleAsync(ShapeSyncEvent message, CancellationToken cancellationToken)
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

            ShapeVisual3D shape = this.Shapes.SingleOrDefault(shape => shape.Id == message.Shape.Id);
            if (shape != null)
            {
                shape.Copy(message.Shape);
                this.FrameToken++;
            }

            return Task.CompletedTask;
        }
        #endregion

        #region 处理形状已删除事件 —— Task HandleAsync(ShapeRemovedEvent message...
        /// <summary>
        /// 处理形状已删除事件
        /// </summary>
        public Task HandleAsync(ShapeRemovedEvent message, CancellationToken cancellationToken)
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

            ShapeVisual3D shape = this.Shapes.SingleOrDefault(shape => shape.Id == message.Shape.Id);
            if (shape != null)
            {
                this.Shapes.Remove(shape);
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


        //Private

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
