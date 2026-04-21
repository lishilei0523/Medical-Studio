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
    /// 绘制包围盒3D元素命令
    /// </summary>
    public class DrawBoundingBoxCommand : ViewportCommand
    {
        /// <summary>
        /// 起始位置
        /// </summary>
        private Vector3? _startPosition;

        /// <summary>
        /// 包围盒3D元素
        /// </summary>
        private BoundingBoxVisual3D _boundingBox;

        /// <summary>
        /// 包围盒绘制完成事件
        /// </summary>
        private readonly Action<BoundingBoxVisual3D> _boxDrawnEvent;

        /// <summary>
        /// 创建绘制包围盒3D元素命令构造器
        /// </summary>
        /// <param name="callback">绘制回调</param>
        public DrawBoundingBoxCommand(Action<BoundingBoxVisual3D> callback)
        {
            this._boxDrawnEvent = callback;
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
                    this._boundingBox = new BoundingBoxVisual3D
                    {
                        Center = mousePos3D.Value.ToVector3()
                    };
                    this._boxDrawnEvent?.Invoke(this._boundingBox);
                }
            }
        }

        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        public override void OnMouseMove(OpenTKViewport viewport, PointerEventArgs eventArgs)
        {
            base.OnMouseMove(viewport, eventArgs);
            if (eventArgs.Properties.IsLeftButtonPressed &&
                viewport is BasicViewport basicViewport &&
                this._startPosition.HasValue &&
                this._boundingBox != null)
            {
                //设置光标
                viewport.Cursor = new Cursor(StandardCursorType.Cross);

                Vector2 mousePos2D = eventArgs.GetPosition(viewport).ToVector2();
                Vector3? mousePos3D = basicViewport.FindNearestPosition(mousePos2D);
                if (mousePos3D.HasValue)
                {
                    float offsetX = Math.Abs(mousePos3D.Value.X - this._startPosition.Value.X);
                    float offsetY = Math.Abs(mousePos3D.Value.Y - this._startPosition.Value.Y);
                    float offsetZ = Math.Abs(mousePos3D.Value.Z - this._startPosition.Value.Z);
                    float side = Math.Max(offsetX, Math.Max(offsetY, offsetZ));
                    this._boundingBox.Width = side;
                    this._boundingBox.Height = side;
                    this._boundingBox.Depth = side;

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
            this._boundingBox = null;

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
    }
}
