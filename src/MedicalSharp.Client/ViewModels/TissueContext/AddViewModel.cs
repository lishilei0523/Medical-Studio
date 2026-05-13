using Avalonia.Media;
using Caliburn.Micro;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Primitives.Builders;
using MedicalSharp.Primitives.Enums;
using SD.Common;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.CustomControls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedicalSharp.Client.ViewModels.TissueContext
{
    /// <summary>
    /// 创建组织视图模型
    /// </summary>
    public class AddViewModel : Screen
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗口管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public AddViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;

            //默认值
            this.MarkMode = MarkMode.Visible;
            this.SelectedMarkMode = new KeyValuePair<string, string>(this.MarkMode.ToString(), this.MarkMode.GetEnumMember());
            this.MarkModes = typeof(MarkMode).GetEnumMembers();
        }

        #endregion

        #region # 属性

        #region 组织名称 —— string TissueName
        /// <summary>
        /// 组织名称
        /// </summary>
        [DependencyProperty]
        public string TissueName { get; set; }
        #endregion

        #region 标记值 —— byte? MarkValue
        /// <summary>
        /// 标记值
        /// </summary>
        public byte? MarkValue
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                if (value.HasValue)
                {
                    this.TissueColor = ColorFactory.StandardMarkColors[value.Value].ToColor();
                }
            }
        }
        #endregion

        #region 标记模式 —— MarkMode MarkMode
        /// <summary>
        /// 标记模式
        /// </summary>
        [DependencyProperty]
        public MarkMode MarkMode { get; set; }
        #endregion

        #region 组织颜色 —— Color TissueColor
        /// <summary>
        /// 组织颜色
        /// </summary>
        [DependencyProperty]
        public Color TissueColor { get; set; }
        #endregion

        #region 已选标记模式 —— KeyValuePair<string, string> SelectedMarkMode
        /// <summary>
        /// 已选标记模式
        /// </summary>
        public KeyValuePair<string, string> SelectedMarkMode
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                this.MarkMode = Enum.Parse<MarkMode>(value.Key);
            }
        }
        #endregion

        #region 标记模式字典 —— IDictionary<string, string> MarkModes
        /// <summary>
        /// 标记模式字典
        /// </summary>
        [DependencyProperty]
        public IDictionary<string, string> MarkModes { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 提交 —— async Task Submit()
        /// <summary>
        /// 提交
        /// </summary>
        public async Task Submit()
        {
            #region # 验证

            if (!this.MarkValue.HasValue)
            {
                await MessageBox.Show("标记值不可为空！", "错误");
                return;
            }

            #endregion

            await this.TryCloseAsync(true);
        }
        #endregion 

        #endregion
    }
}
