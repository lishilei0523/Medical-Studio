using MedicalSharp.Engine.Cameras;

namespace MedicalSharp.Controls.Inputs
{
    /// <summary>
    /// MPR输入管理器
    /// </summary>
    public class MPRInputManager : InputManager
    {
        //TODO 实现

        private readonly MPRCamera _mprCamera;

        /// <summary>
        /// 创建输入管理器构造器
        /// </summary>
        public MPRInputManager(MPRCamera mprCamera)
        {
            this._mprCamera = mprCamera;
        }
    }
}
