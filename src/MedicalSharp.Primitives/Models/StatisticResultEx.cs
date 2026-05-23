using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MedicalSharp.Primitives.Models
{
    /// <summary>
    /// 单位统计结果
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct StatisticResultEx
    {
        /// <summary>
        /// 创建单位统计结果构造器
        /// </summary>
        public StatisticResultEx()
        {
            this.MinHU = uint.MaxValue;
            this.MaxHU = uint.MinValue;
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
        /// <remarks>用于计算平均值</remarks>
        public float HuSum;

        /// <summary>
        /// HU平方和
        /// </summary>
        /// <remarks>用于计算标准差</remarks>
        public float HuSumSq;

        /// <summary>
        /// 边界体素数
        /// </summary>
        /// <remarks>用于表面积估算</remarks>
        public int BoundaryCount;

        /// <summary>
        /// 体素数
        /// </summary>
        public int VoxelsCount;

        /// <summary>
        /// 合并单位统计结果
        /// </summary>
        public static StatisticResultEx MergeResults(IReadOnlyCollection<StatisticResultEx> results)
        {
            int totalVoxels = 0;
            int totalBoundary = 0;
            float totalHuSum = 0;
            float totalHuSumSq = 0;
            float totalMinHU = float.MaxValue;
            float totalMaxHU = float.MinValue;
            foreach (StatisticResultEx result in results)
            {
                totalMinHU = Math.Min(totalMinHU, result.MinHU);
                totalMaxHU = Math.Max(totalMaxHU, result.MaxHU);
                totalHuSum += result.HuSum;
                totalHuSumSq += result.HuSumSq;
                totalBoundary += result.BoundaryCount;
                totalVoxels += result.VoxelsCount;
            }

            StatisticResultEx mergedResult = new StatisticResultEx
            {
                MinHU = totalMinHU,
                MaxHU = totalMaxHU,
                HuSum = totalHuSum,
                HuSumSq = totalHuSumSq,
                BoundaryCount = totalBoundary,
                VoxelsCount = totalVoxels
            };

            return mergedResult;
        }

        /// <summary>
        /// 转换统计结果
        /// </summary>
        public StatisticResult ToResult()
        {
            float averageHU = this.VoxelsCount > 0 ? this.HuSum / this.VoxelsCount : 0;
            float stddevHU = MathF.Sqrt((this.HuSumSq / this.VoxelsCount) - (averageHU * averageHU));
            StatisticResult result = new StatisticResult
            {
                MinHU = this.MinHU,
                MaxHU = this.MaxHU,
                AverageHU = averageHU,
                StdDevHU = stddevHU,
                BoundaryCount = this.BoundaryCount,
                VoxelsCount = this.VoxelsCount
            };

            return result;
        }
    }
}
