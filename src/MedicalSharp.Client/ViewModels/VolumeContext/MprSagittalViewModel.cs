using Caliburn.Micro;
using MedicalSharp.Engine.Cameras;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
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
                    this.SagittalPlane = MPRPlane.CreateSagittalPlane(value.VolumeSize, value.Spacing, value.PhysicalSize, value.VolumeScale);
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
