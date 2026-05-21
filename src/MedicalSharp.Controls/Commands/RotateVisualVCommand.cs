using Avalonia;
using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Maths;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Controls.Commands
{
    /// <summary>
    /// 旋转V轴元素命令
    /// </summary>
    public class RotateVisualVCommand : ViewportCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// 选中的3D元素
        /// </summary>
        private IRotatable _selectedVisual;

        /// <summary>
        /// 旋转结束事件
        /// </summary>
        private readonly Action<IRotatable> _rotateEndEvent;

        /// <summary>
        /// 创建旋转V轴元素命令构造器
        /// </summary>
        /// <param name="rotateEnd">旋转结束回调</param>
        public RotateVisualVCommand(Action<IRotatable> rotateEnd)
        {
            this._rotateEndEvent = rotateEnd;
            this._selectedVisual = null;
        }

        #endregion

        #region # 属性

        #region 旋转中事件 —— Action<IRotatable> RotatingEvent
        /// <summary>
        /// 旋转中事件
        /// </summary>
        public Action<IRotatable> RotatingEvent { get; set; }
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
            if (eventArgs.Properties.IsLeftButtonPressed && viewport is IPickVisual3D pickVisual3D)
            {
                Point mousePos2D = eventArgs.GetPosition(viewport);
                bool success = pickVisual3D.FindNearest(mousePos2D.ToVector2(), out _, out _, out Visual3D visual3D, out _);
                if (success && visual3D is IRotatable rotatable && rotatable.CanRotate)
                {
                    #region # 验证

                    if (visual3D is IFixable { Fixed: true })
                    {
                        return;
                    }

                    #endregion

                    this._selectedVisual = rotatable;
                }
            }
        }
        #endregion

        #region 鼠标移动事件 —— override void OnMouseMove(OpenTKViewport viewport...
        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        public override void OnMouseMove(OpenTKViewport viewport, PointerEventArgs eventArgs)
        {
            base.OnMouseMove(viewport, eventArgs);
            if (eventArgs.Properties.IsLeftButtonPressed && this._selectedVisual is IVisual2DIn3D visual2DIn3D)
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
                    float deltaX = mousePos2D.X - this._mousePos2D!.Value.X;

                    //设置光标
                    if (deltaX != 0)
                    {
                        viewport.Cursor = new Cursor(StandardCursorType.SizeWestEast);
                    }

                    //旋转轴
                    Vector3 axisV = visual2DIn3D.VAxis.ToVector3().Normalized();
                    this._selectedVisual.Transform.Rotate(deltaX, axisV);

                    //旋转中
                    this.RotatingEvent?.Invoke(this._selectedVisual);

                    //请求下一帧
                    viewport.RequestNextFrameRendering();

                    this._mousePos2D = mousePos2D;
                }
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

            //旋转结束
            this._rotateEndEvent?.Invoke(this._selectedVisual);

            //清空选中
            this._selectedVisual = null;

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion 

        #endregion
    }
}
