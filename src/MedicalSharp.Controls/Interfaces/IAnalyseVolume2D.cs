using MedicalSharp.Engine.Renderables;
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
        /// <param name="renderable">体积渲染对象</param>
        /// <param name="markValue">标记值</param>
        /// <returns>统计结果</returns>
        StatisticResult ApplyAnalyseVolume(VolumeRenderable renderable, byte? markValue);
    }
}
