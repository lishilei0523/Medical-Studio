using System.Collections.Generic;

namespace MedicalSharp.Presentation.Models
{
    /// <summary>
    /// 光线投射协议
    /// </summary>
    public class RaycastProtocol
    {
        #region 协议名称 —— string Name
        /// <summary>
        /// 协议名称
        /// </summary>
        public string Name { get; set; }
        #endregion

        #region 窗宽 —— int WindowWidth
        /// <summary>
        /// 窗宽
        /// </summary>
        public int WindowWidth { get; set; }
        #endregion

        #region 窗位 —— int WindowCenter
        /// <summary>
        /// 窗位
        /// </summary>
        public int WindowCenter { get; set; }
        #endregion

        #region 亮度 —— float Brightness
        /// <summary>
        /// 亮度
        /// </summary>
        public float Brightness { get; set; }
        #endregion

        #region 密度缩放 —— float DensityScale
        /// <summary>
        /// 密度缩放
        /// </summary>
        public float DensityScale { get; set; }
        #endregion

        #region 步长 —— float StepSize
        /// <summary>
        /// 步长
        /// </summary>
        public float StepSize { get; set; }
        #endregion

        #region 最大步数 —— int MaxStepsCount
        /// <summary>
        /// 最大步数
        /// </summary>
        public int MaxStepsCount { get; set; }
        #endregion

        #region 透明度阈值 —— float OpacityThreshold
        /// <summary>
        /// 透明度阈值
        /// </summary>
        public float OpacityThreshold { get; set; }
        #endregion

        #region 控制点列表 —— List<RaycastProtocolPoint> ControlPoints
        /// <summary>
        /// 控制点列表
        /// </summary>
        public List<RaycastProtocolPoint> ControlPoints { get; set; }
        #endregion
    }
}
