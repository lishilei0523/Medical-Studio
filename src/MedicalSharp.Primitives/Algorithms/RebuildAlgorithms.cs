using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalSharp.Primitives.Algorithms
{
    /// <summary>
    /// 重建算法
    /// </summary>
    public static class RebuildAlgorithms
    {
        #region # 计算边界距离 —— static float CalculateBoundaryDistance(this VolumeData volumeData...
        /// <summary>
        /// 计算边界距离
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="startPosition">起始位置（世界空间）</param>
        /// <param name="direction">搜索方向（世界空间）</param>
        /// <param name="maxDistance">最大搜索距离（世界空间）</param>
        /// <param name="gradientThreshold">HU值梯度变化阈值</param>
        /// <returns>从起始位置到边界的距离（世界空间）</returns>
        /// <remarks>沿指定方向步进采样，检测HU值梯度突变点作为结构边界</remarks>
        public static float CalculateBoundaryDistance(this VolumeData volumeData, Vector3 startPosition, Vector3 direction, float maxDistance, float gradientThreshold)
        {
            VolumeMetadata metadata = volumeData.Metadata;
            Vector3i startVoxelPos = startPosition.ToVoxelPosition(metadata);
            float prevHU = volumeData.GetPreviewValue(startVoxelPos);

            //步长：取最小体素在世界空间的跨度的一半
            float voxelExtentX = volumeData.Metadata.VolumeScale.X / volumeData.Metadata.VolumeSize.X;
            float voxelExtentY = volumeData.Metadata.VolumeScale.Y / volumeData.Metadata.VolumeSize.Y;
            float voxelExtentZ = volumeData.Metadata.VolumeScale.Z / volumeData.Metadata.VolumeSize.Z;
            float stepSize = Math.Min(Math.Min(voxelExtentX, voxelExtentY), voxelExtentZ) * 0.5f;
            int maxSteps = (int)MathF.Ceiling(maxDistance / stepSize);
            for (int index = 1; index < maxSteps; index++)
            {
                float distance = index * stepSize;
                Vector3 samplePos = startPosition + direction * distance;
                Vector3i sampleVoxelPos = samplePos.ToVoxelPosition(metadata);
                float currentHU = volumeData.GetPreviewValue(sampleVoxelPos);
                float huDiff = currentHU - prevHU;

                //检测HU值梯度突变作为边界
                if (Math.Abs(huDiff) > gradientThreshold)
                {
                    return distance;
                }

                prevHU = currentHU;
            }

            return maxDistance;
        }
        #endregion

        #region # 估算曲线径向宽度 —— static float EstimateRadialWidth(this VolumeData volumeData...
        /// <summary>
        /// 估算曲线径向宽度
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="curve">曲线</param>
        /// <param name="sampleInterval">采样间隔（0~1，默认每隔5%弧长采样一次）</param>
        /// <param name="marginFactor">边距系数（默认1.5，在检测宽度基础上留边显示周围结构）</param>
        /// <param name="gradientThreshold">HU值梯度突变阈值</param>
        /// <returns>估算的径向宽度（世界空间，-0.5~0.5单位空间）</returns>
        /// <remarks>
        /// 沿曲线等间隔采样Frenet框架，在每个框架的Normal正负方向做射线投射；
        /// 通过HU值梯度突变检测结构边界，取所有采样点宽度的中位数并乘以边距系数；
        /// 返回值可直接赋值给u_RadialWidth，与Curve坐标空间一致；
        /// 适用于血管、气管、肠道、牙根管等任意管状/线状结构；
        /// </remarks>
        public static float EstimateRadialWidth(this VolumeData volumeData, Curve curve, float sampleInterval = 0.05f, float marginFactor = 1.5f, float gradientThreshold = 100f)
        {
            #region # 验证

            if (curve == null || curve.FrenetFrames.Length < 2)
            {
                return 0.1f;
            }
            if (volumeData == null)
            {
                return 0.1f;
            }

            #endregion

            //单侧最大搜索距离（世界空间）：取体积最小边长的20%
            Vector3 volumeScale = volumeData.Metadata.VolumeScale;
            float minWorldExtent = Math.Min(Math.Min(volumeScale.X, volumeScale.Y), volumeScale.Z);
            float maxSearchDistance = minWorldExtent * 0.2f;
            float minValidWidth = Math.Min(Math.Min(
                    volumeScale.X / volumeData.Metadata.VolumeSize.X,
                    volumeScale.Y / volumeData.Metadata.VolumeSize.Y),
                    volumeScale.Z / volumeData.Metadata.VolumeSize.Z);

            List<float> widths = [];
            int sampleCount = (int)Math.Ceiling(1f / sampleInterval) + 1;
            for (int index = 0; index < sampleCount; index++)
            {
                float t = index * 1.0f / (sampleCount - 1);
                float arcLength = t * curve.TotalArcLength;
                FrenetFrame frame = curve.GetFrameAtArcLength(arcLength);

                //沿Normal正方向和负方向搜索边界（世界空间）
                float positiveDistance = volumeData.CalculateBoundaryDistance(frame.Position, frame.Normal, maxSearchDistance, gradientThreshold);
                float negativeDistance = volumeData.CalculateBoundaryDistance(frame.Position, -frame.Normal, maxSearchDistance, gradientThreshold);

                //过滤异常值：最小宽度应大于一个体素跨度
                float totalWidth = positiveDistance + negativeDistance;
                if (totalWidth > minValidWidth && totalWidth < maxSearchDistance * 2f)
                {
                    widths.Add(totalWidth);
                }
            }

            if (!widths.Any())
            {
                return 0.1f;
            }

            //取中位数（抗异常值干扰）
            widths.Sort();
            float medianWidth = widths[widths.Count / 2];

            //乘以边距系数
            float radialWidth = medianWidth * marginFactor;

            return radialWidth;
        }
        #endregion
    }
}
