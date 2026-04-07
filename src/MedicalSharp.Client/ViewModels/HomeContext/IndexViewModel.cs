using Avalonia.Platform.Storage;
using Caliburn.Micro;
using MedicalSharp.Client.ViewModels.LayoutContext;
using MedicalSharp.Engine.Managers;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Managers;
using MedicalSharp.Primitives.Models;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using SD.Infrastructure.Avalonia.Caliburn.Extensions;
using SD.Infrastructure.Avalonia.Commands;
using SD.IOC.Core.Mediators;
using System.Collections.Generic;
using System.Linq;
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
        /// DICOM加载器
        /// </summary>
        private readonly IDicomLoader _dicomLoader;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public IndexViewModel(IWindowManager windowManager, IDicomLoader dicomLoader)
        {
            this._windowManager = windowManager;
            this._dicomLoader = dicomLoader;
            this.LayoutViewModel = ResolveMediator.Resolve<Layout22ViewModel>();
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

        #region 布局视图模型 —— LayoutViewModel LayoutViewModel
        /// <summary>
        /// 布局视图模型
        /// </summary>
        [DependencyProperty]
        public LayoutViewModel LayoutViewModel { get; set; }
        #endregion


        //命令

        #region 打开序列命令 —— ICommand OpenSeriesCommand
        /// <summary>
        /// 打开序列命令
        /// </summary>
        public ICommand OpenSeriesCommand => new AsyncRelayCommand(_ => this.OpenSeries());
        #endregion

        #region 关闭序列命令 —— ICommand CloseSeriesCommand
        /// <summary>
        /// 关闭序列命令
        /// </summary>
        public ICommand CloseSeriesCommand => new RelayCommand(_ => this.CloseSeries());
        #endregion

        #region 布局13命令 —— ICommand Layout13Command
        /// <summary>
        /// 布局13命令
        /// </summary>
        public ICommand Layout13Command => new RelayCommand(_ =>
        {
            #region # 验证

            if (this.LayoutViewModel is Layout13ViewModel)
            {
                return;
            }

            #endregion

            this.LayoutViewModel = ResolveMediator.Resolve<Layout13ViewModel>();
            this.LayoutViewModel.SetVolumeData(this.VolumeData);
        });
        #endregion

        #region 布局22命令 —— ICommand Layout22Command
        /// <summary>
        /// 布局22命令
        /// </summary>
        public ICommand Layout22Command => new RelayCommand(_ =>
        {
            #region # 验证

            if (this.LayoutViewModel is Layout22ViewModel)
            {
                return;
            }

            #endregion

            this.LayoutViewModel = ResolveMediator.Resolve<Layout22ViewModel>();
            this.LayoutViewModel.SetVolumeData(this.VolumeData);
        });
        #endregion

        #endregion

        #region # 方法

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
                VolumeData volumeData = await Task.Run(() => this._dicomLoader.LoadSeries(dicomFolder));
                DicomManager.AddVolumeData(volumeData);

                if (this.VolumeData != null)
                {
                    DicomManager.RemoveVolumeData(this.VolumeData.Id);
                    TextureManager.RemoveTexture3D(this.VolumeData.Id);
                }

                this.VolumeData = volumeData;
                this.LayoutViewModel.SetVolumeData(volumeData);
            }

            this.Idle();
        }
        #endregion

        #region 关闭序列 —— void CloseSeries()
        /// <summary>
        /// 关闭序列
        /// </summary>
        public void CloseSeries()
        {
            #region # 验证

            if (this.VolumeData == null)
            {
                return;
            }

            #endregion

            DicomManager.RemoveVolumeData(this.VolumeData.Id);
            TextureManager.RemoveTexture3D(this.VolumeData.Id);
            this.LayoutViewModel.ClearVolumeData();
        }
        #endregion

        #endregion
    }
}
