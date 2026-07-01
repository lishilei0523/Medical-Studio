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
    /// 曲线区域生长命令
    /// </summary>
    public class CurveRegionGrowCommand : DrawShapeCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// 预览点
        /// </summary>
        private Vector3D? _previewPoint;

        /// <summary>
        /// 种子曲线
        /// </summary>
        private CurveVisual3D _curve;

        /// <summary>
        /// 默认构造器
        /// </summary>
        public CurveRegionGrowCommand()
        {

        }

        #endregion

        #region # 属性

        #region 种子点已拾取委托 —— Action<CurveVisual3D> SeedPointPicked
        /// <summary>
        /// 种子点已拾取委托
        /// </summary>
        public Action<CurveVisual3D> SeedPointPicked { get; set; }
        #endregion

        #region 种子点已变化委托 —— Action<CurveVisual3D> SeedPointChanged
        /// <summary>
        /// 种子点已变化委托
        /// </summary>
        public Action<CurveVisual3D> SeedPointChanged { get; set; }
        #endregion

        #region 区域生长已确认委托 —— Action<CurveVisual3D> RegionGrowConfirmed
        /// <summary>
        /// 区域生长已确认委托
        /// </summary>
        public Action<CurveVisual3D> RegionGrowConfirmed { get; set; }
        #endregion

        #region 区域生长已取消委托 —— Action<CurveVisual3D> RegionGrowCancelled
        /// <summary>
        /// 区域生长已取消委托
        /// </summary>
        public Action<CurveVisual3D> RegionGrowCancelled { get; set; }
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
                    if (this._curve == null)
                    {
                        //第一次点击：创建曲线，添加第一个控制点
                        this._curve = new CurveVisual3D
                        {
                            Fill = ColorFactory.Fill2D.ToColor(),
                            ControlPositions = [position],
                            Closed = false
                        };
                        this._isDrawing = true;
                        this.SeedPointPicked?.Invoke(this._curve);
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
                        this.SeedPointChanged?.Invoke(this._curve);
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

                Vector2 mousePos2D = eventArgs.GetPixelPosition(viewport).ToVector2();
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
        /// <returns>上下文菜单项列表</returns>
        public override IReadOnlyList<ContextMenuItem> GetContextMenuItems(OpenTKViewport viewport, PointerReleasedEventArgs eventArgs)
        {
            List<ContextMenuItem> items =
            [
                new ContextMenuItem
                {
                    Header = "确定(_O)",
                    Command = () => this.Confirm(viewport),
                    IsEnabled = this._curve != null
                },
                new ContextMenuItem
                {
                    Header = "取消(_C)",
                    Command = () => this.Cancel(viewport),
                    IsEnabled = this._curve != null
                }
            ];

            if (this._curve != null && this._curve.ControlPositions.Count > 1)
            {
                items.Add(new ContextMenuItem
                {
                    Header = "撤销上一点(_U)",
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
        /// <remarks>命令被停用时调用，切换命令前</remarks>
        public override void Deactivate()
        {
            base.Deactivate();

            this._isDrawing = false;
            this._previewPoint = null;
            this._curve = null;
        }
        #endregion

        #region 确定 —— void Confirm(OpenTKViewport viewport)
        /// <summary>
        /// 确定
        /// </summary>
        private void Confirm(OpenTKViewport viewport)
        {
            #region # 验证

            if (this._curve.ControlPositions.Count < 4)
            {
                this.Cancel(viewport);
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
                    this.Cancel(viewport);
                    return;
                }
            }

            //设置光标
            viewport.Cursor = new Cursor(StandardCursorType.Arrow);

            //绘制结束
            this._isDrawing = false;
            this.RegionGrowConfirmed?.Invoke(this._curve);

            //清空引用
            this._previewPoint = null;
            this._curve = null;

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 取消 —— void Cancel(OpenTKViewport viewport)
        /// <summary>
        /// 取消
        /// </summary>
        private void Cancel(OpenTKViewport viewport)
        {
            if (this._curve != null)
            {
                this.RegionGrowCancelled?.Invoke(this._curve);
            }

            //绘制结束
            this._isDrawing = false;

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
        /// 撤销上一点
        /// </summary>
        private void UndoLastPoint(OpenTKViewport viewport)
        {
            #region # 验证

            if (this._curve == null)
            {
                return;
            }

            #endregion

            //移除预览点
            if (this._previewPoint.HasValue)
            {
                this._curve.ControlPositions.RemoveAt(this._curve.ControlPositions.Count - 1);
                this._previewPoint = null;
            }

            //移除最后一个控制点
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
