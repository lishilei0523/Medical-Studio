using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Models;

namespace MedicalSharp.Controls.Interfaces
{
    /// <summary>
    /// 可切割体积接口
    /// </summary>
    public interface ICutVolume
    {
        /// <summary>
        /// 适用切割体积
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="markTexture">标记纹理</param>
        /// <param name="cutMode">切割模式</param>
        /// <param name="markValue">标记值</param>
        void ApplyCutVolume(VolumeData volumeData, Texture3D markTexture, CutMode cutMode, byte markValue);
    }
}
