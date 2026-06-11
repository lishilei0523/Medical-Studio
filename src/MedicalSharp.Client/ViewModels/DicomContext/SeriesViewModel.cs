using Caliburn.Micro;
using MedicalSharp.Client.ViewModels.HomeContext;
using SD.Infrastructure.Avalonia.Caliburn.Base;

namespace MedicalSharp.Client.ViewModels.DicomContext
{
    /// <summary>
    /// 序列信息视图模型
    /// </summary>
    public class SeriesViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 事件聚合器
        /// </summary>
        private readonly IEventAggregator _eventAggregator;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public SeriesViewModel(IEventAggregator eventAggregator)
        {
            this._eventAggregator = eventAggregator;
        }

        #endregion

        #region # 属性

        #region 首页视图模型 —— IndexViewModel IndexViewModel
        /// <summary>
        /// 首页视图模型
        /// </summary>
        public IndexViewModel IndexViewModel { get; set; }
        #endregion

        #endregion

        #region # 方法

        //

        #endregion
    }
}
