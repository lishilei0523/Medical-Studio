using Caliburn.Micro;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Dicoms.Models;
using MedicalSharp.Engine.Cameras;
using MedicalSharp.Engine.Resources;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using System.Threading;
using System.Threading.Tasks;

namespace MedicalSharp.Client.ViewModels.VolumeContext
{
    /// <summary>
    /// MPR横断位视图模型
    /// </summary>
    public class MprAxialViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗口管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public MprAxialViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
            this.AxialCamera = new MPRCamera();
        }

        #endregion

        #region # 属性

        #region MPR横断面 —— MPRPlane AxialPlane
        /// <summary>
        /// MPR横断面
        /// </summary>
        [DependencyProperty]
        public MPRPlane AxialPlane { get; set; }
        #endregion

        #region MPR横断位相机 —— MPRCamera2 AxialCamera
        /// <summary>
        /// MPR横断位相机
        /// </summary>
        [DependencyProperty]
        public MPRCamera AxialCamera { get; set; }
        #endregion

        #region 体积数据 —— VolumeData VolumeData
        /// <summary>
        /// 体积数据
        /// </summary>
        private VolumeData _volumeData;

        /// <summary>
        /// 体积数据
        /// </summary>
        public VolumeData VolumeData
        {
            get => this._volumeData;
            set
            {
                this._volumeData = value;
                this.NotifyOfPropertyChange();
                if (value != null)
                {
                    this.AxialPlane = MPRPlane.CreateAxialPlane(value.VolumeSize.ToGlmVector3(), value.Spacing.ToGlmVector3(), value.PhysicalSize.ToGlmVector3(), value.VolumeScale.ToGlmVector3());
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


        #endregion
    }
}
