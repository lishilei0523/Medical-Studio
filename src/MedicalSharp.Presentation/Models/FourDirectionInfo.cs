using Caliburn.Micro;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;

namespace MedicalSharp.Presentation.Models
{
    /// <summary>
    /// 四方向信息
    /// </summary>
    public class FourDirectionInfo : PropertyChangedBase
    {
        #region 上 —— string Top
        /// <summary>
        /// 上
        /// </summary>
        [DependencyProperty]
        public string Top { get; set; }
        #endregion

        #region 下 —— string Bottom
        /// <summary>
        /// 下
        /// </summary>
        [DependencyProperty]
        public string Bottom { get; set; }
        #endregion

        #region 左 —— string Left
        /// <summary>
        /// 左
        /// </summary>
        [DependencyProperty]
        public string Left { get; set; }
        #endregion

        #region 右 —— string Right
        /// <summary>
        /// 右
        /// </summary>
        [DependencyProperty]
        public string Right { get; set; }
        #endregion
    }
}
