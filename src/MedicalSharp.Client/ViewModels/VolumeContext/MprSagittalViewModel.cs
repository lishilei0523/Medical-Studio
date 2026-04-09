using Avalonia;
using Avalonia.Input;
using Caliburn.Micro;
using IconPacks.Avalonia.MaterialDesign;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Primitives.Cameras;
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
    /// MPR矢状位视图模型
    /// </summary>
    public class MprSagittalViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗口管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public MprSagittalViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
            this.SagittalCamera = new MPRCamera();
        }

        #endregion

        #region # 属性

        #region MPR矢状面 —— MPRPlane SagittalPlane
        /// <summary>
        /// MPR矢状面
        /// </summary>
        [DependencyProperty]
        public MPRPlane SagittalPlane { get; set; }
        #endregion

        #region MPR矢状位相机 —— MPRCamera SagittalCamera
        /// <summary>
        /// MPR矢状位相机
        /// </summary>
        [DependencyProperty]
        public MPRCamera SagittalCamera { get; set; }
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
                if (value != null)
                {
                    this.SagittalPlane = MPRPlane.CreateSagittalPlane(value.Metadata);
                }
            }
        }
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

        #region MPR矢状位视口鼠标按下事件 —— void OnSagittalViewportPointerPressed(MPRViewport viewport...
        /// <summary>
        /// MPR矢状位视口鼠标按下事件
        /// </summary>
        public void OnSagittalViewportPointerPressed(MPRViewport viewport, PointerPressedEventArgs eventArgs)
        {
            if (this.VolumeData != null && eventArgs.Properties.IsLeftButtonPressed)
            {
                Point mousePos2D = eventArgs.GetPosition(viewport);
                bool success = viewport.FindNearest(mousePos2D.ToVector2(), out Vector3? textureCoord, out Vector3i? voxelPostion, out short? voxelValue);
                if (success)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine($"点击2D坐标: X:{mousePos2D.X}, Y:{mousePos2D.Y}");
                    builder.AppendLine($"点击纹理坐标: X:{textureCoord.Value.X}, Y:{textureCoord.Value.Y}, Z:{textureCoord.Value.Z}");
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
