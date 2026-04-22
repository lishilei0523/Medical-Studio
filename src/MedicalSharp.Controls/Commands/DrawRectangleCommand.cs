using Avalonia;
using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Controls.Visuals;
using OpenTK.Mathematics;
using System;
using System.Linq;

namespace MedicalSharp.Controls.Commands
{
    /// <summary>
    /// 绘制矩形3D元素命令
    /// </summary>
    public class DrawRectangleCommand : ViewportCommand
    {
        /// <summary>
        /// 起始位置
        /// </summary>
        private Vector3? _startPosition;

        /// <summary>
        /// 矩形3D元素
        /// </summary>
        private RectangleVisual3D _rectangle;

        /// <summary>
        /// 法向量
        /// </summary>
        private readonly Vector3D _normal;

        /// <summary>
        /// 矩形绘制完成事件
        /// </summary>
        private readonly Action<RectangleVisual3D> _rectangleDrawnEvent;

        /// <summary>
        /// 创建绘制矩形3D元素命令构造器
        /// </summary>
        /// <param name="normal">法向量</param>
        /// <param name="callback">绘制回调</param>
        public DrawRectangleCommand(Vector3D normal, Action<RectangleVisual3D> callback)
        {
            this._normal = normal;
            this._rectangleDrawnEvent = callback;
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
                    this._rectangle = new RectangleVisual3D
                    {
                        Center = mousePos3D.Value.ToVector3(),
                        Normal = this._normal
                    };
                    this._rectangleDrawnEvent?.Invoke(this._rectangle);
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
                this._rectangle != null)
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
                    float[] sides = new[] { offsetX, offsetY, offsetZ }.OrderByDescending(x => x).ToArray();
                    this._rectangle.Width = sides[0];
                    this._rectangle.Height = sides[1];

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
            this._rectangle = null;

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
    }
}
