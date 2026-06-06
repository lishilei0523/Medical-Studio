using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using MedicalSharp.Primitives.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace MedicalSharp.Client.ViewModels.ProtocolContext
{
    /// <summary>
    /// 传递函数曲线编辑器
    /// </summary>
    public class TransferFunctionCurve : Canvas
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
        static TransferFunctionCurve()
        {
            ControlPointsProperty = AvaloniaProperty.Register<TransferFunctionCurve, AvaloniaList<AlphaControlPoint>>(nameof(ControlPoints), []);
            ControlPointsProperty.Changed.AddClassHandler<TransferFunctionCurve, AvaloniaList<AlphaControlPoint>>(OnControlPointsChanged);
        }


        /// <summary>
        /// 当前拖拽的控制点
        /// </summary>
        private AlphaControlPoint _draggingPoint;

        /// <summary>
        /// 折线图形
        /// </summary>
        private readonly Polyline _polyline;

        /// <summary>
        /// 控制点圆圈列表
        /// </summary>
        private readonly List<Ellipse> _pointMarkers;

        /// <summary>
        /// 创建传递函数曲线编辑器构造器
        /// </summary>
        public TransferFunctionCurve()
        {
            this._pointMarkers = [];

            //折线
            this._polyline = new Polyline
            {
                Stroke = Brushes.White,
                StrokeThickness = 2,
                IsHitTestVisible = false
            };
            this.Children.Add(this._polyline);

            //背景右键添加控制点
            this.PointerPressed += this.OnCanvasPointerPressed;
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

        #region 更新曲线 —— void UpdateCurve()
        /// <summary>
        /// 更新曲线
        /// </summary>
        private void UpdateCurve()
        {
            AvaloniaList<AlphaControlPoint> controlPoints = this.ControlPoints;
            if (controlPoints == null || controlPoints.Count == 0)
            {
                this._polyline.Points.Clear();
                return;
            }

            //按HU值排序
            List<AlphaControlPoint> sorted = controlPoints.OrderBy(p => p.HU).ToList();

            Points points = new Avalonia.Points();
            foreach (AlphaControlPoint controlPoint in sorted)
            {
                double x = this.HUToCanvasX(controlPoint.HU);
                double y = this.AlphaToCanvasY(controlPoint.Alpha);
                points.Add(new Point(x, y));
            }

            this._polyline.Points = points;
            this.UpdateMarkerPositions(sorted);
        }
        #endregion

        #region 更新控制点圆圈位置 —— void UpdateMarkerPositions(IReadOnlyList<AlphaControlPoint> controlPoints)
        /// <summary>
        /// 更新控制点圆圈位置
        /// </summary>
        private void UpdateMarkerPositions(IReadOnlyList<AlphaControlPoint> controlPoints)
        {
            for (int index = 0; index < controlPoints.Count; index++)
            {
                double x = this.HUToCanvasX(controlPoints[index].HU);
                double y = this.AlphaToCanvasY(controlPoints[index].Alpha);

                Canvas.SetLeft(this._pointMarkers[index], x - 5);
                Canvas.SetTop(this._pointMarkers[index], y - 5);
            }
        }
        #endregion

        #region 重建控制点圆圈 —— void RebuildMarkers(IReadOnlyList<AlphaControlPoint> controlPoints)
        /// <summary>
        /// 重建控制点圆圈
        /// </summary>
        private void RebuildMarkers(IReadOnlyList<AlphaControlPoint> controlPoints)
        {
            //移除旧圆圈
            foreach (Ellipse marker in this._pointMarkers)
            {
                marker.PointerPressed -= this.OnControlPointPointerPressed;
                marker.PointerMoved -= this.OnControlPointPointerMoved;
                marker.PointerReleased -= this.OnControlPointPointerReleased;
                this.Children.Remove(marker);
            }
            this._pointMarkers.Clear();

            //创建新圆圈
            foreach (AlphaControlPoint point in controlPoints)
            {
                Ellipse ellipse = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = Brushes.Yellow,
                    Stroke = Brushes.White,
                    StrokeThickness = 1,
                    Cursor = new Cursor(StandardCursorType.Hand)
                };

                ellipse.PointerPressed += this.OnControlPointPointerPressed;
                ellipse.PointerMoved += this.OnControlPointPointerMoved;
                ellipse.PointerReleased += this.OnControlPointPointerReleased;

                this._pointMarkers.Add(ellipse);
                this.Children.Add(ellipse);
            }
        }
        #endregion

        #region HU值转Canvas X坐标 —— double HUToCanvasX(int hu)
        /// <summary>
        /// HU值转Canvas X坐标
        /// </summary>
        private double HUToCanvasX(int hu)
        {
            if (this.Bounds.Width <= 0)
            {
                return 0;
            }

            return (hu * 1.0 - HUMin) / (HUMax - HUMin) * this.Bounds.Width;
        }
        #endregion

        #region Canvas X坐标转HU值 —— short CanvasXToHU(double x)
        /// <summary>
        /// Canvas X坐标转HU值
        /// </summary>
        private short CanvasXToHU(double x)
        {
            if (this.Bounds.Width <= 0)
            {
                return 0;
            }

            short hu = (short)Math.Round(x / this.Bounds.Width * (HUMax - HUMin) + HUMin);

            return hu;
        }
        #endregion

        #region Alpha值转Canvas Y坐标 —— double AlphaToCanvasY(double alpha)
        /// <summary>
        /// Alpha值转Canvas Y坐标
        /// </summary>
        private double AlphaToCanvasY(double alpha)
        {
            if (this.Bounds.Height <= 0)
            {
                return 0;
            }

            return (1.0 - alpha) * this.Bounds.Height;
        }
        #endregion

        #region Canvas Y坐标转Alpha值 —— double CanvasYToAlpha(double y)
        /// <summary>
        /// Canvas Y坐标转Alpha值
        /// </summary>
        private double CanvasYToAlpha(double y)
        {
            if (this.Bounds.Height <= 0)
            {
                return 0;
            }

            return 1.0 - (y / this.Bounds.Height);
        }
        #endregion


        //Events

        #region 控制点列表改变事件 —— static void OnControlPointsChanged(TransferFunctionCurve control...
        /// <summary>
        /// 控制点列表改变事件
        /// </summary>
        private static void OnControlPointsChanged(TransferFunctionCurve control, AvaloniaPropertyChangedEventArgs<AvaloniaList<AlphaControlPoint>> eventArgs)
        {
            if (eventArgs.OldValue.Value != null)
            {
                eventArgs.OldValue.Value.CollectionChanged -= control.OnControlPointsCollectionChanged;
            }

            if (eventArgs.NewValue.Value != null)
            {
                eventArgs.NewValue.Value.CollectionChanged += control.OnControlPointsCollectionChanged;
                control.RebuildMarkers(eventArgs.NewValue.Value);
                control.UpdateCurve();
            }
        }
        #endregion

        #region 控制点列表集合改变事件 —— void OnControlPointsCollectionChanged(object sender...
        /// <summary>
        /// 控制点列表集合改变事件
        /// </summary>
        private void OnControlPointsCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            AvaloniaList<AlphaControlPoint> controlPoints = this.ControlPoints;
            if (controlPoints != null)
            {
                this.RebuildMarkers(controlPoints);
                this.UpdateCurve();
            }
        }
        #endregion

        #region 控制点按下事件 —— void OnControlPointPointerPressed(object sender...
        /// <summary>
        /// 控制点按下事件
        /// </summary>
        private void OnControlPointPointerPressed(object sender, PointerPressedEventArgs eventArgs)
        {
            Ellipse ellipse = (Ellipse)sender;
            int index = this._pointMarkers.IndexOf(ellipse);
            if (index >= 0)
            {
                //通过圆圈索引映射到控制点（用当前未排序的controlPoints对应）
                AvaloniaList<AlphaControlPoint> controlPoints = this.ControlPoints;
                if (controlPoints != null && index < controlPoints.Count)
                {
                    this._draggingPoint = controlPoints[index];
                }
                eventArgs.Handled = true;
            }
        }
        #endregion

        #region 控制点移动事件 —— void OnControlPointPointerMoved(object sender...
        /// <summary>
        /// 控制点移动事件
        /// </summary>
        private void OnControlPointPointerMoved(object sender, PointerEventArgs eventArgs)
        {
            if (this._draggingPoint == null)
            {
                return;
            }

            Point position = eventArgs.GetPosition(this);
            short hu = Math.Clamp(this.CanvasXToHU(position.X), HUMin, HUMax);
            double alpha = Math.Clamp(this.CanvasYToAlpha(position.Y), 0.0, 1.0);

            this._draggingPoint.HU = hu;
            this._draggingPoint.Alpha = alpha;
            this.UpdateCurve();

            eventArgs.Handled = true;
        }
        #endregion

        #region 控制点松开事件 —— void OnControlPointPointerReleased(object sender...
        /// <summary>
        /// 控制点松开事件
        /// </summary>
        private void OnControlPointPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            this._draggingPoint = null;
        }
        #endregion

        #region 画布按下事件（添加控制点） —— void OnCanvasPointerPressed(object sender...
        /// <summary>
        /// 画布按下事件（添加控制点）
        /// </summary>
        private void OnCanvasPointerPressed(object sender, PointerPressedEventArgs eventArgs)
        {
            PointerPoint pointerPoint = eventArgs.GetCurrentPoint(this);
            if (pointerPoint.Properties.IsRightButtonPressed)
            {
                Point position = eventArgs.GetPosition(this);
                short hu = Math.Clamp(this.CanvasXToHU(position.X), HUMin, HUMax);
                double alpha = Math.Clamp(this.CanvasYToAlpha(position.Y), 0.0, 1.0);

                this.ControlPoints.Add(new AlphaControlPoint { HU = hu, Alpha = alpha });

                //原地排序，避免Clear + Add触发多次CollectionChanged
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

                eventArgs.Handled = true;
            }
        }
        #endregion

        #endregion
    }
}
