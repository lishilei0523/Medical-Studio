using Avalonia.Platform.Storage;
using Caliburn.Micro;
using MedicalSharp.Dicoms;
using MedicalSharp.Dicoms.Models;
using MedicalSharp.Engine.Resources;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using SD.Infrastructure.Avalonia.Caliburn.Extensions;
using SD.Infrastructure.Avalonia.Commands;
using SD.IOC.Core.Mediators;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MedicalSharp.Client.ViewModels.HomeContext
{
    /// <summary>
    /// 首页视图模型
    /// </summary>
    public class IndexViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗口管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public IndexViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        //通知属性

        #region 体积数据 —— VolumeData VolumeData
        /// <summary>
        /// 体积数据
        /// </summary>
        public VolumeData VolumeData { get; set; }
        #endregion

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


        //命令

        #region 打开序列命令 —— ICommand OpenSeriesCommand
        /// <summary>
        /// 打开序列命令
        /// </summary>
        public ICommand OpenSeriesCommand => new AsyncRelayCommand(_ => this.OpenSeries());
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


        //Actions

        #region 打开序列 —— async Task OpenSeries()
        /// <summary>
        /// 打开序列
        /// </summary>
        public async Task OpenSeries()
        {
            this.Busy();

            //打开文件夹对话框
            FolderPickerOpenOptions openOptions = new FolderPickerOpenOptions
            {
                Title = "打开文件夹",
                AllowMultiple = false
            };

            //获取文件夹
            IReadOnlyList<IStorageFolder> folders = await this.OpenFolderPickerAsync(openOptions);
            if (folders.Any())
            {
                string dicomFolder = folders[0].Path.AbsolutePath;
                VolumeData volumeData = await Task.Run(() => DicomManager.LoadSeries(dicomFolder));
                if (this.VolumeData != null)
                {
                    DicomManager.RemoveVolumeData(this.VolumeData.Id);
                    ResourceManager.RemoveTexture3D(this.VolumeData.Id);
                }

                this.VolumeData = volumeData;
                this.VolumeViewModel.VolumeData = this.VolumeData;
                this.MprAxialViewModel.VolumeData = this.VolumeData;
                this.MprCoronalViewModel.VolumeData = this.VolumeData;
                this.MprSagittalViewModel.VolumeData = this.VolumeData;
            }

            this.Idle();
        }
        #endregion

        #endregion
    }
}
