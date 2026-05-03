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
using MedicalSharp.Primitives.Managers;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using SD.Infrastructure.Avalonia.CustomControls;
using SD.Infrastructure.Avalonia.Enums;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MedicalSharp.Client.ViewModels.VolumeContext
{
    /// <summary>
    /// 体积渲染视图模型
    /// </summary>
    public class VolumeViewModel : ScreenBase, IHandle<ShapeCreatedEvent>
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
            this.Shapes = [];

            //初始化相机
            Vector3 cameraPosition = new Vector3(0, 2, 0);
            Vector3 targetPosition = new Vector3(0.0f);
            Vector3 upDirection = new Vector3(0, 0, 1);
            this.Camera = new OrbitPerspectiveCamera(cameraPosition, targetPosition, upDirection);
            this.TFControlPoints = new AvaloniaList<TFControlPoint>(ResourceManager.GrayControlPoints);

            //初始化输入管理器
            this.InputManager = new OrbitInputManager(this.Camera);
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
        [DependencyProperty]
        public VolumeData VolumeData { get; set; }
        #endregion

        #region 传递函数控制点列表 —— AvaloniaList<TFControlPoint> TFControlPoints
        /// <summary>
        /// 传递函数控制点列表
        /// </summary>
        [DependencyProperty]
        public AvaloniaList<TFControlPoint> TFControlPoints { get; set; }
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
            TranslateVisual3DCommand command = new TranslateVisual3DCommand();
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 沿法向量平移 —— void TranslateNormal()
        /// <summary>
        /// 沿法向量平移
        /// </summary>
        public void TranslateNormal()
        {
            TranslateVisualNormalCommand command = new TranslateVisualNormalCommand();
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 3D旋转 —— void Rotate3D()
        /// <summary>
        /// 3D旋转
        /// </summary>
        public void Rotate3D()
        {
            RotateVisual3DCommand command = new RotateVisual3DCommand();
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 调整尺寸 —— void Resize()
        /// <summary>
        /// 调整尺寸
        /// </summary>
        public void Resize()
        {
            ResizeVisual3DCommand command = new ResizeVisual3DCommand();
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 编辑顶点 —— void EditVertex()
        /// <summary>
        /// 编辑顶点
        /// </summary>
        public void EditVertex()
        {
            EditVertexCommand command = new EditVertexCommand();
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
                //TODO 同步
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
                //TODO 同步
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
            Action<RectangleVisual3D> drawEnd = shape =>
            {
                //TODO 同步
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
            Func<Vector3D> getNormal = () => -this.Camera.LookDirection.ToVector3();
            Action<CircleVisual3D> drawStart = shape => this.Shapes.Add(shape);
            Action<CircleVisual3D> drawEnd = shape =>
            {
                //TODO 同步
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
            Func<Vector3D> getNormal = () => -this.Camera.LookDirection.ToVector3();
            Action<EllipseVisual3D> drawStart = shape => this.Shapes.Add(shape);
            Action<EllipseVisual3D> drawEnd = shape =>
            {
                //TODO 同步
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
                //TODO 同步
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
                //TODO 同步
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
                //TODO 同步
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
                //TODO 同步
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
                //TODO 同步
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
                //TODO 同步
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
                //TODO 同步
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
                //TODO 同步
            };
            Action<ConvexPolyhedronVisual3D> drawCancel = shape => this.Shapes.Remove(shape);

            DrawConvexPolyhedronCommand command = new DrawConvexPolyhedronCommand(drawStart, drawEnd, drawCancel);
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 处理形状创建事件 —— Task HandleAsync(ShapeCreatedEvent message...
        /// <summary>
        /// 处理形状创建事件
        /// </summary>
        public Task HandleAsync(ShapeCreatedEvent message, CancellationToken cancellationToken)
        {
            if (message.Shape is RectangleVisual3D rectangle)
            {
                //TODO 实现；
                RectangleVisual3D newRectangle = new RectangleVisual3D
                {
                    Stroke = rectangle.Stroke,
                    StrokeThickness = rectangle.StrokeThickness,
                    Fill = rectangle.Fill,
                    Width = rectangle.Width,
                    Height = rectangle.Height,
                    Center = rectangle.Center,
                    Normal = rectangle.Normal
                };

                this.Shapes.Add(newRectangle);
                this.FrameToken++;
            }

            return Task.CompletedTask;
        }
        #endregion

        #endregion
    }
}
