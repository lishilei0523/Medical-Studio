using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Controls.Visual3Ds;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace MedicalSharp.Controls.Commands
{
    /// <summary>
    /// 拾取3D元素命令
    /// </summary>
    public class PickVisual3DCommand : ShapeCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// 默认构造器
        /// </summary>
        public PickVisual3DCommand()
        {

        }

        #endregion

        #region # 属性

        #region 3D元素已拾取委托 —— Action<Visual3D> VisualPicked
        /// <summary>
        /// 3D元素已拾取委托
        /// </summary>
        public Action<Visual3D> VisualPicked { get; set; }
        #endregion

        #region 3D元素已删除委托 —— Action<Visual3D> VisualRemoved
        /// <summary>
        /// 3D元素已删除委托
        /// </summary>
        public Action<Visual3D> VisualRemoved { get; set; }
        #endregion

        #region 获取标记值委托 —— Func<byte> GetMarkValue
        /// <summary>
        /// 获取标记值委托
        /// </summary>
        public Func<byte> GetMarkValue { get; set; }
        #endregion

        #region 形状已切割委托 —— Action ShapeCut
        /// <summary>
        /// 形状已切割委托
        /// </summary>
        public Action ShapeCut { get; set; }
        #endregion

        #region 形状已统计委托 —— Action<StatisticResult> ShapeAnalysed
        /// <summary>
        /// 形状已统计委托
        /// </summary>
        public Action<StatisticResult> ShapeAnalysed { get; set; }
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
                Vector2 mousePos2D = eventArgs.GetPixelPosition(viewport).ToVector2();
                if (pickVisual3D.FindNearest(mousePos2D, out Vector3 point, out _, out Visual3D visual3D, out _))
                {
                    //看向目标
                    if (KeyModifiers.Shift == (eventArgs.KeyModifiers & KeyModifiers.Shift))
                    {
                        viewport.Camera.LookAt(point);
                    }
                }
                else
                {
                    visual3D = null;
                }

                this.VisualPicked?.Invoke(visual3D);

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
            if (viewport is IPickVisual3D pickVisual3D)
            {
                Vector2 mousePos2D = eventArgs.GetPixelPosition(viewport).ToVector2();
                if (pickVisual3D.FindNearest(mousePos2D, out _, out _, out Visual3D visual, out _) &&
                    visual is not IFunctionalVisual3D &&
                    visual is not IFixable)
                {
                    List<ContextMenuItem> items =
                    [
                        new ContextMenuItem
                        {
                            Header = "删除(_D)",
                            Command = () => this.RemoveVisual(viewport, visual)
                        }
                    ];
                    if (visual is ICutVolume cutVolume && this.GetMarkValue != null)
                    {
                        items.Add(new ContextMenuItem
                        {
                            Header = "内切(_I)",
                            Command = () => this.ApplyCut(viewport, cutVolume, CutMode.Inside),
                            IsEnabled = this.GetMarkValue != null
                        });
                        items.Add(new ContextMenuItem
                        {
                            Header = "外切(_O)",
                            Command = () => this.ApplyCut(viewport, cutVolume, CutMode.OutSide),
                            IsEnabled = this.GetMarkValue != null
                        });
                    }
                    if (visual is IAnalyseVolume2D analyseVolume2D && viewport is MPRViewport mprViewport)
                    {
                        items.Add(new ContextMenuItem
                        {
                            Header = "统计(_S)",
                            Command = () => this.ApplyAnalyse2D(mprViewport, analyseVolume2D)
                        });
                    }
                    if (visual is IAnalyseVolume3D analyseVolume3D && viewport is VolumeViewport volumeViewport)
                    {
                        items.Add(new ContextMenuItem
                        {
                            Header = "统计(_S)",
                            Command = () => this.ApplyAnalyse3D(volumeViewport, analyseVolume3D)
                        });
                    }

                    return items;
                }
            }

            return base.GetContextMenuItems(viewport, eventArgs);
        }
        #endregion

        #region 删除元素 —— void RemoveVisual(OpenTKViewport viewport, Visual3D visual)
        /// <summary>
        /// 删除元素
        /// </summary>
        /// <param name="viewport">OpenTK视口</param>
        /// <param name="visual">3D元素</param>
        private void RemoveVisual(OpenTKViewport viewport, Visual3D visual)
        {
            this.VisualRemoved?.Invoke(visual);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region 适用切割 —— void ApplyCut(OpenTKViewport viewport, ICutVolume cutVolume...
        /// <summary>
        /// 适用切割
        /// </summary>
        /// <param name="viewport">OpenTK视口</param>
        /// <param name="cutVolume">切割体积3D元素</param>
        /// <param name="cutMode">切割模式</param>
        private void ApplyCut(OpenTKViewport viewport, ICutVolume cutVolume, CutMode cutMode)
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
                cutVolume.ApplyCutVolume(volumeViewport.VolumeData, volumeViewport.VolumeRenderable.MarkTexture, cutMode, markValue);
            }
            if (viewport is MPRViewport mprViewport)
            {
                cutVolume.ApplyCutVolume(mprViewport.VolumeData, mprViewport.VolumeRenderable.MarkTexture, cutMode, markValue);
            }

            //请求下一帧
            viewport.RequestNextFrameRendering();

            this.ShapeCut?.Invoke();
        }
        #endregion

        #region 适用统计(2D) —— void ApplyAnalyse2D(MPRViewport viewport, IAnalyseVolume2D analyseVolume2D)
        /// <summary>
        /// 适用统计(2D)
        /// </summary>
        /// <param name="viewport">MPR渲染视口</param>
        /// <param name="analyseVolume2D">可统计体积2D元素</param>
        private void ApplyAnalyse2D(MPRViewport viewport, IAnalyseVolume2D analyseVolume2D)
        {
            StatisticResult result = analyseVolume2D.ApplyAnalyseVolume(viewport, null);
            this.ShapeAnalysed?.Invoke(result);
        }
        #endregion

        #region 适用统计(3D) —— void ApplyAnalyse3D(VolumeViewport viewport, IAnalyseVolume3D analyseVolume3D)
        /// <summary>
        /// 适用统计(3D)
        /// </summary>
        /// <param name="viewport">体积渲染视口</param>
        /// <param name="analyseVolume3D">可统计体积3D元素</param>
        private async void ApplyAnalyse3D(VolumeViewport viewport, IAnalyseVolume3D analyseVolume3D)
        {
            StatisticResult result = await analyseVolume3D.ApplyAnalyseVolume(viewport.VolumeData, null);
            this.ShapeAnalysed?.Invoke(result);
        }
        #endregion

        #endregion
    }
}
