using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Commands;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Primitives.Cameras;
using System.Linq;

namespace MedicalSharp.Controls.InputManagers
{
    /// <summary>
    /// 轨道输入管理器
    /// </summary>
    public class OrbitInputManager : InputManager
    {
        #region # 字段及构造器

        /// <summary>
        /// 轨道相机
        /// </summary>
        private readonly OrbitCamera _camera;

        /// <summary>
        /// 创建轨道输入管理器构造器
        /// </summary>
        /// <param name="camera">轨道相机</param>
        public OrbitInputManager(OrbitCamera camera)
        {
            this._camera = camera;

            OrbitCameraCommand cameraCommand = new OrbitCameraCommand(camera);
            this._command = new CompositeCommand(cameraCommand);
        }

        #endregion

        #region # 属性

        #region 只读属性 - 轨道相机 —— OrbitCamera Camera
        /// <summary>
        /// 只读属性 - 轨道相机
        /// </summary>
        public OrbitCamera Camera
        {
            get => this._camera;
        }
        #endregion

        #endregion

        #region # 方法

        #region 切换命令 —— override void SwitchCommand(IViewportCommand command)
        /// <summary>
        /// 切换命令
        /// </summary>
        /// <param name="command">视口命令</param>
        public override void SwitchCommand(IViewportCommand command)
        {
            if (this._command is CompositeCommand compositeCommand)
            {
                if (compositeCommand.Commands.Count == 2)
                {
                    compositeCommand.RemoveCommand(compositeCommand.Commands.Last());
                }
                if (command != null)
                {
                    compositeCommand.AddCommand(command);
                }
            }
        }
        #endregion

        #endregion
    }
}
