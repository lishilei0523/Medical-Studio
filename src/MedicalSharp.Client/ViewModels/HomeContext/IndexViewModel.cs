using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Caliburn.Micro;
using FluentAvalonia.UI.Controls;
using MedicalSharp.Client.ViewModels.LayoutContext;
using MedicalSharp.Client.ViewModels.TissueContext;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Engine.Algorithms;
using MedicalSharp.Engine.Managers;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Inspiration.Algorithms;
using MedicalSharp.Inspiration.Managers;
using MedicalSharp.Inspiration.Resources;
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
using Silk.NET.OpenCL;
using System.Collections.Generic;
using System.Diagnostics;
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
                new TissueInfo("皮肤", 1, MarkMode.Collapsed, colors[1].ToColor()),
                new TissueInfo("骨骼", 2, MarkMode.Tinted, colors[2].ToColor()),
                new TissueInfo("血管", 3, MarkMode.Tinted, colors[3].ToColor()),
                new TissueInfo("软组织", 4, MarkMode.Visible, colors[4].ToColor()),
                new TissueInfo("心脏", 5, MarkMode.Visible, colors[5].ToColor()),
                new TissueInfo("肺", 6, MarkMode.Visible, colors[6].ToColor()),
                new TissueInfo("肝脏", 7, MarkMode.Tinted, colors[7].ToColor()),
                new TissueInfo("肾脏", 8, MarkMode.Tinted, colors[8].ToColor()),
                new TissueInfo("脾脏", 9, MarkMode.Collapsed, colors[9].ToColor()),
                new TissueInfo("病变", 10, MarkMode.Tinted, colors[10].ToColor()),
                new TissueInfo("钙化", 11, MarkMode.Tinted, colors[11].ToColor()),
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

        #region 重置预览命令 —— ICommand ResetPreviewCommand
        /// <summary>
        /// 重置预览命令
        /// </summary>
        public ICommand ResetPreviewCommand => new RelayCommand(_ =>
        {
            #region # 验证

            if (this.VolumeData == null)
            {
                return;
            }

            #endregion

            VolumeSession volumeSession = SessionManager.VolumeSessions[this.VolumeData.Metadata.Id];
            volumeSession.ResetPreviewTexture();

            //发布消息
            SyncViewportEvent message = new SyncViewportEvent();
            this._eventAggregator.PublishOnUIThreadAsync(message);
        });
        #endregion

        #region 重置标记命令 —— ICommand ResetMarkCommand
        /// <summary>
        /// 重置标记命令
        /// </summary>
        public ICommand ResetMarkCommand => new RelayCommand(_ =>
        {
            #region # 验证

            if (this.VolumeData == null)
            {
                return;
            }

            #endregion

            VolumeSession volumeSession = SessionManager.VolumeSessions[this.VolumeData.Metadata.Id];
            volumeSession.ResetMarkTexture();

            //发布消息
            SyncViewportEvent message = new SyncViewportEvent();
            this._eventAggregator.PublishOnUIThreadAsync(message);
        });
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
                TissueInfo tissue = new TissueInfo(viewModel.TissueName, viewModel.MarkValue!.Value, viewModel.MarkMode, viewModel.TissueColor);
                this.Tissues.Add(tissue);
            }
        });
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
                this.SelectedTissue.Name = viewModel.TissueName;
                this.SelectedTissue.MarkValue = viewModel.MarkValue!.Value;
                this.SelectedTissue.Color = viewModel.TissueColor;
                this.SelectedTissue.SelectedMarkMode = new KeyValuePair<string, string>(viewModel.MarkMode.ToString(), viewModel.MarkMode.GetEnumMember());
            }

        }, _ => this.SelectedTissue != null && this.SelectedTissue.MarkValue != 0);
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

        }, _ => this.SelectedTissue != null && this.SelectedTissue.MarkValue != 0);
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

        }, _ => this.SelectedTissue != null && this.SelectedTissue.MarkValue != 0);
        #endregion

        #region 高斯滤波命令 —— ICommand GaussianBlurCommand
        /// <summary>
        /// 高斯滤波命令
        /// </summary>
        public ICommand GaussianBlurCommand => new AsyncRelayCommand(async _ =>
        {
            #region # 验证

            if (this.VolumeData == null)
            {
                return;
            }

            #endregion

            VolumeSession volumeSession = SessionManager.VolumeSessions[this.VolumeData.Metadata.Id];
            ClContext clContext = ClContextManager.Current;

            //TODO 参数化
            int width = this.VolumeData.Metadata.VolumeSize.X;
            int height = this.VolumeData.Metadata.VolumeSize.Y;
            int depth = this.VolumeData.Metadata.VolumeSize.Z;

            Stopwatch stopwatch = new Stopwatch();
            Stopwatch stopwatch1 = new Stopwatch();
            Stopwatch stopwatch2 = new Stopwatch();
            Stopwatch stopwatch3 = new Stopwatch();
            Stopwatch stopwatch4 = new Stopwatch();
            stopwatch.Start();

            this.Busy();
            await Task.Run(() =>
            {
                //创建并写入图像
                stopwatch1.Start();
                using ClImage3D image = ClImage3D.Create(clContext, width, height, depth, MemFlags.ReadWrite, ChannelOrder.Intensity, ChannelType.SNormInt16);
                image.Write(clContext.CommandQueue, this.VolumeData.OriginalData);
                clContext.Finish();
                stopwatch1.Stop();
                Trace.WriteLine($"创建并写入图像耗时：{stopwatch1.Elapsed}");

                //执行算法
                stopwatch2.Start();
                using GaussianBlur3D gaussianBlur = new GaussianBlur3D(clContext);
                gaussianBlur.ExecuteInPlace(image);
                stopwatch2.Stop();
                Trace.WriteLine($"高斯滤波算法耗时：{stopwatch2.Elapsed}");

                //读回CPU
                stopwatch3.Start();
                image.Read(clContext.CommandQueue, this.VolumeData.PreviewData);
                stopwatch3.Stop();
                Trace.WriteLine($"回读CPU耗时：{stopwatch3.Elapsed}");

            });
            this.Idle();

            //同步到预览纹理
            stopwatch4.Start();
            SyncAlgorithms.SyncPreviewDataToGpu(this.VolumeData, volumeSession.PreviewTexture);
            stopwatch4.Stop();
            Trace.WriteLine($"同步纹理耗时：{stopwatch4.Elapsed}");

            stopwatch.Stop();
            Trace.WriteLine($"总耗时：{stopwatch.Elapsed}");

            //发布消息
            SyncViewportEvent message = new SyncViewportEvent();
            await this._eventAggregator.PublishOnUIThreadAsync(message);
        });
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
            this.VolumeData = null;
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
