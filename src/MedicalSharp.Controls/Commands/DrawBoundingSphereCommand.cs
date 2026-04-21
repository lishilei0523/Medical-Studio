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
    /// 绘制包围球3D元素命令
    /// </summary>
    public class DrawBoundingSphereCommand : ViewportCommand
    {
        /// <summary>
        /// 起始位置
        /// </summary>
        private Vector3? _startPosition;

        /// <summary>
        /// 包围球3D元素
        /// </summary>
        private BoundingSphereVisual3D _boundingSphere;

        /// <summary>
        /// 包围球绘制完成事件
        /// </summary>
        private readonly Action<BoundingSphereVisual3D> _sphereDrawnEvent;

        /// <summary>
        /// 创建绘制包围球3D元素命令构造器
        /// </summary>
        /// <param name="callback">绘制回调</param>
        public DrawBoundingSphereCommand(Action<BoundingSphereVisual3D> callback)
        {
            this._sphereDrawnEvent = callback;
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
                    this._boundingSphere = new BoundingSphereVisual3D
                    {
                        Center = mousePos3D.Value.ToVector3()
                    };
                    this._sphereDrawnEvent?.Invoke(this._boundingSphere);
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
                this._boundingSphere != null)
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
                    this._boundingSphere.Radius = side / 2.0f;

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
            this._boundingSphere = null;

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
    }
}
