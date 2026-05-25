using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MedicalSharp.Engine.Algorithms
{
    /// <summary>
    /// 统计算法
    /// </summary>
    public static class StatisticsAlgorithms
    {
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
                    if (!GeometryAlgorithms.IsVoxelInBox(voxelPosition, volumeSize, volumeScale, boxLocalMin, boxLocalMax, worldToLocal))
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
                    if (GeometryAlgorithms.IsVoxelOnBoxBoundary(voxelPosition, volumeSize, volumeScale, boxLocalMin, boxLocalMax, worldToLocal))
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

        #region # 适用矩形统计 —— static StatisticResult ApplyRectangleAnalyse(this VolumeData volumeData...
        /// <summary>
        /// 适用矩形统计
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="pointA">点A</param>
        /// <param name="pointB">点B</param>
        /// <param name="pointC">点C</param>
        /// <param name="pointD">点D</param>
        /// <param name="viewportWidth">视口宽度</param>
        /// <param name="viewportHeight">视口高度</param>
        /// <param name="zoomFactor">缩放因子</param>
        /// <param name="layerPixels">层像素指针</param>
        /// <param name="markValue">标记值</param>
        /// <returns>统计结果</returns>
        public static StatisticResult ApplyRectangleAnalyse(this VolumeData volumeData, Vector2 pointA, Vector2 pointB, Vector2 pointC, Vector2 pointD, int viewportWidth, int viewportHeight, float zoomFactor, byte[] layerPixels, byte? markValue)
        {
            Vector2[] screenCorners = [pointA, pointB, pointC, pointD];
            float rescaleSlope = volumeData.Metadata.RescaleSlope;
            float rescaleIntercept = volumeData.Metadata.RescaleIntercept;

            //统计变量
            float minHu = float.MaxValue;
            float maxHu = float.MinValue;
            double huSum = 0;
            double huSumSq = 0;
            int boundaryPixelsCount = 0;
            int pixelsCount = 0;

            //遍历全部像素
            for (int y = 0; y < viewportHeight; y++)
            {
                for (int x = 0; x < viewportWidth; x++)
                {
                    Vector2 pixelPosition = new Vector2(x + 0.5f, y + 0.5f);

                    //精确判断像素是否在多边形内
                    if (!GeometryAlgorithms.IsPointInPolygon(pixelPosition, screenCorners))
                    {
                        continue;
                    }

                    //翻转Y轴
                    int flippedY = viewportHeight - 1 - y;
                    int index = (flippedY * viewportWidth + x) * 4;
                    byte pixelValue = layerPixels[index];
                    byte currentMark = layerPixels[index + 3];

                    //跳过背景
                    if (pixelValue == 0 && currentMark == 0)
                    {
                        continue;
                    }

                    //标记值过滤
                    if (markValue.HasValue && currentMark != markValue.Value)
                    {
                        continue;
                    }

                    //还原HU值
                    float normalized = pixelValue / 255.0f;
                    float snormValue = normalized * 2.0f - 1.0f;
                    float rawValue = snormValue * 32767.0f;
                    float huValue = rawValue * rescaleSlope + rescaleIntercept;

                    //累加统计
                    if (huValue < minHu)
                    {
                        minHu = huValue;
                    }
                    if (huValue > maxHu)
                    {
                        maxHu = huValue;
                    }
                    huSum += huValue;
                    huSumSq += huValue * huValue;

                    //边界判断：像素在多边形的边上
                    if (GeometryAlgorithms.IsPointOnPolygonEdge(pixelPosition, screenCorners, 0.5f))
                    {
                        boundaryPixelsCount++;
                    }

                    pixelsCount++;
                }
            }

            //像素数量转体素数量，面积缩放因子 = ZoomFactor 的平方
            float areaScale = zoomFactor * zoomFactor;
            int boundaryVoxelsCount = (int)Math.Round(boundaryPixelsCount / areaScale);
            int voxelsCount = (int)Math.Round(pixelsCount / areaScale);

            //计算统计指标
            float averageHu = voxelsCount > 0 ? (float)(huSum / voxelsCount) : 0;
            float variance = voxelsCount > 0 ? (float)((huSumSq / voxelsCount) - (averageHu * averageHu)) : 0;
            float stdDevHu = variance > 0 ? MathF.Sqrt(variance) : 0;

            //构造结果
            StatisticResult result = new StatisticResult
            {
                MinHU = minHu.Equals(float.MaxValue) ? 0 : minHu,
                MaxHU = maxHu.Equals(float.MinValue) ? 0 : maxHu,
                AverageHU = averageHu,
                StdDevHU = stdDevHu,
                BoundaryCount = boundaryVoxelsCount,
                VoxelsCount = voxelsCount
            };
            result.CalculateGeometry(volumeData.Metadata.VoxelVolume, volumeData.Metadata.AverageVoxelArea);

            //保存图像测试
            //Task.Run(() => GeometryAlgorithms.SaveImage(viewportWidth, viewportHeight, layerPixels, screenCorners));

            return result;
        }
        #endregion
    }
}
