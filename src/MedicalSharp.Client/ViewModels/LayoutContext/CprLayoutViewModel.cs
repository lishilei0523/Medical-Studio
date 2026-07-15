using Caliburn.Micro;
using MedicalSharp.Client.ViewModels.VolumeContext;
using MedicalSharp.Controls.InputManagers;
using MedicalSharp.Controls.Visual3Ds;
using MedicalSharp.Presentation.Models;
using MedicalSharp.Primitives.Cameras;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Models;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using System.Threading;
using System.Threading.Tasks;

namespace MedicalSharp.Client.ViewModels.LayoutContext
{
    /// <summary>
    /// CPR布局视图模型
    /// </summary>
    public class CprLayoutViewModel : ScreenBase
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
        /// 曲线3D元素
        /// </summary>
        private readonly CurveVisual3D _curve;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public CprLayoutViewModel(IWindowManager windowManager, IEventAggregator eventAggregator, CurveVisual3D curve)
        {
            this._windowManager = windowManager;
            this._eventAggregator = eventAggregator;
            this._curve = curve;
            this.Layout = CprLayout6.CreateLayout();

            //初始化视图模型
            this.InitializeViewModels();
        }

        #endregion

        #region # 属性

        #region 布局信息 —— CprLayout6 Layout
        /// <summary>
        /// 布局信息
        /// </summary>
        [DependencyProperty]
        public CprLayout6 Layout { get; set; }
        #endregion

        #region 水平CPR视图模型 —— CprViewModel HorizontalViewModel
        /// <summary>
        /// 水平CPR视图模型
        /// </summary>
        [DependencyProperty]
        public CprViewModel HorizontalViewModel { get; set; }
        #endregion

        #region 垂直CPR视图模型 —— CprViewModel VerticalViewModel
        /// <summary>
        /// 垂直CPR视图模型
        /// </summary>
        [DependencyProperty]
        public CprViewModel VerticalViewModel { get; set; }
        #endregion

        #region 拉直CPR视图模型 —— CprViewModel StraightenedViewModel
        /// <summary>
        /// 拉直CPR视图模型
        /// </summary>
        [DependencyProperty]
        public CprViewModel StraightenedViewModel { get; set; }
        #endregion

        #region 剖面CPR1视图模型 —— CprViewModel CrossSectional1ViewModel
        /// <summary>
        /// 剖面CPR1视图模型
        /// </summary>
        [DependencyProperty]
        public CprViewModel CrossSectional1ViewModel { get; set; }
        #endregion

        #region 剖面CPR2视图模型 —— CprViewModel CrossSectional2ViewModel
        /// <summary>
        /// 剖面CPR1视图模型
        /// </summary>
        [DependencyProperty]
        public CprViewModel CrossSectional2ViewModel { get; set; }
        #endregion

        #region 剖面CPR3视图模型 —— CprViewModel CrossSectional3ViewModel
        /// <summary>
        /// 剖面CPR1视图模型
        /// </summary>
        [DependencyProperty]
        public CprViewModel CrossSectional3ViewModel { get; set; }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 设置体积数据 —— void SetVolumeData(VolumeData volumeData)
        /// <summary>
        /// 设置体积数据
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        public void SetVolumeData(VolumeData volumeData)
        {
            this.HorizontalViewModel.VolumeData = volumeData;
            this.VerticalViewModel.VolumeData = volumeData;
            this.StraightenedViewModel.VolumeData = volumeData;
            this.CrossSectional1ViewModel.VolumeData = volumeData;
            this.CrossSectional2ViewModel.VolumeData = volumeData;
            this.CrossSectional3ViewModel.VolumeData = volumeData;
        }
        #endregion

        #region 清空体积数据 —— void ClearVolumeData()
        /// <summary>
        /// 清空体积数据
        /// </summary>
        public void ClearVolumeData()
        {
            this.HorizontalViewModel.VolumeData = null;
            this.VerticalViewModel.VolumeData = null;
            this.StraightenedViewModel.VolumeData = null;
            this.CrossSectional1ViewModel.VolumeData = null;
            this.CrossSectional2ViewModel.VolumeData = null;
            this.CrossSectional3ViewModel.VolumeData = null;
        }
        #endregion


