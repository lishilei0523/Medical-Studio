using System;

namespace MedicalSharp.Primitives.Models
{
    /// <summary>
    /// 右键菜单项
    /// </summary>
    public class ContextMenuItem
    {
        /// <summary>
        /// 默认构造器
        /// </summary>
        public ContextMenuItem()
        {
            this.IsEnabled = true;
        }

        /// <summary>
        /// 菜单标题
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// 点击执行的命令
        /// </summary>
        public Action Command { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 分隔符
        /// </summary>
        public bool IsSeparator { get; set; }

        /// <summary>
        /// 创建分隔符
        /// </summary>
        public static ContextMenuItem CreateSeparator()
        {
            return new ContextMenuItem { IsSeparator = true };
        }
    }
}
