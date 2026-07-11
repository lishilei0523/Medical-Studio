using MedicalSharp.Primitives.Maths;

namespace MedicalSharp.Primitives.Interfaces
{
    /// <summary>
    /// 可沿曲线拖拽接口
    /// </summary>
    public interface IDraggableAlongCurve
    {
        /// <summary>
        /// 曲线
        /// </summary>
        Curve Curve { get; }

        /// <summary>
        /// 弧长位置（归一化0~1）
        /// </summary>
        float ArcPosition { get; set; }
    }
}
