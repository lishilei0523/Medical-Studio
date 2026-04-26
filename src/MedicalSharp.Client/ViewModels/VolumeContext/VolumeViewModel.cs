using Avalonia;
using Avalonia.Collections;
using Caliburn.Micro;
using IconPacks.Avalonia.MaterialDesign;
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
    public class VolumeViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗口管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public VolumeViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
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

        #region 传输函数控制点列表 —— AvaloniaList<TFControlPoint> TFControlPoints
        /// <summary>
        /// 传输函数控制点列表
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

        //Initializations

        #region 初始化 —— override Task OnInitializedAsync(CancellationToken...
        /// <summary>
        /// 初始化
        /// </summary>
        protected override Task OnInitializedAsync(CancellationToken cancellationToken)
        {
            return base.OnInitializedAsync(cancellationToken);
        }
        #endregion


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
                Vector3i? voxelPostion = e.PickedVoxelPosition;
                short? voxelValue = e.PickedVoxelValue;
                byte? markValue = e.PickedMarkValue;
                if (textureCoord.HasValue)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine($"点击2D坐标: X:{mousePos2D.X}, Y:{mousePos2D.Y}");
                    builder.AppendLine($"点击纹理坐标: X:{textureCoord.Value.X}, Y:{textureCoord.Value.Y}, Z:{textureCoord.Value.Z}");
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

        #region 平移 —— void Translate()
        /// <summary>
        /// 平移
        /// </summary>
        public void Translate()
        {
            TranslateVisual3DCommand command = new TranslateVisual3DCommand();
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 旋转 —— void Rotate()
        /// <summary>
        /// 旋转
        /// </summary>
        public void Rotate()
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
            DrawPointCommand command = new DrawPointCommand(shape => this.Shapes.Add(shape));
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制线段 —— void DrawLineSegment()
        /// <summary>
        /// 绘制线段
        /// </summary>
        public void DrawLineSegment()
        {
            DrawLineSegmentCommand command = new DrawLineSegmentCommand(shape => this.Shapes.Add(shape));
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制矩形 —— void DrawRectangle()
        /// <summary>
        /// 绘制矩形
        /// </summary>
        public void DrawRectangle()
        {
            Func<Vector3D> getNormal = () => this.Camera.LookDirection.ToVector3();
            DrawRectangleCommand command = new DrawRectangleCommand(shape => this.Shapes.Add(shape), getNormal);
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制圆形 —— void DrawCircle()
        /// <summary>
        /// 绘制圆形
        /// </summary>
        public void DrawCircle()
        {
            Func<Vector3D> getNormal = () => this.Camera.LookDirection.ToVector3();
            DrawCircleCommand command = new DrawCircleCommand(shape => this.Shapes.Add(shape), getNormal);
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制椭圆形 —— void DrawEllipse()
        /// <summary>
        /// 绘制椭圆形
        /// </summary>
        public void DrawEllipse()
        {
            Func<Vector3D> getNormal = () => this.Camera.LookDirection.ToVector3();
            DrawEllipseCommand command = new DrawEllipseCommand(shape => this.Shapes.Add(shape), getNormal);
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制折线 —— void DrawPolyline()
        /// <summary>
        /// 绘制折线
        /// </summary>
        public void DrawPolyline()
        {
            DrawPolylineCommand command = new DrawPolylineCommand(shape => this.Shapes.Add(shape), shape => this.Shapes.Remove(shape));
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制曲线 —— void DrawCurve()
        /// <summary>
        /// 绘制曲线
        /// </summary>
        public void DrawCurve()
        {
            DrawCurveCommand command = new DrawCurveCommand(shape => this.Shapes.Add(shape), shape => this.Shapes.Remove(shape));
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制多边形 —— void DrawPolyline()
        /// <summary>
        /// 绘制多边形
        /// </summary>
        public void DrawPolygon()
        {
            DrawPolylineCommand command = new DrawPolylineCommand(shape => this.Shapes.Add(shape), shape => this.Shapes.Remove(shape), true);
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制立方体 —— void DrawBox()
        /// <summary>
        /// 绘制立方体
        /// </summary>
        public void DrawBox()
        {
            DrawBoundingBoxCommand command = new DrawBoundingBoxCommand(shape => this.Shapes.Add(shape));
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 绘制球体 —— void DrawSphere()
        /// <summary>
        /// 绘制球体
        /// </summary>
        public void DrawSphere()
        {
            DrawBoundingSphereCommand command = new DrawBoundingSphereCommand(shape => this.Shapes.Add(shape));
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #endregion
    }
}
