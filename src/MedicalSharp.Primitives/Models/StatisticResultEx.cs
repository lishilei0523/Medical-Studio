using System;
using System.Runtime.InteropServices;

namespace MedicalSharp.Primitives.Models
{
    /// <summary>
    /// 统计结果
    /// </summary>
    /// <remarks>GPU交换版本</remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct StatisticResultGPU
    {
        /// <summary>
        /// 创建统计结果构造器
        /// </summary>
        public StatisticResultGPU()
        {
            this.MinHU = float.MaxValue;
            this.MaxHU = float.MinValue;
        }

        /// <summary>
        /// 周长
        /// </summary>
        /// <remarks>mm</remarks>
        public float Length;

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
        /// 长径
        /// </summary>
        /// <remarks>mm</remarks>
        public float LongDiameter;

        /// <summary>
        /// 短径
        /// </summary>
        /// <remarks>mm</remarks>
        public float ShortDiameter;

        /// <summary>
        /// 最大直径
        /// </summary>
        /// <remarks>mm</remarks>
        public float MaxDiameter;

        /// <summary>
        /// 球形度
        /// </summary>
        /// <remarks>值域：0~1，越接近1越接近球体</remarks>
        public float Sphericity;

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
        /// 体素数
        /// </summary>
        public int VoxelsCount;

        /// <summary>
        /// 边界体素数
        /// </summary>
        /// <remarks>用于表面积估算</remarks>
        public int BoundaryCount;

        /// <summary>
        /// 16字节对齐填充
        /// </summary>
        private int _padding;

        /// <summary>
        /// 只读属性 - 平均HU
        /// </summary>
        public float AverageHU
        {
            get => this.VoxelsCount > 0 ? this.HuSum / this.VoxelsCount : 0;
        }

        /// <summary>
        /// 只读属性 - 标准差
        /// </summary>
        public float StdDevHU
        {
            get
            {
                #region # 验证

                if (this.VoxelsCount <= 0)
                {
                    return 0;
                }

                #endregion

                float mean = this.AverageHU;
                float variance = (this.HuSumSq / this.VoxelsCount) - (mean * mean);
                float stddev = (float)Math.Sqrt(variance);

                return stddev;
            }
        }

        /// <summary>
        /// 转换统计信息
        /// </summary>
        public StatisticResult ToResult()
        {
            StatisticResult result = new StatisticResult(this.Length, this.SurfaceArea, this.Volume, this.LongDiameter, this.ShortDiameter, this.MaxDiameter, this.Sphericity, this.MinHU, this.MaxHU, this.AverageHU, this.StdDevHU, this.VoxelsCount);

            return result;
        }
    }
}
