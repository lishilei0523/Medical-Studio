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
    /// 单点区域生长命令
    /// </summary>
    public class MonoRegionGrowCommand : DrawShapeCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// 当前种子点
        /// </summary>
        private PointVisual3D _currentSeedPoint;

        /// <summary>
        /// 上一种子点
        /// </summary>
        private PointVisual3D _prevSeedPoint;

        /// <summary>
        /// 默认构造器
        /// </summary>
        public MonoRegionGrowCommand()
        {

        }

        #endregion

        #region # 属性

        #region 种子点已拾取委托 —— Action<PointVisual3D, PointVisual3D> SeedPointPicked
        /// <summary>
        /// 种子点已拾取委托
        /// </summary>
        /// <remarks>当前种子点, 上一种子点</remarks>
        public Action<PointVisual3D, PointVisual3D> SeedPointPicked { get; set; }
        #endregion

        #region 区域生长已确认委托 —— Action<PointVisual3D, PointVisual3D> RegionGrowConfirmed
        /// <summary>
        /// 区域生长已确认委托
        /// </summary>
        /// <remarks>当前种子点, 上一种子点</remarks>
        public Action<PointVisual3D, PointVisual3D> RegionGrowConfirmed { get; set; }
        #endregion

        #region 区域生长已取消委托 —— Action<PointVisual3D, PointVisual3D> RegionGrowCancelled
        /// <summary>
        /// 区域生长已取消委托
        /// </summary>
        /// <remarks>当前种子点, 上一种子点</remarks>
        public Action<PointVisual3D, PointVisual3D> RegionGrowCancelled { get; set; }
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
                    this._prevSeedPoint = this._currentSeedPoint;
                    this._currentSeedPoint = new PointVisual3D
                    {
                        Fill = ColorFactory.PointColor.ToColor(),
                        Position = mousePos3D.Value.ToVector3(),
                        PointSize = 5
                    };
                    this.SeedPointPicked?.Invoke(this._currentSeedPoint, this._prevSeedPoint);

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
                    Command = () => this.RegionGrowConfirmed?.Invoke(this._currentSeedPoint, this._prevSeedPoint),
                    IsEnabled = this._currentSeedPoint != null
                },
                new ContextMenuItem
                {
                    Header = "取消(_C)",
                    Command = () => this.RegionGrowCancelled?.Invoke(this._currentSeedPoint, this._prevSeedPoint),
                    IsEnabled = this._currentSeedPoint != null
                }
            ];

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

            this._currentSeedPoint = null;
            this._prevSeedPoint = null;
        }
        #endregion

        #endregion
    }
}
