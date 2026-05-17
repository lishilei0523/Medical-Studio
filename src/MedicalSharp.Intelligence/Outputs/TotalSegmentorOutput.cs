using System;

namespace MedicalSharp.Intelligence.Outputs
{
    /// <summary>
    /// TotalSegmentor分割模型输出
    /// </summary>
    public class TotalSegmentorOutput
    {
        /// <summary>
        /// 分割结果数据
        /// </summary>
        /// <remarks>byte类型，标签值，尺寸与输入 Shape 相同</remarks>
        public IntPtr Data { get; set; }

        /// <summary>
        /// 结果尺寸
        /// </summary>
        /// <remarks>[Depth, Height, Width]</remarks>
        public int[] Shape { get; set; }
    }
}
