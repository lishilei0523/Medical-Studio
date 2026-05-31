using Avalonia;
using Avalonia.Media;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Engine.Overlays;

namespace MedicalSharp.Controls.Visual2Ds
{
    /// <summary>
    /// 文本Overlay元素
    /// </summary>
    public class TextOverlay2D : ShapeOverlay2D
    {
        #region # 字段及构造器

        /// <summary>
        /// 文本内容依赖属性
        /// </summary>
        public static readonly StyledProperty<string> TextProperty;

        /// <summary>
        /// 字体大小依赖属性
        /// </summary>
        public static readonly StyledProperty<float> FontSizeProperty;

        /// <summary>
        /// 文本颜色依赖属性
        /// </summary>
        public static readonly StyledProperty<Color> ColorProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static TextOverlay2D()
        {
            TextProperty = AvaloniaProperty.Register<TextOverlay2D, string>(nameof(Text));
            FontSizeProperty = AvaloniaProperty.Register<TextOverlay2D, float>(nameof(FontSize), 14.0f);
            ColorProperty = AvaloniaProperty.Register<TextOverlay2D, Color>(nameof(Color), Colors.White);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public TextOverlay2D()
        {

        }

        #endregion

        #region # 属性

        #region 依赖属性 - 文本内容 —— string Text
        /// <summary>
        /// 依赖属性 - 文本内容
        /// </summary>
        public string Text
        {
            get => this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }
        #endregion

        #region 依赖属性 - 字体大小 —— float FontSize
        /// <summary>
        /// 依赖属性 - 字体大小
        /// </summary>
        public float FontSize
        {
            get => this.GetValue(FontSizeProperty);
            set => this.SetValue(FontSizeProperty, value);
        }
        #endregion

        #region 依赖属性 - 文本颜色 —— Color Color
        /// <summary>
        /// 依赖属性 - 文本颜色
        /// </summary>
        public Color Color
        {
            get => this.GetValue(ColorProperty);
            set => this.SetValue(ColorProperty, value);
        }
        #endregion

        #endregion

        #region # 方法

        #region 确保渲染对象 —— override void EnsureRenderable()
        /// <summary>
        /// 确保渲染对象
        /// </summary>
        internal override void EnsureRenderable()
        {
            if (this.Renderable == null)
            {
                TextOverlay overlay = new TextOverlay(this.Text, this.Position.ToVector2(), this.FontSize, this.Color.ToVector4());
                this.Renderable = overlay;
            }
            else
            {
                TextOverlay overlay = (TextOverlay)this.Renderable;
                overlay.Update(this.Text, this.FontSize);
                overlay.SetColor(this.Color.ToVector4());
            }
        }
        #endregion

        #endregion
    }
}
