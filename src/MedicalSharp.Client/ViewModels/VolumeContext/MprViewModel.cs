using Avalonia;
using Avalonia.Collections;
using Caliburn.Micro;
using IconPacks.Avalonia.MaterialDesign;
using MedicalSharp.Client.Events;
using MedicalSharp.Controls.Commands;
using MedicalSharp.Controls.Commands.Arguments;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.InputManagers;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Primitives.Cameras;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using MedicalSharp.Primitives.Models.Arguments;
using OpenTK.Mathematics;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using SD.Infrastructure.Avalonia.CustomControls;
using SD.Infrastructure.Avalonia.Enums;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MedicalSharp.Client.ViewModels.VolumeContext
{
    /// <summary>
    /// MPR视图模型
    /// </summary>
    public class MprViewModel : ScreenBase, IHandle<SyncViewportEvent>, IHandle<ShapeDrawEndEvent>, IHandle<ShapeSyncEvent>, IHandle<ShapeTranslatingEvent>, IHandle<ShapeRotatingEvent>, IHandle<ShapeRemovedEvent>, IHandle<MPRPlaneChangedEvent>
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

            //默认值
            this.Crosshair = new CrosshairVisual3D();
            this.Shapes = [];
            this.PickVoxel();
        }

        #endregion

        #region # 属性

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
                    this.Crosshair.UAxis = value.WorldUAxis.ToVector3();
                    this.Crosshair.VAxis = value.WorldVAxis.ToVector3();
                    this.Crosshair.Center = value.WorldCenter.ToVector3();
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
        [DependencyProperty]
        public VolumeData VolumeData { get; set; }
        #endregion

        #region 十字线 —— CrosshairVisual3D Crosshair
        /// <summary>
        /// 十字线
        /// </summary>
        [DependencyProperty]
        public CrosshairVisual3D Crosshair { get; set; }
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
            command.TranslatingEvent = translating;
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 2D旋转 —— void Rotate2D()
        /// <summary>
        /// 2D旋转
        /// </summary>
        public void Rotate2D()
        {
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

            RotateVisual2DCommand command = new RotateVisual2DCommand(rotateEnd);
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 调整尺寸 —— void Resize()
        /// <summary>
        /// 调整尺寸
        /// </summary>
        public void Resize()
        {
            Action<IResizable2D> resizeEnd = resizable =>
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

            ResizeVisual2DCommand command = new ResizeVisual2DCommand(resizeEnd);
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
                ShapeDrawEndEvent message = new ShapeDrawEndEvent
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
                ShapeDrawEndEvent message = new ShapeDrawEndEvent
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
            Func<Vector3D> getNormal = () => this.Plane.Normal.ToVector3();
            Action<RectangleVisual3D> drawStart = shape => this.Shapes.Add(shape);
            Action<RectangleVisual3D> drawEnd = shape =>
            {
                ShapeDrawEndEvent message = new ShapeDrawEndEvent
                {
                    Publisher = this,
                    Shape = shape
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };

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
            Func<Vector3D> getNormal = () => this.Plane.Normal.ToVector3();
            Action<CircleVisual3D> drawStart = shape => this.Shapes.Add(shape);
            Action<CircleVisual3D> drawEnd = shape =>
            {
                ShapeDrawEndEvent message = new ShapeDrawEndEvent
                {
                    Publisher = this,
                    Shape = shape
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };

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
            Func<Vector3D> getNormal = () => this.Plane.Normal.ToVector3();
            Action<EllipseVisual3D> drawStart = shape => this.Shapes.Add(shape);
            Action<EllipseVisual3D> drawEnd = shape =>
            {
                ShapeDrawEndEvent message = new ShapeDrawEndEvent
                {
                    Publisher = this,
                    Shape = shape
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            };

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
                ShapeDrawEndEvent message = new ShapeDrawEndEvent
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
                ShapeDrawEndEvent message = new ShapeDrawEndEvent
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
                ShapeDrawEndEvent message = new ShapeDrawEndEvent
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
                ShapeDrawEndEvent message = new ShapeDrawEndEvent
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


        //Events

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

                //逻辑空间偏移 -> 世界空间偏移
                float worldDelta = plane.SliceOffsetDelta * plane.WorldSliceSpacing;
                Vector3 worldStep = plane.WorldNormal * worldDelta;
                this.Crosshair.Transform?.Translate(worldStep);
            }
            if (eventArgs.TriggerSource == MPRPlaneChangeSource.ExternalSync)
            {
                this.Crosshair.UAxis = plane.WorldUAxis.ToVector3();
                this.Crosshair.VAxis = plane.WorldVAxis.ToVector3();
                this.Crosshair.Center = plane.WorldCenter.ToVector3();
                this.Crosshair.Transform?.SetPosition(plane.WorldCenter);
            }

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

        #region 处理形状绘制结束事件 —— Task HandleAsync(ShapeDrawEndEvent message...
        /// <summary>
        /// 处理形状绘制结束事件
        /// </summary>
        public Task HandleAsync(ShapeDrawEndEvent message, CancellationToken cancellationToken)
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
                this.Crosshair.Transform.SetPosition(crosshair.Transform.Position);
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
            if (message.Crosshair == null)
            {
                return Task.CompletedTask;
            }

            #endregion

            if (message.TriggerSource is MPRPlaneChangeSource.SliceScroll or MPRPlaneChangeSource.ExternalSync)
            {
                this.Crosshair.Transform.SetPosition(message.Crosshair.Transform.Position);
                this.FrameToken++;
            }

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

        #endregion
    }
}
