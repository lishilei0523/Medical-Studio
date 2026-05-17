using System;

namespace MedicalSharp.Intelligence.Inputs
{
    /// <summary>
    /// TotalSegmentor分割模型输入
    /// </summary>
    public class TotalSegmentorInput
    {
        /// <summary>
        /// 体积数据
        /// </summary>
        /// <remarks>
        /// 预处理后的体积数据数据（float类型，已归一化到[0,1]）
        /// 形状为[Depth, Height, Width]，无需resize
        /// </remarks>
        public IntPtr Data { get; set; }

        /// <summary>
        /// 数据形状
        /// </summary>
        /// <remarks>[Depth, Height, Width]</remarks>
        public int[] Shape { get; set; }

        /// <summary>
        /// 目标器官名称列表
        /// </summary>
        /// <remarks>null = 全部器官</remarks>
        public string[] TargetOrgans { get; set; }
    }
}
