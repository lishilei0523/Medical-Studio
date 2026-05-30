using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Commands.Arguments;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Controls.Visual3Ds;
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

        #region 获取标记值委托 —— Func<byte> GetMarkValue
        /// <summary>
        /// 获取标记值委托
        /// </summary>
        public Func<byte> GetMarkValue { get; set; }
        #endregion

        #region 切割结束委托 —— Action CutEnd
        /// <summary>
        /// 切割结束委托
        /// </summary>
        public Action CutEnd { get; set; }
        #endregion

        #region 统计结束委托 —— Action<StatisticResult> AnalyseEnd
        /// <summary>
        /// 统计结束委托
        /// </summary>
        public Action<StatisticResult> AnalyseEnd { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 鼠标按下事件 —— override void OnMouseDown(OpenTKViewport viewport...
        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        public override void OnMouseDown(OpenTKViewport viewport, PointerPressedEventArgs eventArgs)
        {
            base.OnMouseDown(viewport, eventArgs);
            if (viewport is IPickVisual3D pickVisual3D)
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

                    //看向目标
                    if (KeyModifiers.Shift == (eventArgs.KeyModifiers & KeyModifiers.Shift))
                    {
                        viewport.Camera.LookAt(point);
                    }
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
                    Header = "删除(_D)",
                    Command = () => this.RemoveVisual(viewport)
                });
                items.Add(new ContextMenuItem
                {
                    Header = "内切(_I)",
                    Command = () => this.ApplyCut(viewport, CutMode.Inside),
                    IsEnabled = this._selectedVisual is ICutVolume && this.GetMarkValue != null
                });
                items.Add(new ContextMenuItem
                {
                    Header = "外切(_O)",
                    Command = () => this.ApplyCut(viewport, CutMode.OutSide),
                    IsEnabled = this._selectedVisual is ICutVolume && this.GetMarkValue != null
                });
                items.Add(new ContextMenuItem
                {
                    Header = "统计(_S)",
                    Command = () => this.ApplyAnalyse(viewport),
                    IsEnabled = (this._selectedVisual is IAnalyseVolume2D && viewport is MPRViewport) ||
                                (this._selectedVisual is IAnalyseVolume3D && viewport is VolumeViewport)
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

        #region 适用切割 —— void ApplyCut(OpenTKViewport viewport, CutMode cutMode)
        /// <summary>
        /// 适用切割
        /// </summary>
        /// <param name="viewport">OpenTK视口</param>
        /// <param name="cutMode">切割模式</param>
        private void ApplyCut(OpenTKViewport viewport, CutMode cutMode)
        {
            if (this._selectedVisual is ICutVolume cutVolume)
            {
                #region # 验证

                if (this.GetMarkValue == null)
                {
                    return;
                }

                #endregion

                byte markValue = this.GetMarkValue.Invoke();

                #region # 验证

                if (markValue == 0)
                {
                    return;
                }

                #endregion

                if (viewport is VolumeViewport volumeViewport)
                {
                    cutVolume.ApplyCutVolume(volumeViewport.VolumeRenderable, cutMode, markValue);
                }
                if (viewport is MPRViewport mprViewport)
                {
                    cutVolume.ApplyCutVolume(mprViewport.VolumeRenderable, cutMode, markValue);
                }

                //请求下一帧
                viewport.RequestNextFrameRendering();

                this.CutEnd?.Invoke();
            }
        }
        #endregion

        #region 适用统计 —— void ApplyAnalyse(OpenTKViewport viewport)
        /// <summary>
        /// 适用统计
        /// </summary>
        /// <param name="viewport">OpenTK视口</param>
        private async void ApplyAnalyse(OpenTKViewport viewport)
        {
            if (this._selectedVisual is IAnalyseVolume2D analyseVolume2D && viewport is MPRViewport mprViewport)
            {
                StatisticResult result = analyseVolume2D.ApplyAnalyseVolume(mprViewport, null);
                this.AnalyseEnd?.Invoke(result);
            }
            if (this._selectedVisual is IAnalyseVolume3D analyseVolume3D && viewport is VolumeViewport volumeViewport)
            {
                StatisticResult result = await analyseVolume3D.ApplyAnalyseVolume(volumeViewport.VolumeRenderable, null);
                this.AnalyseEnd?.Invoke(result);
            }
        }
        #endregion

        #endregion
    }
}
