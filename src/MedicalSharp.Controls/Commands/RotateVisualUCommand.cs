using Avalonia;
using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Controls.Visual3Ds;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Maths;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Controls.Commands
{
    /// <summary>
    /// 旋转U轴元素命令
    /// </summary>
    public class RotateVisualUCommand : EditShapeCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// 选中的3D元素
        /// </summary>
        private IRotatable _selectedVisual;

        /// <summary>
        /// 默认构造器
        /// </summary>
        public RotateVisualUCommand()
        {
            this._selectedVisual = null;
        }

        #endregion

        #region # 属性

        #region 旋转中委托 —— Action<IRotatable> Rotating
        /// <summary>
        /// 旋转中委托
        /// </summary>
        public Action<IRotatable> Rotating { get; set; }
        #endregion

        #region 旋转结束委托 —— Action<IRotatable> RotateEnd
        /// <summary>
        /// 旋转结束委托
        /// </summary>
        public Action<IRotatable> RotateEnd { get; set; }
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
                Point mousePos2D = eventArgs.GetPixelPosition(viewport);
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
                Vector2 mousePos2D = eventArgs.GetPixelPosition(viewport).ToVector2();
                Ray ray = viewport.UnProject(mousePos2D);

                //移动平面上的交点
                bool success = ray.IntersectsPlane(worldCenter, viewport.Camera.LookDirection, out _, out _);
                if (success)
                {
                    float deltaY = mousePos2D.Y - this._mousePos2D!.Value.Y;

                    //设置光标
                    if (deltaY != 0)
                    {
                        viewport.Cursor = new Cursor(StandardCursorType.SizeNorthSouth);
                    }

                    //旋转U轴
                    Vector3 axisU = visual2DIn3D.UAxis.ToVector3().Normalized();
                    this._selectedVisual.Transform.Rotate(deltaY, axisU);

                    //旋转中
                    this.Rotating?.Invoke(this._selectedVisual);

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
            this.RotateEnd?.Invoke(this._selectedVisual);

            //清空选中
            this._selectedVisual = null;

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion 

        #endregion
    }
}
