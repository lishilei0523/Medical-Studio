using Avalonia.Controls;
using System;

namespace MedicalSharp.Controls.Visual2Ds
{
    /// <summary>
    /// 2D元素
    /// </summary>
    public abstract class Visual2D : Control
    {
        #region # 字段及构造器

        /// <summary>
        /// 默认构造器
        /// </summary>
        protected Visual2D()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        #endregion

        #region # 属性

        #region 标识Id —— string Id
        /// <summary>
        /// 标识Id
        /// </summary>
        public string Id { get; internal set; }
        #endregion

        #endregion

        #region # 方法

        //

        #endregion
    }
}
