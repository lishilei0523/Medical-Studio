using Avalonia.Platform.Storage;
using Caliburn.Micro;
using MedicalSharp.Client.ViewModels.LayoutContext;
using MedicalSharp.Client.ViewModels.ShapeContext;
using MedicalSharp.Engine.Managers;
using MedicalSharp.Primitives.Interfaces;
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
        /// 布局13
        /// </summary>
        private readonly Layout13ViewModel _layout13;

        /// <summary>
        /// 布局22
        /// </summary>
        private readonly Layout22ViewModel _layout22;

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

            //初始化布局
            this._layout13 = ResolveMediator.Resolve<Layout13ViewModel>();
            this._layout22 = ResolveMediator.Resolve<Layout22ViewModel>();
            this.LayoutViewModel = this._layout22;
        }

        #endregion

        #region # 属性

        //通知属性

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
                this._layout13.SetVolumeData(value);
                this._layout22.SetVolumeData(value);
            }
        }
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

            this.LayoutViewModel = this._layout13;
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

            this.LayoutViewModel = this._layout22;
        });
        #endregion

        #region 查看形状命令 —— ICommand LookShapeCommand
        /// <summary>
        /// 查看形状命令
        /// </summary>
        public ICommand LookShapeCommand => new AsyncRelayCommand(async _ =>
        {
            LookViewModel viewModel = ResolveMediator.Resolve<LookViewModel>();
            await this._windowManager.ShowWindowAsync(viewModel);
        });
        #endregion

        #region 拖拽形状命令 —— ICommand DragShapeCommand
        /// <summary>
        /// 拖拽形状命令
        /// </summary>
        public ICommand DragShapeCommand => new AsyncRelayCommand(async _ =>
        {
            DragViewModel viewModel = ResolveMediator.Resolve<DragViewModel>();
            await this._windowManager.ShowWindowAsync(viewModel);
        });
        #endregion

        #region 绘制形状命令 —— ICommand DrawShapeCommand
        /// <summary>
        /// 绘制形状命令
        /// </summary>
        public ICommand DrawShapeCommand => new AsyncRelayCommand(async _ =>
        {
            DrawViewModel viewModel = ResolveMediator.Resolve<DrawViewModel>();
            await this._windowManager.ShowWindowAsync(viewModel);
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
                if (this.VolumeData != null)
                {
                    SessionManager.RemoveVolumeSession(this.VolumeData.Metadata.Id);
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

            SessionManager.RemoveVolumeSession(this.VolumeData.Metadata.Id);
            this._layout13.ClearVolumeData();
            this._layout22.ClearVolumeData();
            this.VolumeData = null;
        }
        #endregion

        #endregion
    }
}
