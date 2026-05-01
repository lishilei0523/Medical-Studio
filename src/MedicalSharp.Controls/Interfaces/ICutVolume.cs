using MedicalSharp.Engine.Renderables;
using MedicalSharp.Primitives.Enums;

namespace MedicalSharp.Controls.Interfaces
{
    /// <summary>
    /// 可切割体积
    /// </summary>
    public interface ICutVolume
    {
        /// <summary>
        /// 适用切割体积
        /// </summary>
        /// <param name="renderable">体积渲染对象</param>
        /// <param name="cutMode">切割模式</param>
        /// <param name="markValue">标记值</param>
        void ApplyCutVolume(VolumeRenderable renderable, CutMode cutMode, byte markValue);
    }
}
