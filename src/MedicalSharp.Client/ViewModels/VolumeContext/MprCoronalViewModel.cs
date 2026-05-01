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
    /// MPR冠状位视图模型
    /// </summary>
    public class MprCoronalViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗口管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public MprCoronalViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
            this.Camera = new MPRCamera();
        }

        #endregion

        #region # 属性

        #region MPR平面 —— MPRPlane Plane
        /// <summary>
        /// MPR平面
        /// </summary>
        [DependencyProperty]
        public MPRPlane Plane { get; set; }
        #endregion

        #region MPR相机 —— MPRCamera Camera
        /// <summary>
        /// MPR相机
        /// </summary>
        [DependencyProperty]
        public MPRCamera Camera { get; set; }
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
                    this.Plane = MPRPlane.CreateCoronalPlane(value.Metadata);
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

        #region MPR视口鼠标按下事件 —— void OnViewportPointerPressed(MPRViewport viewport...
        /// <summary>
        /// MPR视口鼠标按下事件
        /// </summary>
        public void OnViewportPointerPressed(MPRViewport viewport, PointerPressedEventArgs eventArgs)
        {
            if (this.VolumeData != null && eventArgs.Properties.IsLeftButtonPressed)
            {
                Point mousePos2D = eventArgs.GetPosition(viewport);
                bool success = viewport.FindNearestVoxel(mousePos2D.ToVector2(), out Vector3 textureCoord, out Vector3 worldPosition, out Vector3i voxelPostion, out short voxelValue, out byte markValue, out Ray ray);
                if (success)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine($"点击2D坐标: X:{mousePos2D.X}, Y:{mousePos2D.Y}");
                    builder.AppendLine($"点击纹理坐标: X:{textureCoord.X}, Y:{textureCoord.Y}, Z:{textureCoord.Z}");
                    builder.AppendLine($"点击世界坐标: X:{worldPosition.X}, Y:{worldPosition.Y}, Z:{worldPosition.Z}");
                    builder.AppendLine($"点击体素坐标: X:{voxelPostion.X}, Y:{voxelPostion.Y}, Z:{voxelPostion.Z}");
                    builder.AppendLine($"点击体素HU值: {voxelValue}");
                    builder.AppendLine($"点击标记值: {markValue}");
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
