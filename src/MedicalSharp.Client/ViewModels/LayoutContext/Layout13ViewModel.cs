using Caliburn.Micro;
using MedicalSharp.Client.ViewModels.VolumeContext;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using SD.IOC.Core.Mediators;
using System.Threading;
using System.Threading.Tasks;

namespace MedicalSharp.Client.ViewModels.LayoutContext
{
    /// <summary>
    /// 13布局视图模型
    /// </summary>
    public class Layout13ViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗口管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public Layout13ViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 体积渲染视图模型 —— VolumeViewModel VolumeViewModel
        /// <summary>
        /// 体积渲染视图模型
        /// </summary>
        [DependencyProperty]
        public VolumeViewModel VolumeViewModel { get; set; }
        #endregion

        #region MPR横断位视图模型 —— MprAxialViewModel MprAxialViewModel
        /// <summary>
        /// MPR横断位视图模型
        /// </summary>
        [DependencyProperty]
        public MprAxialViewModel MprAxialViewModel { get; set; }
        #endregion

        #region MPR冠状位视图模型 —— MprCoronalViewModel MprCoronalViewModel
        /// <summary>
        /// MPR冠状位视图模型
        /// </summary>
        [DependencyProperty]
        public MprCoronalViewModel MprCoronalViewModel { get; set; }
        #endregion

        #region MPR矢状位视图模型 —— MprSagittalViewModel MprSagittalViewModel
        /// <summary>
        /// MPR矢状位视图模型
        /// </summary>
        [DependencyProperty]
        public MprSagittalViewModel MprSagittalViewModel { get; set; }
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
            this.VolumeViewModel = ResolveMediator.Resolve<VolumeViewModel>();
            this.MprAxialViewModel = ResolveMediator.Resolve<MprAxialViewModel>();
            this.MprCoronalViewModel = ResolveMediator.Resolve<MprCoronalViewModel>();
            this.MprSagittalViewModel = ResolveMediator.Resolve<MprSagittalViewModel>();

            return base.OnInitializedAsync(cancellationToken);
        }
        #endregion

        #endregion
    }
}
