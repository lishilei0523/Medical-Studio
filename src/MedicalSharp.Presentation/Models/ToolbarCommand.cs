using Caliburn.Micro;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using System.Windows.Input;

namespace MedicalSharp.Presentation.Models
{
    /// <summary>
    /// 工具栏命令
    /// </summary>
    public class ToolbarCommand : PropertyChangedBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 默认构造器
        /// </summary>
        public ToolbarCommand()
        {

        }

        /// <summary>
        /// 创建工具栏命令构造器
        /// </summary>
        /// <param name="name">命令名称</param>
        /// <param name="icon">命令图标</param>
        /// <param name="relayCommand">转接命令</param>
        /// <param name="isChecked">是否勾选</param>
        /// <param name="isVisible">是否可见</param>
        public ToolbarCommand(string name, string icon, ICommand relayCommand, bool isChecked = false, bool isVisible = true)
            : this()
        {
            this.Name = name;
            this.Icon = icon;
            this.RelayCommand = relayCommand;
            this.IsChecked = isChecked;
            this.IsVisible = isVisible;
        }

        #endregion

        #region # 属性

        #region 命令名称 —— string Name
        /// <summary>
        /// 命令名称
        /// </summary>
        public string Name { get; set; }
        #endregion

        #region 命令图标 —— string Icon
        /// <summary>
        /// 命令图标
        /// </summary>
        public string Icon { get; set; }
        #endregion

        #region 转接命令 —— ICommand RelayCommand
        /// <summary>
        /// 转接命令
        /// </summary>
        public ICommand RelayCommand { get; set; }
        #endregion

        #region 是否勾选 —— bool IsChecked
        /// <summary>
        /// 是否勾选
        /// </summary>
        [DependencyProperty]
        public bool IsChecked { get; set; }
        #endregion

        #region 是否可见 —— bool IsVisible
        /// <summary>
        /// 是否可见
        /// </summary>
        [DependencyProperty]
        public bool IsVisible { get; set; }
        #endregion

        #endregion

        #region # 方法

        //

        #endregion
    }
}
