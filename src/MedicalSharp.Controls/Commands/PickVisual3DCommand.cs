using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Commands.Arguments;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace MedicalSharp.Controls.Commands
{
    /// <summary>
    /// 拾取3D元素命令
    /// </summary>
    public class PickVisual3DCommand : ViewportCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// 选中的3D元素
        /// </summary>
        private Visual3D _selectedVisual;

        /// <summary>
        /// 3D元素拾取事件
        /// </summary>
        private readonly Action<Visual3DPickedEventArgs> _visual3DPickedEvent;

        /// <summary>
        /// 3D元素删除事件
        /// </summary>
        private readonly Action<Visual3D> _visual3DRemovedEvent;

        /// <summary>
        /// 创建拾取3D元素命令构造器
        /// </summary>
        /// <param name="picked">3D元素拾取回调</param>
        /// <param name="removed">3D元素删除回调</param>
        public PickVisual3DCommand(Action<Visual3DPickedEventArgs> picked, Action<Visual3D> removed)
        {
            this._visual3DPickedEvent = picked;
            this._visual3DRemovedEvent = removed;
        }

        #endregion

        #region # 属性

        //

        #endregion

        #region # 方法

        #region 鼠标按下事件 —— override void OnMouseDown(OpenTKViewport viewport...
        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        public override void OnMouseDown(OpenTKViewport viewport, PointerPressedEventArgs eventArgs)
        {
            base.OnMouseDown(viewport, eventArgs);
            if (eventArgs.Properties.IsLeftButtonPressed && viewport is IPickVisual3D pickVisual3D)
            {
                Vector2 mousePos2D = eventArgs.GetPosition(viewport).ToVector2();
                Visual3DPickedEventArgs commandEventArgs = new Visual3DPickedEventArgs
                {
                    Viewport = viewport,
                    MousePos2D = mousePos2D
                };
                if (pickVisual3D.FindNearest(mousePos2D, out Vector3 point, out Vector3 normal, out Visual3D visual, out Ray ray))
                {
                    this._selectedVisual = visual;
                    commandEventArgs.HitPoint = point;
                    commandEventArgs.Normal = normal;
                    commandEventArgs.PickedVisual = visual;
                    commandEventArgs.Ray = ray;

                    viewport.Camera.LookAt(point);
                }
                else
                {
                    commandEventArgs.PickedVisual = null;
                }

                this._visual3DPickedEvent?.Invoke(commandEventArgs);

                //请求下一帧
                viewport.RequestNextFrameRendering();
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
            List<ContextMenuItem> items = [];
            if (this._selectedVisual != null)
            {
                items.Add(new ContextMenuItem
                {
                    Header = "删除",
                    Command = () => this.RemoveVisual(viewport)
                });
                items.Add(new ContextMenuItem
                {
                    Header = "挖空",
                    Command = () => this.ApplyMark(viewport)
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

            this._selectedVisual = null;
        }
        #endregion

        #region 删除元素 —— void RemoveVisual(OpenTKViewport viewport)
        /// <summary>
        /// 删除元素
        /// </summary>
        private void RemoveVisual(OpenTKViewport viewport)
        {
            if (this._selectedVisual != null)
            {
                this._visual3DRemovedEvent?.Invoke(this._selectedVisual);

                //清空引用
                this._selectedVisual = null;
            }

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 适用标记 —— void ApplyMark(OpenTKViewport viewport)
        /// <summary>
        /// 适用标记
        /// </summary>
        private void ApplyMark(OpenTKViewport viewport)
        {
            if (this._selectedVisual is BoundingBoxVisual3D box && viewport is VolumeViewport volumeViewport)
            {
                volumeViewport.VolumeRenderable.ApplyBoxROI(box.Minimum, box.Maximum, box.Transform.Matrix, 1);
                volumeViewport.VolumeRenderer.MarkStrategy.SwitchMarkMode(1, MarkMode.Collapsed);
                volumeViewport.VolumeRenderable.SyncMarkDataFromGpu();

                //请求下一帧
                viewport.RequestNextFrameRendering();
            }
        }
        #endregion

        #endregion
    }
}
