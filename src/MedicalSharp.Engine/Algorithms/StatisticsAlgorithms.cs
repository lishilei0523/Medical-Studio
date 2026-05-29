using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MedicalSharp.Engine.Algorithms
{
    /// <summary>
    /// 统计算法
    /// </summary>
    public static class StatisticsAlgorithms
    {
        #region # 适用立方体统计 —— static StatisticResult ApplyBoxAnalyse(this VolumeData volumeData...
        /// <summary>
        /// 适用立方体统计
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="boxLocalMin">立方体最小点（局部空间）</param>
        /// <param name="boxLocalMax">立方体最大点（局部空间）</param>
        /// <param name="localToWorld">局部到世界变换矩阵</param>
        /// <param name="markValue">标记值（null=全部，0~255=指定标记值）</param>
        /// <returns>统计结果</returns>
        public static unsafe StatisticResult ApplyBoxAnalyse(this VolumeData volumeData, Vector3 boxLocalMin, Vector3 boxLocalMax, Matrix4 localToWorld, byte? markValue)
        {
            Vector3i volumeSize = volumeData.Metadata.VolumeSize;
            Vector3 volumeScale = volumeData.Metadata.VolumeScale;
            byte* markPtr = (byte*)volumeData.MarkData.ToPointer();
            short* volumePtr = (short*)volumeData.PreviewData.ToPointer();

            //计算逆矩阵
            Matrix4 worldToLocal = localToWorld.Inverted();

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

        #region # 适用球体统计 —— static StatisticResult ApplySphereAnalyse(this VolumeData volumeData...
        /// <summary>
        /// 适用球体统计
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="sphereCenter">球心（世界坐标）</param>
        /// <param name="sphereRadius">半径（世界单位）</param>
        /// <param name="markValue">标记值（null=全部，0~255=指定标记值）</param>
        /// <returns>统计结果</returns>
        public static unsafe StatisticResult ApplySphereAnalyse(this VolumeData volumeData, Vector3 sphereCenter, float sphereRadius, byte? markValue)
        {
            Vector3i volumeSize = volumeData.Metadata.VolumeSize;
            Vector3 volumeScale = volumeData.Metadata.VolumeScale;
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

                    //判断体素是否在球体内
                    if (!GeometryAlgorithms.IsVoxelInSphere(voxelPosition, volumeSize, volumeScale, sphereCenter, sphereRadius))
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
                    if (GeometryAlgorithms.IsVoxelOnSphereBoundary(voxelPosition, volumeSize, volumeScale, sphereCenter, sphereRadius, 0.5f))
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

        #region # 适用圆柱体统计 —— static StatisticResult ApplyCylinderAnalyse(this VolumeData volumeData...
        /// <summary>
        /// 适用圆柱体统计
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="cylinderCenter">圆柱中心（世界坐标）</param>
        /// <param name="cylinderAxis">圆柱轴方向（归一化）</param>
        /// <param name="cylinderRadius">半径（世界单位）</param>
        /// <param name="cylinderHeight">高度（世界单位）</param>
        /// <param name="markValue">标记值（null=全部，0~255=指定标记值）</param>
        /// <returns>统计结果</returns>
        public static unsafe StatisticResult ApplyCylinderAnalyse(this VolumeData volumeData, Vector3 cylinderCenter, Vector3 cylinderAxis, float cylinderRadius, float cylinderHeight, byte? markValue)
        {
            Vector3i volumeSize = volumeData.Metadata.VolumeSize;
            Vector3 volumeScale = volumeData.Metadata.VolumeScale;
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

                    //判断体素是否在圆柱体内
                    if (!GeometryAlgorithms.IsVoxelInCylinder(voxelPosition, volumeSize, volumeScale,
                        cylinderCenter, cylinderAxis, cylinderRadius, cylinderHeight))
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
                    if (huValue < localResult.MinHU) localResult.MinHU = huValue;
                    if (huValue > localResult.MaxHU) localResult.MaxHU = huValue;
                    localResult.HuSum += huValue;
                    localResult.HuSumSq += huValue * huValue;

                    //边界判断
                    if (GeometryAlgorithms.IsVoxelOnCylinderBoundary(voxelPosition, volumeSize, volumeScale,
                        cylinderCenter, cylinderAxis, cylinderRadius, cylinderHeight, 0.5f))
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

        #region # 适用凸多面体统计 —— static StatisticResult ApplyConvexPolyhedronAnalyse(this VolumeData volumeData...
        /// <summary>
        /// 适用凸多面体统计
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="faces">凸多面体的面列表（平面方程）</param>
        /// <param name="markValue">标记值（null=全部，0~255=指定标记值）</param>
        /// <returns>统计结果</returns>
        public static unsafe StatisticResult ApplyConvexPolyhedronAnalyse(this VolumeData volumeData, IReadOnlyList<Plane> faces, byte? markValue)
        {
            Vector3i volumeSize = volumeData.Metadata.VolumeSize;
            Vector3 volumeScale = volumeData.Metadata.VolumeScale;
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

                    //判断体素是否在凸多面体内
                    if (!GeometryAlgorithms.IsVoxelInConvexPolyhedron(voxelPosition, volumeSize, volumeScale, faces))
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
                    if (huValue < localResult.MinHU) localResult.MinHU = huValue;
                    if (huValue > localResult.MaxHU) localResult.MaxHU = huValue;
                    localResult.HuSum += huValue;
                    localResult.HuSumSq += huValue * huValue;

                    //边界判断
                    if (GeometryAlgorithms.IsVoxelOnConvexPolyhedronBoundary(voxelPosition, volumeSize, volumeScale, faces, 0.5f))
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
        public static StatisticResult ApplyRectangleAnalyse(this VolumeData volumeData, in Vector2 pointA, in Vector2 pointB, in Vector2 pointC, in Vector2 pointD, int viewportWidth, int viewportHeight, float zoomFactor, byte[] layerPixels, byte? markValue)
        {
            Vector2[] screenCorners = [pointA, pointB, pointC, pointD];

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
                    float huValue = snormValue * 32767.0f;

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
            //Task.Run(() => SaveImage(viewportWidth, viewportHeight, layerPixels, screenCorners));

            return result;
        }
        #endregion

        #region # 适用圆形统计 —— static StatisticResult ApplyCircleAnalyse(this VolumeData volumeData...
        /// <summary>
        /// 适用圆形统计
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="center">圆心（屏幕坐标）</param>
        /// <param name="radius">半径（像素）</param>
        /// <param name="viewportWidth">视口宽度</param>
        /// <param name="viewportHeight">视口高度</param>
        /// <param name="zoomFactor">缩放因子</param>
        /// <param name="layerPixels">层像素指针</param>
        /// <param name="markValue">标记值</param>
        /// <returns>统计结果</returns>
        public static StatisticResult ApplyCircleAnalyse(this VolumeData volumeData, Vector2 center, float radius, int viewportWidth, int viewportHeight, float zoomFactor, byte[] layerPixels, byte? markValue)
        {
            float radiusSq = radius * radius;

            //统计变量
            float minHu = float.MaxValue;
            float maxHu = float.MinValue;
            double huSum = 0;
            double huSumSq = 0;
            int boundaryPixelsCount = 0;
            int pixelsCount = 0;

            //计算圆形的包围盒（优化遍历范围）
            int minX = (int)Math.Max(0, center.X - radius - 1);
            int maxX = (int)Math.Min(viewportWidth - 1, center.X + radius + 1);
            int minY = (int)Math.Max(0, center.Y - radius - 1);
            int maxY = (int)Math.Min(viewportHeight - 1, center.Y + radius + 1);

            //遍历包围盒内的像素
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    Vector2 pixelPos = new Vector2(x + 0.5f, y + 0.5f);

                    //计算到圆心的距离平方
                    float dx = pixelPos.X - center.X;
                    float dy = pixelPos.Y - center.Y;
                    float distSq = dx * dx + dy * dy;

                    //判断是否在圆内
                    if (distSq > radiusSq)
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
                    float huValue = snormValue * 32767.0f;

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

                    //边界判断：像素在圆边上（距离在半径附近）
                    float distance = MathF.Sqrt(distSq);
                    if (Math.Abs(distance - radius) <= 0.8f)
                    {
                        boundaryPixelsCount++;
                    }

                    pixelsCount++;
                }
            }

            //像素数量转体素数量
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
            //Task.Run(() => SaveImage(viewportWidth, viewportHeight, layerPixels, center, radius));

            return result;
        }
        #endregion

        #region # 适用椭圆形统计 —— static StatisticResult ApplyEllipseAnalyse(this VolumeData volumeData...
        /// <summary>
        /// 适用椭圆形统计
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="center">椭圆中心（屏幕坐标）</param>
        /// <param name="halfWidth">半宽（像素）</param>
        /// <param name="halfHeight">半高（像素）</param>
        /// <param name="viewportWidth">视口宽度</param>
        /// <param name="viewportHeight">视口高度</param>
        /// <param name="zoomFactor">缩放因子</param>
        /// <param name="layerPixels">层像素指针</param>
        /// <param name="markValue">标记值</param>
        /// <returns>统计结果</returns>
        public static StatisticResult ApplyEllipseAnalyse(this VolumeData volumeData, Vector2 center, float halfWidth, float halfHeight, int viewportWidth, int viewportHeight, float zoomFactor, byte[] layerPixels, byte? markValue)
        {
            //椭圆方程参数
            float aSq = halfWidth * halfWidth;   // 半宽平方
            float bSq = halfHeight * halfHeight; // 半高平方

            //统计变量
            float minHu = float.MaxValue;
            float maxHu = float.MinValue;
            double huSum = 0;
            double huSumSq = 0;
            int boundaryPixelsCount = 0;
            int pixelsCount = 0;

            //计算椭圆包围盒
            int minX = (int)Math.Max(0, center.X - halfWidth - 1);
            int maxX = (int)Math.Min(viewportWidth - 1, center.X + halfWidth + 1);
            int minY = (int)Math.Max(0, center.Y - halfHeight - 1);
            int maxY = (int)Math.Min(viewportHeight - 1, center.Y + halfHeight + 1);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    Vector2 pixelPos = new Vector2(x + 0.5f, y + 0.5f);

                    //计算相对于椭圆中心的偏移
                    float dx = pixelPos.X - center.X;
                    float dy = pixelPos.Y - center.Y;

                    //椭圆判断：(dx²/a²) + (dy²/b²) ≤ 1
                    float value = (dx * dx) / aSq + (dy * dy) / bSq;

                    if (value > 1.0f)
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
                    float huValue = snormValue * 32767.0f;

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

                    //边界判断：像素在椭圆边上（0.9 ≤ value ≤ 1.1）
                    if (Math.Abs(value - 1.0f) <= 0.15f)
                    {
                        boundaryPixelsCount++;
                    }

                    pixelsCount++;
                }
            }

            //像素数量转体素数量
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

            return result;
        }
        #endregion

        #region # 适用多边形统计 —— static StatisticResult ApplyPolygonAnalyse(this VolumeData volumeData...
        /// <summary>
        /// 适用多边形统计
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="screenVertices">多边形顶点（屏幕坐标，按顺序）</param>
        /// <param name="viewportWidth">视口宽度</param>
        /// <param name="viewportHeight">视口高度</param>
        /// <param name="zoomFactor">缩放因子</param>
        /// <param name="layerPixels">层像素指针</param>
        /// <param name="markValue">标记值</param>
        /// <returns>统计结果</returns>
        public static StatisticResult ApplyPolygonAnalyse(this VolumeData volumeData, Vector2[] screenVertices, int viewportWidth, int viewportHeight, float zoomFactor, byte[] layerPixels, byte? markValue)
        {
            //计算多边形包围盒（优化遍历范围）
            float minX = screenVertices.Min(v => v.X);
            float maxX = screenVertices.Max(v => v.X);
            float minY = screenVertices.Min(v => v.Y);
            float maxY = screenVertices.Max(v => v.Y);

            int startX = (int)Math.Max(0, minX - 1);
            int endX = (int)Math.Min(viewportWidth - 1, maxX + 1);
            int startY = (int)Math.Max(0, minY - 1);
            int endY = (int)Math.Min(viewportHeight - 1, maxY + 1);

            //统计变量
            float minHu = float.MaxValue;
            float maxHu = float.MinValue;
            double huSum = 0;
            double huSumSq = 0;
            int boundaryPixelsCount = 0;
            int pixelsCount = 0;

            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    Vector2 pixelPos = new Vector2(x + 0.5f, y + 0.5f);

                    //判断点是否在多边形内
                    if (!GeometryAlgorithms.IsPointInPolygon(pixelPos, screenVertices))
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
                    float huValue = snormValue * 32767.0f;

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
                    if (GeometryAlgorithms.IsPointOnPolygonEdge(pixelPos, screenVertices, 0.8f))
                    {
                        boundaryPixelsCount++;
                    }

                    pixelsCount++;
                }
            }

            //像素数量转体素数量
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

            return result;
        }
        #endregion


        //Private

        #region # 保存图像 —— static void SaveImage(int viewportWidth, int viewportHeight...
        /// <summary>
        /// 保存图像
        /// </summary>
        /// <remarks>用于调试</remarks>
        public static unsafe void SaveImage(int viewportWidth, int viewportHeight, byte[] layerPixels, Vector2[] screenCorners)
        {
            //翻转生成SK图像
            using SKBitmap bitmap = new SKBitmap(viewportWidth, viewportHeight, SKColorType.Rgba8888, SKAlphaType.Unpremul);
            byte* targetPtr = (byte*)bitmap.GetPixels().ToPointer();
            fixed (byte* sourcePtr = layerPixels)
            {
                int stride = viewportWidth * 4;
                for (int y = 0; y < viewportHeight; y++)
                {
                    int sourceY = viewportHeight - 1 - y;  //翻转Y轴
                    byte* sourceRow = sourcePtr + sourceY * stride;
                    byte* targetRow = targetPtr + y * stride;

                    //复制整行（RGBA -> RGBA，顺序相同）
                    Buffer.MemoryCopy(sourceRow, targetRow, stride, stride);
                }
            }

            //定义矩形
            int minX = (int)Math.Min(Math.Min(screenCorners[0].X, screenCorners[1].X), Math.Min(screenCorners[2].X, screenCorners[3].X));
            int maxX = (int)Math.Max(Math.Max(screenCorners[0].X, screenCorners[1].X), Math.Max(screenCorners[2].X, screenCorners[3].X));
            int minY = (int)Math.Min(Math.Min(screenCorners[0].Y, screenCorners[1].Y), Math.Min(screenCorners[2].Y, screenCorners[3].Y));
            int maxY = (int)Math.Max(Math.Max(screenCorners[0].Y, screenCorners[1].Y), Math.Max(screenCorners[2].Y, screenCorners[3].Y));
            SKRect reactangle = SKRect.Create(minX, minY, maxX - minX, maxY - minY);

            //绘制矩形
            using SKCanvas canvas = new SKCanvas(bitmap);
            using SKPaint fill = new SKPaint();
            using SKPaint stroke = new SKPaint();
            fill.Style = SKPaintStyle.Fill;
            fill.Color = SKColors.White;
            fill.IsAntialias = true;
            stroke.Style = SKPaintStyle.Stroke;
            stroke.Color = SKColors.Black;
            stroke.StrokeWidth = 1;
            stroke.IsAntialias = true;
            canvas.DrawRect(reactangle, fill);
            canvas.DrawRect(reactangle, stroke);

            //保存文件
            using FileStream stream = File.OpenWrite("MPR.png");
            bitmap.Encode(SKEncodedImageFormat.Png, 80).SaveTo(stream);
        }
        #endregion

        #region # 保存图像 —— static void SaveImage(int viewportWidth, int viewportHeight...
        /// <summary>
        /// 保存图像
        /// </summary>
        /// <remarks>用于调试</remarks>
        public static unsafe void SaveImage(int viewportWidth, int viewportHeight, byte[] layerPixels, Vector2 center, float radius)
        {
            //翻转生成SK图像
            using SKBitmap bitmap = new SKBitmap(viewportWidth, viewportHeight, SKColorType.Rgba8888, SKAlphaType.Unpremul);
            byte* targetPtr = (byte*)bitmap.GetPixels().ToPointer();
            fixed (byte* sourcePtr = layerPixels)
            {
                int stride = viewportWidth * 4;
                for (int y = 0; y < viewportHeight; y++)
                {
                    int sourceY = viewportHeight - 1 - y;  //翻转Y轴
                    byte* sourceRow = sourcePtr + sourceY * stride;
                    byte* targetRow = targetPtr + y * stride;

                    //复制整行（RGBA -> RGBA，顺序相同）
                    Buffer.MemoryCopy(sourceRow, targetRow, stride, stride);
                }
            }

            //绘制圆形
            using SKCanvas canvas = new SKCanvas(bitmap);
            using SKPaint fill = new SKPaint();
            using SKPaint stroke = new SKPaint();
            fill.Style = SKPaintStyle.Fill;
            fill.Color = SKColors.White;
            fill.IsAntialias = true;
            stroke.Style = SKPaintStyle.Stroke;
            stroke.Color = SKColors.Black;
            stroke.StrokeWidth = 1;
            stroke.IsAntialias = true;
            canvas.DrawCircle(center.X, center.Y, radius, fill);
            canvas.DrawCircle(center.X, center.Y, radius, stroke);

            //保存文件
            using FileStream stream = File.OpenWrite("MPR-Circle.png");
            bitmap.Encode(SKEncodedImageFormat.Png, 80).SaveTo(stream);
        }
        #endregion
    }
}
