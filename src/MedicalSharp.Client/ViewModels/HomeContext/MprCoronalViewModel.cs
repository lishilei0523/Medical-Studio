using Caliburn.Micro;
using MedicalSharp.Dicoms.Models;
using MedicalSharp.Engine.Cameras;
using MedicalSharp.Engine.ValueTypes;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using System.Threading;
using System.Threading.Tasks;

namespace MedicalSharp.Client.ViewModels.HomeContext
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
            this.CoronalCamera = new MPRCamera(MPRPlaneType.Coronal);
        }

        #endregion

        #region # 属性

        #region MPR冠状位相机 —— MPRCamera CoronalCamera
        /// <summary>
        /// MPR冠状位相机
        /// </summary>
        [DependencyProperty]
        public MPRCamera CoronalCamera { get; set; }
        #endregion

        #region 体积数据 —— VolumeData VolumeData
        /// <summary>
        /// 体积数据
        /// </summary>
        [DependencyProperty]
        public VolumeData VolumeData { get; set; }
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
