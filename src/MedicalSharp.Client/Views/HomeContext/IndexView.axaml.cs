using FluentAvalonia.UI.Windowing;

namespace MedicalSharp.Client.Views.HomeContext
{
    /// <summary>
    /// 首页视图
    /// </summary>
    public partial class IndexView : AppWindow
    {
        /// <summary>
        /// 默认构造器
        /// </summary>
        public IndexView()
        {
            this.InitializeComponent();
            this.TitleBar.ExtendsContentIntoTitleBar = true;
            this.TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
        }
    }
}
