namespace MedicalSharp.Primitives.Models
{
    /// <summary>
    /// 体积扫描数据
    /// </summary>
    public class VolumeScanData
    {
        //CT

        #region 管电压 —— string KVP
        /// <summary>
        /// 管电压
        /// </summary>
        /// <remarks>kVp</remarks>
        public string KVP { get; set; }
        #endregion

        #region 管电流 —— string XRayTubeCurrent
        /// <summary>
        /// 管电流
        /// </summary>
        /// <remarks>mA</remarks>
        public string XRayTubeCurrent { get; set; }
        #endregion

        #region 曝光时间 —— string ExposureTime
        /// <summary>
        /// 曝光时间
        /// </summary>
        /// <remarks>ms</remarks>
        public string ExposureTime { get; set; }
        #endregion

        #region 卷积核 —— string ConvolutionKernel
        /// <summary>
        /// 卷积核
        /// </summary>
        public string ConvolutionKernel { get; set; }
        #endregion

        #region 重建直径 —— string ReconstructionDiameter
        /// <summary>
        /// 重建直径
        /// </summary>
        /// <remarks>FOV，单位 mm</remarks>
        public string ReconstructionDiameter { get; set; }
        #endregion

        #region 螺距因子 —— string PitchFactor
        /// <summary>
        /// 螺距因子
        /// </summary>
        public string PitchFactor { get; set; }
        #endregion

        #region 重建算法 —— string ReconstructionAlgorithm
        /// <summary>
        /// 重建算法
        /// </summary>
        /// <remarks>FBP / 迭代重建</remarks>
        public string ReconstructionAlgorithm { get; set; }
        #endregion


        //MR

        #region 磁场强度 —— string MagneticFieldStrength
        /// <summary>
        /// 磁场强度
        /// </summary>
        /// <remarks>T</remarks>
        public string MagneticFieldStrength { get; set; }
        #endregion

        #region 重复时间 —— string RepetitionTime
        /// <summary>
        /// 重复时间
        /// </summary>
        /// <remarks>TR，单位 ms</remarks>
        public string RepetitionTime { get; set; }
        #endregion

        #region 回波时间 —— string EchoTime
        /// <summary>
        /// 回波时间
        /// </summary>
        /// <remarks>TE，单位 ms</remarks>
        public string EchoTime { get; set; }
        #endregion

        #region 序列名称 —— string SequenceName
        /// <summary>
        /// 序列名称
        /// </summary>
        public string SequenceName { get; set; }
        #endregion


        //造影剂

        #region 造影剂名称 —— string ContrastAgent
        /// <summary>
        /// 造影剂名称
        /// </summary>
        public string ContrastAgent { get; set; }
        #endregion

        #region 造影剂剂量 —— string ContrastDose
        /// <summary>
        /// 造影剂剂量
        /// </summary>
        /// <remarks>ml</remarks>
        public string ContrastDose { get; set; }
        #endregion
    }
}
