using Caliburn.Micro;
using IconPacks.Avalonia.MaterialDesign;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using SD.Infrastructure.Avalonia.CustomControls;
using SD.Infrastructure.Avalonia.Enums;
using System.Threading.Tasks;

namespace MedicalSharp.Client.ViewModels.CommonContext
{
    /// <summary>
    /// 文本视图模型
    /// </summary>
    public class TextViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 事件聚合器
        /// </summary>
        private readonly IEventAggregator _eventAggregator;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public TextViewModel(IEventAggregator eventAggregator)
        {
            this._eventAggregator = eventAggregator;
        }

        #endregion

        #region # 属性

        #region 内容 —— string Content
        /// <summary>
        /// 内容
        /// </summary>
        [DependencyProperty]
        public string Content { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 提交 —— async Task Submit()
        /// <summary>
        /// 提交
        /// </summary>
        public async Task Submit()
        {
            #region # 验证

            if (string.IsNullOrWhiteSpace(this.Content))
            {
                await MessageBox.Show("内容不可为空！", "错误", MessageBoxButton.OK, PackIconMaterialDesignKind.Error);
                return;
            }

            #endregion

            await this.TryCloseAsync(true);
        }
        #endregion

        #endregion
    }
}
