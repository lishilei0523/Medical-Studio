using MedicalSharp.Engine.Renderables;
using MedicalSharp.Primitives.Models;

namespace MedicalSharp.Controls.Interfaces
{
    /// <summary>
    /// 可统计体积接口
    /// </summary>
    public interface IAnalyseVolume
    {
        /// <summary>
        /// 统计体积
        /// </summary>
        /// <param name="renderable">体积渲染对象</param>
        /// <returns>统计结果</returns>
        StatisticResult AnalyseVolume(VolumeRenderable renderable);
    }
}
