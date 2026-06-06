using Avalonia.Collections;
using Avalonia.Media;
using Caliburn.Micro;
using MedicalSharp.Controls.UserControls;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using System.Threading;
using System.Threading.Tasks;

namespace MedicalSharp.Client.ViewModels.ProtocolContext
{
    /// <summary>
    /// 传递函数视图模型
    /// </summary>
    public class TransferFunctionViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗口管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public TransferFunctionViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

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
            this.ColorControlPoints =
            [
                new ColorControlPoint { HU = -1024, Color = Color.FromRgb(0, 0, 0) },
                new ColorControlPoint { HU = -800, Color = Color.FromRgb(46, 31, 38) },
                new ColorControlPoint { HU = -400, Color = Color.FromRgb(89, 56, 71) },
                new ColorControlPoint { HU = -200, Color = Color.FromRgb(77, 51, 64) },
                new ColorControlPoint { HU = 0, Color = Color.FromRgb(140, 64, 46) },
                new ColorControlPoint { HU = 100, Color = Color.FromRgb(173, 82, 56) },
                new ColorControlPoint { HU = 200, Color = Color.FromRgb(153, 51, 38) },
                new ColorControlPoint { HU = 400, Color = Color.FromRgb(184, 71, 51) },
                new ColorControlPoint { HU = 600, Color = Color.FromRgb(191, 102, 77) },
                new ColorControlPoint { HU = 800, Color = Color.FromRgb(224, 184, 128) },
                new ColorControlPoint { HU = 1200, Color = Color.FromRgb(242, 224, 184) },
                new ColorControlPoint { HU = 2000, Color = Color.FromRgb(255, 245, 224) },
                new ColorControlPoint { HU = 3071, Color = Color.FromRgb(255, 255, 255) }
            ];
            this.AlphaControlPoints =
            [
                new AlphaControlPoint { HU = -1024, Alpha = 0 },
                new AlphaControlPoint { HU = 0, Alpha = 0.1 },
                new AlphaControlPoint { HU = 500, Alpha = 0.5 },
                new AlphaControlPoint { HU = 3071, Alpha = 1 }
            ];

            return base.OnActivatedAsync(cancellationToken);
        }
        #endregion

        #endregion
    }
}
