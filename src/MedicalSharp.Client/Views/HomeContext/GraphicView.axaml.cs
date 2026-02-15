using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Rendering.Composition;
using FluentAvalonia.UI.Windowing;

namespace MedicalSharp.Client.Views.HomeContext
{
    public partial class GraphicView : AppWindow
    {
        public GraphicView()
        {
            this.InitializeComponent();
        }

        private async void SnapshotClick(object sender, RoutedEventArgs eventArgs)
        {
            CompositionVisual visual = ElementComposition.GetElementVisual(this);
            Bitmap snapshot = await visual!.Compositor.CreateCompositionVisualSnapshot(visual, 1);
            await new Window()
            {
                Width = this.Width,
                Height = this.Height,
                Content = new ScrollViewer()
                {
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Content = new Image()
                    {
                        Source = snapshot
                    }
                }
            }.ShowDialog((Window)TopLevel.GetTopLevel(this)!);
        }
    }
}
