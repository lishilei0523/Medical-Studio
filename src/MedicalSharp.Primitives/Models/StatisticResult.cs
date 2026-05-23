using System;
using System.Runtime.InteropServices;

namespace MedicalSharp.Primitives.Models
{
    /// <summary>
    /// 统计结果
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public record struct StatisticResult
    {
        /// <summary>
        /// 最小HU
        /// </summary>
        public float MinHU;

        /// <summary>
        /// 最大HU
        /// </summary>
        public float MaxHU;

        /// <summary>
        /// 平均HU
        /// </summary>
        public float AverageHU;

        /// <summary>
        /// 标准差
        /// </summary>
        public float StdDevHU;

        /// <summary>
        /// 体素数
        /// </summary>
        public int VoxelsCount;

        /// <summary>
        /// 边界体素数
        /// </summary>
        public int BoundaryCount;

        /// <summary>
        /// 表面积
        /// </summary>
        /// <remarks>mm²</remarks>
        public float SurfaceArea;

        /// <summary>
        /// 体积
        /// </summary>
        /// <remarks>mm³</remarks>
        public float Volume;

        /// <summary>
        /// 球形度
        /// </summary>
        /// <remarks>值域：0~1，越接近1越接近球体</remarks>
        public float Sphericity;

        /// <summary>
        /// 计算几何指标
        /// </summary>
        /// <param name="voxelVolume">体素体积（mm³）</param>
        /// <param name="voxelArea">体素表面积（mm²）</param>
        public void CalculateGeometry(float voxelVolume, float voxelArea)
        {
            //计算体积 = 体素数 × 单个体素体积
            this.Volume = this.VoxelsCount * voxelVolume;

            //计算表面积 = 边界体素数 × 单个体素表面积
            this.SurfaceArea = this.BoundaryCount * voxelArea;

            //计算球形度，V: 体积, S: 表面积
            if (this.Volume > 0 && this.SurfaceArea > 0)
            {
                //球形度 = (π^(1/3) × (6V)^(2/3)) / S
                float numerator = MathF.Pow(MathF.PI, 1.0f / 3.0f) * MathF.Pow(6 * this.Volume, 2.0f / 3.0f);
                this.Sphericity = numerator / this.SurfaceArea;
                this.Sphericity = Math.Clamp(this.Sphericity, 0, 1);
            }
            else
            {
                this.Sphericity = 0;
            }
        }
    }
}
