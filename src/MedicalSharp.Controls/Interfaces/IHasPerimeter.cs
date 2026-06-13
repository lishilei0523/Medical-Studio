using MedicalSharp.Primitives.Models;

namespace MedicalSharp.Controls.Interfaces
{
    /// <summary>
    /// 具有周长接口
    /// </summary>
    public interface IHasPerimeter
    {
        /// <summary>
        /// 计算周长
        /// </summary>
        /// <param name="metadata">体积元数据</param>
        /// <returns>长度（mm）</returns>
        float CalculatePerimeter(VolumeMetadata metadata);
    }
}
