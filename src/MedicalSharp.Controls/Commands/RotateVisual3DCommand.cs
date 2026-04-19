using Avalonia;
using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Primitives.Maths;
using OpenTK.Mathematics;

namespace MedicalSharp.Controls.Commands
{
    /// <summary>
    /// 旋转3D元素命令
    /// </summary>
    public class RotateVisual3DCommand : ViewportCommand
    {
        /// <summary>
        /// 选中的3D元素
        /// </summary>
        private Visual3D _selectedVisual;

        /// <summary>
        /// 默认构造器
        /// </summary>
        public RotateVisual3DCommand()
        {
            this._selectedVisual = null;
        }

        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        public override void OnMouseDown(OpenTKViewport viewport, PointerPressedEventArgs eventArgs)
        {
            base.OnMouseDown(viewport, eventArgs);
            if (eventArgs.Properties.IsLeftButtonPressed && viewport is IPickVisual3D pickVisual3D)
            {
                Point mousePos2D = eventArgs.GetPosition(viewport);
                bool success = pickVisual3D.FindNearest(mousePos2D.ToVector2(), out _, out _, out Visual3D visual3D, out _);
                if (success)
                {
                    this._selectedVisual = visual3D;
                }
            }
        }

        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        public override void OnMouseMove(OpenTKViewport viewport, PointerEventArgs eventArgs)
        {
            base.OnMouseMove(viewport, eventArgs);
            if (eventArgs.Properties.IsLeftButtonPressed && this._selectedVisual != null)
            {
                //计算模型位置
                Matrix4 modelMatrix = this._selectedVisual.Transform.Matrix;
                Vector3 localCenter = this._selectedVisual.Bounds.Center;
                Vector3 worldCenter = Vector3.TransformPosition(localCenter, modelMatrix);

                //获取鼠标射线
                Vector2 mousePos2D = eventArgs.GetPosition(viewport).ToVector2();
                Ray ray = viewport.UnProject(mousePos2D);

                //移动平面上的交点
                bool success = ray.IntersectsPlane(worldCenter, viewport.Camera.LookDirection, out _, out _);
                if (success)
                {
                    float deltaX = (float)(mousePos2D.X - this._mousePos2D!.Value.X);
                    float deltaY = (float)(mousePos2D.Y - this._mousePos2D!.Value.Y);

                    //设置光标
                    if (deltaX != 0 && deltaY == 0)
                    {
                        viewport.Cursor = new Cursor(StandardCursorType.SizeWestEast);
                    }
                    if (deltaX == 0 && deltaY != 0)
                    {
                        viewport.Cursor = new Cursor(StandardCursorType.SizeNorthSouth);
                    }
                    if (deltaX != 0 && deltaY != 0)
                    {
                        viewport.Cursor = new Cursor(StandardCursorType.SizeAll);
                    }

                    //旋转轴
                    Vector3 axisY = viewport.Camera.UpDirection.Normalized();
                    Vector3 axisX = viewport.Camera.RightDirection.Normalized();
                    this._selectedVisual.Transform.Rotate(deltaX, axisY);
                    this._selectedVisual.Transform.Rotate(deltaY, axisX);

                    viewport.RequestNextFrameRendering();

                    this._mousePos2D = mousePos2D;
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

            //清空选中
            this._selectedVisual = null;

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
    }
}
