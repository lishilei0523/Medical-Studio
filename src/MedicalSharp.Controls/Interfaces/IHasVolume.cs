using MedicalSharp.Primitives.Models;

namespace MedicalSharp.Controls.Interfaces
{
    /// <summary>
    /// 具有体积接口
    /// </summary>
    public interface IHasVolume
    {
        /// <summary>
        /// 计算体积
        /// </summary>
        /// <param name="metadata">体积元数据</param>
        /// <returns>体积（mm³）</returns>
        float CalculateVolume(VolumeMetadata metadata);
    }
}
