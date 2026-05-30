using Caliburn.Micro;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;

namespace MedicalSharp.Presentation.Models
{
    /// <summary>
    /// 扫描信息
    /// </summary>
    public class ScanInfo : PropertyChangedBase
    {
        #region 管电压 —— string KVP
        /// <summary>
        /// 管电压
        /// </summary>
        /// <remarks>kVp</remarks>
        [DependencyProperty]
        public string KVP { get; set; }
        #endregion

        #region 管电流 —— string XRayTubeCurrent
        /// <summary>
        /// 管电流
        /// </summary>
        /// <remarks>mA</remarks>
        [DependencyProperty]
        public string XRayTubeCurrent { get; set; }
        #endregion

        #region 曝光时间 —— string ExposureTime
        /// <summary>
        /// 曝光时间
        /// </summary>
        /// <remarks>ms</remarks>
        [DependencyProperty]
        public string ExposureTime { get; set; }
        #endregion

        #region 卷积核 —— string ConvolutionKernel
        /// <summary>
        /// 卷积核
        /// </summary>
        [DependencyProperty]
        public string ConvolutionKernel { get; set; }
        #endregion

        #region 重建直径 —— string ReconstructionDiameter
        /// <summary>
        /// 重建直径
        /// </summary>
        /// <remarks>FOV，单位 mm</remarks>
        [DependencyProperty]
        public string ReconstructionDiameter { get; set; }
        #endregion
    }
}
