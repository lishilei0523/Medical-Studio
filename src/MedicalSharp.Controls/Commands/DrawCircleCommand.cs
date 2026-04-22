using Avalonia;
using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace MedicalSharp.Controls.Commands
{
    /// <summary>
    /// 绘制圆形3D元素命令
    /// </summary>
    public class DrawCircleCommand : ViewportCommand
    {
        /// <summary>
        /// 起始位置
        /// </summary>
        private Vector3? _startPosition;

        /// <summary>
        /// 圆形3D元素
        /// </summary>
        private CircleVisual3D _circle;

        /// <summary>
        /// 法向量
        /// </summary>
        private readonly Vector3D _normal;

        /// <summary>
        /// 圆形绘制完成事件
        /// </summary>
        private readonly Action<CircleVisual3D> _circleDrawnEvent;

        /// <summary>
        /// 创建绘制圆形3D元素命令构造器
        /// </summary>
        /// <param name="normal">法向量</param>
        /// <param name="callback">绘制回调</param>
        public DrawCircleCommand(Vector3D normal, Action<CircleVisual3D> callback)
        {
            this._normal = normal;
            this._circleDrawnEvent = callback;
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
                    this._circle = new CircleVisual3D
                    {
                        Center = mousePos3D.Value.ToVector3(),
                        Normal = this._normal
                    };
                    this._circleDrawnEvent?.Invoke(this._circle);
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
                this._circle != null)
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
                    this._circle.Radius = side / 2.0f;

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
            this._circle = null;

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }

        /// <summary>
        /// 测试右键菜单
        /// </summary>
        public override IReadOnlyList<ContextMenuItem> GetContextMenuItems(OpenTKViewport viewport, PointerReleasedEventArgs eventArgs)
        {
            List<ContextMenuItem> items =
            [
                new ContextMenuItem
                {
                    Header = $"属性"
                },
                ContextMenuItem.CreateSeparator(),
                new ContextMenuItem
                {
                    Header = "删除"
                },
                new ContextMenuItem
                {
                    Header = "复制"
                },
                ContextMenuItem.CreateSeparator(),
                new ContextMenuItem
                {
                    Header = "重置相机"
                }
            ];

            return items;
        }
    }
}
