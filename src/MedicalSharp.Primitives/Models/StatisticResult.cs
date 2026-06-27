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
        public double HuSum;

        /// <summary>
        /// HU平方和
        /// </summary>
        /// <remarks>用于计算标准差</remarks>
        public double HuSumSq;

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
        /// 周长
        /// </summary>
        /// <remarks>mm</remarks>
        public float Perimeter;

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

            double average = this.HuSum / this.VoxelsCount;
            double averageSq = average * average;
            double variance = (this.HuSumSq / this.VoxelsCount) - averageSq;
            this.AverageHU = (float)average;
            this.StdDevHU = (float)Math.Sqrt(Math.Max(variance, 0));
        }
    }
}
