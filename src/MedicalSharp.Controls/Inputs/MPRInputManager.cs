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
        private readonly MPRCamera _mprCamera;

        /// <summary>
        /// 创建输入管理器构造器
        /// </summary>
        public MPRInputManager(MPRCamera mprCamera)
        {
            this._mprCamera = mprCamera;
        }

        #endregion

        #region # 属性

        #region 只读属性 - MPR相机 —— MPRCamera Camera
        /// <summary>
        /// 只读属性 - MPR相机
        /// </summary>
        public MPRCamera Camera
        {
            get => this._mprCamera;
        }
        #endregion

        #endregion

        #region # 方法

        #region 鼠标滚轮事件 —— override void OnMouseWheel(OpenTKViewport viewport, double offsetX...
        /// <summary>
        /// 鼠标滚轮事件
        /// </summary>
        /// <param name="viewport">OpenTK视口</param>
        /// <param name="offsetX">X轴偏移量</param>
        /// <param name="offsetY">Y轴偏移量</param>
        public override void OnMouseWheel(OpenTKViewport viewport, double offsetX, double offsetY)
        {
            this._mprCamera.SliceIndex += (int)Math.Ceiling(offsetY);
            viewport.RequestNextFrameRendering();
        }
        #endregion 

        #endregion
    }
}
