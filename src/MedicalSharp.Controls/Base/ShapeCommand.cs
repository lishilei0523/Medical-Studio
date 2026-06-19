using Avalonia.Input;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Controls.Visual3Ds;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedicalSharp.Controls.Base
{
    /// <summary>
    /// 形状命令
    /// </summary>
    public abstract class ShapeCommand : ViewportCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// 默认构造器
        /// </summary>
        protected ShapeCommand()
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

        #region 获取标记值委托 —— Func<Task<byte>> GetMarkValue
        /// <summary>
        /// 获取标记值委托
        /// </summary>
        public Func<Task<byte>> GetMarkValue { get; set; }
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
            if (eventArgs.Properties.IsLeftButtonPressed || eventArgs.Properties.IsRightButtonPressed)
            {
                if (viewport is IPickVisual3D pickVisual3D)
                {
                    Vector2 mousePos2D = eventArgs.GetPixelPosition(viewport).ToVector2();
                    if (pickVisual3D.FindNearest(mousePos2D, out Vector3 point, out _, out Visual3D visual3D, out _))
                    {
                        //看向目标
                        if (eventArgs.Properties.IsLeftButtonPressed && KeyModifiers.Shift == (eventArgs.KeyModifiers & KeyModifiers.Shift))
                        {
                            viewport.Camera.LookAt(point);

                            //请求下一帧
                            viewport.RequestNextFrameRendering();
                        }
                    }
                    else
                    {
                        visual3D = null;
                    }

                    this.VisualPicked?.Invoke(visual3D);
                }
            }
        }
        #endregion

        #region 获取上下文菜单项列表 —— override IReadOnlyList<ContextMenuItem> GetContextMenuItems(...
        /// <summary>
        /// 获取上下文菜单项列表
        /// </summary>
        /// <returns>上下文菜单项列表</returns>
        /// <remarks>右键点击松开时调用，返回null或空列表表示不弹出菜单</remarks>
        public override IReadOnlyList<ContextMenuItem> GetContextMenuItems(OpenTKViewport viewport, PointerReleasedEventArgs eventArgs)
        {
            if (viewport is IPickVisual3D pickVisual3D)
            {
                Vector2 mousePos2D = eventArgs.GetPixelPosition(viewport).ToVector2();
                if (pickVisual3D.FindNearest(mousePos2D, out _, out _, out Visual3D visual, out _))
                {
                    return visual.GetContextMenuItems(viewport, this);
                }
            }

            return base.GetContextMenuItems(viewport, eventArgs);
        }
        #endregion

        #endregion
    }
}
