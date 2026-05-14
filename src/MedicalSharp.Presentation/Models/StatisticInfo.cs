using Caliburn.Micro;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;

namespace MedicalSharp.Presentation.Models
{
    /// <summary>
    /// 统计信息
    /// </summary>
    public class StatisticInfo : PropertyChangedBase
    {
        #region 周长 —— string Length
        /// <summary>
        /// 周长
        /// </summary>
        /// <remarks>mm</remarks>
        [DependencyProperty]
        public string Length { get; set; }
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

        #region 长径 —— string LongDiameter
        /// <summary>
        /// 长径
        /// </summary>
        /// <remarks>mm</remarks>
        [DependencyProperty]
        public string LongDiameter { get; set; }
        #endregion

        #region 短径 —— string ShortDiameter
        /// <summary>
        /// 短径
        /// </summary>
        /// <remarks>mm</remarks>
        [DependencyProperty]
        public string ShortDiameter { get; set; }
        #endregion

        #region 最大直径 —— string MaxDiameter
        /// <summary>
        /// 最大直径
        /// </summary>
        /// <remarks>mm</remarks>
        [DependencyProperty]
        public string MaxDiameter { get; set; }
        #endregion

        #region 球形度 —— string Sphericity
        /// <summary>
        /// 球形度
        /// </summary>
        /// <remarks>值域：0~1，越接近1越接近球体</remarks>
        [DependencyProperty]
        public string Sphericity { get; set; }
        #endregion

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

        #region 体素数 —— string VoxelsCount
        /// <summary>
        /// 体素数
        /// </summary>
        [DependencyProperty]
        public string VoxelsCount { get; set; }
        #endregion
    }
}
