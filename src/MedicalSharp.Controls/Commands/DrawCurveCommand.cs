using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace MedicalSharp.Controls.Commands
{
    /// <summary>
    /// 绘制曲线3D元素命令
    /// </summary>
    public class DrawCurveCommand : ViewportCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// 预览点
        /// </summary>
        private Vector3D? _previewPoint;

        /// <summary>
        /// 曲线3D元素
        /// </summary>
        private CurveVisual3D _curve;

        /// <summary>
        /// 是否闭合
        /// </summary>
        private readonly bool _closed;

        /// <summary>
        /// 曲线绘制开始事件
        /// </summary>
        private readonly Action<CurveVisual3D> _curveDrawStartEvent;

        /// <summary>
        /// 曲线绘制结束事件
        /// </summary>
        private readonly Action<CurveVisual3D> _curveDrawEndEvent;

        /// <summary>
        /// 曲线绘制取消事件
        /// </summary>
        private readonly Action<CurveVisual3D> _curveDrawCancelEvent;

        /// <summary>
        /// 创建绘制曲线3D元素命令构造器
        /// </summary>
        /// <param name="drawStart">绘制开始回调</param>
        /// <param name="drawEnd">绘制结束回调</param>
        /// <param name="drawCancel">绘制取消回调</param>
        /// <param name="closed">是否闭合</param>
        public DrawCurveCommand(Action<CurveVisual3D> drawStart, Action<CurveVisual3D> drawEnd, Action<CurveVisual3D> drawCancel, bool closed)
        {
            this._closed = closed;
            this._curveDrawStartEvent = drawStart;
            this._curveDrawEndEvent = drawEnd;
            this._curveDrawCancelEvent = drawCancel;
        }

        #endregion

        #region # 属性

        //

        #endregion

        #region # 方法

        //Public

        #region 鼠标按下事件 —— override void OnMouseDown(OpenTKViewport viewport...
        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        public override void OnMouseDown(OpenTKViewport viewport, PointerPressedEventArgs eventArgs)
        {
            base.OnMouseDown(viewport, eventArgs);
            if (eventArgs.Properties.IsLeftButtonPressed && viewport is BasicViewport basicViewport)
            {
                Vector2 mousePos2D = eventArgs.GetPosition(viewport).ToVector2();
                Vector3? mousePos3D = basicViewport.FindNearestPosition(mousePos2D);
                if (mousePos3D.HasValue)
                {
                    Vector3D position = mousePos3D.Value.ToVector3();
                    if (this._curve == null)
                    {
                        //第一次点击：创建曲线，添加第一个控制点
                        this._curve = new CurveVisual3D
                        {
                            Fill = Color.Parse("#0F00FF00"),
                            ControlPositions = [position],
                            Closed = this._closed
                        };
                        this._curveDrawStartEvent?.Invoke(this._curve);
                    }
                    else
                    {
                        //先移除预览点（如果存在）
                        if (this._previewPoint.HasValue)
                        {
                            this._curve.ControlPositions.RemoveAt(this._curve.ControlPositions.Count - 1);
                            this._previewPoint = null;
                        }
                        this._curve.ControlPositions.Add(position);
                    }

                    //请求下一帧
                    viewport.RequestNextFrameRendering();
                }
            }
        }
        #endregion

        #region 鼠标移动事件 —— override void OnMouseMove(OpenTKViewport viewport...
        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        public override void OnMouseMove(OpenTKViewport viewport, PointerEventArgs eventArgs)
        {
            base.OnMouseMove(viewport, eventArgs);
            if (this._curve != null && viewport is BasicViewport basicViewport)
            {
                //设置光标
                viewport.Cursor = new Cursor(StandardCursorType.Cross);

                Vector2 mousePos2D = eventArgs.GetPosition(viewport).ToVector2();
                Vector3? mousePos3D = basicViewport.FindNearestPosition(mousePos2D);
                if (mousePos3D.HasValue)
                {
                    Vector3D currentPos = mousePos3D.Value.ToVector3();

                    //预览更新：替换最后一个点（预览点）
                    if (this._previewPoint.HasValue)
                    {
                        this._curve.ControlPositions[^1] = currentPos;
                    }
                    else
                    {
                        this._curve.ControlPositions.Add(currentPos);
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
        public override IReadOnlyList<ContextMenuItem> GetContextMenuItems(OpenTKViewport viewport, PointerReleasedEventArgs eventArgs)
        {
            List<ContextMenuItem> items =
            [
                new ContextMenuItem
                {
                    Header = "完成",
                    Command = () => this.CompleteDrawing(viewport),
                    IsEnabled = this._curve != null
                },
                new ContextMenuItem
                {
                    Header = "取消",
                    Command = () => this.CancelDrawing(viewport),
                    IsEnabled = this._curve != null
                }
            ];

            if (this._curve != null && this._curve.ControlPositions.Count > 1)
            {
                items.Add(new ContextMenuItem
                {
                    Header = "撤销上一点",
                    Command = () => this.UndoLastPoint(viewport)
                });
            }

            return items;
        }
        #endregion

        #region 失效命令 —— override void Deactivate()
        /// <summary>
        /// 失效命令
        /// </summary>
        public override void Deactivate()
        {
            base.Deactivate();
            this._previewPoint = null;
            this._curve = null;
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

            if (this._curve.ControlPositions.Count < 4)
            {
                this.CancelDrawing(viewport);
                return;
            }

            #endregion

            //移除预览点
            if (this._previewPoint.HasValue && this._curve != null)
            {
                this._curve.ControlPositions.RemoveAt(this._curve.ControlPositions.Count - 1);
                this._previewPoint = null;
            }

            //检查点数要求
            if (this._curve != null)
            {
                //Catmull-Rom曲线至少需要3个控制点才能生成有效曲线
                //1个点：显示点
                //2个点：显示直线
                //3个点及以上：显示曲线
                if (this._curve.ControlPositions.Count < 3)
                {
                    //点数不足，取消绘制
                    this.CancelDrawing(viewport);
                    return;
                }
            }

            //设置光标
            viewport.Cursor = new Cursor(StandardCursorType.Arrow);

            //绘制结束
            this._curveDrawEndEvent?.Invoke(this._curve);

            //清空引用
            this._previewPoint = null;
            this._curve = null;

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
            if (this._curve != null)
            {
                this._curveDrawCancelEvent?.Invoke(this._curve);
            }

            //设置光标
            viewport.Cursor = new Cursor(StandardCursorType.Arrow);

            //清空引用
            this._previewPoint = null;
            this._curve = null;

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 撤销上一点 —— void UndoLastPoint(OpenTKViewport viewport)
        /// <summary>
        /// 撤销上一个点
        /// </summary>
        private void UndoLastPoint(OpenTKViewport viewport)
        {
            if (this._curve == null)
            {
                return;
            }

            //移除预览点
            if (this._previewPoint.HasValue)
            {
                this._curve.ControlPositions.RemoveAt(this._curve.ControlPositions.Count - 1);
                this._previewPoint = null;
            }

            //移除最后一个固定点
            if (this._curve.ControlPositions.Count > 1)
            {
                this._curve.ControlPositions.RemoveAt(this._curve.ControlPositions.Count - 1);
            }

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #endregion
    }
}
