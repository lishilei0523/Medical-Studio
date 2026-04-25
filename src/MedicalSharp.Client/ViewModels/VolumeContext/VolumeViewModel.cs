using Avalonia;
using Avalonia.Collections;
using Avalonia.Input;
using Caliburn.Micro;
using IconPacks.Avalonia.MaterialDesign;
using MedicalSharp.Controls.Commands;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.InputManagers;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Primitives.Cameras;
using MedicalSharp.Primitives.Managers;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using SD.Infrastructure.Avalonia.CustomControls;
using SD.Infrastructure.Avalonia.Enums;
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

        #region VR视口鼠标按下事件 —— void OnVolumeViewportPointerPressed(VolumeViewport viewport...
        /// <summary>
        /// VR视口鼠标按下事件
        /// </summary>
        public void OnVolumeViewportPointerPressed(VolumeViewport viewport, PointerPressedEventArgs eventArgs)
        {
            if (this.VolumeData != null && eventArgs.Properties.IsLeftButtonPressed)
            {
                Point mousePos2D = eventArgs.GetPosition(viewport);
                bool success = viewport.FindNearestVoxel(mousePos2D.ToVector2(), out Vector3 textureCoord, out Vector3i voxelPostion, out short voxelValue, out Ray ray);
                if (success)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine($"点击2D坐标: X:{mousePos2D.X}, Y:{mousePos2D.Y}");
                    builder.AppendLine($"点击纹理坐标: X:{textureCoord.X}, Y:{textureCoord.Y}, Z:{textureCoord.Z}");
                    builder.AppendLine($"点击体素坐标: X:{voxelPostion.X}, Y:{voxelPostion.Y}, Z:{voxelPostion.Z}");
                    builder.AppendLine($"点击体素HU值: {voxelValue}");
                    MessageBox.Show(builder.ToString(), "成功", MessageBoxButton.OK, PackIconMaterialDesignKind.Info);
                }
                else
                {
                    MessageBox.Show("获取失败！", "错误", MessageBoxButton.OK, PackIconMaterialDesignKind.Error);
                }
            }
        }
        #endregion

        #endregion
    }
}
