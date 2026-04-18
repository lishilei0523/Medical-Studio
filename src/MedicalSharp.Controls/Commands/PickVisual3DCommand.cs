using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Commands.Arguments;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Primitives.Maths;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Controls.Commands
{
    /// <summary>
    /// 拾取3D元素命令
    /// </summary>
    public class PickVisual3DCommand : ViewportCommand
    {
        /// <summary>
        /// 3D元素拾取事件
        /// </summary>
        private readonly Action<Visual3DPickedEventArgs> _visual3DPicked;

        /// <summary>
        /// 创建拾取3D元素命令构造器
        /// </summary>
        /// <param name="callback">3D元素拾取回调</param>
        public PickVisual3DCommand(Action<Visual3DPickedEventArgs> callback)
        {
            this._visual3DPicked = callback;
        }

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
                    commandEventArgs.HitPoint = point;
                    commandEventArgs.Normal = normal;
                    commandEventArgs.PickedVisual = visual;
                    commandEventArgs.Ray = ray;
                }
                else
                {
                    commandEventArgs.PickedVisual = null;
                }

                this._visual3DPicked?.Invoke(commandEventArgs);
            }
        }
    }
}
