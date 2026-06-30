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
    /// 多点区域生长命令
    /// </summary>
    public class PolyRegionGrowCommand : DrawShapeCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// 种子点云
        /// </summary>
        private PointCloudVisual3D _pointCloud;

        /// <summary>
        /// 默认构造器
        /// </summary>
        public PolyRegionGrowCommand()
        {

        }

        #endregion

        #region # 属性

        #region 种子点已拾取委托 —— Action<PointCloudVisual3D> SeedPointPicked
        /// <summary>
        /// 种子点已拾取委托
        /// </summary>
        public Action<PointCloudVisual3D> SeedPointPicked { get; set; }
        #endregion

        #region 种子点已变化委托 —— Action<PointCloudVisual3D> SeedPointChanged
        /// <summary>
        /// 种子点已变化委托
        /// </summary>
        public Action<PointCloudVisual3D> SeedPointChanged { get; set; }
        #endregion

        #region 区域生长已确认委托 —— Action<PointCloudVisual3D> RegionGrowConfirmed
        /// <summary>
        /// 区域生长已确认委托
        /// </summary>
        public Action<PointCloudVisual3D> RegionGrowConfirmed { get; set; }
        #endregion

        #region 区域生长已取消委托 —— Action<PointCloudVisual3D> RegionGrowCancelled
        /// <summary>
        /// 区域生长已取消委托
        /// </summary>
        public Action<PointCloudVisual3D> RegionGrowCancelled { get; set; }
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
                    if (this._pointCloud == null)
                    {
                        //第一次点击：创建点云，添加第一个种子点
                        this._pointCloud = new PointCloudVisual3D()
                        {
                            Fill = ColorFactory.PointColor.ToColor(),
                            Positions = [position]
                        };
                        this._isDrawing = true;
                        this.SeedPointPicked?.Invoke(this._pointCloud);
                    }
                    else
                    {
                        //后续点击：添加种子点
                        this._pointCloud.Positions.Add(position);
                        this.SeedPointChanged?.Invoke(this._pointCloud);
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
                    Command = this.Confirm,
                    IsEnabled = this._pointCloud != null
                },
                new ContextMenuItem
                {
                    Header = "取消(_C)",
                    Command = this.Cancel,
                    IsEnabled = this._pointCloud != null
                }
            ];

            if (this._pointCloud != null && this._pointCloud.Positions.Count > 1)
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
            this._pointCloud = null;
        }
        #endregion

        #region 确定 —— void Confirm()
        /// <summary>
        /// 确定
        /// </summary>
        private void Confirm()
        {
            this._isDrawing = false;
            this.RegionGrowConfirmed?.Invoke(this._pointCloud);
            this._pointCloud = null;
        }
        #endregion

        #region 取消 —— void Cancel()
        /// <summary>
        /// 取消
        /// </summary>
        private void Cancel()
        {
            this._isDrawing = false;
            this.RegionGrowCancelled?.Invoke(this._pointCloud);
            this._pointCloud = null;
        }
        #endregion

        #region 撤销上一点 —— void UndoLastPoint(OpenTKViewport viewport)
        /// <summary>
        /// 撤销上一点
        /// </summary>
        private void UndoLastPoint(OpenTKViewport viewport)
        {
            #region # 验证

            if (this._pointCloud == null)
            {
                return;
            }

            #endregion

            //移除最后一个种子点
            if (this._pointCloud.Positions.Count > 1)
            {
                this._pointCloud.Positions.RemoveAt(this._pointCloud.Positions.Count - 1);
                this.SeedPointChanged?.Invoke(this._pointCloud);
            }

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion 

        #endregion
    }
}
