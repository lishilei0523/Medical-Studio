using Avalonia;
using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Engine.Cameras;
using System;

namespace MedicalSharp.Controls.Inputs
{
    /// <summary>
    /// MPR输入管理器
    /// </summary>
    public class MPRInputManager : InputManager
    {
        #region # 字段及构造器

        /// <summary>
        /// MPR相机
        /// </summary>
        private readonly MPRCamera _camera;

        /// <summary>
        /// 创建输入管理器构造器
        /// </summary>
        public MPRInputManager(MPRCamera camera)
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

        #region 鼠标移动事件 —— override void OnMouseMove(OpenTKViewport viewport, MouseButton button...
        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        /// <param name="viewport">OpenTK视口</param>
        /// <param name="button">鼠标按键</param>
        /// <param name="position">鼠标位置</param>
        public override void OnMouseMove(OpenTKViewport viewport, MouseButton button, Point position)
        {
            if (this._mousePosition2D.HasValue)
            {
                float deltaX = (float)(position.X - this._mousePosition2D.Value.X);
                float deltaY = (float)(position.Y - this._mousePosition2D.Value.Y);
                if (button == MouseButton.Middle)
                {
                    this._camera.Pan(deltaX / 50.0f, deltaY / 50.0f);
                    viewport.RequestNextFrameRendering();
                }
                if (button == MouseButton.Right)
                {
                    this._camera.Zoom(deltaX + deltaY);
                    viewport.RequestNextFrameRendering();
                }
            }
            this._mousePosition2D = position;
        }
        #endregion

        #region 鼠标滚轮事件 —— override void OnMouseWheel(OpenTKViewport viewport, double offsetX...
        /// <summary>
        /// 鼠标滚轮事件
        /// </summary>
        /// <param name="viewport">OpenTK视口</param>
        /// <param name="offsetX">X轴偏移量</param>
        /// <param name="offsetY">Y轴偏移量</param>
        public override void OnMouseWheel(OpenTKViewport viewport, double offsetX, double offsetY)
        {
            this._camera.SliceIndex += (int)Math.Ceiling(offsetY);
            viewport.RequestNextFrameRendering();
        }
        #endregion 

        #endregion
    }
}
