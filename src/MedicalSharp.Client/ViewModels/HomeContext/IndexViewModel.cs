using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Caliburn.Micro;
using FluentAvalonia.UI.Controls;
using MedicalSharp.Client.ViewModels.AlgorithmContext;
using MedicalSharp.Client.ViewModels.LayoutContext;
using MedicalSharp.Client.ViewModels.TissueContext;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Engine.Algorithms;
using MedicalSharp.Engine.Managers;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Presentation.Events;
using MedicalSharp.Presentation.Maps;
using MedicalSharp.Presentation.Models;
using MedicalSharp.Primitives.Builders;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Interfaces;
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
            this.LayoutViewModel = ResolveMediator.Resolve<LayoutViewModel>();

            //默认值
            this.MarkModes = typeof(MarkMode).GetEnumMembers();

            Vector4[] colors = ColorFactory.StandardMarkColors;
            this.Tissues =
            [
                new TissueInfo("Base", 0, MarkMode.Visible, Colors.Transparent, true),
                new TissueInfo("骨骼", 1, MarkMode.Tinted, colors[1].ToColor()),
                new TissueInfo("血管", 2, MarkMode.Tinted, colors[2].ToColor()),
                new TissueInfo("软组织", 3, MarkMode.Visible, colors[3].ToColor()),
                new TissueInfo("心脏", 4, MarkMode.Visible, colors[4].ToColor()),
                new TissueInfo("肺", 5, MarkMode.Visible, colors[5].ToColor()),
                new TissueInfo("肝脏", 6, MarkMode.Tinted, colors[6].ToColor()),
                new TissueInfo("肾脏", 7, MarkMode.Tinted, colors[7].ToColor()),
                new TissueInfo("脾脏", 8, MarkMode.Collapsed, colors[8].ToColor()),
                new TissueInfo("病变", 9, MarkMode.Tinted, colors[9].ToColor()),
                new TissueInfo("钙化", 10, MarkMode.Tinted, colors[10].ToColor()),
            ];
        }

        #endregion

        #region # 属性

        //属性

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
                this.LayoutViewModel.SetVolumeData(value);
                if (value != null)
                {
                    this.VolumeInfo = value.Metadata.ToVolumeInfo();
                    this.PatientInfo = value.PatientData.ToPatientInfo();
                    this.StudyInfo = value.StudyData.ToStudyInfo();
                    this.ScanInfo = value.ScanData.ToScanInfo();

                    //初始化标记策略
                    VolumeSession session = SessionManager.VolumeSessions[value.Metadata.Id];
                    foreach (TissueInfo tissue in this.Tissues)
                    {
                        session.MarkStrategy.SwitchMarkMode(tissue.MarkValue, tissue.MarkMode);
                    }
                }
                else
                {
                    this.VolumeInfo = null;
                    this.PatientInfo = null;
                    this.StudyInfo = null;
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

        #region 布局视图模型 —— LayoutViewModel LayoutViewModel
        /// <summary>
        /// 布局视图模型
        /// </summary>
        [DependencyProperty]
        public LayoutViewModel LayoutViewModel { get; set; }
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
                TissueSelectedEvent message = new TissueSelectedEvent
                {
                    TissueInfo = value
                };
                this._eventAggregator.PublishOnUIThreadAsync(message);
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
                SyncAlgorithms.SyncPreviewDataToGpu(this.VolumeData, session.PreviewTexture);

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
                SyncAlgorithms.SyncPreviewDataToGpu(this.VolumeData, session.PreviewTexture);

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
                SyncAlgorithms.SyncMarkDataToGpu(this.VolumeData, session.MarkTexture);

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
                SyncAlgorithms.SyncMarkDataToGpu(this.VolumeData, session.MarkTexture);

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
                ClearShapesEvent message = new ClearShapesEvent();
                await this._eventAggregator.PublishOnUIThreadAsync(message);
            }
        }, _ => this.VolumeData != null);
        #endregion

        #region 布局13命令 —— ICommand Layout13Command
        /// <summary>
        /// 布局13命令
        /// </summary>
        public ICommand Layout13Command => new RelayCommand(_ =>
        {
            //#region # 验证

            //if (this.LayoutViewModel is Layout13ViewModel)
            //{
            //    return;
            //}

            //#endregion

            //this.LayoutViewModel = ResolveMediator.Resolve<Layout13ViewModel>();
        });
        #endregion

        #region 布局22命令 —— ICommand Layout22Command
        /// <summary>
        /// 布局22命令
        /// </summary>
        public ICommand Layout22Command => new RelayCommand(_ =>
        {
            //#region # 验证

            //if (this.LayoutViewModel is LayoutViewModel)
            //{
            //    return;
            //}

            //#endregion

            //this.LayoutViewModel = ResolveMediator.Resolve<LayoutViewModel>();
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
                session.ResetMarkValue(this.SelectedTissue.MarkValue);

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
                session.ResetMarkValue(this.SelectedTissue.MarkValue);

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

        #region 阈值分割命令 —— ICommand ThresholdSegmentCommand
        /// <summary>
        /// 阈值分割命令
        /// </summary>
        public ICommand ThresholdSegmentCommand => new AsyncRelayCommand(async _ =>
        {
            ThresholdSegmentViewModel viewModel = ResolveMediator.Resolve<ThresholdSegmentViewModel>();
            viewModel.VolumeData = this.VolumeData;
            viewModel.SelectedTissue = this.SelectedTissue;
            viewModel.Tissues = this.Tissues;
            await this._windowManager.ShowWindowAsync(viewModel);
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

        #endregion

        #region # 方法

        #region 初始化 —— override Task OnInitializedAsync(...
        /// <summary>
        /// 初始化事件
        /// </summary>
        protected override Task OnInitializedAsync(CancellationToken cancellationToken)
        {
            this.SelectedTissue = this.Tissues[1];

            return base.OnInitializedAsync(cancellationToken);
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
