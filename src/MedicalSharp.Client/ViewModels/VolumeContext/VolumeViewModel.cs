using Avalonia;
using Avalonia.Collections;
using Avalonia.Input;
using Caliburn.Micro;
using IconPacks.Avalonia.MaterialDesign;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Primitives.Cameras;
using MedicalSharp.Primitives.Managers;
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

            //Vector3 targetPosition = new Vector3(0.0f);
            //const float distance = 7.0f;
            //const float yaw = 45.0f;
            //const float pitch = -45.0f;
            //this.OrbitCamera = new OrbitPerspectiveCamera(targetPosition, distance, yaw, pitch);

            //X-Up
            //Vector3 cameraPosition = new Vector3(0, 4, 0);
            //Vector3 targetPosition = new Vector3(0.0f);
            //Vector3 upDirection = new Vector3(1, 0, 0);

            //Y-Up
            //Vector3 cameraPosition = new Vector3(0, 0, 4);
            //Vector3 targetPosition = new Vector3(0.0f);
            //Vector3 upDirection = new Vector3(0, 1, 0);

            //Z-Up
            Vector3 cameraPosition = new Vector3(0, 2, 0);
            Vector3 targetPosition = new Vector3(0.0f);
            Vector3 upDirection = new Vector3(0, 0, 1);
            this.OrbitCamera = new OrbitPerspectiveCamera(cameraPosition, targetPosition, upDirection);
            this.TFControlPoints = new AvaloniaList<TFControlPoint>(ResourceManager.GrayControlPoints);
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

        #region 查看体积渲染视图 —— async Task LookVolumeView()
        /// <summary>
        /// 查看体积渲染视图
        /// </summary>
        public async Task LookVolumeView()
        {
            //ContainerViewModel viewModel = ResolveMediator.Resolve<ContainerViewModel>();
            //viewModel.Title = "VR";
            //viewModel.Content = this;

            //await this._windowManager.ShowWindowAsync(viewModel);
        }
        #endregion

        #region VR视口鼠标按下事件 —— void OnVolumeViewportPointerPressed(WireframeViewport viewport...
        /// <summary>
        /// VR视口鼠标按下事件
        /// </summary>
        public void OnVolumeViewportPointerPressed(VolumeViewport viewport, PointerPressedEventArgs eventArgs)
        {
            if (this.VolumeData != null && eventArgs.Properties.IsLeftButtonPressed)
            {
                Point mousePos2D = eventArgs.GetPosition(viewport);
                bool success = viewport.FindNearest(mousePos2D.ToVector2(), out Vector3i? voxelPostion, out short? voxelValue);
                if (success)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine($"点击2D坐标: X:{mousePos2D.X}, Y:{mousePos2D.Y}");
                    builder.AppendLine($"点击体素坐标: X:{voxelPostion.Value.X}, Y:{voxelPostion.Value.Y}, Z:{voxelPostion.Value.Z}");
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
