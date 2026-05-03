using Caliburn.Micro;
using MedicalSharp.Controls.InputManagers;
using MedicalSharp.Primitives.Cameras;
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
        /// 事件聚合器
        /// </summary>
        private readonly IEventAggregator _eventAggregator;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public MprSagittalViewModel(IWindowManager windowManager, IEventAggregator eventAggregator)
        {
            this._windowManager = windowManager;
            this._eventAggregator = eventAggregator;
            this._eventAggregator.SubscribeOnUIThread(this);

            const string title = "MPR-Sagittal";
            MPRCamera camera = new MPRCamera();
            MPRInputManager inputManager = new MPRInputManager(camera);
            this.MprViewModel = new MprViewModel(windowManager, eventAggregator, title, camera, inputManager);
        }

        #endregion

        #region # 属性

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
                this.MprViewModel.VolumeData = value;
                if (value != null)
                {
                    this.MprViewModel.Plane = MPRPlane.CreateSagittalPlane(value.Metadata);
                }
            }
        }
        #endregion

        #region MPR视图模型 —— MprViewModel MprViewModel
        /// <summary>
        /// MPR视图模型
        /// </summary>
        [DependencyProperty]
        public MprViewModel MprViewModel { get; set; }
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

        #endregion
    }
}
