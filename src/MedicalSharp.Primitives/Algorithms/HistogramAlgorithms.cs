using MedicalSharp.Primitives.Models;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MedicalSharp.Primitives.Algorithms
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
            short* dataPtr = (short*)volumeData.PreviewData.ToPointer();
            float scale = bins / (maxHU - minHU);

            //局部直方图容器
            ConcurrentBag<uint[]> localHistograms = [];

            //动态分块
            Partitioner<Tuple<long, long>> partitioner = Partitioner.Create(0, totalVoxels);
            Parallel.ForEach(partitioner, range =>
            {
                uint[] localHist = new uint[bins];
                for (long index = range.Item1; index < range.Item2; index++)
                {
                    float hu = dataPtr[index];  //short -> float

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

        #region # 适用直方图均衡化 —— static void ApplyHistogramEqualization(this VolumeData volumeData...
        /// <summary>
        /// 适用直方图均衡化
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="bins">桶数量（默认4096，覆盖[-1024, 3071]）</param>
        /// <param name="minHU">最小HU</param>
        /// <param name="maxHU">最大HU</param>
        /// <remarks>原地修改PreviewData，仅用于增强预览，不可用于定量分析</remarks>
        public static unsafe void ApplyHistogramEqualization(this VolumeData volumeData, int bins = 4096, float minHU = -1024f, float maxHU = 3071f)
        {
            #region # 验证

            if (volumeData == null)
            {
                throw new ArgumentNullException(nameof(volumeData), "体积数据不可为空！");
            }
            if (volumeData.PreviewData == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(volumeData), "预览数据指针未分配！");
            }
            if (bins <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bins), "桶数量必须大于0！");
            }
            if (minHU >= maxHU)
            {
                throw new ArgumentOutOfRangeException(nameof(minHU), "最小HU必须小于最大HU！");
            }

            #endregion

            //统计直方图
            uint[] histogram = volumeData.ApplyHistogram(bins, minHU, maxHU);

            //计算累积分布函数
            float[] cdf = new float[bins];
            cdf[0] = (float)histogram[0] / volumeData.Metadata.VoxelsCount;
            for (int index = 1; index < bins; index++)
            {
                cdf[index] = cdf[index - 1] + (float)histogram[index] / volumeData.Metadata.VoxelsCount;
            }

            //构建LUT表
            short[] lut = new short[65536];  //short范围[-32768, 32767]
            for (int index = 0; index < 65536; index++)
            {
                short huValue = (short)(index - 32768);  //无符号索引 -> 有符号HU

                //计算桶索引
                float binFloat = (huValue - minHU) / (maxHU - minHU) * bins;
                int bin = (int)binFloat;
                if (bin < 0)
                {
                    bin = 0;
                }
                if (bin >= bins)
                {
                    bin = bins - 1;
                }

                //CDF映射到HU范围
                float equalizedHU = minHU + cdf[bin] * (maxHU - minHU);
                int result = (int)equalizedHU;
                if (result < short.MinValue)
                {
                    result = short.MinValue;
                }
                if (result > short.MaxValue)
                {
                    result = short.MaxValue;
                }

                lut[index] = (short)result;
            }

            //逐体素LUT替换
            short* dataPtr = (short*)volumeData.PreviewData.ToPointer();
            long totalVoxels = volumeData.Metadata.VoxelsCount;
            OrderablePartitioner<Tuple<long, long>> partitioner = Partitioner.Create(0L, totalVoxels);
            Parallel.ForEach(partitioner, range =>
            {
                for (long i = range.Item1; i < range.Item2; i++)
                {
                    int index = dataPtr[i] + 32768;  //short -> uint16索引
                    dataPtr[i] = lut[index];
                }
            });
        }
        #endregion

        #region # 计算Otsu最优阈值 —— static void CalculateOtsuThreshold(this VolumeData volumeData...
        /// <summary>
        /// 计算Otsu最优阈值
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="minHU">输出：最小HU（最优阈值 - 50）</param>
        /// <param name="maxHU">输出：最大HU（最优阈值 + 50）</param>
        /// <remarks>基于直方图统计，最大化类间方差，返回分割范围</remarks>
        public static void CalculateOtsuThreshold(this VolumeData volumeData, out float minHU, out float maxHU)
        {
            //统计直方图
            uint[] histogram = volumeData.ApplyHistogram();
            int bins = histogram.Length;
            float huMin = volumeData.Metadata.MinHU;
            float huMax = volumeData.Metadata.MaxHU;

            //总像素数
            long total = 0;
            for (int index = 0; index < bins; index++)
            {
                total += histogram[index];
            }

            if (total == 0)
            {
                minHU = 0;
                maxHU = 0;
                return;
            }

            //计算全局均值
            float globalMean = 0;
            for (int index = 0; index < bins; index++)
            {
                globalMean += index * histogram[index];
            }
            globalMean /= total;

            //遍历所有阈值，找到最大类间方差
            float maxVariance = 0;
            int bestBin = 0;
            long foregroundCount = 0;
            float foregroundSum = 0;
            for (int t = 0; t < bins; t++)
            {
                foregroundCount += histogram[t];
                if (foregroundCount == 0)
                {
                    continue;
                }
                if (foregroundCount == total)
                {
                    break;
                }

                foregroundSum += t * histogram[t];
                float foregroundMean = foregroundSum / foregroundCount;
                float backgroundMean = (globalMean * total - foregroundSum) / (total - foregroundCount);

                //类间方差
                float variance = (float)foregroundCount * (total - foregroundCount)
                                                        * (foregroundMean - backgroundMean)
                                                        * (foregroundMean - backgroundMean);
                if (variance > maxVariance)
                {
                    maxVariance = variance;
                    bestBin = t;
                }
            }

            //桶索引 -> HU值
            float optimalHU = huMin + (bestBin + 0.5f) * (huMax - huMin) / bins;

            //返回分割范围（最优阈值 ± 50HU）
            minHU = optimalHU - 50f;
            maxHU = optimalHU + 50f;
        }
        #endregion
    }
}
