using Caliburn.Micro;
using MedicalSharp.Engine.Cameras;
using OpenTK.Mathematics;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using System.Threading;
using System.Threading.Tasks;

namespace MedicalSharp.Client.ViewModels.HomeContext
{
    /// <summary>
    /// 线框视图模型
    /// </summary>
    public class WireframeViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗口管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public WireframeViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        //通知属性

        #region 轨道相机 —— OrbitCamera OrbitCamera
        /// <summary>
        /// 轨道相机
        /// </summary>
        [DependencyProperty]
        public OrbitCamera OrbitCamera { get; set; }
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
            Vector3 targetPosition = new Vector3(0.0f);
            const float distance = 7.0f;
            const float yaw = 45.0f;
            const float pitch = -45.0f;
            this.OrbitCamera = new OrbitPerspectiveCamera(targetPosition, distance, yaw, pitch);

            return base.OnInitializedAsync(cancellationToken);
        }
        #endregion

        #endregion
    }
}
