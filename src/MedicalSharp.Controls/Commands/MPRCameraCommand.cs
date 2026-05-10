using Avalonia;
using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Primitives.Cameras;
using System;

namespace MedicalSharp.Controls.Commands
{
    /// <summary>
    /// MPR相机命令
    /// </summary>
    public class MPRCameraCommand : ViewportCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// MPR相机
        /// </summary>
        private readonly MPRCamera _camera;

        /// <summary>
        /// 创建MPR相机命令构造器
        /// </summary>
        public MPRCameraCommand(MPRCamera camera)
        {
            this._camera = camera;
        }

        #endregion

        #region # 属性

        #region 只读属性 - MPR相机 —— MPRCamera Camera
        /// <summary>
        /// 只读属性 - MPR相机
        /// </summary>
        public MPRCamera Camera
        {
            get => this._camera;
        }
        #endregion

        #endregion

        #region # 方法

        #region 鼠标移动事件 —— override void OnMouseMove(OpenTKViewport viewport, PointerEventArgs eventArgs)
        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        public override void OnMouseMove(OpenTKViewport viewport, PointerEventArgs eventArgs)
        {
            Point position = eventArgs.GetPosition(viewport);
            if (this._mousePos2D.HasValue)
            {
                float deltaX = (float)(position.X - this._mousePos2D.Value.X);
                float deltaY = (float)(position.Y - this._mousePos2D.Value.Y);
                if (eventArgs.Properties.IsMiddleButtonPressed)
                {
                    this._camera.Pan(deltaX, deltaY);
                    viewport.RequestNextFrameRendering();
                }
                if (eventArgs.Properties.IsRightButtonPressed)
                {
                    this._camera.Zoom(-deltaY);
                    viewport.RequestNextFrameRendering();
                }
            }
            this._mousePos2D = position.ToVector2();
        }
        #endregion

        #region 鼠标滚轮事件 —— override void OnMouseWheel(OpenTKViewport viewport, PointerWheelEventArgs eventArgs)
        /// <summary>
        /// 鼠标滚轮事件
        /// </summary>
        public override void OnMouseWheel(OpenTKViewport viewport, PointerWheelEventArgs eventArgs)
        {
            int sliceIndex = this._camera.TargetPlane.SliceIndex + (int)Math.Ceiling(eventArgs.Delta.Y);
            this._camera.TargetPlane.SetSliceIndex(sliceIndex);
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #endregion
    }
}
