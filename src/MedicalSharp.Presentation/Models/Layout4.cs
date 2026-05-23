using SD.Infrastructure.Avalonia.Models;

namespace MedicalSharp.Presentation.Models
{
    /// <summary>
    /// 4格布局信息
    /// </summary>
    public class Layout4 : LayoutBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 单元格数量
        /// </summary>
        private const int CellsCount = 4;

        /// <summary>
        /// 默认构造器
        /// </summary>
        private Layout4()
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

        #endregion

        #region # 方法

        #region 创建2×2布局 —— static Layout4 Create2x2()
        /// <summary>
        /// 创建2×2布局
        /// </summary>
        public static Layout4 Create2x2()
        {
            Layout4 layout = new Layout4
            {
                Rows = 2,
                Columns = 2
            };
            layout.Cells[0] = new GridCell(row: 0, column: 0);
            layout.Cells[1] = new GridCell(row: 0, column: 1);
            layout.Cells[2] = new GridCell(row: 1, column: 0);
            layout.Cells[3] = new GridCell(row: 1, column: 1);
            layout.BuildDefinitions();

            return layout;
        }
        #endregion

        #region 创建1×3布局 —— static Layout4 Create1x3()
        /// <summary>
        /// 创建1×3布局
        /// </summary>
        public static Layout4 Create1x3()
        {
            Layout4 layout = new Layout4
            {
                Rows = 3,
                Columns = 2
            };
            layout.Cells[0] = new GridCell(row: 0, column: 0, rowSpan: 3, columnSpan: 1);
            layout.Cells[1] = new GridCell(row: 0, column: 1);
            layout.Cells[2] = new GridCell(row: 1, column: 1);
            layout.Cells[3] = new GridCell(row: 2, column: 1);
            layout.BuildDefinitions();

            return layout;
        }
        #endregion

        #endregion
    }
}
