using FluentAvalonia.UI.Windowing;

namespace MedicalSharp.Client.Views.ShapeContext
{
    /// <summary>
    /// 绘制视图
    /// </summary>
    public partial class DrawView : AppWindow
    {
        /// <summary>
        /// 默认构造器
        /// </summary>
        public DrawView()
        {
            this.InitializeComponent();
            this.TitleBar.ExtendsContentIntoTitleBar = true;
            this.TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
        }
    }
}
