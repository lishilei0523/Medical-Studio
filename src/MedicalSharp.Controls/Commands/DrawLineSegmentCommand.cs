using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Controls.Visuals;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Controls.Commands
{
    /// <summary>
    /// 绘制线段3D元素命令
    /// </summary>
    public class DrawLineSegmentCommand : ViewportCommand
    {
        /// <summary>
        /// 起始位置
        /// </summary>
        private Vector3? _startPosition;

        /// <summary>
        /// 线段3D元素
        /// </summary>
        private LineSegmentVisual3D _lineSegment;

        /// <summary>
        /// 线段绘制完成事件
        /// </summary>
        private readonly Action<LineSegmentVisual3D> _lineSegmentDrawnEvent;

        /// <summary>
        /// 创建绘制线段3D元素命令构造器
        /// </summary>
        /// <param name="callback">绘制回调</param>
        public DrawLineSegmentCommand(Action<LineSegmentVisual3D> callback)
        {
            this._lineSegmentDrawnEvent = callback;
        }

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
                    this._startPosition = mousePos3D.Value;
                    this._lineSegment = new LineSegmentVisual3D
                    {
                        StartPoint = mousePos3D.Value.ToVector3()
                    };
                    this._lineSegmentDrawnEvent?.Invoke(this._lineSegment);
                }
            }
        }

        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        public override void OnMouseMove(OpenTKViewport viewport, PointerEventArgs eventArgs)
        {
            base.OnMouseMove(viewport, eventArgs);
            if (eventArgs.Properties.IsLeftButtonPressed && viewport is BasicViewport basicViewport && this._lineSegment != null)
            {
                //设置光标
                viewport.Cursor = new Cursor(StandardCursorType.Cross);

                Vector2 mousePos2D = eventArgs.GetPosition(viewport).ToVector2();
                Vector3? mousePos3D = basicViewport.FindNearestPosition(mousePos2D);
                if (mousePos3D.HasValue)
                {
                    this._lineSegment.EndPoint = mousePos3D.Value.ToVector3();

                    //请求下一帧
                    viewport.RequestNextFrameRendering();
                }
            }
        }

        /// <summary>
        /// 鼠标松开事件
        /// </summary>
        public override void OnMouseUp(OpenTKViewport viewport, PointerReleasedEventArgs eventArgs)
        {
            base.OnMouseUp(viewport, eventArgs);

            //设置光标
            viewport.Cursor = new Cursor(StandardCursorType.Arrow);

            //清空
            this._startPosition = null;
            this._lineSegment = null;

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
    }
}
