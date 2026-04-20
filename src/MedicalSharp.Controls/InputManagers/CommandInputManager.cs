using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Interfaces;
using System;

namespace MedicalSharp.Controls.InputManagers
{
    /// <summary>
    /// 命令输入管理器
    /// </summary>
    public class CommandInputManager : InputManager
    {
        #region # 字段及构造器

        /// <summary>
        /// 创建命令输入管理器构造器
        /// </summary>
        /// <param name="command">视口命令</param>
        public CommandInputManager(IViewportCommand command)
            : base(command)
        {

        }

        #endregion

        #region # 属性

        //

        #endregion

        #region # 方法

        #region 切换命令 —— override void SwitchCommand(IViewportCommand command)
        /// <summary>
        /// 切换命令
        /// </summary>
        /// <param name="command">视口命令</param>
        public override void SwitchCommand(IViewportCommand command)
        {
            #region # 验证

            if (command == null)
            {
                throw new ArgumentNullException(nameof(command), "视口命令不可为空！");
            }

            #endregion

            base.SwitchCommand(command);
        }
        #endregion

        #endregion
    }
}
