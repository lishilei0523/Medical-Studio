namespace MedicalSharp.Controls.Base
{
    /// <summary>
    /// 绘制形状命令
    /// </summary>
    public abstract class DrawShapeCommand : ShapeCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// 是否绘制中
        /// </summary>
        protected bool _isDrawing;

        /// <summary>
        /// 默认构造器
        /// </summary>
        protected DrawShapeCommand()
        {
            this._isDrawing = false;
        }

        #endregion

        #region # 属性

        #region 只读属性 - 是否绘制中 —— bool IsDrawing
        /// <summary>
        /// 只读属性 - 是否绘制中
        /// </summary>
        public bool IsDrawing
        {
            get => this._isDrawing;
        }
        #endregion

        #endregion

        #region # 方法

        //

        #endregion
    }
}
