using System;
using System.Collections.Generic;
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
        /// 默认构造器
        /// </summary>
        public StatisticResult()
        {
            this.MinHU = float.MaxValue;
            this.MaxHU = float.MinValue;
        }

        /// <summary>
        /// 最小HU
        /// </summary>
        public float MinHU;

        /// <summary>
        /// 最大HU
        /// </summary>
        public float MaxHU;

        /// <summary>
        /// HU累加和
        /// </summary>
        /// <remarks>用于计算平均HU</remarks>
        public float HuSum;

        /// <summary>
        /// HU平方和
        /// </summary>
        /// <remarks>用于计算标准差</remarks>
        public float HuSumSq;

        /// <summary>
        /// 平均HU
        /// </summary>
        public float AverageHU;

        /// <summary>
        /// 标准差
        /// </summary>
        public float StdDevHU;

        /// <summary>
        /// 边界体素数
        /// </summary>
        public int BoundaryCount;

        /// <summary>
        /// 体素数
        /// </summary>
        public int VoxelsCount;

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
        /// 计算统计指标
        /// </summary>
        public void CalculateExpectations()
        {
            #region # 验证

            if (this.VoxelsCount <= 0)
            {
                this.AverageHU = 0;
                this.StdDevHU = 0;
                return;
            }

            #endregion

            this.AverageHU = this.HuSum / this.VoxelsCount;
            this.StdDevHU = MathF.Sqrt((this.HuSumSq / this.VoxelsCount) - (this.AverageHU * this.AverageHU));
        }

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
        }

        /// <summary>
        /// 合并单位统计结果
        /// </summary>
        public static StatisticResult MergeResults(IReadOnlyCollection<StatisticResult> results)
        {
            StatisticResult mergedResult = new StatisticResult();
            foreach (StatisticResult result in results)
            {
                mergedResult.MinHU = Math.Min(mergedResult.MinHU, result.MinHU);
                mergedResult.MaxHU = Math.Max(mergedResult.MaxHU, result.MaxHU);
                mergedResult.HuSum += result.HuSum;
                mergedResult.HuSumSq += result.HuSumSq;
                mergedResult.BoundaryCount += result.BoundaryCount;
                mergedResult.VoxelsCount += result.VoxelsCount;
            }

            return mergedResult;
        }
    }
}
