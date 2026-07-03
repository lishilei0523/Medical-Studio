using MedicalSharp.Primitives.Models;

namespace MedicalSharp.Primitives.Interfaces
{
    /// <summary>
    /// 具有表面积接口
    /// </summary>
    public interface IHasSurfaceArea
    {
        /// <summary>
        /// 计算表面积
        /// </summary>
        /// <param name="metadata">体积元数据</param>
        /// <returns>表面积（mm²）</returns>
        float CalculateSurfaceArea(VolumeMetadata metadata);
    }
}
