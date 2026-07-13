using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Caliburn.Micro;
using FluentAvalonia.UI.Controls;
using MedicalSharp.Client.ViewModels.AlgorithmContext;
using MedicalSharp.Client.ViewModels.CommonContext;
using MedicalSharp.Client.ViewModels.DicomContext;
using MedicalSharp.Client.ViewModels.LayoutContext;
using MedicalSharp.Client.ViewModels.ShapeContext;
using MedicalSharp.Client.ViewModels.TissueContext;
using MedicalSharp.Client.Views.HomeContext;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Visual3Ds;
using MedicalSharp.Engine.Base;
using MedicalSharp.Engine.Managers;
using MedicalSharp.Presentation.Events;
using MedicalSharp.Presentation.Maps;
using MedicalSharp.Presentation.Models;
using MedicalSharp.Primitives.Algorithms;
using MedicalSharp.Primitives.Builders;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Managers;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using SD.Common;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using SD.Infrastructure.Avalonia.Caliburn.Extensions;
using SD.Infrastructure.Avalonia.Commands;
using SD.Infrastructure.Avalonia.CustomControls;
using SD.Infrastructure.Avalonia.Enums;
using SD.IOC.Core.Mediators;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MedicalSharp.Client.ViewModels.HomeContext
{
    /// <summary>
    /// 首页视图模型
    /// </summary>
    public class IndexViewModel : ScreenBase, IHandle<GlobalBusyEvent>, IHandle<SyncViewportEvent>, IHandle<StatisticFinishedEvent>, IHandle<AppendShapeEvent>, IHandle<RemoveShapeEvent>
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
        /// DICOM加载器
        /// </summary>
        private readonly IDicomLoader _dicomLoader;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public IndexViewModel(IWindowManager windowManager, IEventAggregator eventAggregator, IDicomLoader dicomLoader)
        {
            this._windowManager = windowManager;
            this._eventAggregator = eventAggregator;
            this._eventAggregator.SubscribeOnUIThread(this);
            this._dicomLoader = dicomLoader;

            //默认值
            this.ViewEnabled = false;
            this.Shapes = [];
            this.Shapes.CollectionChanged += this.OnShapesItemChanged;
            this.MarkModes = typeof(MarkMode).GetEnumMembers();
            this.Tissues =
            [
                new TissueInfo("Base", 0, MarkMode.Visible, Colors.Transparent, true),
    
                //体壁
                new TissueInfo("皮肤", 1, MarkMode.Collapsed, ColorFactory.StandardMarkColors[1].ToColor()),
                new TissueInfo("软组织", 2, MarkMode.Visible, ColorFactory.StandardMarkColors[2].ToColor()),
                new TissueInfo("骨骼", 3, MarkMode.Tinted, ColorFactory.StandardMarkColors[3].ToColor()),
    
                //循环系统
                new TissueInfo("血管", 4, MarkMode.Tinted, ColorFactory.StandardMarkColors[4].ToColor()),
                new TissueInfo("心脏", 5, MarkMode.Visible, ColorFactory.StandardMarkColors[5].ToColor()),
    
                //呼吸系统
                new TissueInfo("肺", 6, MarkMode.Visible, ColorFactory.StandardMarkColors[6].ToColor()),
    
                //消化系统
                new TissueInfo("肝脏", 7, MarkMode.Tinted, ColorFactory.StandardMarkColors[7].ToColor()),
                new TissueInfo("胃", 8, MarkMode.Tinted, ColorFactory.StandardMarkColors[8].ToColor()),
                new TissueInfo("肠", 9, MarkMode.Tinted, ColorFactory.StandardMarkColors[9].ToColor()),
    
                //泌尿系统
                new TissueInfo("肾脏", 10, MarkMode.Tinted, ColorFactory.StandardMarkColors[10].ToColor()),
                new TissueInfo("膀胱", 11, MarkMode.Tinted, ColorFactory.StandardMarkColors[11].ToColor()),
    
                //病理
                new TissueInfo("病变", 12, MarkMode.Tinted, ColorFactory.StandardMarkColors[12].ToColor()),
                new TissueInfo("钙化", 13, MarkMode.Tinted, ColorFactory.StandardMarkColors[13].ToColor())
            ];

            //布局
            this.LayoutViewModel = ResolveMediator.Resolve<HomeLayoutViewModel>();
            this.LayoutViewModel.VolumeViewModel.Shapes = this.Shapes;
            this.LayoutViewModel.VolumeViewModel.Tissues = this.Tissues;
            this.LayoutViewModel.MprAxialViewModel.MprViewModel.Shapes = this.Shapes;
            this.LayoutViewModel.MprAxialViewModel.MprViewModel.Tissues = this.Tissues;
            this.LayoutViewModel.MprCoronalViewModel.MprViewModel.Shapes = this.Shapes;
            this.LayoutViewModel.MprCoronalViewModel.MprViewModel.Tissues = this.Tissues;
            this.LayoutViewModel.MprSagittalViewModel.MprViewModel.Shapes = this.Shapes;
            this.LayoutViewModel.MprSagittalViewModel.MprViewModel.Tissues = this.Tissues;
        }

        #endregion

        #region # 属性

        //属性

        #region 视图是否启用 —— bool ViewEnabled
        /// <summary>
        /// 视图是否启用
        /// </summary>
        [DependencyProperty]
        public bool ViewEnabled { get; set; }
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
                if (value != null)
                {
                    this.ViewEnabled = true;
                    this.LayoutViewModel.SetVolumeData(value);
                    this.VolumeInfo = value.Metadata.ToVolumeInfo();
                    this.PatientInfo = value.PatientData.ToPatientInfo();
                    this.StudyInfo = value.StudyData.ToStudyInfo();
                    this.SeriesInfo = value.SeriesData.ToSeriesInfo();
                    this.ScanInfo = value.ScanData.ToScanInfo();
                    this.WindowWidth = value.Metadata.WindowWidth;
                    this.WindowCenter = value.Metadata.WindowCenter;

                    //初始化标记策略
                    VolumeSession session = SessionManager.VolumeSessions[value.Metadata.Id];
                    foreach (TissueInfo tissue in this.Tissues)
                    {
                        session.MarkStrategy.SwitchMarkMode(tissue.MarkValue, tissue.MarkMode);
                    }

                    //打开区域生长分割面板
                    this.RegionGrowCommand.Execute(null);
                }
                else
                {
                    this.ViewEnabled = false;
                    this.LayoutViewModel.ClearVolumeData();
                    this.VolumeInfo = null;
                    this.PatientInfo = null;
                    this.StudyInfo = null;
                    this.SeriesInfo = null;
                    this.ScanInfo = null;
                    this.StatisticInfo = null;
                }
            }
        }
        #endregion

        #region 体积信息 —— VolumeInfo VolumeInfo
        /// <summary>
        /// 体积信息
        /// </summary>
        [DependencyProperty]
        public VolumeInfo VolumeInfo { get; set; }
        #endregion

        #region 患者信息 —— PatientInfo PatientInfo
        /// <summary>
        /// 患者信息
        /// </summary>
        [DependencyProperty]
        public PatientInfo PatientInfo { get; set; }
        #endregion

        #region 检查信息 —— StudyInfo StudyInfo
        /// <summary>
        /// 检查信息
        /// </summary>
        [DependencyProperty]
        public StudyInfo StudyInfo { get; set; }
        #endregion

        #region 序列信息 —— SeriesInfo SeriesInfo
        /// <summary>
        /// 序列信息
        /// </summary>
        [DependencyProperty]
        public SeriesInfo SeriesInfo { get; set; }
        #endregion

        #region 扫描信息 —— ScanInfo ScanInfo
        /// <summary>
        /// 扫描信息
        /// </summary>
        [DependencyProperty]
        public ScanInfo ScanInfo { get; set; }
        #endregion

        #region 统计信息 —— StatisticInfo StatisticInfo
        /// <summary>
        /// 统计信息
        /// </summary>
        [DependencyProperty]
        public StatisticInfo StatisticInfo { get; set; }
        #endregion

        #region 功能面板 —— ScreenBase FunctionPanel
        /// <summary>
        /// 功能面板
        /// </summary>
        [DependencyProperty]
        public ScreenBase FunctionPanel { get; set; }
        #endregion

        #region 布局视图模型 —— HomeLayoutViewModel LayoutViewModel
        /// <summary>
        /// 布局视图模型
        /// </summary>
        [DependencyProperty]
        public HomeLayoutViewModel LayoutViewModel { get; set; }
        #endregion

        #region 已选组织 —— TissueInfo SelectedTissue
        /// <summary>
        /// 已选组织
        /// </summary>
        public TissueInfo SelectedTissue
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();

                this.LayoutViewModel.VolumeViewModel.SelectedTissue = value;
                this.LayoutViewModel.MprAxialViewModel.MprViewModel.SelectedTissue = value;
                this.LayoutViewModel.MprCoronalViewModel.MprViewModel.SelectedTissue = value;
                this.LayoutViewModel.MprSagittalViewModel.MprViewModel.SelectedTissue = value;
            }
        }
        #endregion

        #region 组织列表 —— AvaloniaList<TissueInfo> Tissues
        /// <summary>
        /// 组织列表
        /// </summary>
        [DependencyProperty]
        public AvaloniaList<TissueInfo> Tissues { get; set; }
        #endregion

        #region 已选形状 —— ShapeVisual3D SelectedShape
        /// <summary>
        /// 已选形状
        /// </summary>
        public ShapeVisual3D SelectedShape
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                foreach (ShapeVisual3D shape in this.Shapes)
                {
                    shape.IsSelected = false;
                }
                if (value != null)
                {
                    value.IsSelected = true;

                    //发布消息
                    SyncViewportEvent message = new SyncViewportEvent
                    {
                        Publisher = this
                    };
                    this._eventAggregator.PublishOnUIThreadAsync(message);
                }
            }
        }
        #endregion

        #region 形状列表 —— AvaloniaList<ShapeVisual3D> Shapes
        /// <summary>
        /// 形状列表
        /// </summary>
        [DependencyProperty]
        public AvaloniaList<ShapeVisual3D> Shapes { get; set; }
        #endregion

        #region 已选预设窗 —— WindowLevel SelectedWindowLevel
        /// <summary>
        /// 已选预设窗
        /// </summary>
        public WindowLevel SelectedWindowLevel
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                if (value != null)
                {
                    this.WindowWidth = value.WindowWidth;
                    this.WindowCenter = value.WindowCenter;
                }
            }
        }
        #endregion

        #region 预设窗列表 —— AvaloniaList<WindowLevel> WindowLevels
        /// <summary>
        /// 预设窗列表
        /// </summary>
        [DependencyProperty]
        public AvaloniaList<WindowLevel> WindowLevels { get; set; }
        #endregion

        #region 窗宽 —— int WindowWidth
        /// <summary>
        /// 窗宽
        /// </summary>
        public int WindowWidth
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                this.LayoutViewModel.MprAxialViewModel.MprViewModel.WindowWidth = value;
                this.LayoutViewModel.MprCoronalViewModel.MprViewModel.WindowWidth = value;
                this.LayoutViewModel.MprSagittalViewModel.MprViewModel.WindowWidth = value;
            }
        }
        #endregion

        #region 窗位 —— int WindowCenter
        /// <summary>
        /// 窗位
        /// </summary>
        public int WindowCenter
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                this.LayoutViewModel.MprAxialViewModel.MprViewModel.WindowCenter = value;
                this.LayoutViewModel.MprCoronalViewModel.MprViewModel.WindowCenter = value;
                this.LayoutViewModel.MprSagittalViewModel.MprViewModel.WindowCenter = value;
            }
        }
        #endregion

        #region 三平面是否显示 —— bool MprPlanesVisible
        /// <summary>
        /// 三平面是否显示
        /// </summary>
        public bool MprPlanesVisible
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                this.LayoutViewModel.VolumeViewModel.AxialPlaneVisible = value;
                this.LayoutViewModel.VolumeViewModel.CoronalPlaneVisible = value;
                this.LayoutViewModel.VolumeViewModel.SagittalPlaneVisible = value;
            }
        }
        #endregion

        #region 十字线是否显示 —— bool CrosshairVisible
        /// <summary>
        /// 十字线是否显示
        /// </summary>
        public bool CrosshairVisible
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                this.LayoutViewModel.MprAxialViewModel.MprViewModel.CrosshairVisible = value;
                this.LayoutViewModel.MprCoronalViewModel.MprViewModel.CrosshairVisible = value;
                this.LayoutViewModel.MprSagittalViewModel.MprViewModel.CrosshairVisible = value;
            }
        }
        #endregion

        #region 标记模式字典 —— IDictionary<string, string> MarkModes
        /// <summary>
        /// 标记模式字典
        /// </summary>
        [DependencyProperty]
        public IDictionary<string, string> MarkModes { get; set; }
        #endregion


        //命令

        #region 打开序列文件命令 —— ICommand OpenSeriesFilesCommand
        /// <summary>
        /// 打开序列文件命令
        /// </summary>
        public ICommand OpenSeriesFilesCommand => new AsyncRelayCommand(async _ =>
        {
            this.Busy();

            //打开文件对话框
            FilePickerOpenOptions openOptions = new FilePickerOpenOptions
            {
                Title = "打开序列文件",
                AllowMultiple = true,
                FileTypeFilter = [
                    new FilePickerFileType("DICOM文件")
                    {
                        Patterns = ["*.dcm"]
                    }
                ]
            };

            //获取文件
            IReadOnlyList<IStorageFile> files = await this.OpenFilePickerAsync(openOptions);
            if (files.Any())
            {
                string[] filePaths = files.Select(x => x.TryGetLocalPath()).ToArray();
                VolumeData volumeData = await Task.Run(() => this._dicomLoader.LoadSeries(filePaths));
                if (this.VolumeData != null)
                {
                    SessionManager.RemoveVolumeSession(this.VolumeData.Metadata.Id);
                }
                this.VolumeData = volumeData;
            }

            this.Idle();
        });
        #endregion

        #region 打开序列文件夹命令 —— ICommand OpenSeriesFolderCommand
        /// <summary>
        /// 打开序列文件夹命令
        /// </summary>
        public ICommand OpenSeriesFolderCommand => new AsyncRelayCommand(async _ =>
        {
            this.Busy();

            //打开文件夹对话框
            FolderPickerOpenOptions openOptions = new FolderPickerOpenOptions
            {
                Title = "打开序列文件夹",
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
            }

            this.Idle();
        });
        #endregion

        #region 打开(NIFTI)命令 —— ICommand OpenNiiCommand
        /// <summary>
        /// 打开(NIFTI)命令
        /// </summary>
        public ICommand OpenNiiCommand => new AsyncRelayCommand(async _ =>
        {
            this.Busy();

            //打开文件对话框
            FilePickerOpenOptions openOptions = new FilePickerOpenOptions
            {
                Title = "打开NIFTI文件",
                AllowMultiple = false,
                FileTypeFilter = [
                    new FilePickerFileType("NIFTI文件")
                    {
                        Patterns = ["*.nii"]
                    }
                ]
            };

            //获取文件
            IReadOnlyList<IStorageFile> files = await this.OpenFilePickerAsync(openOptions);
            if (files.Any())
            {
                string filePath = files[0].TryGetLocalPath();
                VolumeData volumeData = await Task.Run(() => this._dicomLoader.LoadNiiImage(filePath));
                if (this.VolumeData != null)
                {
                    SessionManager.RemoveVolumeSession(this.VolumeData.Metadata.Id);
                }
                this.VolumeData = volumeData;
            }

            this.Idle();
        });
        #endregion

        #region 打开(MHD+RAW)命令 —— ICommand OpenRawCommand
        /// <summary>
        /// 打开(MHD+RAW)命令
        /// </summary>
        public ICommand OpenRawCommand => new AsyncRelayCommand(async _ =>
        {
            this.Busy();

            //打开文件对话框
            FilePickerOpenOptions openOptions = new FilePickerOpenOptions
            {
                Title = "打开(MHD+RAW)文件",
                AllowMultiple = false,
                FileTypeFilter = [
                    new FilePickerFileType("(MHD+RAW)文件")
                    {
                        Patterns = ["*.mhd"]
                    }
                ]
            };

            //获取文件
            IReadOnlyList<IStorageFile> files = await this.OpenFilePickerAsync(openOptions);
            if (files.Any())
            {
                string filePath = files[0].TryGetLocalPath();
                VolumeData volumeData = await Task.Run(() => this._dicomLoader.LoadRawImage(filePath));
                if (this.VolumeData != null)
                {
                    SessionManager.RemoveVolumeSession(this.VolumeData.Metadata.Id);
                }
                this.VolumeData = volumeData;
            }

            this.Idle();
        });
        #endregion

        #region 保存原始数据(NIFTI)命令 —— ICommand SaveOriginalNiiCommand
        /// <summary>
        /// 保存原始数据(NIFTI)命令
        /// </summary>
        public ICommand SaveOriginalNiiCommand => new AsyncRelayCommand(async _ =>
        {
            this.Busy();

            //保存文件对话框
            FilePickerSaveOptions openOptions = new FilePickerSaveOptions
            {
                Title = "保存原始数据为NIFTI文件",
                SuggestedFileName = "Original.nii",
                FileTypeChoices = [
                    new FilePickerFileType("NIFTI文件")
                    {
                        Patterns = ["*.nii"]
                    }
                ]
            };

            //保存文件
            IStorageFile storageFile = await this.SaveFilePickerAsync(openOptions);
            if (storageFile != null)
            {
                string filePath = storageFile.TryGetLocalPath();
                await Task.Run(() => this._dicomLoader.SaveOriginalNiiImage(this.VolumeData, filePath));
            }

            this.Idle();
        }, _ => this.VolumeData != null);
        #endregion

        #region 保存原始数据(MHD+RAW)命令 —— ICommand SaveOriginalRawCommand
        /// <summary>
        /// 保存原始数据(MHD+RAW)命令
        /// </summary>
        public ICommand SaveOriginalRawCommand => new AsyncRelayCommand(async _ =>
        {
            this.Busy();

            //保存文件对话框
            FilePickerSaveOptions openOptions = new FilePickerSaveOptions
            {
                Title = "保存原始数据为(MHD+RAW)文件",
                SuggestedFileName = "Original.mhd",
                FileTypeChoices = [
                    new FilePickerFileType("(MHD+RAW)文件")
                    {
                        Patterns = ["*.mhd"]
                    }
                ]
            };

            //保存文件
            IStorageFile storageFile = await this.SaveFilePickerAsync(openOptions);
            if (storageFile != null)
            {
                string filePath = storageFile.TryGetLocalPath();
                await Task.Run(() => this._dicomLoader.SaveOriginalRawImage(this.VolumeData, filePath));
            }

            this.Idle();
        }, _ => this.VolumeData != null);
        #endregion

        #region 保存预览数据(NIFTI)命令 —— ICommand SavePreviewNiiCommand
        /// <summary>
        /// 保存预览数据(NIFTI)命令
        /// </summary>
        public ICommand SavePreviewNiiCommand => new AsyncRelayCommand(async _ =>
        {
            this.Busy();

            //保存文件对话框
            FilePickerSaveOptions openOptions = new FilePickerSaveOptions
            {
                Title = "保存预览数据为NIFTI文件",
                SuggestedFileName = "Preview.nii",
                FileTypeChoices = [
                    new FilePickerFileType("NIFTI文件")
                    {
                        Patterns = ["*.nii"]
                    }
                ]
            };

            //保存文件
            IStorageFile storageFile = await this.SaveFilePickerAsync(openOptions);
            if (storageFile != null)
            {
                string filePath = storageFile.TryGetLocalPath();
                await Task.Run(() => this._dicomLoader.SavePreviewNiiImage(this.VolumeData, filePath));
            }

            this.Idle();
        }, _ => this.VolumeData != null);
        #endregion

        #region 保存预览数据(MHD+RAW)命令 —— ICommand SavePreviewRawCommand
        /// <summary>
        /// 保存预览数据(MHD+RAW)命令
        /// </summary>
        public ICommand SavePreviewRawCommand => new AsyncRelayCommand(async _ =>
        {
            this.Busy();

            //保存文件对话框
            FilePickerSaveOptions openOptions = new FilePickerSaveOptions
            {
                Title = "保存预览数据为(MHD+RAW)文件",
                SuggestedFileName = "Preview.mhd",
                FileTypeChoices = [
                    new FilePickerFileType("(MHD+RAW)文件")
                    {
                        Patterns = ["*.mhd"]
                    }
                ]
            };

            //保存文件
            IStorageFile storageFile = await this.SaveFilePickerAsync(openOptions);
            if (storageFile != null)
            {
                string filePath = storageFile.TryGetLocalPath();
                await Task.Run(() => this._dicomLoader.SavePreviewRawImage(this.VolumeData, filePath));
            }

            this.Idle();
        }, _ => this.VolumeData != null);
        #endregion

        #region 保存标记数据(NIFTI)命令 —— ICommand SaveMarkNiiCommand
        /// <summary>
        /// 保存标记数据(NIFTI)命令
        /// </summary>
        public ICommand SaveMarkNiiCommand => new AsyncRelayCommand(async _ =>
        {
            this.Busy();

            //保存文件对话框
            FilePickerSaveOptions openOptions = new FilePickerSaveOptions
            {
                Title = "保存标记数据为NIFTI文件",
                SuggestedFileName = "Mark.nii",
                FileTypeChoices = [
                    new FilePickerFileType("NIFTI文件")
                    {
                        Patterns = ["*.nii"]
                    }
                ]
            };

            //保存文件
            IStorageFile storageFile = await this.SaveFilePickerAsync(openOptions);
            if (storageFile != null)
            {
                string filePath = storageFile.TryGetLocalPath();
                await Task.Run(() => this._dicomLoader.SaveMarkNiiImage(this.VolumeData, filePath));
            }

            this.Idle();
        }, _ => this.VolumeData != null);
        #endregion

        #region 保存标记数据(MHD+RAW)命令 —— ICommand SaveMarkRawCommand
        /// <summary>
        /// 保存标记数据(MHD+RAW)命令
        /// </summary>
        public ICommand SaveMarkRawCommand => new AsyncRelayCommand(async _ =>
        {
            this.Busy();

            //保存文件对话框
            FilePickerSaveOptions openOptions = new FilePickerSaveOptions
            {
                Title = "保存标记数据为(MHD+RAW)文件",
                SuggestedFileName = "Mark.mhd",
                FileTypeChoices = [
                    new FilePickerFileType("(MHD+RAW)文件")
                    {
                        Patterns = ["*.mhd"]
                    }
                ]
            };

            //保存文件
            IStorageFile storageFile = await this.SaveFilePickerAsync(openOptions);
            if (storageFile != null)
            {
                string filePath = storageFile.TryGetLocalPath();
                await Task.Run(() => this._dicomLoader.SaveMarkNiiImage(this.VolumeData, filePath));
            }

            this.Idle();
        }, _ => this.VolumeData != null);
        #endregion

        #region 加载预览数据(NIFTI)命令 —— ICommand LoadPreviewNiiCommand
        /// <summary>
        /// 加载预览数据(NIFTI)命令
        /// </summary>
        public ICommand LoadPreviewNiiCommand => new AsyncRelayCommand(async _ =>
        {
            this.Busy();

            //打开文件对话框
            FilePickerOpenOptions openOptions = new FilePickerOpenOptions
            {
                Title = "打开预览NIFTI文件",
                AllowMultiple = false,
                FileTypeFilter = [
                    new FilePickerFileType("NIFTI文件")
                    {
                        Patterns = ["*.nii"]
                    }
                ]
            };

            //获取文件
            IReadOnlyList<IStorageFile> files = await this.OpenFilePickerAsync(openOptions);
            if (files.Any())
            {
                string filePath = files[0].TryGetLocalPath();
                await Task.Run(() => this._dicomLoader.LoadNiiPreview(this.VolumeData, filePath));

                //同步GPU端
                VolumeSession session = SessionManager.VolumeSessions[this.VolumeData.Metadata.Id];
                this.VolumeData.SyncPreviewDataToGpu(session.PreviewTexture);

                //发布消息
                SyncViewportEvent message = new SyncViewportEvent();
                await this._eventAggregator.PublishOnUIThreadAsync(message);
            }

            this.Idle();
        }, _ => this.VolumeData != null);
        #endregion

        #region 加载预览数据(MHD+RAW)命令 —— ICommand LoadPreviewRawCommand
        /// <summary>
        /// 加载预览数据(MHD+RAW)命令
        /// </summary>
        public ICommand LoadPreviewRawCommand => new AsyncRelayCommand(async _ =>
        {
            this.Busy();

            //打开文件对话框
            FilePickerOpenOptions openOptions = new FilePickerOpenOptions
            {
                Title = "打开预览(MHD+RAW)文件",
                AllowMultiple = false,
                FileTypeFilter = [
                    new FilePickerFileType("(MHD+RAW)文件")
                    {
                        Patterns = ["*.mhd"]
                    }
                ]
            };

            //获取文件
            IReadOnlyList<IStorageFile> files = await this.OpenFilePickerAsync(openOptions);
            if (files.Any())
            {
                string filePath = files[0].TryGetLocalPath();
                await Task.Run(() => this._dicomLoader.LoadRawPreview(this.VolumeData, filePath));

                //同步GPU端
                VolumeSession session = SessionManager.VolumeSessions[this.VolumeData.Metadata.Id];
                this.VolumeData.SyncPreviewDataToGpu(session.PreviewTexture);

                //发布消息
                SyncViewportEvent message = new SyncViewportEvent();
                await this._eventAggregator.PublishOnUIThreadAsync(message);
            }

            this.Idle();
        }, _ => this.VolumeData != null);
        #endregion

        #region 加载标记数据(NIFTI)命令 —— ICommand LoadMarkNiiCommand
        /// <summary>
        /// 加载标记数据(NIFTI)命令
        /// </summary>
        public ICommand LoadMarkNiiCommand => new AsyncRelayCommand(async _ =>
        {
            this.Busy();

            //打开文件对话框
            FilePickerOpenOptions openOptions = new FilePickerOpenOptions
            {
                Title = "打开标记NIFTI文件",
                AllowMultiple = false,
                FileTypeFilter = [
                    new FilePickerFileType("NIFTI文件")
                    {
                        Patterns = ["*.nii"]
                    }
                ]
            };

            //获取文件
            IReadOnlyList<IStorageFile> files = await this.OpenFilePickerAsync(openOptions);
            if (files.Any())
            {
                string filePath = files[0].TryGetLocalPath();
                await Task.Run(() => this._dicomLoader.LoadNiiMark(this.VolumeData, filePath));

                //同步GPU端
                VolumeSession session = SessionManager.VolumeSessions[this.VolumeData.Metadata.Id];
                this.VolumeData.SyncMarkDataToGpu(session.MarkTexture);

                //发布消息
                SyncViewportEvent message = new SyncViewportEvent();
                await this._eventAggregator.PublishOnUIThreadAsync(message);
            }

            this.Idle();
        }, _ => this.VolumeData != null);
        #endregion

        #region 加载标记数据(MHD+RAW)命令 —— ICommand LoadMarkRawCommand
        /// <summary>
        /// 加载标记数据(MHD+RAW)命令
        /// </summary>
        public ICommand LoadMarkRawCommand => new AsyncRelayCommand(async _ =>
        {
            this.Busy();

            //打开文件对话框
            FilePickerOpenOptions openOptions = new FilePickerOpenOptions
            {
                Title = "打开标记(MHD+RAW)文件",
                AllowMultiple = false,
                FileTypeFilter = [
                    new FilePickerFileType("(MHD+RAW)文件")
                    {
                        Patterns = ["*.mhd"]
                    }
                ]
            };

            //获取文件
            IReadOnlyList<IStorageFile> files = await this.OpenFilePickerAsync(openOptions);
            if (files.Any())
            {
                string filePath = files[0].TryGetLocalPath();
                await Task.Run(() => this._dicomLoader.LoadRawMark(this.VolumeData, filePath));

                //同步GPU端
                VolumeSession session = SessionManager.VolumeSessions[this.VolumeData.Metadata.Id];
                this.VolumeData.SyncMarkDataToGpu(session.MarkTexture);

                //发布消息
                SyncViewportEvent message = new SyncViewportEvent();
                await this._eventAggregator.PublishOnUIThreadAsync(message);
            }

            this.Idle();
        }, _ => this.VolumeData != null);
        #endregion

        #region 关闭会话命令 —— ICommand CloseSessionCommand
        /// <summary>
        /// 关闭会话命令
        /// </summary>
        public ICommand CloseSessionCommand => new RelayCommand(_ =>
        {
            SessionManager.RemoveVolumeSession(this.VolumeData.Metadata.Id);
            this.VolumeData = null;
        }, _ => this.VolumeData != null);
        #endregion

        #region 重置预览命令 —— ICommand ResetPreviewCommand
        /// <summary>
        /// 重置预览命令
        /// </summary>
        public ICommand ResetPreviewCommand => new AsyncRelayCommand(async _ =>
        {
            TaskDialogStandardResult result = await MessageBox.Show("确定要重置吗？", "警告", MessageBoxButton.OKCancel);
            if (result == TaskDialogStandardResult.OK)
            {
                VolumeSession volumeSession = SessionManager.VolumeSessions[this.VolumeData.Metadata.Id];
                volumeSession.ResetPreviewTexture();

                //发布消息
                SyncViewportEvent message = new SyncViewportEvent();
                await this._eventAggregator.PublishOnUIThreadAsync(message);
            }
        }, _ => this.VolumeData != null);
        #endregion

        #region 重置标记命令 —— ICommand ResetMarkCommand
        /// <summary>
        /// 重置标记命令
        /// </summary>
        public ICommand ResetMarkCommand => new AsyncRelayCommand(async _ =>
        {
            TaskDialogStandardResult result = await MessageBox.Show("确定要重置吗？", "警告", MessageBoxButton.OKCancel);
            if (result == TaskDialogStandardResult.OK)
            {
                VolumeSession volumeSession = SessionManager.VolumeSessions[this.VolumeData.Metadata.Id];
                volumeSession.ResetMarkTexture();

                //发布消息
                SyncViewportEvent message = new SyncViewportEvent();
                await this._eventAggregator.PublishOnUIThreadAsync(message);
            }
        }, _ => this.VolumeData != null);
        #endregion

        #region 清空形状命令 —— ICommand ClearShapesCommand
        /// <summary>
        /// 清空形状命令
        /// </summary>
        public ICommand ClearShapesCommand => new AsyncRelayCommand(async _ =>
        {
            TaskDialogStandardResult result = await MessageBox.Show("确定要清空吗？", "警告", MessageBoxButton.OKCancel);
            if (result == TaskDialogStandardResult.OK)
            {
                this.Shapes.Clear();
            }
        }, _ => this.VolumeData != null);
        #endregion

        #region 查看患者信息命令 —— ICommand LookPatientInfoCommand
        /// <summary>
        /// 查看患者信息命令
        /// </summary>
        public ICommand LookPatientInfoCommand => new AsyncRelayCommand(async _ =>
        {
            PatientViewModel viewModel = ResolveMediator.Resolve<PatientViewModel>();
            viewModel.IndexViewModel = this;
            await this._windowManager.ShowWindowAsync(viewModel);

        }, _ => this.VolumeData != null);
        #endregion

        #region 查看检查信息命令 —— ICommand LookStudyInfoCommand
        /// <summary>
        /// 查看检查信息命令
        /// </summary>
        public ICommand LookStudyInfoCommand => new AsyncRelayCommand(async _ =>
        {
            StudyViewModel viewModel = ResolveMediator.Resolve<StudyViewModel>();
            viewModel.IndexViewModel = this;
            await this._windowManager.ShowWindowAsync(viewModel);

        }, _ => this.VolumeData != null);
        #endregion

        #region 查看序列信息命令 —— ICommand LookSeriesInfoCommand
        /// <summary>
        /// 查看序列信息命令
        /// </summary>
        public ICommand LookSeriesInfoCommand => new AsyncRelayCommand(async _ =>
        {
            SeriesViewModel viewModel = ResolveMediator.Resolve<SeriesViewModel>();
            viewModel.IndexViewModel = this;
            await this._windowManager.ShowWindowAsync(viewModel);

        }, _ => this.VolumeData != null);
        #endregion

        #region 查看扫描信息命令 —— ICommand LookScanInfoCommand
        /// <summary>
        /// 查看扫描信息命令
        /// </summary>
        public ICommand LookScanInfoCommand => new AsyncRelayCommand(async _ =>
        {
            ScanViewModel viewModel = ResolveMediator.Resolve<ScanViewModel>();
            viewModel.IndexViewModel = this;
            await this._windowManager.ShowWindowAsync(viewModel);

        }, _ => this.VolumeData != null);
        #endregion

        #region 查看体积信息命令 —— ICommand LookVolumeInfoCommand
        /// <summary>
        /// 查看体积信息命令
        /// </summary>
        public ICommand LookVolumeInfoCommand => new AsyncRelayCommand(async _ =>
        {
            MetadataViewModel viewModel = ResolveMediator.Resolve<MetadataViewModel>();
            viewModel.IndexViewModel = this;
            await this._windowManager.ShowWindowAsync(viewModel);

        }, _ => this.VolumeData != null);
        #endregion

        #region 布局22命令 —— ICommand Layout22Command
        /// <summary>
        /// 布局22命令
        /// </summary>
        public ICommand Layout22Command => new RelayCommand(_ =>
        {
            this.LayoutViewModel.SwitchToLayout22();
        });
        #endregion

        #region 布局13命令 —— ICommand Layout13Command
        /// <summary>
        /// 布局13命令
        /// </summary>
        public ICommand Layout13Command => new RelayCommand(_ =>
        {
            this.LayoutViewModel.SwitchToLayout13();
        });
        #endregion

        #region 创建组织命令 —— ICommand AddTissueCommand
        /// <summary>
        /// 创建组织命令
        /// </summary>
        public ICommand AddTissueCommand => new AsyncRelayCommand(async _ =>
        {
            byte markValue = (byte)(this.Tissues.Max(x => x.MarkValue) + 1);

            AddViewModel viewModel = ResolveMediator.Resolve<AddViewModel>();
            viewModel.TissueName = $"组织{markValue}";
            viewModel.MarkValue = markValue;
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                #region # 验证

                if (this.Tissues.Any(x => x.MarkValue == viewModel.MarkValue!.Value))
                {
                    await MessageBox.Show("组织标记值已存在，请重试！");
                    return;
                }

                #endregion

                TissueInfo tissue = new TissueInfo(viewModel.TissueName, viewModel.MarkValue!.Value, viewModel.MarkMode, viewModel.TissueColor);
                this.Tissues.Add(tissue);
            }
        }, _ => this.VolumeData != null);
        #endregion

        #region 编辑组织命令 —— ICommand UpdateTissueCommand
        /// <summary>
        /// 编辑组织命令
        /// </summary>
        public ICommand UpdateTissueCommand => new AsyncRelayCommand(async _ =>
        {
            UpdateViewModel viewModel = ResolveMediator.Resolve<UpdateViewModel>();
            viewModel.TissueName = this.SelectedTissue.Name;
            viewModel.MarkValue = this.SelectedTissue.MarkValue;
            viewModel.TissueColor = this.SelectedTissue.Color;
            viewModel.SelectedMarkMode = new KeyValuePair<string, string>(this.SelectedTissue.MarkMode.ToString(), this.SelectedTissue.MarkMode.GetEnumMember());
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                #region # 验证

                if (viewModel.MarkValue != this.SelectedTissue.MarkValue &&
                    this.Tissues.Any(x => x.MarkValue == viewModel.MarkValue!.Value))
                {
                    await MessageBox.Show("组织标记值已存在，请重试！");
                    return;
                }

                #endregion

                this.SelectedTissue.Name = viewModel.TissueName;
                this.SelectedTissue.MarkValue = viewModel.MarkValue!.Value;
                this.SelectedTissue.Color = viewModel.TissueColor;
                this.SelectedTissue.SelectedMarkMode = new KeyValuePair<string, string>(viewModel.MarkMode.ToString(), viewModel.MarkMode.GetEnumMember());
            }

        }, _ => this.VolumeData != null && this.SelectedTissue != null && this.SelectedTissue.MarkValue != 0);
        #endregion

        #region 重置组织命令 —— ICommand ResetTissueCommand
        /// <summary>
        /// 重置组织命令
        /// </summary>
        public ICommand ResetTissueCommand => new AsyncRelayCommand(async _ =>
        {
            TaskDialogStandardResult result = await MessageBox.Show("确定要重置吗？", "警告", MessageBoxButton.OKCancel);
            if (result == TaskDialogStandardResult.OK)
            {
                //组织Mark值置为0
                VolumeSession session = SessionManager.VolumeSessions[this.VolumeData.Metadata.Id];
                session.ResetMark(this.SelectedTissue.MarkValue);

                //发布消息
                SyncViewportEvent message = new SyncViewportEvent();
                await this._eventAggregator.PublishOnUIThreadAsync(message);
            }

        }, _ => this.VolumeData != null && this.SelectedTissue != null && this.SelectedTissue.MarkValue != 0);
        #endregion

        #region 删除组织命令 —— ICommand RemoveTissueCommand
        /// <summary>
        /// 删除组织命令
        /// </summary>
        public ICommand RemoveTissueCommand => new AsyncRelayCommand(async _ =>
        {
            TaskDialogStandardResult result = await MessageBox.Show("确定要删除吗？", "警告", MessageBoxButton.OKCancel);
            if (result == TaskDialogStandardResult.OK)
            {
                //组织Mark值置为0
                VolumeSession session = SessionManager.VolumeSessions[this.VolumeData.Metadata.Id];
                session.ResetMark(this.SelectedTissue.MarkValue);

                //删除组织，重设选中
                this.Tissues.Remove(this.SelectedTissue);
                if (this.Tissues.Any())
                {
                    this.SelectedTissue = this.Tissues[0];
                }

                //发布消息
                SyncViewportEvent message = new SyncViewportEvent();
                await this._eventAggregator.PublishOnUIThreadAsync(message);
            }

        }, _ => this.VolumeData != null && this.SelectedTissue != null && this.SelectedTissue.MarkValue != 0);
        #endregion

        #region 导出PCD点云命令 —— ICommand ExportPCDCommand
        /// <summary>
        /// 导出PCD点云命令
        /// </summary>
        public ICommand ExportPCDCommand => new AsyncRelayCommand(async _ =>
        {
            this.Busy();

            //保存文件对话框
            FilePickerSaveOptions openOptions = new FilePickerSaveOptions
            {
                Title = "保存点云(PCD)文件",
                SuggestedFileName = $"{this.SelectedTissue.Name}.pcd",
                FileTypeChoices = [
                    new FilePickerFileType("点云(PCD)文件")
                    {
                        Patterns = ["*.pcd"]
                    }
                ]
            };

            //保存文件
            IStorageFile storageFile = await this.SaveFilePickerAsync(openOptions);
            if (storageFile != null)
            {
                string filePath = storageFile.TryGetLocalPath();
                IReadOnlyList<Vector4> pointCloud = this.VolumeData.ExportPointCloud(this.SelectedTissue.MarkValue);
                string pcd = ExportAlgorithms.EncodePointCloudToPCD(pointCloud);

                #region # 验证

                if (string.IsNullOrWhiteSpace(pcd))
                {
                    await MessageBox.Show($"组织\"{this.SelectedTissue.Name}\"不存在有效数据！");
                    return;
                }

                #endregion

                await Task.Run(() => File.WriteAllTextAsync(filePath!, pcd));
            }

            this.Idle();
        }, _ => this.VolumeData != null && this.SelectedTissue != null);
        #endregion

        #region 导出PLY点云命令 —— ICommand ExportPLYCommand
        /// <summary>
        /// 导出PLY点云命令
        /// </summary>
        public ICommand ExportPLYCommand => new AsyncRelayCommand(async _ =>
        {
            this.Busy();

            //保存文件对话框
            FilePickerSaveOptions openOptions = new FilePickerSaveOptions
            {
                Title = "保存点云(PLY)文件",
                SuggestedFileName = $"{this.SelectedTissue.Name}.ply",
                FileTypeChoices = [
                    new FilePickerFileType("点云(PLY)文件")
                    {
                        Patterns = ["*.ply"]
                    }
                ]
            };

            //保存文件
            IStorageFile storageFile = await this.SaveFilePickerAsync(openOptions);
            if (storageFile != null)
            {
                string filePath = storageFile.TryGetLocalPath();
                IReadOnlyList<Vector4> pointCloud = this.VolumeData.ExportPointCloud(this.SelectedTissue.MarkValue);
                string ply = ExportAlgorithms.EncodePointCloudToPLY(pointCloud);

                #region # 验证

                if (string.IsNullOrWhiteSpace(ply))
                {
                    await MessageBox.Show($"组织\"{this.SelectedTissue.Name}\"不存在有效数据！");
                    return;
                }

                #endregion

                await Task.Run(() => File.WriteAllTextAsync(filePath!, ply));
            }

            this.Idle();
        }, _ => this.VolumeData != null && this.SelectedTissue != null);
        #endregion

        #region CPR重建命令 —— ICommand CPRCommand
        /// <summary>
        /// CPR重建命令
        /// </summary>
        public ICommand CPRCommand => new AsyncRelayCommand(async _ =>
        {
            CurveVisual3D curve = (CurveVisual3D)this.SelectedShape;
            CprLayoutViewModel viewModel = new CprLayoutViewModel(curve);
            viewModel.SetVolumeData(this.VolumeData);
            await this._windowManager.ShowWindowAsync(viewModel);
        }, _ => this.VolumeData != null && this.SelectedShape is CurveVisual3D);
        #endregion

        #region HU直方图命令 —— ICommand HUHistogramCommand
        /// <summary>
        /// HU直方图命令
        /// </summary>
        public ICommand HUHistogramCommand => new AsyncRelayCommand(async _ =>
        {
            this.Busy();

            //计算HU直方图
            uint[] histogram = await Task.Run(() => this.VolumeData.ApplyHistogram());
            Bitmap histImage = await Task.Run(() => histogram.GenerateHistogramImage(1440, 898));

            this.Idle();

            //打开窗口
            ImageViewModel viewModel = ResolveMediator.Resolve<ImageViewModel>();
            viewModel.Load(histImage);
            await this._windowManager.ShowWindowAsync(viewModel);
        }, _ => this.VolumeData != null);
        #endregion

        #region 归一化直方图命令 —— ICommand NormalizedHistogramCommand
        /// <summary>
        /// 归一化直方图命令
        /// </summary>
        public ICommand NormalizedHistogramCommand => new AsyncRelayCommand(async _ =>
        {
            this.Busy();

            //计算归一化直方图
            float[] histogram = await Task.Run(() => this.VolumeData.ApplyNormalizedHistogram());
            Bitmap histImage = await Task.Run(() => histogram.GenerateHistogramImage(1440, 898));

            this.Idle();

            //打开窗口
            ImageViewModel viewModel = ResolveMediator.Resolve<ImageViewModel>();
            viewModel.Load(histImage);
            await this._windowManager.ShowWindowAsync(viewModel);
        }, _ => this.VolumeData != null);
        #endregion

        #region 累积分布函数命令 —— ICommand CDFHistogramCommand
        /// <summary>
        /// 累积分布函数命令
        /// </summary>
        public ICommand CDFHistogramCommand => new AsyncRelayCommand(async _ =>
        {
            this.Busy();

            //计算累积分布函数
            float[] histogram = await Task.Run(() => this.VolumeData.ApplyCDF());
            Bitmap histImage = await Task.Run(() => histogram.GenerateHistogramImage(1440, 898));

            this.Idle();

            //打开窗口
            ImageViewModel viewModel = ResolveMediator.Resolve<ImageViewModel>();
            viewModel.Load(histImage);
            await this._windowManager.ShowWindowAsync(viewModel);
        }, _ => this.VolumeData != null);
        #endregion

        #region 直方图均衡化命令 —— ICommand HistogramEqualizationCommand
        /// <summary>
        /// 直方图均衡化命令
        /// </summary>
        public ICommand HistogramEqualizationCommand => new AsyncRelayCommand(async _ =>
        {
            VolumeSession volumeSession = SessionManager.VolumeSessions[this.VolumeData.Metadata.Id];

            this.Busy();

            //执行算法
            int bins = this.VolumeData.Metadata.MaxHU - this.VolumeData.Metadata.MinHU;
            float minHU = this.VolumeData.Metadata.MinHU;
            float maxHU = this.VolumeData.Metadata.MaxHU;
            await Task.Run(() => this.VolumeData.ApplyHistogramEqualization(bins, minHU, maxHU));

            this.Idle();

            //同步到预览纹理
            this.VolumeData.SyncPreviewDataToGpu(volumeSession.PreviewTexture);

            //发布消息
            SyncViewportEvent message = new SyncViewportEvent();
            await this._eventAggregator.PublishOnUIThreadAsync(message);
        }, _ => this.VolumeData != null);
        #endregion

        #region 形态学腐蚀命令 —— ICommand MorphErodeCommand
        /// <summary>
        /// 形态学腐蚀命令
        /// </summary>
        public ICommand MorphErodeCommand => new AsyncRelayCommand(async _ =>
        {
            MorphologyViewModel viewModel = ResolveMediator.Resolve<MorphologyViewModel>();
            viewModel.VolumeData = this.VolumeData;
            viewModel.MorphMode = MorphMode.Erode;
            await this._windowManager.ShowWindowAsync(viewModel);
        }, _ => this.VolumeData != null);
        #endregion

        #region 形态学膨胀命令 —— ICommand MorphDilateCommand
        /// <summary>
        /// 形态学膨胀命令
        /// </summary>
        public ICommand MorphDilateCommand => new AsyncRelayCommand(async _ =>
        {
            MorphologyViewModel viewModel = ResolveMediator.Resolve<MorphologyViewModel>();
            viewModel.VolumeData = this.VolumeData;
            viewModel.MorphMode = MorphMode.Dilate;
            await this._windowManager.ShowWindowAsync(viewModel);
        }, _ => this.VolumeData != null);
        #endregion

        #region 形态学开运算命令 —— ICommand MorphOpenCommand
        /// <summary>
        /// 形态学开运算命令
        /// </summary>
        public ICommand MorphOpenCommand => new AsyncRelayCommand(async _ =>
        {
            MorphologyViewModel viewModel = ResolveMediator.Resolve<MorphologyViewModel>();
            viewModel.VolumeData = this.VolumeData;
            viewModel.MorphMode = MorphMode.Open;
            await this._windowManager.ShowWindowAsync(viewModel);
        }, _ => this.VolumeData != null);
        #endregion

        #region 形态学闭运算命令 —— ICommand MorphCloseCommand
        /// <summary>
        /// 形态学闭运算命令
        /// </summary>
        public ICommand MorphCloseCommand => new AsyncRelayCommand(async _ =>
        {
            MorphologyViewModel viewModel = ResolveMediator.Resolve<MorphologyViewModel>();
            viewModel.VolumeData = this.VolumeData;
            viewModel.MorphMode = MorphMode.Close;
            await this._windowManager.ShowWindowAsync(viewModel);
        }, _ => this.VolumeData != null);
        #endregion

        #region 形态学礼帽运算命令 —— ICommand MorphTopHatCommand
        /// <summary>
        /// 形态学礼帽运算命令
        /// </summary>
        public ICommand MorphTopHatCommand => new AsyncRelayCommand(async _ =>
        {
            MorphologyViewModel viewModel = ResolveMediator.Resolve<MorphologyViewModel>();
            viewModel.VolumeData = this.VolumeData;
            viewModel.MorphMode = MorphMode.TopHat;
            await this._windowManager.ShowWindowAsync(viewModel);
        }, _ => this.VolumeData != null);
        #endregion

        #region 形态学黑帽运算命令 —— ICommand MorphBlackHatCommand
        /// <summary>
        /// 形态学黑帽运算命令
        /// </summary>
        public ICommand MorphBlackHatCommand => new AsyncRelayCommand(async _ =>
        {
            MorphologyViewModel viewModel = ResolveMediator.Resolve<MorphologyViewModel>();
            viewModel.VolumeData = this.VolumeData;
            viewModel.MorphMode = MorphMode.BlackHat;
            await this._windowManager.ShowWindowAsync(viewModel);
        }, _ => this.VolumeData != null);
        #endregion

        #region 形态学梯度运算命令 —— ICommand MorphGradientCommand
        /// <summary>
        /// 形态学梯度运算命令
        /// </summary>
        public ICommand MorphGradientCommand => new AsyncRelayCommand(async _ =>
        {
            MorphologyViewModel viewModel = ResolveMediator.Resolve<MorphologyViewModel>();
            viewModel.VolumeData = this.VolumeData;
            viewModel.MorphMode = MorphMode.Gradient;
            await this._windowManager.ShowWindowAsync(viewModel);
        }, _ => this.VolumeData != null);
        #endregion

        #region 高斯滤波命令 —— ICommand GaussianBlurCommand
        /// <summary>
        /// 高斯滤波命令
        /// </summary>
        public ICommand GaussianBlurCommand => new AsyncRelayCommand(async _ =>
        {
            GaussianBlurViewModel viewModel = ResolveMediator.Resolve<GaussianBlurViewModel>();
            viewModel.VolumeData = this.VolumeData;
            await this._windowManager.ShowWindowAsync(viewModel);
        }, _ => this.VolumeData != null);
        #endregion

        #region 均值滤波命令 —— ICommand MeanBlurCommand
        /// <summary>
        /// 均值滤波命令
        /// </summary>
        public ICommand MeanBlurCommand => new AsyncRelayCommand(async _ =>
        {
            MeanBlurViewModel viewModel = ResolveMediator.Resolve<MeanBlurViewModel>();
            viewModel.VolumeData = this.VolumeData;
            await this._windowManager.ShowWindowAsync(viewModel);
        }, _ => this.VolumeData != null);
        #endregion

        #region 中值滤波命令 —— ICommand MedianBlurCommand
        /// <summary>
        /// 中值滤波命令
        /// </summary>
        public ICommand MedianBlurCommand => new AsyncRelayCommand(async _ =>
        {
            MedianBlurViewModel viewModel = ResolveMediator.Resolve<MedianBlurViewModel>();
            viewModel.VolumeData = this.VolumeData;
            await this._windowManager.ShowWindowAsync(viewModel);
        }, _ => this.VolumeData != null);
        #endregion

        #region Sobel边缘检测命令 —— ICommand SobelCommand
        /// <summary>
        /// Sobel边缘检测命令
        /// </summary>
        public ICommand SobelCommand => new AsyncRelayCommand(async _ =>
        {
            SobelViewModel viewModel = ResolveMediator.Resolve<SobelViewModel>();
            viewModel.VolumeData = this.VolumeData;
            await this._windowManager.ShowWindowAsync(viewModel);
        }, _ => this.VolumeData != null);
        #endregion

        #region Scharr边缘检测命令 —— ICommand ScharrCommand
        /// <summary>
        /// Scharr边缘检测命令
        /// </summary>
        public ICommand ScharrCommand => new AsyncRelayCommand(async _ =>
        {
            ScharrViewModel viewModel = ResolveMediator.Resolve<ScharrViewModel>();
            viewModel.VolumeData = this.VolumeData;
            await this._windowManager.ShowWindowAsync(viewModel);
        }, _ => this.VolumeData != null);
        #endregion

        #region Canny边缘检测命令 —— ICommand CannyCommand
        /// <summary>
        /// Canny边缘检测命令
        /// </summary>
        public ICommand CannyCommand => new AsyncRelayCommand(async _ =>
        {
            CannyViewModel viewModel = ResolveMediator.Resolve<CannyViewModel>();
            viewModel.VolumeData = this.VolumeData;
            await this._windowManager.ShowWindowAsync(viewModel);
        }, _ => this.VolumeData != null);
        #endregion

        #region Laplacian边缘检测命令 —— ICommand LaplacianCommand
        /// <summary>
        /// Laplacian边缘检测命令
        /// </summary>
        public ICommand LaplacianCommand => new AsyncRelayCommand(async _ =>
        {
            LaplacianViewModel viewModel = ResolveMediator.Resolve<LaplacianViewModel>();
            viewModel.VolumeData = this.VolumeData;
            await this._windowManager.ShowWindowAsync(viewModel);
        }, _ => this.VolumeData != null);
        #endregion

        #region 阈值分割命令 —— ICommand ThresholdSegmentCommand
        /// <summary>
        /// 阈值分割命令
        /// </summary>
        public ICommand ThresholdSegmentCommand => new RelayCommand(_ =>
        {
            #region # 验证

            if (this.FunctionPanel is ThresholdSegmentViewModel)
            {
                return;
            }

            #endregion

            ThresholdSegmentViewModel viewModel = ResolveMediator.Resolve<ThresholdSegmentViewModel>();
            viewModel.VolumeData = this.VolumeData;
            viewModel.Tissues = this.Tissues;
            this.FunctionPanel = viewModel;
        }, _ => this.VolumeData != null);
        #endregion

        #region 区域生长分割命令 —— ICommand RegionGrowCommand
        /// <summary>
        /// 区域生长分割命令
        /// </summary>
        public ICommand RegionGrowCommand => new RelayCommand(_ =>
        {
            #region # 验证

            if (this.FunctionPanel is RegionGrowViewModel)
            {
                return;
            }

            #endregion

            RegionGrowViewModel viewModel = ResolveMediator.Resolve<RegionGrowViewModel>();
            viewModel.VolumeData = this.VolumeData;
            viewModel.SelectedTissue = this.SelectedTissue;
            viewModel.Tissues = this.Tissues;
            this.FunctionPanel = viewModel;
        }, _ => this.VolumeData != null);
        #endregion

        #endregion

        #region # 方法

        #region 初始化 —— override Task OnInitializedAsync(...
        /// <summary>
        /// 初始化事件
        /// </summary>
        protected override Task OnInitializedAsync(CancellationToken cancellationToken)
        {
            //预设窗
            this.WindowLevels =
            [
                WindowLevelManager.Brain,
                WindowLevelManager.Cardiac,
                WindowLevelManager.Liver,
                WindowLevelManager.Lung,
                WindowLevelManager.Abdomen,
                WindowLevelManager.Bone,
                WindowLevelManager.Vascular,
                WindowLevelManager.Mediastinum
            ];

            this.WindowWidth = 400;
            this.WindowCenter = 40;
            this.MprPlanesVisible = false;
            this.CrosshairVisible = true;
            this.SelectedTissue = this.Tissues[1];

            return base.OnInitializedAsync(cancellationToken);
        }
        #endregion

        #region 重命名形状 —— async Task RenameShape(ShapeVisual3D shape)
        /// <summary>
        /// 重命名形状
        /// </summary>
        /// <param name="shape">形状3D元素</param>
        public async Task RenameShape(ShapeVisual3D shape)
        {
            RenameViewModel viewModel = ResolveMediator.Resolve<RenameViewModel>();
            viewModel.ShapeName = shape.DisplayName;

            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                shape.DisplayName = viewModel.ShapeName;
            }
        }
        #endregion

        #region 删除形状 —— void RemoveShape(ShapeVisual3D shape)
        /// <summary>
        /// 删除形状
        /// </summary>
        /// <param name="shape">形状3D元素</param>
        public void RemoveShape(ShapeVisual3D shape)
        {
            this.Shapes.Remove(shape);
        }
        #endregion

        #region 关闭功能面板 —— void CloseFunctionPanel()
        /// <summary>
        /// 关闭功能面板
        /// </summary>
        public void CloseFunctionPanel()
        {
            #region # 验证

            if (this.FunctionPanel == null)
            {
                return;
            }

            #endregion

            this.FunctionPanel = null;

            //发布消息
            RestoreViewportCommandEvent message = new RestoreViewportCommandEvent
            {
                Publisher = this
            };
            this._eventAggregator.PublishOnUIThreadAsync(message);
        }
        #endregion

        #region 复制统计信息 —— void CopyStatistics()
        /// <summary>
        /// 复制统计信息
        /// </summary>
        public async Task CopyStatistics()
        {
            #region # 验证

            if (this.StatisticInfo == null)
            {
                return;
            }

            #endregion

            IndexView view = (IndexView)this.GetView();
            TopLevel topLevel = TopLevel.GetTopLevel(view);
            if (topLevel != null && topLevel.Clipboard != null)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"最小HU：{this.StatisticInfo.MinHU}");
                builder.AppendLine($"最大HU：{this.StatisticInfo.MaxHU}");
                builder.AppendLine($"平均HU：{this.StatisticInfo.AverageHU}");
                builder.AppendLine($"标准差：{this.StatisticInfo.StdDevHU}");
                builder.AppendLine($"周长：{this.StatisticInfo.Perimeter}");
                builder.AppendLine($"表面积：{this.StatisticInfo.SurfaceArea}");
                builder.AppendLine($"体积：{this.StatisticInfo.Volume}");
                builder.AppendLine($"体素数：{this.StatisticInfo.VoxelsCount}");

                await topLevel!.Clipboard!.SetTextAsync(builder.ToString());
                await MessageBox.Show("统计信息已复制到剪贴板！");
            }
        }
        #endregion

        #region 处理全局繁忙事件 —— Task HandleAsync(GlobalBusyEvent message...
        /// <summary>
        /// 处理全局繁忙事件
        /// </summary>
        public Task HandleAsync(GlobalBusyEvent message, CancellationToken cancellationToken)
        {
            #region # 验证

            if (message.Publisher == this)
            {
                return Task.CompletedTask;
            }

            #endregion

            if (message.IsBusy)
            {
                this.Busy();
            }
            else
            {
                this.Idle();
            }

            return Task.CompletedTask;
        }
        #endregion

        #region 处理同步视口事件 —— Task HandleAsync(SyncViewportEvent message...
        /// <summary>
        /// 处理同步视口事件
        /// </summary>
        public Task HandleAsync(SyncViewportEvent message, CancellationToken cancellationToken)
        {
            #region # 验证

            if (message.Publisher == this)
            {
                return Task.CompletedTask;
            }

            #endregion

            ShapeVisual3D selectedShape = this.Shapes.FirstOrDefault(shape => shape.IsSelected);
            this.SelectedShape = selectedShape;

            return Task.CompletedTask;
        }
        #endregion

        #region 处理统计完成事件 —— Task HandleAsync(StatisticFinishedEvent message...
        /// <summary>
        /// 处理统计完成事件
        /// </summary>
        public Task HandleAsync(StatisticFinishedEvent message, CancellationToken cancellationToken)
        {
            #region # 验证

            if (message.Publisher == this)
            {
                return Task.CompletedTask;
            }

            #endregion

            this.StatisticInfo = message.StatisticResult.ToStatisticInfo();

            return Task.CompletedTask;
        }
        #endregion

        #region 处理追加形状事件 —— Task HandleAsync(AppendShapeEvent message...
        /// <summary>
        /// 处理追加形状事件
        /// </summary>
        public Task HandleAsync(AppendShapeEvent message, CancellationToken cancellationToken)
        {
            #region # 验证

            if (message.Publisher == this)
            {
                return Task.CompletedTask;
            }

            #endregion

            this.Shapes.Add(message.Shape);

            //发布消息
            SyncViewportEvent syncMessage = new SyncViewportEvent
            {
                Publisher = this
            };
            this._eventAggregator.PublishOnUIThreadAsync(syncMessage, cancellationToken);

            return Task.CompletedTask;
        }
        #endregion

        #region 处理删除形状事件 —— Task HandleAsync(AppendShapeEvent message...
        /// <summary>
        /// 处理删除形状事件
        /// </summary>
        public Task HandleAsync(RemoveShapeEvent message, CancellationToken cancellationToken)
        {
            #region # 验证

            if (message.Publisher == this)
            {
                return Task.CompletedTask;
            }

            #endregion

            this.Shapes.Remove(message.Shape);

            return Task.CompletedTask;
        }
        #endregion

        #region 形状列表元素改变事件 —— void OnShapesItemChanged(object sender...
        /// <summary>
        /// 形状列表元素改变事件
        /// </summary>
        private void OnShapesItemChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            if (eventArgs.Action is NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Reset)
            {
                //发布消息
                SyncViewportEvent message = new SyncViewportEvent
                {
                    Publisher = this
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
            }
        }
        #endregion

        #region 失活事件 —— override Task OnDeactivateAsync(bool close...
        /// <summary>
        /// 失活事件
        /// </summary>
        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (close)
            {
                this._eventAggregator.Unsubscribe(this);
            }

            return base.OnDeactivateAsync(close, cancellationToken);
        }
        #endregion

        #endregion
    }
}
