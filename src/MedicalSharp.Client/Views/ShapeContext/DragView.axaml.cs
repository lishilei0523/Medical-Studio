using FluentAvalonia.UI.Windowing;

namespace MedicalSharp.Client.Views.ShapeContext
{
    /// <summary>
    /// 拖拽视图
    /// </summary>
    public partial class DragView : AppWindow
    {
        /// <summary>
        /// 默认构造器
        /// </summary>
        public DragView()
        {
            this.InitializeComponent();
            this.TitleBar.ExtendsContentIntoTitleBar = true;
            this.TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
        }
    }
}
