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
    /// 旋转2D元素命令
    /// </summary>
    /// <remarks>MPR视图使用</remarks>
    public class RotateVisual2DCommand : EditShapeCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// 选中的3D元素
        /// </summary>
        private IRotatable _selectedVisual;

        /// <summary>
        /// 默认构造器
        /// </summary>
        public RotateVisual2DCommand()
        {
            this._selectedVisual = null;
        }

        #endregion

        #region # 属性

        #region 已旋转委托 —— Action<IRotatable> Rotated
        /// <summary>
        /// 已旋转委托
        /// </summary>
        public Action<IRotatable> Rotated { get; set; }
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
                Point mousePos2D = eventArgs.GetPixelPosition(viewport);
                bool success = pickVisual3D.FindNearest(mousePos2D.ToVector2(), out _, out _, out Visual3D visual3D, out _);
                if (success && visual3D is IRotatable rotatable)
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
                    viewport.Cursor = new Cursor(StandardCursorType.SizeNorthSouth);

                    //旋转轴
                    Vector3 axis = viewport.Camera.LookDirection.Normalized();
                    this._selectedVisual.Transform.Rotate(deltaY, axis);

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
            this.Rotated?.Invoke(this._selectedVisual);

            //清空选中
            this._selectedVisual = null;

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion 

        #endregion
    }
}
