using Avalonia.Collections;
using Caliburn.Micro;
using MedicalSharp.Primitives.Models;
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

        #region 控制点列表 —— AvaloniaList<AlphaControlPoint> ControlPoints
        /// <summary>
        /// 控制点列表
        /// </summary>
        [DependencyProperty]
        public AvaloniaList<AlphaControlPoint> ControlPoints { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 初始化 —— override Task OnActivatedAsync(CancellationToken cancellationToken)
        /// <summary>
        /// 初始化
        /// </summary>
        protected override Task OnActivatedAsync(CancellationToken cancellationToken)
        {
            this.ControlPoints =
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
