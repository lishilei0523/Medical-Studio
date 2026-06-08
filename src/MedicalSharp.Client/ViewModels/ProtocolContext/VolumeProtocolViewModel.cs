using Avalonia.Collections;
using Avalonia.Media;
using Caliburn.Micro;
using MedicalSharp.Client.ViewModels.VolumeContext;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.UserControls;
using MedicalSharp.Engine.Algorithms;
using MedicalSharp.Presentation.Maps;
using MedicalSharp.Presentation.Models;
using MedicalSharp.Primitives;
using MedicalSharp.Primitives.Managers;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using SD.Infrastructure.Avalonia.CustomControls;
using SD.IOC.Core.Mediators;
using SD.Toolkits.Json;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MedicalSharp.Client.ViewModels.ProtocolContext
{
    /// <summary>
    /// 体积渲染协议视图模型
    /// </summary>
    public class VolumeProtocolViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗口管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public VolumeProtocolViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 体积渲染视图模型 —— VolumeViewModel VolumeViewModel
        /// <summary>
        /// 体积渲染视图模型
        /// </summary>
        public VolumeViewModel VolumeViewModel { get; set; }
        #endregion

        #region Alpha控制点列表 —— AvaloniaList<AlphaControlPoint> AlphaControlPoints
        /// <summary>
        /// Alpha控制点列表
        /// </summary>
        [DependencyProperty]
        public AvaloniaList<AlphaControlPoint> AlphaControlPoints { get; set; }
        #endregion

        #region 颜色控制点列表 —— AvaloniaList<ColorControlPoint> ColorControlPoints
        /// <summary>
        /// 颜色控制点列表
        /// </summary>
        [DependencyProperty]
        public AvaloniaList<ColorControlPoint> ColorControlPoints { get; set; }
        #endregion

        #region 归一化直方图 —— float[] NormalizedHistogram
        /// <summary>
        /// 归一化直方图
        /// </summary>
        [DependencyProperty]
        public float[] NormalizedHistogram { get; set; }
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
                    this.VolumeViewModel?.WindowWidth = value.WindowWidth;
                    this.VolumeViewModel?.WindowCenter = value.WindowCenter;
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

        #region 已选传递函数控制点列表 —— {string, DensityControlPoint[]} SelectedControlPoints
        /// <summary>
        /// 已选传递函数控制点列表
        /// </summary>
        public KeyValuePair<string, DensityControlPoint[]> SelectedControlPoints
        {
            get;
            set
            {
                //注销旧事件
                if (field.Value != null && this.AlphaControlPoints != null && this.ColorControlPoints != null)
                {
                    //注销控制点列表事件
                    this.AlphaControlPoints.CollectionChanged -= this.OnAlphaControlPointsCollectionChanged;
                    this.ColorControlPoints.CollectionChanged -= this.OnColorControlPointsCollectionChanged;

                    //注销控制点属性事件
                    foreach (AlphaControlPoint controlPoint in this.AlphaControlPoints)
                    {
                        controlPoint.PropertyChanged -= this.OnControlPointPropertyChanged;
                    }
                    foreach (ColorControlPoint controlPoint in this.ColorControlPoints)
                    {
                        controlPoint.PropertyChanged -= this.OnControlPointPropertyChanged;
                    }
                }

                field = value;
                this.NotifyOfPropertyChange();

                //注册新事件
                if (value.Value != null)
                {
                    this.VolumeViewModel?.TFControlPoints = new AvaloniaList<DensityControlPoint>(value.Value);

                    IEnumerable<AlphaControlPoint> alphaControlPoints = value.Value.Select(x => new AlphaControlPoint
                    {
                        HU = ProtocolManager.DensityToHU(x.Position),
                        Alpha = x.Color.W
                    });
                    IEnumerable<ColorControlPoint> colorControlPoints = value.Value.Select(x => new ColorControlPoint
                    {
                        HU = ProtocolManager.DensityToHU(x.Position),
                        Color = x.Color.ToSolidColor()
                    });
                    this.AlphaControlPoints = new AvaloniaList<AlphaControlPoint>(alphaControlPoints);
                    this.ColorControlPoints = new AvaloniaList<ColorControlPoint>(colorControlPoints);

                    //注册控制点列表事件
                    this.AlphaControlPoints.CollectionChanged += this.OnAlphaControlPointsCollectionChanged;
                    this.ColorControlPoints.CollectionChanged += this.OnColorControlPointsCollectionChanged;

                    //注册控制点属性事件
                    foreach (AlphaControlPoint point in this.AlphaControlPoints)
                    {
                        point.PropertyChanged += this.OnControlPointPropertyChanged;
                    }
                    foreach (ColorControlPoint point in this.ColorControlPoints)
                    {
                        point.PropertyChanged += this.OnControlPointPropertyChanged;
                    }
                }
            }
        }
        #endregion

        #region 预设传递函数控制点字典 —— IDictionary<string, DensityControlPoint[]> ControlPointGroups
        /// <summary>
        /// 预设传递函数控制点字典
        /// </summary>
        [DependencyProperty]
        public IDictionary<string, DensityControlPoint[]> ControlPointGroups { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 初始化 —— override async Task OnActivatedAsync(CancellationToken cancellationToken)
        /// <summary>
        /// 初始化
        /// </summary>
        protected override async Task OnActivatedAsync(CancellationToken cancellationToken)
        {
            //默认值
            this.WindowLevels =
            [
                WindowLevelManager.Default,
                WindowLevelManager.Brain,
                WindowLevelManager.Cardiac,
                WindowLevelManager.Liver,
                WindowLevelManager.Lung,
                WindowLevelManager.Abdomen,
                WindowLevelManager.Bone,
                WindowLevelManager.Vascular,
                WindowLevelManager.Mediastinum
            ];
            this.SelectedWindowLevel = WindowLevelManager.Default;

            this.ControlPointGroups = new Dictionary<string, DensityControlPoint[]>();
            KeyValuePair<string, DensityControlPoint[]> defaultControlPoints = new("默认", [.. this.VolumeViewModel.TFControlPoints]);
            this.ControlPointGroups.Add(defaultControlPoints);
            this.SelectedControlPoints = defaultControlPoints;
            foreach (KeyValuePair<string, DensityControlPoint[]> controlPoints in ProtocolManager.PresetControlPointGroups)
            {
                this.ControlPointGroups.Add(controlPoints);
            }

            this.NormalizedHistogram = await Task.Run(() => this.VolumeViewModel.VolumeData.ApplyNormalizedHistogram(), cancellationToken);

            await base.OnActivatedAsync(cancellationToken);
        }
        #endregion

        #region 保存协议 —— async Task SaveProtocol()
        /// <summary>
        /// 保存协议
        /// </summary>
        public async Task SaveProtocol()
        {
            SaveProtocolViewModel viewModel = ResolveMediator.Resolve<SaveProtocolViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                RaycastProtocol protocol = new RaycastProtocol
                {
                    Name = viewModel.ProtocolName,
                    WindowWidth = this.VolumeViewModel.WindowWidth,
                    WindowCenter = this.VolumeViewModel.WindowCenter,
                    Brightness = this.VolumeViewModel.Brightness,
                    DensityScale = this.VolumeViewModel.DensityScale,
                    StepSize = this.VolumeViewModel.StepSize,
                    MaxStepsCount = this.VolumeViewModel.MaxStepsCount,
                    OpacityThreshold = this.VolumeViewModel.OpacityThreshold,
                    ControlPoints = this.VolumeViewModel.TFControlPoints.Select(x => x.ToRaycastProtocolPoint()).ToList()
                };

                //保存文件
                string path = $"{Constants.ProtocolPath}/{protocol.Name}.json";
                string json = protocol.ToJson();
                await File.WriteAllTextAsync(path, json);

                //重新初始化
                await this.VolumeViewModel.InitPresetProtocols();

                await MessageBox.Show("协议已保存！", "成功");
            }
        }
        #endregion

        #region 同步到体积渲染视图模型 —— void SyncToVolumeViewModel()
        /// <summary>
        /// 同步到体积渲染视图模型
        /// </summary>
        private void SyncToVolumeViewModel()
        {
            #region # 验证

            if (this.AlphaControlPoints == null || this.ColorControlPoints == null)
            {
                return;
            }
            if (!this.AlphaControlPoints.Any() || !this.ColorControlPoints.Any())
            {
                return;
            }

            #endregion

            //按HU值排序的Alpha控制点
            List<AlphaControlPoint> sortedAlphas = this.AlphaControlPoints.OrderBy(x => x.HU).ToList();
            List<ColorControlPoint> sortedColors = this.ColorControlPoints.OrderBy(x => x.HU).ToList();
            List<DensityControlPoint> densityControlPoints = [];
            foreach (AlphaControlPoint alphaControlPoint in sortedAlphas)
            {
                //在颜色控制点中插值该HU位置的颜色
                Color interpolatedColor = ColorControlPoint.InterpolateColor(sortedColors, alphaControlPoint.HU);
                Vector4 color = new Vector4(
                    interpolatedColor.R / 255f,
                    interpolatedColor.G / 255f,
                    interpolatedColor.B / 255f,
                    alphaControlPoint.Alpha);

                float density = ProtocolManager.HUToDensity(alphaControlPoint.HU);
                DensityControlPoint densityControlPoint = new DensityControlPoint(density, color);
                densityControlPoints.Add(densityControlPoint);
            }

            this.VolumeViewModel.TFControlPoints = new AvaloniaList<DensityControlPoint>(densityControlPoints);
        }
        #endregion

        #region Alpha控制点列表元素改变事件 —— void OnAlphaControlPointsCollectionChanged(object sender...
        /// <summary>
        /// Alpha控制点列表元素改变事件
        /// </summary>
        private void OnAlphaControlPointsCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            if (eventArgs.OldItems != null)
            {
                foreach (AlphaControlPoint point in eventArgs.OldItems)
                {
                    point.PropertyChanged -= this.OnControlPointPropertyChanged;
                }
            }
            if (eventArgs.NewItems != null)
            {
                foreach (AlphaControlPoint point in eventArgs.NewItems)
                {
                    point.PropertyChanged += this.OnControlPointPropertyChanged;
                }
            }

            this.SyncToVolumeViewModel();
        }
        #endregion

        #region 颜色控制点列表元素改变事件 —— void OnColorControlPointsCollectionChanged(object sender...
        /// <summary>
        /// 颜色控制点列表元素改变事件
        /// </summary>
        private void OnColorControlPointsCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            if (eventArgs.OldItems != null)
            {
                foreach (ColorControlPoint point in eventArgs.OldItems)
                {
                    point.PropertyChanged -= this.OnControlPointPropertyChanged;
                }
            }
            if (eventArgs.NewItems != null)
            {
                foreach (ColorControlPoint point in eventArgs.NewItems)
                {
                    point.PropertyChanged += this.OnControlPointPropertyChanged;
                }
            }

            this.SyncToVolumeViewModel();
        }
        #endregion

        #region Alpha/颜色控制点属性改变事件 —— void OnControlPointPropertyChanged(object sender...
        /// <summary>
        /// Alpha/颜色控制点属性改变事件
        /// </summary>
        private void OnControlPointPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            this.SyncToVolumeViewModel();
        }
        #endregion

        #endregion
    }
}
