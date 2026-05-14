using System.Runtime.InteropServices;

namespace MedicalSharp.Primitives.Models
{
    /// <summary>
    /// 统计结果
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct StatisticResult
    {
        /// <summary>
        /// 创建统计结果构造器
        /// </summary>
        /// <param name="length">周长</param>
        /// <param name="surfaceArea">表面积</param>
        /// <param name="volume">体积</param>
        /// <param name="longDiameter">长径</param>
        /// <param name="shortDiameter">短径</param>
        /// <param name="maxDiameter">最大直径</param>
        /// <param name="sphericity">球形度</param>
        /// <param name="minHU">最小HU</param>
        /// <param name="maxHU">最大HU</param>
        /// <param name="averageHU">平均HU</param>
        /// <param name="stdDevHU">标准差</param>
        /// <param name="voxelsCount">体素数</param>
        public StatisticResult(float length, float surfaceArea, float volume, float longDiameter, float shortDiameter, float maxDiameter,
            float sphericity, float minHU, float maxHU, float averageHU, float stdDevHU, int voxelsCount)
            : this()
        {
            this.Length = length;
            this.SurfaceArea = surfaceArea;
            this.Volume = volume;
            this.LongDiameter = longDiameter;
            this.ShortDiameter = shortDiameter;
            this.MaxDiameter = maxDiameter;
            this.Sphericity = sphericity;
            this.MinHU = minHU;
            this.MaxHU = maxHU;
            this.AverageHU = averageHU;
            this.StdDevHU = stdDevHU;
            this.VoxelsCount = voxelsCount;
        }

        /// <summary>
        /// 周长
        /// </summary>
        /// <remarks>mm</remarks>
        public readonly float Length;

        /// <summary>
        /// 表面积
        /// </summary>
        /// <remarks>mm²</remarks>
        public readonly float SurfaceArea;

        /// <summary>
        /// 体积
        /// </summary>
        /// <remarks>mm³</remarks>
        public readonly float Volume;

        /// <summary>
        /// 长径
        /// </summary>
        /// <remarks>mm</remarks>
        public readonly float LongDiameter;

        /// <summary>
        /// 短径
        /// </summary>
        /// <remarks>mm</remarks>
        public readonly float ShortDiameter;

        /// <summary>
        /// 最大直径
        /// </summary>
        /// <remarks>mm</remarks>
        public readonly float MaxDiameter;

        /// <summary>
        /// 球形度
        /// </summary>
        /// <remarks>值域：0~1，越接近1越接近球体</remarks>
        public readonly float Sphericity;

        /// <summary>
        /// 最小HU
        /// </summary>
        public readonly float MinHU;

        /// <summary>
        /// 最大HU
        /// </summary>
        public readonly float MaxHU;

        /// <summary>
        /// 平均HU
        /// </summary>
        public readonly float AverageHU;

        /// <summary>
        /// 标准差
        /// </summary>
        public readonly float StdDevHU;

        /// <summary>
        /// 体素数
        /// </summary>
        public readonly int VoxelsCount;
    }
}
