using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace MedicalSharp.Controls.UserControls
{
    /// <summary>
    /// Alpha曲线编辑器
    /// </summary>
    public class AlphaCurveEditor : Canvas
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
        /// 控制点列表依赖属性
        /// </summary>
        public static readonly StyledProperty<AvaloniaList<AlphaControlPoint>> ControlPointsProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static AlphaCurveEditor()
        {
            ControlPointsProperty = AvaloniaProperty.Register<AlphaCurveEditor, AvaloniaList<AlphaControlPoint>>(nameof(ControlPoints), []);
            ControlPointsProperty.Changed.AddClassHandler<AlphaCurveEditor, AvaloniaList<AlphaControlPoint>>(OnControlPointsChanged);
        }


        /// <summary>
        /// 是否已初始绘制
        /// </summary>
        private bool _initialRendered;

        /// <summary>
        /// 刻度是否已绘制
        /// </summary>
        private bool _scaleDrawn;

        /// <summary>
        /// 当前拖拽的控制点
        /// </summary>
        private AlphaControlPoint _draggingPoint;

        /// <summary>
        /// 右键位置的控制点
        /// </summary>
        private AlphaControlPoint _rightClickTargetPoint;

        /// <summary>
        /// 右键位置的HU值
        /// </summary>
        private short _rightClickHU;

        /// <summary>
        /// 右键位置的Alpha值
        /// </summary>
        private float _rightClickAlpha;

        /// <summary>
        /// 画布右键菜单
        /// </summary>
        private readonly MenuFlyout _canvasContextMenu;

        /// <summary>
        /// 插入菜单项
        /// </summary>
        private readonly MenuItem _insertMenuItem;

        /// <summary>
        /// 删除菜单项
        /// </summary>
        private readonly MenuItem _deleteMenuItem;

        /// <summary>
        /// 控制点提示文本
        /// </summary>
        private readonly TextBlock _controlPointTooltip;

        /// <summary>
        /// 折线图形
        /// </summary>
        private readonly Polyline _polyline;

        /// <summary>
        /// 控制点圆圈字典
        /// </summary>
        private readonly Dictionary<AlphaControlPoint, Ellipse> _pointToMarker;

        /// <summary>
        /// 创建传递函数曲线编辑器构造器
        /// </summary>
        public AlphaCurveEditor()
        {
            this._pointToMarker = new Dictionary<AlphaControlPoint, Ellipse>();

            //折线
            this._polyline = new Polyline
            {
                Stroke = Brushes.Red,
                StrokeThickness = 1,
                IsHitTestVisible = false
            };
            this.Children.Add(this._polyline);

            //提示文本
            this._controlPointTooltip = new TextBlock
            {
                FontSize = 10,
                Foreground = Brushes.White,
                IsHitTestVisible = false
            };
            this.Children.Add(this._controlPointTooltip);

            //右键菜单
            this._insertMenuItem = new MenuItem
            {
                Header = "插入控制点"
            };
            this._insertMenuItem.Click += this.OnInsertControlPointClicked;
            this._deleteMenuItem = new MenuItem
            {
                Header = "删除控制点"
            };
            this._deleteMenuItem.Click += this.OnDeleteControlPointClicked;
            this._canvasContextMenu = new MenuFlyout
            {
                Items = { this._insertMenuItem, this._deleteMenuItem }
            };
            this.ContextFlyout = this._canvasContextMenu;

            //布局完成后初始绘制
            this.SizeChanged += this.OnSizeChanged;

            //画布鼠标按下事件
            this.PointerPressed += this.OnCanvasMouseDown;
        }

        #endregion

        #region # 属性

        #region 依赖属性 - 控制点列表 —— AvaloniaList<AlphaControlPoint> ControlPoints
        /// <summary>
        /// 依赖属性 - 控制点列表
        /// </summary>
        public AvaloniaList<AlphaControlPoint> ControlPoints
        {
            get => this.GetValue(ControlPointsProperty);
            set => this.SetValue(ControlPointsProperty, value);
        }
        #endregion

        #endregion

        #region # 方法

        //Private

        #region 绘制坐标轴刻度 —— void DrawScale()
        /// <summary>
        /// 绘制坐标轴刻度
        /// </summary>
        private void DrawScale()
        {
            #region # 验证

            if (this.Bounds.Width <= 0 || this.Bounds.Height <= 0)
            {
                return;
            }
            if (this._scaleDrawn)
            {
                return;
            }

            #endregion

            this._scaleDrawn = true;

            //横轴刻度（每100HU一条线，每500HU显示数字）
            for (int hu = -1000; hu <= 3000; hu += 100)
            {
                float x = this.HUToCanvasX(hu);

                Line tickLine = new Line
                {
                    StartPoint = new Point(x, this.Bounds.Height - 3),
                    EndPoint = new Point(x, this.Bounds.Height),
                    Stroke = Brushes.White,
                    StrokeThickness = 1
                };
                this.Children.Add(tickLine);

                if (hu % 500 == 0)
                {
                    TextBlock label = new TextBlock
                    {
                        Text = hu switch
                        {
                            -1000 => "-1024",
                            3000 => "3071",
                            _ => hu.ToString()
                        },
                        FontSize = 10,
                        Foreground = Brushes.White
                    };
                    Canvas.SetLeft(label, x - 12);
                    Canvas.SetTop(label, this.Bounds.Height + 2);
                    this.Children.Add(label);
                }
            }

            //纵轴刻度（0到1，每0.1一条线）
            for (float alpha = 0.0f; alpha < 1.1f; alpha += 0.1f)
            {
                float y = this.AlphaToCanvasY(alpha);
                Line tickLine = new Line
                {
                    StartPoint = new Point(0, y),
                    EndPoint = new Point(5, y),
                    Stroke = Brushes.White,
                    StrokeThickness = 1
                };
                this.Children.Add(tickLine);

                TextBlock label = new TextBlock
                {
                    Text = alpha.ToString("F1"),
                    FontSize = 10,
                    Foreground = Brushes.White
                };
                Canvas.SetLeft(label, -17);
                Canvas.SetTop(label, y - 7);
                this.Children.Add(label);
            }
        }
        #endregion

        #region 同步控制点圆圈 —— void SyncMarkers(IReadOnlyList<AlphaControlPoint> controlPoints)
        /// <summary>
        /// 同步控制点圆圈
        /// </summary>
        private void SyncMarkers(IReadOnlyList<AlphaControlPoint> controlPoints)
        {
            //移除已删除控制点的圆圈
            IEnumerable<AlphaControlPoint> removedPoints = this._pointToMarker.Keys.Except(controlPoints);
            foreach (AlphaControlPoint controlPoint in removedPoints)
            {
                Ellipse marker = this._pointToMarker[controlPoint];
                marker.PointerPressed -= this.OnControlPointMouseDown;
                marker.PointerMoved -= this.OnControlPointMouseMoved;
                marker.PointerReleased -= this.OnControlPointMouseUp;
                this._pointToMarker.Remove(controlPoint);
                this.Children.Remove(marker);
            }

            //添加新控制点的圆圈
            foreach (AlphaControlPoint controlPoint in controlPoints)
            {
                if (!this._pointToMarker.ContainsKey(controlPoint))
                {
                    Ellipse ellipse = new Ellipse
                    {
                        Width = 10,
                        Height = 10,
                        Stroke = Brushes.Cyan,
                        StrokeThickness = 2.5,
                        Fill = Brushes.Black,
                        Cursor = new Cursor(StandardCursorType.Hand)
                    };

                    ellipse.PointerPressed += this.OnControlPointMouseDown;
                    ellipse.PointerMoved += this.OnControlPointMouseMoved;
                    ellipse.PointerReleased += this.OnControlPointMouseUp;

                    this._pointToMarker[controlPoint] = ellipse;
                    this.Children.Add(ellipse);
                }
            }
        }
        #endregion

        #region 更新折线 —— void UpdatePolyline()
        /// <summary>
        /// 更新折线
        /// </summary>
        private void UpdatePolyline()
        {
            if (this.ControlPoints == null || !this.ControlPoints.Any() || this.Bounds.Width <= 0 || this.Bounds.Height <= 0)
            {
                this._polyline.Points.Clear();
                return;
            }

            //按HU值排序
            List<AlphaControlPoint> sortedControlPoints = this.ControlPoints.OrderBy(point => point.HU).ToList();

            Points points = [];
            foreach (AlphaControlPoint controlPoint in sortedControlPoints)
            {
                float x = this.HUToCanvasX(controlPoint.HU);
                float y = this.AlphaToCanvasY(controlPoint.Alpha);
                Point point = new Point(x, y);
                points.Add(point);
            }
            this._polyline.Points = points;

            //更新圆圈位置（按控制点引用直接定位）
            foreach ((AlphaControlPoint controlPoint, Ellipse ellipse) in this._pointToMarker)
            {
                float x = this.HUToCanvasX(controlPoint.HU);
                float y = this.AlphaToCanvasY(controlPoint.Alpha);
                SetLeft(ellipse, x - 5);
                SetTop(ellipse, y - 5);
            }
        }
        #endregion

        #region HU值转X坐标 —— float HUToCanvasX(int hu)
        /// <summary>
        /// HU值转X坐标
        /// </summary>
        private float HUToCanvasX(int hu)
        {
            if (this.Bounds.Width <= 0)
            {
                return 0;
            }

            return (hu * 1.0f - HUMin) / (HUMax - HUMin) * (float)this.Bounds.Width;
        }
        #endregion

        #region X坐标转HU值 —— short CanvasXToHU(float x)
        /// <summary>
        /// X坐标转HU值
        /// </summary>
        private short CanvasXToHU(float x)
        {
            if (this.Bounds.Width <= 0)
            {
                return 0;
            }

            short hu = (short)Math.Round(x / this.Bounds.Width * (HUMax - HUMin) + HUMin);

            return hu;
        }
        #endregion

        #region Alpha值转Y坐标 —— float AlphaToCanvasY(float alpha)
        /// <summary>
        /// Alpha值转Y坐标
        /// </summary>
        private float AlphaToCanvasY(float alpha)
        {
            if (this.Bounds.Height <= 0)
            {
                return 0;
            }

            return (1.0f - alpha) * (float)this.Bounds.Height;
        }
        #endregion

        #region Y坐标转Alpha值 —— float CanvasYToAlpha(float y)
        /// <summary>
        /// Y坐标转Alpha值
        /// </summary>
        private float CanvasYToAlpha(float y)
        {
            if (this.Bounds.Height <= 0)
            {
                return 0;
            }

            return 1.0f - (y / (float)this.Bounds.Height);
        }
        #endregion

        #region 查找指定位置的控制点 —— AlphaControlPoint FindControlPointAtPosition(Point position)
        /// <summary>
        /// 查找指定位置的控制点
        /// </summary>
        private AlphaControlPoint FindControlPointAtPosition(Point position)
        {
            #region # 验证

            if (this.Bounds.Width <= 0 || this.Bounds.Height <= 0)
            {
                return null;
            }

            #endregion

            const float hitRadius = 8.0f;
            foreach ((AlphaControlPoint controlPoint, Ellipse ellipse) in this._pointToMarker)
            {
                float centerX = (float)(Canvas.GetLeft(ellipse) + ellipse.Width / 2.0);
                float centerY = (float)(Canvas.GetTop(ellipse) + ellipse.Height / 2.0);
                float distance = (float)Math.Sqrt((position.X - centerX) * (position.X - centerX) +
                                                  (position.Y - centerY) * (position.Y - centerY));
                if (distance <= hitRadius)
                {
                    return controlPoint;
                }
            }

            return null;
        }
        #endregion


        //Events

        #region 尺寸改变事件 —— void OnSizeChanged(object sender, SizeChangedEventArgs eventArgs)
        /// <summary>
        /// 尺寸改变事件
        /// </summary>
        private void OnSizeChanged(object sender, SizeChangedEventArgs eventArgs)
        {
            if (!this._initialRendered && eventArgs.NewSize.Width > 0 && eventArgs.NewSize.Height > 0)
            {
                this.DrawScale();
                if (this.ControlPoints != null && this.ControlPoints.Count > 0)
                {
                    this.SyncMarkers(this.ControlPoints);
                    this.UpdatePolyline();
                }

                //设置背景色
                LinearGradientBrush backgroundBrush = new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                    GradientStops =
                    {
                        new GradientStop(Color.FromRgb(15, 15, 15), 0.0),    //底部近黑（Alpha=1.0）
                        new GradientStop(Color.FromRgb(100, 100, 100), 1.0)  //顶部浅灰（Alpha=0.0）
                    }
                };
                this.Background = backgroundBrush;

                this._initialRendered = true;
            }
        }
        #endregion

        #region 控制点列表改变事件 —— static void OnControlPointsChanged(TransferFunctionCanvas canvas...
        /// <summary>
        /// 控制点列表改变事件
        /// </summary>
        private static void OnControlPointsChanged(AlphaCurveEditor canvas, AvaloniaPropertyChangedEventArgs<AvaloniaList<AlphaControlPoint>> eventArgs)
        {
            if (eventArgs.OldValue.Value != null)
            {
                eventArgs.OldValue.Value.CollectionChanged -= canvas.OnControlPointsCollectionChanged;
            }
            if (eventArgs.NewValue.Value != null)
            {
                eventArgs.NewValue.Value.CollectionChanged += canvas.OnControlPointsCollectionChanged;
                canvas.SyncMarkers(eventArgs.NewValue.Value);
                canvas.UpdatePolyline();
            }
        }
        #endregion

        #region 控制点列表元素改变事件 —— void OnControlPointsCollectionChanged(object sender...
        /// <summary>
        /// 控制点列表元素改变事件
        /// </summary>
        private void OnControlPointsCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            if (this.ControlPoints != null)
            {
                this.SyncMarkers(this.ControlPoints);
                this.UpdatePolyline();
            }
        }
        #endregion

        #region 控制点鼠标按下事件 —— void OnControlPointMouseDown(object sender...
        /// <summary>
        /// 控制点鼠标按下事件
        /// </summary>
        private void OnControlPointMouseDown(object sender, PointerPressedEventArgs eventArgs)
        {
            if (eventArgs.Properties.IsLeftButtonPressed)
            {
                Ellipse ellipse = (Ellipse)sender;

                //通过圆圈反查控制点引用
                this._draggingPoint = this._pointToMarker.FirstOrDefault(kvp => kvp.Value == ellipse).Key;
                if (this._draggingPoint != null)
                {
                    eventArgs.Handled = true;
                }
            }
        }
        #endregion

        #region 控制点鼠标移动事件 —— void OnControlPointMouseMoved(object sender...
        /// <summary>
        /// 控制点鼠标移动事件
        /// </summary>
        private void OnControlPointMouseMoved(object sender, PointerEventArgs eventArgs)
        {
            #region # 验证

            if (this._draggingPoint == null)
            {
                return;
            }

            #endregion

            if (eventArgs.Properties.IsLeftButtonPressed)
            {
                Point position = eventArgs.GetPosition(this);

                short hu = this.CanvasXToHU((float)position.X);
                float alpha = this.CanvasYToAlpha((float)position.Y);
                hu = Math.Clamp(hu, HUMin, HUMax);
                alpha = Math.Clamp(alpha, 0.0f, 1.0f);

                this._draggingPoint.HU = hu;
                this._draggingPoint.Alpha = alpha;
                this.UpdatePolyline();

                //更新提示文本
                this._controlPointTooltip.Text = $"({hu}, {alpha:F2})";
                Canvas.SetLeft(this._controlPointTooltip, position.X - 20);
                Canvas.SetTop(this._controlPointTooltip, position.Y - 20);
                this._controlPointTooltip.IsVisible = true;

                eventArgs.Handled = true;
            }
        }
        #endregion

        #region 控制点鼠标松开事件 —— void OnControlPointMouseUp(object sender...
        /// <summary>
        /// 控制点鼠标松开事件
        /// </summary>
        private void OnControlPointMouseUp(object sender, PointerReleasedEventArgs e)
        {
            this._draggingPoint = null;
            this._controlPointTooltip.IsVisible = false;
        }
        #endregion

        #region 画布鼠标按下事件 —— void OnCanvasMouseDown(object sender...
        /// <summary>
        /// 画布鼠标按下事件
        /// </summary>
        private void OnCanvasMouseDown(object sender, PointerPressedEventArgs eventArgs)
        {
            if (eventArgs.Properties.IsRightButtonPressed)
            {
                Point position = eventArgs.GetPosition(this);
                this._rightClickHU = Math.Clamp(this.CanvasXToHU((float)position.X), HUMin, HUMax);
                this._rightClickAlpha = Math.Clamp(this.CanvasYToAlpha((float)position.Y), 0.0f, 1.0f);

                //查找右键位置附近是否有控制点
                this._rightClickTargetPoint = this.FindControlPointAtPosition(position);

                //无控制点则启用插入，有则启用删除
                this._insertMenuItem.IsEnabled = this._rightClickTargetPoint == null;
                this._deleteMenuItem.IsEnabled = this._rightClickTargetPoint != null;

                this._canvasContextMenu.Placement = PlacementMode.Pointer;
                this._canvasContextMenu.ShowAt(this, true);
                eventArgs.Handled = true;
            }
        }
        #endregion

        #region 插入控制点菜单事件 —— void OnInsertControlPointClicked(object sender...
        /// <summary>
        /// 插入控制点菜单事件
        /// </summary>
        private void OnInsertControlPointClicked(object sender, RoutedEventArgs e)
        {
            AlphaControlPoint controlPoint = new AlphaControlPoint
            {
                HU = this._rightClickHU,
                Alpha = this._rightClickAlpha
            };
            this.ControlPoints.Add(controlPoint);

            //原地冒泡排序
            for (int i = this.ControlPoints.Count - 1; i > 0; i--)
            {
                if (this.ControlPoints[i].HU < this.ControlPoints[i - 1].HU)
                {
                    (this.ControlPoints[i], this.ControlPoints[i - 1]) = (this.ControlPoints[i - 1], this.ControlPoints[i]);
                }
                else
                {
                    break;
                }
            }
        }
        #endregion

        #region 删除控制点菜单事件 —— void OnDeleteControlPointClicked(object sender...
        /// <summary>
        /// 删除控制点菜单事件
        /// </summary>
        private void OnDeleteControlPointClicked(object sender, RoutedEventArgs e)
        {
            if (this._rightClickTargetPoint != null)
            {
                this.ControlPoints.Remove(this._rightClickTargetPoint);
                this._rightClickTargetPoint = null;
            }
        }
        #endregion

        #endregion
    }
}
