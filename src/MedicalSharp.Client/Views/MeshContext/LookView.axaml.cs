using FluentAvalonia.UI.Windowing;

namespace MedicalSharp.Client.Views.MeshContext
{
    /// <summary>
    /// 查看视图
    /// </summary>
    public partial class LookView : AppWindow
    {
        /// <summary>
        /// 默认构造器
        /// </summary>
        public LookView()
        {
            this.InitializeComponent();
            this.TitleBar.ExtendsContentIntoTitleBar = true;
            this.TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
        }
    }
}
