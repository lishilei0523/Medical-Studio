using Caliburn.Micro;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;

namespace MedicalSharp.Presentation.Models
{
    /// <summary>
    /// 统计信息
    /// </summary>
    public class StatisticInfo : PropertyChangedBase
    {
        #region 最小HU —— string MinHU
        /// <summary>
        /// 最小HU
        /// </summary>
        [DependencyProperty]
        public string MinHU { get; set; }
        #endregion

        #region 最大HU —— string MaxHU
        /// <summary>
        /// 最大HU
        /// </summary>
        [DependencyProperty]
        public string MaxHU { get; set; }
        #endregion

        #region 平均HU —— string AverageHU
        /// <summary>
        /// 平均HU
        /// </summary>
        [DependencyProperty]
        public string AverageHU { get; set; }
        #endregion

        #region 标准差 —— string StdDevHU
        /// <summary>
        /// 标准差
        /// </summary>
        [DependencyProperty]
        public string StdDevHU { get; set; }
        #endregion

        #region 周长 —— string Perimeter
        /// <summary>
        /// 周长
        /// </summary>
        /// <remarks>mm</remarks>
        [DependencyProperty]
        public string Perimeter { get; set; }
        #endregion

        #region 表面积 —— string SurfaceArea
        /// <summary>
        /// 表面积
        /// </summary>
        /// <remarks>mm²</remarks>
        [DependencyProperty]
        public string SurfaceArea { get; set; }
        #endregion

        #region 体积 —— string Volume
        /// <summary>
        /// 体积
        /// </summary>
        /// <remarks>mm³</remarks>
        [DependencyProperty]
        public string Volume { get; set; }
        #endregion

        #region 体素数 —— string VoxelsCount
        /// <summary>
        /// 体素数
        /// </summary>
        [DependencyProperty]
        public string VoxelsCount { get; set; }
        #endregion
    }
}
