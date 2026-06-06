using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

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
        /// 右键菜单
        /// </summary>
        private readonly MenuFlyout _contextMenu;

        /// <summary>
        /// 修改颜色菜单项
        /// </summary>
        private readonly MenuItem _editColorMenuItem;

        /// <summary>
        /// 插入颜色菜单项
        /// </summary>
        private readonly MenuItem _insertColorMenuItem;

        /// <summary>
        /// 删除颜色菜单项
        /// </summary>
        private readonly MenuItem _deleteColorMenuItem;

        /// <summary>
        /// 右键位置的控制点
        /// </summary>
        private ColorControlPoint _rightClickTargetPoint;

        /// <summary>
        /// 右键位置的HU值
        /// </summary>
        private short _rightClickHU;

        /// <summary>
        /// 默认构造器
        /// </summary>
        public ColorBand()
        {
            this.ClipToBounds = true;
            this.Cursor = new Cursor(StandardCursorType.Hand);

            //右键菜单
            this._editColorMenuItem = new MenuItem { Header = "修改颜色" };
            this._editColorMenuItem.Click += this.OnEditColorClicked;
            this._insertColorMenuItem = new MenuItem { Header = "插入颜色" };
            this._insertColorMenuItem.Click += this.OnInsertColorClicked;
            this._deleteColorMenuItem = new MenuItem { Header = "删除颜色" };
            this._deleteColorMenuItem.Click += this.OnDeleteColorClicked;

            this._contextMenu = new MenuFlyout
            {
                Items = { this._editColorMenuItem, this._insertColorMenuItem, this._deleteColorMenuItem }
            };
            this.ContextFlyout = this._contextMenu;

            this.PointerPressed += this.OnColorBandMouseDown;
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

        //Private

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
                return;
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
            #region # 验证

            if (!points.Any())
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
            #endregion

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

        #region 查找指定位置的控制点 —— ColorControlPoint FindControlPointAtPosition(Point position)
        /// <summary>
        /// 查找指定位置的控制点
        /// </summary>
        private ColorControlPoint FindControlPointAtPosition(Point position)
        {
            #region # 验证

            if (this.ControlPoints == null || !this.ControlPoints.Any())
            {
                return null;
            }

            #endregion

            const double hitRadius = 6.0;
            foreach (ColorControlPoint point in this.ControlPoints)
            {
                double x = (point.HU - HUMin) / (double)(HUMax - HUMin) * this.Bounds.Width;
                double distance = Math.Abs(position.X - x);
                if (distance <= hitRadius)
                {
                    return point;
                }
            }

            return null;
        }
        #endregion

        #region 显示颜色选择面板 —— Task<Color?> ShowColorPicker(Color defaultColor)
        /// <summary>
        /// 显示颜色选择面板
        /// </summary>
        private async Task<Color?> ShowColorPicker(Color defaultColor)
        {
            ColorView colorPicker = new ColorView
            {
                Width = 340,
                Height = 330,
                Color = defaultColor,
                IsAlphaEnabled = false,
                IsAlphaVisible = false
            };

            Button okButton = new Button { Content = "确定", Width = 70, Margin = new Thickness(0, 0, 8, 0) };
            Button cancelButton = new Button { Content = "取消", Width = 70 };
            StackPanel flyoutContent = new StackPanel
            {
                Children =
                {
                    colorPicker,
                    new WrapPanel
                    {
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        Margin = new Thickness(0, 8, 0, 0),
                        Children = { okButton, cancelButton }
                    }
                }
            };

            Flyout flyout = new Flyout
            {
                Content = flyoutContent,
                Placement = PlacementMode.Bottom
            };

            Color? selectedColor = null;
            okButton.Click += (_, _) =>
            {
                selectedColor = colorPicker.Color;
                flyout.Hide();
            };
            cancelButton.Click += (_, _) =>
            {
                selectedColor = null;
                flyout.Hide();
            };

            flyout.ShowAt(this);

            //等待Flyout关闭
            TaskCompletionSource<Color?> completionSource = new TaskCompletionSource<Color?>();
            flyout.Closed += (_, _) => completionSource.TrySetResult(selectedColor);

            return await completionSource.Task;
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

        #region 颜色带鼠标按下事件 —— void OnColorBandMouseDown(object sender...
        /// <summary>
        /// 颜色带鼠标按下事件
        /// </summary>
        private void OnColorBandMouseDown(object sender, PointerPressedEventArgs eventArgs)
        {
            if (eventArgs.Properties.IsRightButtonPressed)
            {
                Point position = eventArgs.GetPosition(this);
                this._rightClickHU = (short)(HUMin + (position.X / this.Bounds.Width) * (HUMax - HUMin));
                this._rightClickTargetPoint = this.FindControlPointAtPosition(position);

                this._editColorMenuItem.IsEnabled = this._rightClickTargetPoint != null;
                this._deleteColorMenuItem.IsEnabled = this._rightClickTargetPoint != null;
                this._insertColorMenuItem.IsEnabled = this._rightClickTargetPoint == null;

                this._contextMenu.Placement = PlacementMode.Pointer;
                this._contextMenu.ShowAt(this, true);
                eventArgs.Handled = true;
            }
        }
        #endregion

        #region 修改颜色菜单事件 —— async void OnEditColorClicked(object sender...
        /// <summary>
        /// 修改颜色菜单事件
        /// </summary>
        private async void OnEditColorClicked(object sender, RoutedEventArgs e)
        {
            if (this._rightClickTargetPoint == null)
            {
                return;
            }

            Color? newColor = await this.ShowColorPicker(this._rightClickTargetPoint.Color);
            if (newColor.HasValue)
            {
                this._rightClickTargetPoint.Color = newColor.Value;
                this.InvalidateVisual();
            }
        }
        #endregion

        #region 插入颜色菜单事件 —— async void OnInsertColorClicked(object sender...
        /// <summary>
        /// 插入颜色菜单事件
        /// </summary>
        private async void OnInsertColorClicked(object sender, RoutedEventArgs e)
        {
            AvaloniaList<ColorControlPoint> controlPoints = this.ControlPoints;
            if (controlPoints == null)
            {
                return;
            }

            List<ColorControlPoint> sorted = controlPoints.OrderBy(p => p.HU).ToList();
            Color currentColor = this.InterpolateColor(sorted, this._rightClickHU);

            Color? selectedColor = await this.ShowColorPicker(currentColor);
            if (selectedColor.HasValue)
            {
                controlPoints.Add(new ColorControlPoint { HU = this._rightClickHU, Color = selectedColor.Value });

                //原地冒泡排序
                for (int i = controlPoints.Count - 1; i > 0; i--)
                {
                    if (controlPoints[i].HU < controlPoints[i - 1].HU)
                    {
                        (controlPoints[i], controlPoints[i - 1]) = (controlPoints[i - 1], controlPoints[i]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        #endregion

        #region 删除颜色菜单事件 —— void OnDeleteColorClicked(object sender...
        /// <summary>
        /// 删除颜色菜单事件
        /// </summary>
        private void OnDeleteColorClicked(object sender, RoutedEventArgs e)
        {
            if (this._rightClickTargetPoint != null)
            {
                this.ControlPoints?.Remove(this._rightClickTargetPoint);
                this._rightClickTargetPoint = null;
            }
        }
        #endregion

        #endregion
    }
}
