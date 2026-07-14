using SD.Infrastructure.Avalonia.Models;

namespace MedicalSharp.Presentation.Models
{
    /// <summary>
    /// CPR视图6格布局信息
    /// </summary>
    public class CprLayout6 : LayoutBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 单元格数量
        /// </summary>
        private const int CellsCount = 6;

        /// <summary>
        /// 默认构造器
        /// </summary>
        private CprLayout6()
        {
            this.Cells = new GridCell[CellsCount];
        }

        #endregion

        #region # 属性

        #region 只读属性 - 单元格列表 —— override GridCell[] Cells
        /// <summary>
        /// 只读属性 - 单元格列表
        /// </summary>
        public override GridCell[] Cells { get; }
        #endregion

        #region 只读属性 - 单元格1 —— GridCell Cell1
        /// <summary>
        /// 只读属性 - 单元格1
        /// </summary>
        public GridCell Cell1
        {
            get => this.Cells[0];
        }
        #endregion

        #region 只读属性 - 单元格2 —— GridCell Cell2
        /// <summary>
        /// 只读属性 - 单元格2
        /// </summary>
        public GridCell Cell2
        {
            get => this.Cells[1];
        }
        #endregion

        #region 只读属性 - 单元格3 —— GridCell Cell3
        /// <summary>
        /// 只读属性 - 单元格3
        /// </summary>
        public GridCell Cell3
        {
            get => this.Cells[2];
        }
        #endregion

        #region 只读属性 - 单元格4 —— GridCell Cell4
        /// <summary>
        /// 只读属性 - 单元格4
        /// </summary>
        public GridCell Cell4
        {
            get => this.Cells[3];
        }
        #endregion

        #region 只读属性 - 单元格5 —— GridCell Cell5
        /// <summary>
        /// 只读属性 - 单元格5
        /// </summary>
        public GridCell Cell5
        {
            get => this.Cells[4];
        }
        #endregion

        #region 只读属性 - 单元格6 —— GridCell Cell6
        /// <summary>
        /// 只读属性 - 单元格6
        /// </summary>
        public GridCell Cell6
        {
            get => this.Cells[5];
        }
        #endregion

        #endregion

        #region # 方法

        #region 创建布局 —— static CprLayout6 CreateLayout()
        /// <summary>
        /// 创建布局
        /// </summary>
        public static CprLayout6 CreateLayout()
        {
            CprLayout6 layout = new CprLayout6
            {
                Rows = 6,
                Columns = 3
            };
            layout.Cells[0] = new GridCell(row: 0, column: 0, rowSpan: 3, columnSpan: 1);
            layout.Cells[1] = new GridCell(row: 3, column: 0, rowSpan: 3, columnSpan: 1);
            layout.Cells[2] = new GridCell(row: 0, column: 1, rowSpan: 2, columnSpan: 1);
            layout.Cells[3] = new GridCell(row: 2, column: 1, rowSpan: 2, columnSpan: 1);
            layout.Cells[4] = new GridCell(row: 4, column: 1, rowSpan: 2, columnSpan: 1);
            layout.Cells[5] = new GridCell(row: 0, column: 2, rowSpan: 6, columnSpan: 1);
            layout.BuildDefinitions();

            return layout;
        }
        #endregion

        #endregion
    }
}
