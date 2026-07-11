using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Controls.Visual3Ds;
using MedicalSharp.Primitives.Interfaces;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Controls.Commands
{
    /// <summary>
    /// 拖拽曲线引导线命令
    /// </summary>
    /// <remarks>
    /// 沿曲线弧长方向拖拽CurveGuide，仅更新ArcPosition（一维约束）；
    /// 对标TranslateVisual3DCommand，但拖拽方向约束为弧长方向；
    /// </remarks>
    public class DragCurveGuideCommand : EditShapeCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// 选中的3D元素
        /// </summary>
        private IDraggableAlongCurve _selectedVisual;

        /// <summary>
        /// 默认构造器
        /// </summary>
        public DragCurveGuideCommand()
        {
            this._selectedVisual = null;
        }

        #endregion

        #region # 属性

        #region 拖拽中委托 —— Action<ITranslatableCurve> ArcPositionChanging
        /// <summary>
        /// 拖拽中委托
        /// </summary>
        public Action<IDraggableAlongCurve> ArcPositionChanging { get; set; }
        #endregion

        #region 已拖拽委托 —— Action<ITranslatableCurve> ArcPositionChanged
        /// <summary>
        /// 已拖拽委托
        /// </summary>
        public Action<IDraggableAlongCurve> ArcPositionChanged { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 鼠标按下事件 —— override void OnMouseDown(OpenTKViewport viewport...
        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        public override void OnMouseDown(OpenTKViewport viewport, PointerPressedEventArgs eventArgs)
        {
            if (eventArgs.Properties.IsLeftButtonPressed && viewport is IPickVisual3D pickVisual3D)
            {
                Vector2 mousePos2D = eventArgs.GetPixelPosition(viewport).ToVector2();
                bool success = pickVisual3D.FindNearest(mousePos2D, out _, out _, out Visual3D visual3D, out _);
                if (success && visual3D is IDraggableAlongCurve draggableAlongCurve)
                {
                    #region # 验证

                    if (visual3D is IFixable { Fixed: true })
                    {
                        return;
                    }

                    #endregion

                    this._selectedVisual = draggableAlongCurve;
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
            if (eventArgs.Properties.IsLeftButtonPressed && this._selectedVisual != null)
            {
                //设置光标
                viewport.Cursor = new Cursor(StandardCursorType.DragMove);

                //屏幕坐标 -> UV（和顶点着色器一致：UV = aPos.xy + 0.5）
                Vector2 mousePos2D = eventArgs.GetPixelPosition(viewport).ToVector2();
                float ndcX = (2.0f * mousePos2D.X) / viewport.ViewportSize.Width - 1.0f;
                float uvX = ndcX * 0.5f + 0.5f;

                //UV.x -> 弧长归一化值（限制在0~1）
                float arcPosition = Math.Clamp(uvX, 0f, 1f);

                //更新弧长位置
                this._selectedVisual.ArcPosition = arcPosition;

                //拖拽中
                this.ArcPositionChanging?.Invoke(this._selectedVisual);

                //请求下一帧
                viewport.RequestNextFrameRendering();
            }
        }
        #endregion

        #region 鼠标松开事件 —— override void OnMouseUp(OpenTKViewport viewport...
        /// <summary>
        /// 鼠标松开事件
        /// </summary>
        public override void OnMouseUp(OpenTKViewport viewport, PointerReleasedEventArgs eventArgs)
        {
            base.OnMouseUp(viewport, eventArgs);

            //设置光标
            viewport.Cursor = new Cursor(StandardCursorType.Arrow);

            //拖拽结束
            this.ArcPositionChanged?.Invoke(this._selectedVisual);

            //清空选中
            this._selectedVisual = null;

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #endregion
    }
}
