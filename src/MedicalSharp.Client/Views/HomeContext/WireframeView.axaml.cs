using FluentAvalonia.UI.Windowing;

namespace MedicalSharp.Client.Views.HomeContext
{
    /// <summary>
    /// 线框视图
    /// </summary>
    public partial class WireframeView : AppWindow
    {
        /// <summary>
        /// 默认构造器
        /// </summary>
        public WireframeView()
        {
            this.InitializeComponent();
            this.TitleBar.ExtendsContentIntoTitleBar = true;
            this.TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
        }
    }
}
