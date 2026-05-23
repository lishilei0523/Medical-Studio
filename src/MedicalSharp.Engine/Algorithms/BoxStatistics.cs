using MedicalSharp.Engine.Renderables;
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

        #region # 适用立方体统计 —— static StatisticResultEx ApplyBoxAnalyse(this VolumeRenderable renderable...
        /// <summary>
        /// 适用立方体统计
        /// </summary>
        /// <param name="renderable">体积渲染对象</param>
        /// <param name="boxLocalMin">立方体局部最小点</param>
        /// <param name="boxLocalMax">立方体局部最大点</param>
        /// <param name="localToWorld">局部到世界变换矩阵</param>
        /// <param name="markValue">标记值（-1=全部，0~255=指定标记值）</param>
        /// <returns>统计结果</returns>
        public static unsafe StatisticResult ApplyBoxAnalyse(this VolumeRenderable renderable, Vector3 boxLocalMin, Vector3 boxLocalMax, Matrix4 localToWorld, int markValue = -1)
        {
            VolumeData volumeData = renderable.VolumeData;
            Vector3i volumeSize = volumeData.Metadata.VolumeSize;
            Vector3 volumeScale = volumeData.Metadata.VolumeScale;
            Matrix4 worldToLocal = localToWorld.Inverted();
            byte* markPtr = (byte*)volumeData.MarkData.ToPointer();
            short* volumePtr = (short*)volumeData.PreviewData.ToPointer();

            //使用Partitioner自动分块
            OrderablePartitioner<Tuple<long, long>> partitioner = Partitioner.Create(0, volumeData.Metadata.VoxelsCount);
            ConcurrentBag<StatisticResultEx> localResults = [];
            Parallel.ForEach(partitioner, range =>
            {
                float localMinHU = float.MaxValue;
                float localMaxHU = float.MinValue;
                float localHuSum = 0;
                float localHuSumSq = 0;
                int localVoxelsCount = 0;
                int localBoundaryCount = 0;
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
                    byte currentMark = markPtr[index];
                    if (markValue != -1 && currentMark != markValue)
                    {
                        continue;
                    }

                    //统计
                    float huValue = volumePtr[index];
                    localHuSum += huValue;
                    localHuSumSq += huValue * huValue;
                    localVoxelsCount++;
                    if (huValue < localMinHU)
                    {
                        localMinHU = huValue;
                    }
                    if (huValue > localMaxHU)
                    {
                        localMaxHU = huValue;
                    }

                    //边界判断
                    if (IsBoundaryVoxel(voxelPosition, volumeSize, boxLocalMin, boxLocalMax, volumeScale, worldToLocal))
                    {
                        localBoundaryCount++;
                    }
                }

                StatisticResultEx localResult = new StatisticResultEx
                {
                    MinHU = localMinHU,
                    MaxHU = localMaxHU,
                    HuSum = localHuSum,
                    HuSumSq = localHuSumSq,
                    BoundaryCount = localBoundaryCount,
                    VoxelsCount = localVoxelsCount
                };

                localResults.Add(localResult);
            });

            //合并结果
            StatisticResultEx mergedResult = StatisticResultEx.MergeResults(localResults);

            //计算最终结果
            StatisticResult result = mergedResult.ToResult();
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

        #region # 判断是否为边界体素 —— static bool IsBoundaryVoxel(in Vector3i voxelPosition, in Vector3i volumeSize...
        /// <summary>
        /// 判断是否为边界体素
        /// </summary>
        private static bool IsBoundaryVoxel(in Vector3i voxelPosition, in Vector3i volumeSize, in Vector3 boxMin, in Vector3 boxMax, in Vector3 volumeScale, in Matrix4 worldToLocal)
        {
            //检查6个邻域
            int[] dx = [1, -1, 0, 0, 0, 0];
            int[] dy = [0, 0, 1, -1, 0, 0];
            int[] dz = [0, 0, 0, 0, 1, -1];
            for (int i = 0; i < 6; i++)
            {
                int nx = voxelPosition.X + dx[i];
                int ny = voxelPosition.Y + dy[i];
                int nz = voxelPosition.Z + dz[i];

                //超出体积边界
                if (nx < 0 || nx >= volumeSize.X || ny < 0 || ny >= volumeSize.Y || nz < 0 || nz >= volumeSize.Z)
                {
                    return true;
                }

                //邻居不在立方体内
                Vector3i nVoxelPosition = new Vector3i(nx, ny, nz);
                if (!IsInBox(nVoxelPosition, boxMin, boxMax, volumeSize, volumeScale, worldToLocal))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
