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
    /// MPR渲染协议视图模型
    /// </summary>
    public class MprProtocolViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗口管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public MprProtocolViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region MPR视图模型 —— MprViewModel MprViewModel
        /// <summary>
        /// MPR视图模型
        /// </summary>
        public MprViewModel MprViewModel { get; set; }
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
                    this.MprViewModel?.WindowWidth = value.WindowWidth;
                    this.MprViewModel?.WindowCenter = value.WindowCenter;
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

        #region 已选传递函数控制点列表 —— {string, HUControlPoint[]} SelectedControlPoints
        /// <summary>
        /// 已选传递函数控制点列表
        /// </summary>
        public KeyValuePair<string, HUControlPoint[]> SelectedControlPoints
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
                    this.MprViewModel?.TFControlPoints = new AvaloniaList<HUControlPoint>(value.Value);

                    IEnumerable<AlphaControlPoint> alphaControlPoints = value.Value.Select(x => new AlphaControlPoint
                    {
                        HU = x.HU,
                        Alpha = x.Color.W
                    });
                    IEnumerable<ColorControlPoint> colorControlPoints = value.Value.Select(x => new ColorControlPoint
                    {
                        HU = x.HU,
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

        #region 预设传递函数控制点字典 —— IDictionary<string, HUControlPoint[]> ControlPointGroups
        /// <summary>
        /// 预设传递函数控制点字典
        /// </summary>
        [DependencyProperty]
        public IDictionary<string, HUControlPoint[]> ControlPointGroups { get; set; }
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

            this.ControlPointGroups = new Dictionary<string, HUControlPoint[]>();
            KeyValuePair<string, HUControlPoint[]> defaultControlPoints = new("默认", [.. this.MprViewModel.TFControlPoints]);
            this.ControlPointGroups.Add(defaultControlPoints);
            this.SelectedControlPoints = defaultControlPoints;
            foreach (KeyValuePair<string, HUControlPoint[]> controlPoints in ProtocolManager.PresetHUControlPointGroups)
            {
                this.ControlPointGroups.Add(controlPoints);
            }

            this.NormalizedHistogram = await Task.Run(() => this.MprViewModel.VolumeData.ApplyNormalizedHistogram(), cancellationToken);

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
            viewModel.ProtocolName = this.MprViewModel.SelectedProtocol?.Name;
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                MprProtocol protocol = new MprProtocol
                {
                    Name = viewModel.ProtocolName,
                    WindowWidth = this.MprViewModel.WindowWidth,
                    WindowCenter = this.MprViewModel.WindowCenter,
                    Brightness = this.MprViewModel.Brightness,
                    Contrast = this.MprViewModel.Contrast,
                    ControlPoints = this.MprViewModel.TFControlPoints.Select(x => x.ToMprProtocolPoint()).ToList()
                };

                //保存文件
                string path = $"{Constants.MPRProtocolPath}/{protocol.Name}.json";
                string json = protocol.ToJson();
                await File.WriteAllTextAsync(path, json);

                //重新初始化
                await this.MprViewModel.InitPresetProtocols();

                await MessageBox.Show("协议已保存！", "成功");
            }
        }
        #endregion

        #region 同步到MPR渲染视图模型 —— void SyncToMprViewModel()
        /// <summary>
        /// 同步到MPR渲染视图模型
        /// </summary>
        private void SyncToMprViewModel()
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
            List<HUControlPoint> huControlPoints = [];
            foreach (AlphaControlPoint alphaControlPoint in sortedAlphas)
            {
                //在颜色控制点中插值该HU位置的颜色
                Color interpolatedColor = ColorControlPoint.InterpolateColor(sortedColors, alphaControlPoint.HU);
                Vector4 color = new Vector4(
                    interpolatedColor.R / 255f,
                    interpolatedColor.G / 255f,
                    interpolatedColor.B / 255f,
                    alphaControlPoint.Alpha);

                HUControlPoint huControlPoint = new HUControlPoint(alphaControlPoint.HU, color);
                huControlPoints.Add(huControlPoint);
            }

            this.MprViewModel.TFControlPoints = new AvaloniaList<HUControlPoint>(huControlPoints);
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

            this.SyncToMprViewModel();
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

            this.SyncToMprViewModel();
        }
        #endregion

        #region Alpha/颜色控制点属性改变事件 —— void OnControlPointPropertyChanged(object sender...
        /// <summary>
        /// Alpha/颜色控制点属性改变事件
        /// </summary>
        private void OnControlPointPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            this.SyncToMprViewModel();
        }
        #endregion

        #endregion
    }
}
