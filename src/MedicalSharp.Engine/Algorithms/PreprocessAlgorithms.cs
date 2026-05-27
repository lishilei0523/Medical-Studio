using MedicalSharp.Engine.Managers;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Models;
using OpenTK.Graphics.OpenGL4;
using System;

namespace MedicalSharp.Engine.Algorithms
{
    /// <summary>
    /// 预处理算法
    /// </summary>
    public static class PreprocessAlgorithms
    {
        #region # 阈值分割 —— static void ThresholdSegment(VolumeData volumeData...
        /// <summary>
        /// 阈值分割
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="previewTexture">预览纹理</param>
        /// <param name="markTexture">标记纹理</param>
        /// <param name="minHU">最小HU值</param>
        /// <param name="maxHU">最大HU值</param>
        /// <param name="markValue">标记值</param>
        public static void ThresholdSegment(VolumeData volumeData, Texture3D previewTexture, Texture3D markTexture, float minHU, float maxHU, byte markValue)
        {
            #region # 验证

            if (markValue == 0)
            {
                return;
            }
            if (minHU >= maxHU)
            {
                throw new ArgumentException("最小HU值必须小于最大HU值！");
            }

            #endregion

            //阈值分割计算着色器
            ShaderProgram segmentShader = ComputerManager.ThresholdSegmentComputer;

            //开启Shader程序
            segmentShader.Use();

            //绑定纹理
            previewTexture.BindImageTexture(0, TextureAccess.ReadOnly);
            markTexture.BindImageTexture(1, TextureAccess.WriteOnly);

            //设置参数
            segmentShader.SetUniformVector3i("u_VolumeSize", volumeData.Metadata.VolumeSize);
            segmentShader.SetUniformFloat("u_MinHU", minHU);
            segmentShader.SetUniformFloat("u_MaxHU", maxHU);
            segmentShader.SetUniformUInt("u_MarkValue", markValue);

            //调度执行
            ComputerManager.DispatchCompute3D(volumeData.Metadata.VolumeSize);

            //取消使用
            segmentShader.Unuse();

            //同步CPU端
            SyncAlgorithms.SyncMarkDataFromGpu(volumeData, markTexture);
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