        //Protected & Private

        #region 失活事件 —— override Task OnDeactivateAsync(bool close...
        /// <summary>
        /// 失活事件
        /// </summary>
        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (close)
            {
                this._eventAggregator.Unsubscribe(this);
                this.ClearVolumeData();
            }

            return base.OnDeactivateAsync(close, cancellationToken);
        }
        #endregion

        #region 初始化视图模型 —— void InitializeViewModels()
        /// <summary>
        /// 初始化视图模型
        /// </summary>
        private void InitializeViewModels()
        {
            //水平投影图（Cell0：左上）
            CPRCamera horizontalCamera = new CPRCamera();
            CPRInputManager horizontalInputManager = new CPRInputManager(horizontalCamera);
            this.HorizontalViewModel = new CprViewModel(this._windowManager, this._eventAggregator, "水平CPR", horizontalCamera, horizontalInputManager)
            {
                CurveVisual3D = this._curve,
                CPRMode = CPRMode.Projected,
                ProjectionDirection = CPRProjectionDirection.Tangent,
                RadialWidth = 1f
            };

            //垂直投影图（Cell1：左下）
            CPRCamera verticalCamera = new CPRCamera();
            CPRInputManager verticalInputManager = new CPRInputManager(verticalCamera);
            this.VerticalViewModel = new CprViewModel(this._windowManager, this._eventAggregator, "垂直CPR", verticalCamera, verticalInputManager)
            {
                CurveVisual3D = this._curve,
                CPRMode = CPRMode.Projected,
                ProjectionDirection = CPRProjectionDirection.Normal,
                RadialWidth = 1f
            };

            //拉直图（Cell5：右侧整列）
            CPRCamera straightenedCamera = new CPRCamera();
            CPRInputManager straightenedInputManager = new CPRInputManager(straightenedCamera);
            this.StraightenedViewModel = new CprViewModel(this._windowManager, this._eventAggregator, "拉直图", straightenedCamera, straightenedInputManager)
            {
                CurveVisual3D = this._curve,
                CPRMode = CPRMode.Straightened,
                StraightenDirection = CPRStraightenDirection.Vertical
            };

            //剖面图1（Cell2：中上，弧长25%）
            CPRCamera crossCamera1 = new CPRCamera();
            CPRInputManager crossInputManager1 = new CPRInputManager(crossCamera1);
            this.CrossSectional1ViewModel = new CprViewModel(this._windowManager, this._eventAggregator, "剖面图1", crossCamera1, crossInputManager1)
            {
                CurveVisual3D = this._curve,
                CPRMode = CPRMode.CrossSectional,
                ArcPosition = 0.25f
            };

            //剖面图2（Cell3：中中，弧长50%）
            CPRCamera crossCamera2 = new CPRCamera();
            CPRInputManager crossInputManager2 = new CPRInputManager(crossCamera2);
            this.CrossSectional2ViewModel = new CprViewModel(this._windowManager, this._eventAggregator, "剖面图2", crossCamera2, crossInputManager2)
            {
                CurveVisual3D = this._curve,
                CPRMode = CPRMode.CrossSectional,
                ArcPosition = 0.5f
            };

            //剖面图3（Cell4：中下，弧长75%）
            CPRCamera crossCamera3 = new CPRCamera();
            CPRInputManager crossInputManager3 = new CPRInputManager(crossCamera3);
            this.CrossSectional3ViewModel = new CprViewModel(this._windowManager, this._eventAggregator, "剖面图3", crossCamera3, crossInputManager3)
            {
                CurveVisual3D = this._curve,
                CPRMode = CPRMode.CrossSectional,
                ArcPosition = 0.75f
            };
        }
        #endregion

        #endregion
    }
}
