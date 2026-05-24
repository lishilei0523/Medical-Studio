using MedicalSharp.Controls.Viewports;
using MedicalSharp.Primitives.Models;

namespace MedicalSharp.Controls.Interfaces
{
    /// <summary>
    /// 可统计体积接口(2D)
    /// </summary>
    public interface IAnalyseVolume2D
    {
        /// <summary>
        /// 适用统计体积
        /// </summary>
        /// <param name="viewport">MPR渲染视口</param>
        /// <param name="markValue">标记值</param>
        /// <returns>统计结果</returns>
        StatisticResult ApplyAnalyseVolume(MPRViewport viewport, byte? markValue);
    }
}
