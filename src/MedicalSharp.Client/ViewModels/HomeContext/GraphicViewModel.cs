using Caliburn.Micro;
using MedicalSharp.Controls.Inputs;
using MedicalSharp.Engine.Cameras;
using OpenTK.Mathematics;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using System.Threading;
using System.Threading.Tasks;

namespace MedicalSharp.Client.ViewModels.HomeContext
{
    /// <summary>
    /// OpenGL视图模型
    /// </summary>
    public class GraphicViewModel : Screen
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗口管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public GraphicViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
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

        #region 输入管理器 —— InputManager InputManager
        /// <summary>
        /// 输入管理器
        /// </summary>
        [DependencyProperty]
        public InputManager InputManager { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 初始化 —— override Task OnInitializedAsync(CancellationToken...
        /// <summary>
        /// 初始化
        /// </summary>
        protected override Task OnInitializedAsync(CancellationToken cancellationToken)
        {
            Vector3 targetPosition = new Vector3(0.0f);
            const float distance = 7.0f;
            const float yaw = 45.0f;
            const float pitch = -45.0f;
            const float nearPlaneDistance = 0.125f;
            const float farPlaneDistance = 65535;
            const float fieldOfView = 30.0f;

            this.Camera = new OrbitPerspectiveCamera(targetPosition, distance, yaw, pitch, nearPlaneDistance, farPlaneDistance, fieldOfView);
            this.InputManager = new OrbitInputManager(this.Camera);

            return base.OnInitializedAsync(cancellationToken);
        }
        #endregion

        #endregion
    }
}
