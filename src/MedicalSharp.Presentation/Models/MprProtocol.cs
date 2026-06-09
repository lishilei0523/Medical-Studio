using System.Collections.Generic;

namespace MedicalSharp.Presentation.Models
{
    /// <summary>
    /// MPR协议
    /// </summary>
    public class MprProtocol
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

        #region 对比度 —— float Contrast
        /// <summary>
        /// 对比度
        /// </summary>
        public float Contrast { get; set; }
        #endregion

        #region 控制点列表 —— List<MprProtocolPoint> ControlPoints
        /// <summary>
        /// 控制点列表
        /// </summary>
        public List<MprProtocolPoint> ControlPoints { get; set; }
        #endregion
    }
}
