using MedicalSharp.Primitives.Models;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MedicalSharp.Engine.Algorithms
{
    /// <summary>
    /// 直方图算法
    /// </summary>
    public static class HistogramAlgorithms
    {
        #region # 适用灰度直方图统计 —— static uint[] ApplyHistogram(this VolumeData volumeData...
        /// <summary>
        /// 适用灰度直方图统计
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="bins">桶数量（默认4096，覆盖[-1024, 3071]）</param>
        /// <param name="minHU">最小HU</param>
        /// <param name="maxHU">最大HU</param>
        /// <returns>直方图数组，索引：HU值，值：体素数量</returns>
        public static unsafe uint[] ApplyHistogram(this VolumeData volumeData, int bins = 4096, float minHU = -1024f, float maxHU = 3071f)
        {
            #region # 验证

            if (volumeData == null)
            {
                throw new ArgumentNullException(nameof(volumeData), "体积数据不可为空！");
            }
            if (volumeData.OriginalData == IntPtr.Zero)
            {
                throw new ArgumentException("原始数据指针未分配！", nameof(volumeData));
            }
            if (bins <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bins), "桶数量必须大于 0！");
            }
            if (minHU >= maxHU)
            {
                throw new ArgumentOutOfRangeException(nameof(minHU), "最小 HU 必须小于最大 HU！");
            }

            #endregion

            long totalVoxels = volumeData.Metadata.VoxelsCount;
            short* dataPtr = (short*)volumeData.OriginalData.ToPointer();
            float scale = bins / (maxHU - minHU);

            //局部直方图容器
            ConcurrentBag<uint[]> localHistograms = [];

            //动态分块
            OrderablePartitioner<Tuple<long, long>> partitioner = Partitioner.Create(0, totalVoxels);
            Parallel.ForEach(partitioner, range =>
            {
                uint[] localHist = new uint[bins];
                for (long index = range.Item1; index < range.Item2; index++)
                {
                    float hu = dataPtr[index];  // short → float

                    //计算桶索引
                    int bin = (int)((hu - minHU) * scale);
                    if (bin < 0)
                    {
                        bin = 0;
                    }
                    if (bin >= bins)
                    {
                        bin = bins - 1;
                    }

                    localHist[bin]++;
                }

                localHistograms.Add(localHist);
            });

            //归约所有局部直方图
            uint[] histogram = new uint[bins];
            foreach (uint[] localHistogram in localHistograms)
            {
                for (int index = 0; index < bins; index++)
                {
                    histogram[index] += localHistogram[index];
                }
            }

            return histogram;
        }
        #endregion

        #region # 适用归一化灰度直方图 —— static float[] ApplyNormalizedHistogram(this VolumeData...
        /// <summary>
        /// 适用归一化灰度直方图
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="bins">桶数量（默认4096，覆盖[-1024, 3071]）</param>
        /// <param name="minHU">最小HU</param>
        /// <param name="maxHU">最大HU</param>
        /// <returns>归一化直方图数组，频率总和=1.0</returns>
        public static float[] ApplyNormalizedHistogram(this VolumeData volumeData, int bins = 4096, float minHU = -1024f, float maxHU = 3071f)
        {
            uint[] histogram = volumeData.ApplyHistogram(bins, minHU, maxHU);
            float[] normalized = new float[bins];
            float total = 0;
            for (int index = 0; index < bins; index++)
            {
                total += histogram[index];
            }

            if (total > 0)
            {
                for (int index = 0; index < bins; index++)
                {
                    normalized[index] = histogram[index] / total;
                }
            }

            return normalized;
        }
        #endregion

        #region # 适用灰度累积分布函数 —— static float[] ApplyCDF(this VolumeData volumeData...
        /// <summary>
        /// 适用灰度累积分布函数
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="bins">桶数量（默认4096，覆盖[-1024, 3071]）</param>
        /// <param name="minHU">最小HU</param>
        /// <param name="maxHU">最大HU</param>
        /// <returns>CDF数组，每个元素=小于等于该桶的体素频率</returns>
        public static float[] ApplyCDF(this VolumeData volumeData, int bins = 4096, float minHU = -1024f, float maxHU = 3071f)
        {
            float[] normalized = volumeData.ApplyNormalizedHistogram(bins, minHU, maxHU);
            float[] cdf = new float[bins];
            cdf[0] = normalized[0];
            for (int index = 1; index < bins; index++)
            {
                cdf[index] = cdf[index - 1] + normalized[index];
            }

            return cdf;
        }
        #endregion
    }
}
