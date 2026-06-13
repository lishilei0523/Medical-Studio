using MedicalSharp.Primitives.Models;
using System.Threading.Tasks;

namespace MedicalSharp.Controls.Interfaces
{
    /// <summary>
    /// 可统计体积接口(3D)
    /// </summary>
    public interface IAnalyseVolume3D
    {
        /// <summary>
        /// 适用统计体积
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="markValue">标记值</param>
        /// <returns>统计结果</returns>
        Task<StatisticResult> ApplyAnalyseVolume(VolumeData volumeData, byte? markValue);
    }
}
