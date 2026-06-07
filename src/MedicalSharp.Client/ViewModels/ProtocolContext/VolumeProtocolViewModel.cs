using Avalonia.Collections;
using Caliburn.Micro;
using MedicalSharp.Client.ViewModels.VolumeContext;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.UserControls;
using MedicalSharp.Primitives.Managers;
using MedicalSharp.Primitives.Models;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using System.Collections.Generic;
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

        #region 已选控制点列表 —— {string, DensityControlPoint[]} SelectedControlPoints
        /// <summary>
        /// 已选控制点列表
        /// </summary>
        public KeyValuePair<string, DensityControlPoint[]> SelectedControlPoints
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
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
                }
            }
        }
        #endregion

        #region 预设控制点字典 —— IDictionary<string, DensityControlPoint[]> ControlPointGroups
        /// <summary>
        /// 预设控制点字典
        /// </summary>
        [DependencyProperty]
        public IDictionary<string, DensityControlPoint[]> ControlPointGroups { get; set; }
        #endregion

        #region 颜色控制点列表 —— AvaloniaList<ColorControlPoint> ColorControlPoints
        /// <summary>
        /// 颜色控制点列表
        /// </summary>
        [DependencyProperty]
        public AvaloniaList<ColorControlPoint> ColorControlPoints { get; set; }
        #endregion

        #region 透明度控制点列表 —— AvaloniaList<AlphaControlPoint> AlphaControlPoints
        /// <summary>
        /// 透明度控制点列表
        /// </summary>
        [DependencyProperty]
        public AvaloniaList<AlphaControlPoint> AlphaControlPoints { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 初始化 —— override Task OnActivatedAsync(CancellationToken cancellationToken)
        /// <summary>
        /// 初始化
        /// </summary>
        protected override Task OnActivatedAsync(CancellationToken cancellationToken)
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

            return base.OnActivatedAsync(cancellationToken);
        }
        #endregion

        #endregion
    }
}
