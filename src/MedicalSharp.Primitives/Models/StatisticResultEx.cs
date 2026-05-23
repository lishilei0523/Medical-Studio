using System;
using System.Runtime.InteropServices;

namespace MedicalSharp.Primitives.Models
{
    /// <summary>
    /// 统计结果
    /// </summary>
    /// <remarks>GPU交换版本</remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 32)]
    public struct StatisticResultEx
    {
        /// <summary>
        /// 创建统计结果构造器
        /// </summary>
        public StatisticResultEx()
        {
            this.MinHU = uint.MaxValue;
            this.MaxHU = uint.MinValue;
        }

        /// <summary>
        /// 最小HU
        /// </summary>
        public uint MinHU;

        /// <summary>
        /// 最大HU
        /// </summary>
        public uint MaxHU;

        /// <summary>
        /// HU累加和
        /// </summary>
        /// <remarks>用于计算平均值</remarks>
        public uint HuSum;

        /// <summary>
        /// HU平方和
        /// </summary>
        /// <remarks>用于计算标准差</remarks>
        public uint HuSumSq;

        /// <summary>
        /// 体素数
        /// </summary>
        public uint VoxelsCount;

        /// <summary>
        /// 边界体素数
        /// </summary>
        /// <remarks>用于表面积估算</remarks>
        public uint BoundaryCount;

        /// <summary>
        /// 转换统计信息
        /// </summary>
        public StatisticResult ToResult()
        {
            float huSum = ToFloat(this.HuSum);
            float huSumsq = ToFloat(this.HuSumSq);
            float averageHU = VoxelsCount > 0 ? huSum / this.VoxelsCount : 0;
            float stddevHU = MathF.Sqrt((huSumsq / this.VoxelsCount) - (averageHU * averageHU));
            StatisticResult result = new StatisticResult
            {
                MinHU = ToFloat(this.MinHU),
                MaxHU = ToFloat(this.MaxHU),
                AverageHU = averageHU,
                StdDevHU = stddevHU,
                VoxelsCount = (int)this.VoxelsCount,
                BoundaryCount = (int)this.BoundaryCount
            };

            return result;
        }

        /// <summary>
        /// uint转float
        /// </summary>
        private static float ToFloat(uint value)
        {
            float floatValue = BitConverter.Int32BitsToSingle((int)value);

            return floatValue;
        }
    }
}
