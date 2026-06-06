using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace MedicalSharp.Controls.UserControls
{
    /// <summary>
    /// 颜色带
    /// </summary>
    public class ColorBand : Control
    {
        #region # 字段及构造器

        /// <summary>
        /// HU最小值
        /// </summary>
        private const short HUMin = -1024;

        /// <summary>
        /// HU最大值
        /// </summary>
        private const short HUMax = 3071;

        /// <summary>
        /// 默认解剖色控制点
        /// </summary>
        private static readonly ColorControlPoint[] _DefaultAnatomyColorPoints =
        [
            new ColorControlPoint { HU = -1024, Color = Color.FromRgb(0, 0, 0) },
            new ColorControlPoint { HU = -800, Color = Color.FromRgb(46, 31, 38) },
            new ColorControlPoint { HU = -400, Color = Color.FromRgb(89, 56, 71) },
            new ColorControlPoint { HU = -200, Color = Color.FromRgb(77, 51, 64) },
            new ColorControlPoint { HU = 0, Color = Color.FromRgb(140, 64, 46) },
            new ColorControlPoint { HU = 100, Color = Color.FromRgb(173, 82, 56) },
            new ColorControlPoint { HU = 200, Color = Color.FromRgb(153, 51, 38) },
            new ColorControlPoint { HU = 400, Color = Color.FromRgb(184, 71, 51) },
            new ColorControlPoint { HU = 600, Color = Color.FromRgb(191, 102, 77) },
            new ColorControlPoint { HU = 800, Color = Color.FromRgb(224, 184, 128) },
            new ColorControlPoint { HU = 1200, Color = Color.FromRgb(242, 224, 184) },
            new ColorControlPoint { HU = 2000, Color = Color.FromRgb(255, 245, 224) },
            new ColorControlPoint { HU = 3071, Color = Color.FromRgb(255, 255, 255) },
        ];

        /// <summary>
        /// 颜色控制点列表依赖属性
        /// </summary>
        public static readonly StyledProperty<AvaloniaList<ColorControlPoint>> ControlPointsProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static ColorBand()
        {
            ControlPointsProperty = AvaloniaProperty.Register<ColorBand, AvaloniaList<ColorControlPoint>>(nameof(ControlPoints), []);
            ControlPointsProperty.Changed.AddClassHandler<ColorBand, AvaloniaList<ColorControlPoint>>(OnColorPointsChanged);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public ColorBand()
        {
            this.ClipToBounds = true;
        }

        #endregion

        #region # 属性

        #region 依赖属性 - 颜色控制点列表 —— AvaloniaList<ColorControlPoint> ControlPoints
        /// <summary>
        /// 依赖属性 - 颜色控制点列表
        /// </summary>
        public AvaloniaList<ColorControlPoint> ControlPoints
        {
            get => this.GetValue(ControlPointsProperty);
            set => this.SetValue(ControlPointsProperty, value);
        }
        #endregion

        #endregion

        #region # 方法

        #region 渲染 —— override void Render(DrawingContext context)
        /// <summary>
        /// 渲染
        /// </summary>
        public override void Render(DrawingContext context)
        {
            base.Render(context);

            AvaloniaList<ColorControlPoint> controlPoints = this.ControlPoints;

            #region # 验证

            if (controlPoints == null || !controlPoints.Any())
            {
                controlPoints = new AvaloniaList<ColorControlPoint>(_DefaultAnatomyColorPoints);
            }
            if (this.Bounds.Width <= 0 || this.Bounds.Height <= 0)
            {
                return;
            }

            #endregion

            //按HU值排序
            List<ColorControlPoint> sortedControlPoints = controlPoints.OrderBy(point => point.HU).ToList();

            //逐像素绘制渐变
            for (int x = 0; x < (int)this.Bounds.Width; x++)
            {
                double hu = HUMin + (x / this.Bounds.Width) * (HUMax - HUMin);
                Color color = this.InterpolateColor(sortedControlPoints, hu);

                SolidColorBrush brush = new SolidColorBrush(color);
                Pen pen = new Pen(brush);
                context.DrawLine(pen, new Point(x, 0), new Point(x, this.Bounds.Height));
            }

            //绘制控制点标记
            foreach (ColorControlPoint point in sortedControlPoints)
            {
                double x = (point.HU - HUMin) / (double)(HUMax - HUMin) * this.Bounds.Width;
                double y = this.Bounds.Height / 2;
                SolidColorBrush markerFill = new SolidColorBrush(point.Color);
                Pen markerPen = new Pen(Brushes.White, 1);
                context.DrawEllipse(markerFill, markerPen, new Point(x, y), 4, 4);
            }
        }
        #endregion

        #region 插值颜色 —— Color InterpolateColor(IReadOnlyList<ColorControlPoint> points, double hu)
        /// <summary>
        /// 插值颜色
        /// </summary>
        private Color InterpolateColor(IReadOnlyList<ColorControlPoint> points, double hu)
        {
            if (points.Count == 0)
            {
                return Colors.Black;
            }
            if (hu <= points[0].HU)
            {
                return points[0].Color;
            }
            if (hu >= points[^1].HU)
            {
                return points[^1].Color;
            }

            for (int index = 0; index < points.Count - 1; index++)
            {
                if (hu >= points[index].HU && hu <= points[index + 1].HU)
                {
                    double range = points[index + 1].HU - points[index].HU;
                    double t = range > 0 ? (hu - points[index].HU) / range : 0.0;
                    byte r = (byte)(points[index].Color.R + (points[index + 1].Color.R - points[index].Color.R) * t);
                    byte g = (byte)(points[index].Color.G + (points[index + 1].Color.G - points[index].Color.G) * t);
                    byte b = (byte)(points[index].Color.B + (points[index + 1].Color.B - points[index].Color.B) * t);
                    Color color = Color.FromRgb(r, g, b);

                    return color;
                }
            }

            return Colors.Black;
        }
        #endregion


        //Events

        #region 颜色控制点列表改变事件 —— static void OnColorPointsChanged(ColorBand control...
        /// <summary>
        /// 颜色控制点列表改变事件
        /// </summary>
        private static void OnColorPointsChanged(ColorBand control, AvaloniaPropertyChangedEventArgs<AvaloniaList<ColorControlPoint>> eventArgs)
        {
            if (eventArgs.OldValue.Value != null)
            {
                eventArgs.OldValue.Value.CollectionChanged -= control.OnColorPointsCollectionChanged;
            }
            if (eventArgs.NewValue.Value != null)
            {
                eventArgs.NewValue.Value.CollectionChanged += control.OnColorPointsCollectionChanged;
            }
            control.InvalidateVisual();
        }
        #endregion

        #region 颜色控制点列表元素改变事件 —— void OnColorPointsCollectionChanged(object sender...
        /// <summary>
        /// 颜色控制点列表元素改变事件
        /// </summary>
        private void OnColorPointsCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            this.InvalidateVisual();
        }
        #endregion

        #endregion
    }
}
