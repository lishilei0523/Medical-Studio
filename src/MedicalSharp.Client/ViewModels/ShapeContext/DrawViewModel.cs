using Avalonia.Collections;
using Caliburn.Micro;
using MedicalSharp.Controls.Commands;
using MedicalSharp.Controls.InputManagers;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Primitives.Cameras;
using OpenTK.Mathematics;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using System.Threading;
using System.Threading.Tasks;

namespace MedicalSharp.Client.ViewModels.ShapeContext
{
    /// <summary>
    /// 绘制视图模型
    /// </summary>
    public class DrawViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗口管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public DrawViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
            this.Shapes = [];

            //初始化相机
            Vector3 cameraPosition = new Vector3(0, 7, 0);
            Vector3 targetPosition = new Vector3(0.0f);
            Vector3 upDirection = new Vector3(0, 0, 1);
            this.OrbitCamera = new OrbitPerspectiveCamera(cameraPosition, targetPosition, upDirection);

            //初始化输入管理器
            //DrawBoundingBoxCommand command = new DrawBoundingBoxCommand(shape => this.Shapes.Add(shape));
            //DrawBoundingSphereCommand command = new DrawBoundingSphereCommand(shape => this.Shapes.Add(shape));
            //DrawRectangleCommand command = new DrawRectangleCommand(new Vector3D(0, 1, 0), shape => this.Shapes.Add(shape));
            //DrawEllipseCommand command = new DrawEllipseCommand(new Vector3D(0, 1, 0), shape => this.Shapes.Add(shape));
            //DrawCircleCommand command = new DrawCircleCommand(new Vector3D(0, 1, 0), shape => this.Shapes.Add(shape));
            DrawLineSegmentCommand command = new DrawLineSegmentCommand(shape => this.Shapes.Add(shape));
            this.InputManager = new OrbitInputManager(this.OrbitCamera);
            this.InputManager.SwitchCommand(command);
        }

        #endregion

        #region # 属性

        #region 轨道相机 —— OrbitCamera OrbitCamera
        /// <summary>
        /// 轨道相机
        /// </summary>
        [DependencyProperty]
        public OrbitCamera OrbitCamera { get; set; }
        #endregion

        #region 输入管理器 —— OrbitInputManager InputManager
        /// <summary>
        /// 输入管理器
        /// </summary>
        [DependencyProperty]
        public OrbitInputManager InputManager { get; set; }
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

        #region 初始化 —— override Task OnInitializedAsync(CancellationToken...
        /// <summary>
        /// 初始化
        /// </summary>
        protected override Task OnInitializedAsync(CancellationToken cancellationToken)
        {
            return base.OnInitializedAsync(cancellationToken);
        }
        #endregion

        #endregion
    }
}
