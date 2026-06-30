using Avalonia;
using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Controls.Visual3Ds;
using MedicalSharp.Primitives.Builders;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace MedicalSharp.Controls.Commands
{
    /// <summary>
    /// 绘制折线3D元素命令
    /// </summary>
    public class DrawPolylineCommand : DrawShapeCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// 预览点
        /// </summary>
        private Vector3D? _previewPoint;

        /// <summary>
        /// 折线3D元素
        /// </summary>
        private PolylineVisual3D _polyline;

        /// <summary>
        /// 是否闭合
        /// </summary>
        private readonly bool _closed;

        /// <summary>
        /// 创建绘制折线3D元素命令构造器
        /// </summary>
        /// <param name="closed">是否闭合</param>
        public DrawPolylineCommand(bool closed)
        {
            this._closed = closed;
        }

        #endregion

        #region # 属性

        #region 绘制开始委托 —— Action<PolylineVisual3D> DrawStart
        /// <summary>
        /// 绘制开始委托
        /// </summary>
        public Action<PolylineVisual3D> DrawStart { get; set; }
        #endregion

        #region 绘制结束委托 —— Action<PolylineVisual3D> DrawEnd
        /// <summary>
        /// 绘制结束委托
        /// </summary>
        public Action<PolylineVisual3D> DrawEnd { get; set; }
        #endregion

        #region 绘制已取消委托 —— Action<PolylineVisual3D> DrawCancelled
        /// <summary>
        /// 绘制已取消委托
        /// </summary>
        public Action<PolylineVisual3D> DrawCancelled { get; set; }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 鼠标按下事件 —— override void OnMouseDown(OpenTKViewport viewport...
        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        public override void OnMouseDown(OpenTKViewport viewport, PointerPressedEventArgs eventArgs)
        {
            if (eventArgs.Properties.IsLeftButtonPressed && viewport is BasicViewport basicViewport)
            {
                Vector2 mousePos2D = eventArgs.GetPixelPosition(viewport).ToVector2();
                Vector3? mousePos3D = basicViewport.FindNearestPosition(mousePos2D);
                if (mousePos3D.HasValue)
                {
                    Vector3D position = mousePos3D.Value.ToVector3();
                    if (this._polyline == null)
                    {
                        //第一次点击：创建折线，添加第一个控制点
                        this._polyline = new PolylineVisual3D
                        {
                            Fill = ColorFactory.Fill2D.ToColor(),
                            Positions = [position],
                            Closed = this._closed
                        };
                        this._isDrawing = true;
                        this.DrawStart?.Invoke(this._polyline);
                    }
                    else
                    {
                        //先移除预览点（如果存在）
                        if (this._previewPoint.HasValue)
                        {
                            this._polyline.Positions.RemoveAt(this._polyline.Positions.Count - 1);
                            this._previewPoint = null;
                        }
                        this._polyline.Positions.Add(position);
                    }

                    //请求下一帧
                    viewport.RequestNextFrameRendering();
                }
            }

            base.OnMouseDown(viewport, eventArgs);
        }
        #endregion

        #region 鼠标移动事件 —— override void OnMouseMove(OpenTKViewport viewport...
        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        public override void OnMouseMove(OpenTKViewport viewport, PointerEventArgs eventArgs)
        {
            base.OnMouseMove(viewport, eventArgs);
            if (viewport is BasicViewport basicViewport && this._polyline != null)
            {
                //设置光标
                viewport.Cursor = new Cursor(StandardCursorType.Cross);

                Vector2 mousePos2D = eventArgs.GetPixelPosition(viewport).ToVector2();
                Vector3? mousePos3D = basicViewport.FindNearestPosition(mousePos2D);
                if (mousePos3D.HasValue)
                {
                    Vector3D currentPos = mousePos3D.Value.ToVector3();

                    //预览更新：替换最后一个点（预览点）
                    if (this._previewPoint.HasValue)
                    {
                        this._polyline.Positions[^1] = currentPos;
                    }
                    else
                    {
                        this._polyline.Positions.Add(currentPos);
                        this._previewPoint = currentPos;
                    }

                    //请求下一帧
                    viewport.RequestNextFrameRendering();
                }
            }
        }
        #endregion

        #region 获取上下文菜单项列表 —— override IReadOnlyList<ContextMenuItem> GetContextMenuItems(...
        /// <summary>
        /// 获取上下文菜单项列表
        /// </summary>
        /// <returns>上下文菜单项列表</returns>
        public override IReadOnlyList<ContextMenuItem> GetContextMenuItems(OpenTKViewport viewport, PointerReleasedEventArgs eventArgs)
        {
            if (this._isDrawing)
            {
                List<ContextMenuItem> items =
                [
                    new ContextMenuItem
                    {
                        Header = "完成(_F)",
                        Command = () => this.CompleteDrawing(viewport),
                        IsEnabled = this._polyline != null
                    },
                    new ContextMenuItem
                    {
                        Header = "取消(_C)",
                        Command = () => this.CancelDrawing(viewport),
                        IsEnabled = this._polyline != null
                    }
                ];

                if (this._polyline != null && this._polyline.Positions.Count > 1)
                {
                    items.Add(new ContextMenuItem
                    {
                        Header = "撤销上一点(_U)",
                        Command = () => this.UndoLastPoint(viewport)
                    });
                }

                return items;
            }

            return base.GetContextMenuItems(viewport, eventArgs);
        }
        #endregion

        #region 失效命令 —— override void Deactivate()
        /// <summary>
        /// 失效命令
        /// </summary>
        /// <remarks>命令被停用时调用，切换命令前</remarks>
        public override void Deactivate()
        {
            base.Deactivate();

            this._isDrawing = false;
            this._previewPoint = null;
            this._polyline = null;
        }
        #endregion


        //Private

        #region 完成绘制 —— void CompleteDrawing(OpenTKViewport viewport)
        /// <summary>
        /// 完成绘制
        /// </summary>
        private void CompleteDrawing(OpenTKViewport viewport)
        {
            #region # 验证

            if (this._polyline.Positions.Count < 4)
            {
                this.CancelDrawing(viewport);
                return;
            }

            #endregion

            //移除预览点
            if (this._previewPoint.HasValue && this._polyline != null)
            {
                this._polyline.Positions.RemoveAt(this._polyline.Positions.Count - 1);
                this._previewPoint = null;
            }

            //至少需要2个点才能形成折线（闭合需要至少3个点）
            if (this._polyline != null)
            {
                if (this._closed && this._polyline.Positions.Count < 3)
                {
                    //闭合折线点数不足，取消绘制
                    this.CancelDrawing(viewport);
                    return;
                }
                if (!this._closed && this._polyline.Positions.Count < 2)
                {
                    //开放折线点数不足，取消绘制
                    this.CancelDrawing(viewport);
                    return;
                }
            }

            //设置光标
            viewport.Cursor = new Cursor(StandardCursorType.Arrow);

            //绘制结束
            this._isDrawing = false;
            this.DrawEnd?.Invoke(this._polyline);

            //清空引用，绘制完成
            this._previewPoint = null;
            this._polyline = null;

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 取消绘制 —— void CancelDrawing(OpenTKViewport viewport)
        /// <summary>
        /// 取消绘制
        /// </summary>
        private void CancelDrawing(OpenTKViewport viewport)
        {
            if (this._polyline != null)
            {
                this.DrawCancelled?.Invoke(this._polyline);
            }

            //设置光标
            viewport.Cursor = new Cursor(StandardCursorType.Arrow);

            //绘制结束
            this._isDrawing = false;

            //清空引用
            this._previewPoint = null;
            this._polyline = null;

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 撤销上一点 —— void UndoLastPoint(OpenTKViewport viewport)
        /// <summary>
        /// 撤销上一点
        /// </summary>
        private void UndoLastPoint(OpenTKViewport viewport)
        {
            #region # 验证

            if (this._polyline == null)
            {
                return;
            }

            #endregion

            //移除预览点
            if (this._previewPoint.HasValue)
            {
                this._polyline.Positions.RemoveAt(this._polyline.Positions.Count - 1);
                this._previewPoint = null;
            }

            //移除最后一个固定点
            if (this._polyline.Positions.Count > 1)
            {
                this._polyline.Positions.RemoveAt(this._polyline.Positions.Count - 1);
            }

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion 

        #endregion
    }
}
