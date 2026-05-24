using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MedicalSharp.Engine.Algorithms
{
    /// <summary>
    /// 立方体统计算法
    /// </summary>
    public static class BoxStatistics
    {
        //Public

        #region # 适用立方体统计 —— static StatisticResultEx ApplyBoxAnalyse(this VolumeData volumeData...
        /// <summary>
        /// 适用立方体统计
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="boxLocalMin">立方体局部最小点</param>
        /// <param name="boxLocalMax">立方体局部最大点</param>
        /// <param name="localToWorld">局部到世界变换矩阵</param>
        /// <param name="markValue">标记值（null=全部，0~255=指定标记值）</param>
        /// <returns>统计结果</returns>
        public static unsafe StatisticResult ApplyBoxAnalyse(this VolumeData volumeData, Vector3 boxLocalMin, Vector3 boxLocalMax, Matrix4 localToWorld, byte? markValue)
        {
            Vector3i volumeSize = volumeData.Metadata.VolumeSize;
            Vector3 volumeScale = volumeData.Metadata.VolumeScale;
            Matrix4 worldToLocal = localToWorld.Inverted();
            byte* markPtr = (byte*)volumeData.MarkData.ToPointer();
            short* volumePtr = (short*)volumeData.PreviewData.ToPointer();

            //使用Partitioner分块
            OrderablePartitioner<Tuple<long, long>> partitioner = Partitioner.Create(0, volumeData.Metadata.VoxelsCount);
            ConcurrentBag<StatisticResult> localResults = [];
            Parallel.ForEach(partitioner, range =>
            {
                StatisticResult localResult = new StatisticResult();
                for (long index = range.Item1; index < range.Item2; index++)
                {
                    //将线性索引转换为3D坐标
                    int x = (int)(index % volumeSize.X);
                    int y = (int)((index % (volumeSize.X * volumeSize.Y)) / volumeSize.X);
                    int z = (int)(index / (volumeSize.X * volumeSize.Y));
                    Vector3i voxelPosition = new Vector3i(x, y, z);

                    //判断体素是否在立方体内
                    if (!IsInBox(voxelPosition, boxLocalMin, boxLocalMax, volumeSize, volumeScale, worldToLocal))
                    {
                        continue;
                    }

                    //标记值检查
                    if (markValue.HasValue && markPtr[index] != markValue.Value)
                    {
                        continue;
                    }

                    //统计
                    float huValue = volumePtr[index];
                    if (huValue < localResult.MinHU)
                    {
                        localResult.MinHU = huValue;
                    }
                    if (huValue > localResult.MaxHU)
                    {
                        localResult.MaxHU = huValue;
                    }
                    localResult.HuSum += huValue;
                    localResult.HuSumSq += huValue * huValue;

                    //边界判断
                    if (BoxStatistics.IsBoundary(voxelPosition, volumeSize, boxLocalMin, boxLocalMax, volumeScale, worldToLocal))
                    {
                        localResult.BoundaryCount++;
                    }

                    localResult.VoxelsCount++;
                }

                localResults.Add(localResult);
            });

            //合并结果
            StatisticResult result = StatisticResult.MergeResults(localResults);
            result.CalculateExpectations();
            result.CalculateGeometry(volumeData.Metadata.VoxelVolume, volumeData.Metadata.AverageVoxelArea);

            return result;
        }
        #endregion


        //Private

        #region # 判断体素是否在立方体内 —— static bool IsInBox(in Vector3i voxelPosition, in Vector3 boxMin...
        /// <summary>
        /// 判断体素是否在立方体内
        /// </summary>
        private static bool IsInBox(in Vector3i voxelPosition, in Vector3 boxMin, in Vector3 boxMax, in Vector3i volumeSize, in Vector3 volumeScale, in Matrix4 worldToLocal)
        {
            //体素坐标 -> 世界坐标 -> 局部坐标
            Vector3 texCoord = (voxelPosition + new Vector3(0.5f)) / volumeSize;
            Vector3 worldPos = (texCoord - new Vector3(0.5f)) * volumeScale;
            Vector3 localPos = Vector3.TransformPosition(worldPos, worldToLocal);
            bool isInBox = localPos.X >= boxMin.X && localPos.X <= boxMax.X &&
                           localPos.Y >= boxMin.Y && localPos.Y <= boxMax.Y &&
                           localPos.Z >= boxMin.Z && localPos.Z <= boxMax.Z;

            return isInBox;
        }
        #endregion

        #region # 判断体素是否为边界体素 —— static bool IsBoundary(in Vector3i voxelPosition, in Vector3i volumeSize...
        /// <summary>
        /// 判断体素是否为边界体素
        /// </summary>
        private static bool IsBoundary(in Vector3i voxelPosition, in Vector3i volumeSize, in Vector3 boxMin, in Vector3 boxMax, in Vector3 volumeScale, in Matrix4 worldToLocal)
        {
            //定义6个方向的偏移量
            int[] offsetX = [1, -1, 0, 0, 0, 0];  //右、左、无、无、无、无
            int[] offsetY = [0, 0, 1, -1, 0, 0];  //无、无、前、后、无、无
            int[] offsetZ = [0, 0, 0, 0, 1, -1];  //无、无、无、无、上、下
            for (int index = 0; index < 6; index++)
            {
                int neighborX = voxelPosition.X + offsetX[index];
                int neighborY = voxelPosition.Y + offsetY[index];
                int neighborZ = voxelPosition.Z + offsetZ[index];

                //邻居超出体积边界
                if (neighborX < 0 || neighborX >= volumeSize.X ||
                    neighborY < 0 || neighborY >= volumeSize.Y ||
                    neighborZ < 0 || neighborZ >= volumeSize.Z)
                {
                    return true;
                }

                //邻居不在立方体内
                Vector3i neighborPosition = new Vector3i(neighborX, neighborY, neighborZ);
                if (!IsInBox(neighborPosition, boxMin, boxMax, volumeSize, volumeScale, worldToLocal))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
