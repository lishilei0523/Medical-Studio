using Caliburn.Micro;
using IconPacks.Avalonia.MaterialDesign;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using SD.Infrastructure.Avalonia.CustomControls;
using SD.Infrastructure.Avalonia.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace MedicalSharp.Client.ViewModels.ProtocolContext
{
    /// <summary>
    /// 保存协议视图模型
    /// </summary>
    public class SaveProtocolViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 事件聚合器
        /// </summary>
        private readonly IEventAggregator _eventAggregator;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public SaveProtocolViewModel(IEventAggregator eventAggregator)
        {
            this._eventAggregator = eventAggregator;
        }

        #endregion

        #region # 属性

        #region 协议名称 —— string ProtocolName
        /// <summary>
        /// 协议名称
        /// </summary>
        [DependencyProperty]
        public string ProtocolName { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 初始化 —— override Task OnActivatedAsync(CancellationToken cancellationToken)
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override Task OnActivatedAsync(CancellationToken cancellationToken)
        {
            //默认值
            this.ProtocolName = "Protocol_1";

            return base.OnActivatedAsync(cancellationToken);
        }
        #endregion

        #region 提交 —— async Task Submit()
        /// <summary>
        /// 提交
        /// </summary>
        public async Task Submit()
        {
            #region # 验证

            if (string.IsNullOrWhiteSpace(this.ProtocolName))
            {
                await MessageBox.Show("协议名称不可为空！", "错误", MessageBoxButton.OK, PackIconMaterialDesignKind.Error);
                return;
            }

            #endregion

            await this.TryCloseAsync(true);
        }
        #endregion

        #endregion
    }
}
