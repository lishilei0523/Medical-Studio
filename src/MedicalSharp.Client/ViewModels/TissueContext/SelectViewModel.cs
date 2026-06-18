using Avalonia.Collections;
using Caliburn.Micro;
using MedicalSharp.Presentation.Models;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.CustomControls;
using System.Threading.Tasks;

namespace MedicalSharp.Client.ViewModels.TissueContext
{
    /// <summary>
    /// 选择组织视图模型
    /// </summary>
    public class SelectViewModel : Screen
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗口管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public SelectViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 已选组织 —— TissueInfo SelectedTissue
        /// <summary>
        /// 已选组织
        /// </summary>
        [DependencyProperty]
        public TissueInfo SelectedTissue { get; set; }
        #endregion

        #region 组织列表 —— AvaloniaList<TissueInfo> Tissues
        /// <summary>
        /// 组织列表
        /// </summary>
        [DependencyProperty]
        public AvaloniaList<TissueInfo> Tissues { get; set; }
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

            if (this.SelectedTissue == null)
            {
                await MessageBox.Show("目标组织不可为空！", "错误");
                return;
            }

            #endregion

            await this.TryCloseAsync(true);
        }
        #endregion 

        #endregion
    }
}
