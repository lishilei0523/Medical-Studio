using Avalonia.Collections;
using Avalonia.Platform.Storage;
using Caliburn.Micro;
using MedicalSharp.Dicoms;
using MedicalSharp.Dicoms.Models;
using MedicalSharp.Engine.Cameras;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Engine.ValueTypes;
using OpenTK.Mathematics;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using SD.Infrastructure.Avalonia.Caliburn.Extensions;
using SD.Infrastructure.Avalonia.Commands;
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

        #region 轨道相机 —— OrbitCamera OrbitCamera
        /// <summary>
        /// 轨道相机
        /// </summary>
        [DependencyProperty]
        public OrbitCamera OrbitCamera { get; set; }
        #endregion

        #region MPR横断位相机 —— MPRCamera AxialCamera
        /// <summary>
        /// MPR横断位相机
        /// </summary>
        [DependencyProperty]
        public MPRCamera AxialCamera { get; set; }
        #endregion

        #region MPR冠状位相机 —— MPRCamera CoronalCamera
        /// <summary>
        /// MPR冠状位相机
        /// </summary>
        [DependencyProperty]
        public MPRCamera CoronalCamera { get; set; }
        #endregion

        #region MPR矢状位相机 —— MPRCamera SagittalCamera
        /// <summary>
        /// MPR矢状位相机
        /// </summary>
        [DependencyProperty]
        public MPRCamera SagittalCamera { get; set; }
        #endregion

        #region 体积数据 —— VolumeData VolumeData
        /// <summary>
        /// 体积数据
        /// </summary>
        [DependencyProperty]
        public VolumeData VolumeData { get; set; }
        #endregion

        #region 传输函数控制点列表 —— AvaloniaList<TFControlPoint> TFControlPoints
        /// <summary>
        /// 传输函数控制点列表
        /// </summary>
        [DependencyProperty]
        public AvaloniaList<TFControlPoint> TFControlPoints { get; set; }
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
            Vector3 targetPosition = new Vector3(0.0f);
            const float distance = 7.0f;
            const float yaw = 45.0f;
            const float pitch = -45.0f;
            this.OrbitCamera = new OrbitPerspectiveCamera(targetPosition, distance, yaw, pitch);
            this.AxialCamera = new MPRCamera(MPRPlaneType.Axial);
            this.CoronalCamera = new MPRCamera(MPRPlaneType.Coronal);
            this.SagittalCamera = new MPRCamera(MPRPlaneType.Sagittal);
            this.TFControlPoints = new AvaloniaList<TFControlPoint>(ResourceManager.GrayControlPoints);

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
            }

            this.Idle();
        }
        #endregion

        #endregion
    }
}
