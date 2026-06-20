using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalSharp.Primitives.Algorithms
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
        /// <param name="minVoxelPos">最小体素位置</param>
        /// <param name="maxVoxelPos">最大体素位置</param>
        /// <param name="boxLocalMin">立方体最小点（局部空间）</param>
        /// <param name="boxLocalMax">立方体最大点（局部空间）</param>
        /// <param name="localToWorld">局部到世界变换矩阵</param>
        /// <param name="markValue">标记值（null=全部，0~255=指定标记值）</param>
        /// <returns>统计结果</returns>
        public static unsafe StatisticResult ApplyBoxAnalyse(this VolumeData volumeData, Vector3i minVoxelPos, Vector3i maxVoxelPos, Vector3 boxLocalMin, Vector3 boxLocalMax, Matrix4 localToWorld, byte? markValue)
        {
            Vector3i volumeSize = volumeData.Metadata.VolumeSize;
            Vector3 volumeScale = volumeData.Metadata.VolumeScale;
            byte* markPtr = (byte*)volumeData.MarkData.ToPointer();
            short* volumePtr = (short*)volumeData.PreviewData.ToPointer();

            //计算逆矩阵
            Matrix4 worldToLocal = localToWorld.Inverted();

            //遍历包围盒内的体素
            StatisticResult result = new StatisticResult();
            for (int z = minVoxelPos.Z; z <= maxVoxelPos.Z; z++)
            {
                for (int y = minVoxelPos.Y; y <= maxVoxelPos.Y; y++)
                {
                    for (int x = minVoxelPos.X; x <= maxVoxelPos.X; x++)
                    {
                        long index = (long)z * volumeSize.X * volumeSize.Y + y * volumeSize.X + x;
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
                        if (huValue < result.MinHU)
                        {
                            result.MinHU = huValue;
                        }
                        if (huValue > result.MaxHU)
                        {
                            result.MaxHU = huValue;
                        }
                        result.HuSum += huValue;
                        result.HuSumSq += huValue * huValue;
                    }
                }
            }

            return result;
        }
        #endregion

        #region # 适用球体统计 —— static StatisticResult ApplySphereAnalyse(this VolumeData volumeData...
        /// <summary>
        /// 适用球体统计
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="minVoxelPos">最小体素位置</param>
        /// <param name="maxVoxelPos">最大体素位置</param>
        /// <param name="sphereCenter">球心（世界坐标）</param>
        /// <param name="sphereRadius">半径（世界单位）</param>
        /// <param name="markValue">标记值（null=全部，0~255=指定标记值）</param>
        /// <returns>统计结果</returns>
        public static unsafe StatisticResult ApplySphereAnalyse(this VolumeData volumeData, Vector3i minVoxelPos, Vector3i maxVoxelPos, Vector3 sphereCenter, float sphereRadius, byte? markValue)
        {
            Vector3i volumeSize = volumeData.Metadata.VolumeSize;
            Vector3 volumeScale = volumeData.Metadata.VolumeScale;
            byte* markPtr = (byte*)volumeData.MarkData.ToPointer();
            short* volumePtr = (short*)volumeData.PreviewData.ToPointer();

            //遍历包围盒内的体素
            StatisticResult result = new StatisticResult();
            for (int z = minVoxelPos.Z; z <= maxVoxelPos.Z; z++)
            {
                for (int y = minVoxelPos.Y; y <= maxVoxelPos.Y; y++)
                {
                    for (int x = minVoxelPos.X; x <= maxVoxelPos.X; x++)
                    {
                        long index = (long)z * volumeSize.X * volumeSize.Y + y * volumeSize.X + x;
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

                        // 统计
                        float huValue = volumePtr[index];
                        if (huValue < result.MinHU)
                        {
                            result.MinHU = huValue;
                        }
                        if (huValue > result.MaxHU)
                        {
                            result.MaxHU = huValue;
                        }
                        result.HuSum += huValue;
                        result.HuSumSq += huValue * huValue;
                    }
                }
            }

            return result;
        }
        #endregion

        #region # 适用圆柱体统计 —— static StatisticResult ApplyCylinderAnalyse(this VolumeData volumeData...
        /// <summary>
        /// 适用圆柱体统计
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="minVoxelPos">最小体素位置</param>
        /// <param name="maxVoxelPos">最大体素位置</param>
        /// <param name="cylinderCenter">圆柱中心（世界坐标）</param>
        /// <param name="cylinderAxis">圆柱轴方向（归一化）</param>
        /// <param name="cylinderRadius">半径（世界单位）</param>
        /// <param name="cylinderHeight">高度（世界单位）</param>
        /// <param name="markValue">标记值（null=全部，0~255=指定标记值）</param>
        /// <returns>统计结果</returns>
        public static unsafe StatisticResult ApplyCylinderAnalyse(this VolumeData volumeData, Vector3i minVoxelPos, Vector3i maxVoxelPos, Vector3 cylinderCenter, Vector3 cylinderAxis, float cylinderRadius, float cylinderHeight, byte? markValue)
        {
            Vector3i volumeSize = volumeData.Metadata.VolumeSize;
            Vector3 volumeScale = volumeData.Metadata.VolumeScale;
            byte* markPtr = (byte*)volumeData.MarkData.ToPointer();
            short* volumePtr = (short*)volumeData.PreviewData.ToPointer();

            //遍历包围盒内的体素
            StatisticResult result = new StatisticResult();
            for (int z = minVoxelPos.Z; z <= maxVoxelPos.Z; z++)
            {
                for (int y = minVoxelPos.Y; y <= maxVoxelPos.Y; y++)
                {
                    for (int x = minVoxelPos.X; x <= maxVoxelPos.X; x++)
                    {
                        long index = (long)z * volumeSize.X * volumeSize.Y + y * volumeSize.X + x;
                        Vector3i voxelPosition = new Vector3i(x, y, z);

                        //判断体素是否在圆柱体内
                        if (!GeometryAlgorithms.IsVoxelInCylinder(voxelPosition, volumeSize, volumeScale, cylinderCenter, cylinderAxis, cylinderRadius, cylinderHeight))
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
                        if (huValue < result.MinHU)
                        {
                            result.MinHU = huValue;
                        }
                        if (huValue > result.MaxHU)
                        {
                            result.MaxHU = huValue;
                        }
                        result.HuSum += huValue;
                        result.HuSumSq += huValue * huValue;
                    }
                }
            }

            return result;
        }
        #endregion

        #region # 适用凸多面体统计 —— static StatisticResult ApplyConvexPolyhedronAnalyse(this VolumeData volumeData...
        /// <summary>
        /// 适用凸多面体统计
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="minVoxelPos">最小体素位置</param>
        /// <param name="maxVoxelPos">最大体素位置</param>
        /// <param name="faces">凸多面体的面列表（平面方程）</param>
        /// <param name="markValue">标记值（null=全部，0~255=指定标记值）</param>
        /// <returns>统计结果</returns>
        public static unsafe StatisticResult ApplyConvexPolyhedronAnalyse(this VolumeData volumeData, Vector3i minVoxelPos, Vector3i maxVoxelPos, IReadOnlyList<Plane> faces, byte? markValue)
        {
            Vector3i volumeSize = volumeData.Metadata.VolumeSize;
            Vector3 volumeScale = volumeData.Metadata.VolumeScale;
            byte* markPtr = (byte*)volumeData.MarkData.ToPointer();
            short* volumePtr = (short*)volumeData.PreviewData.ToPointer();

            //遍历包围盒内的体素
            StatisticResult result = new StatisticResult();
            for (int z = minVoxelPos.Z; z <= maxVoxelPos.Z; z++)
            {
                for (int y = minVoxelPos.Y; y <= maxVoxelPos.Y; y++)
                {
                    for (int x = minVoxelPos.X; x <= maxVoxelPos.X; x++)
                    {
                        long index = (long)z * volumeSize.X * volumeSize.Y + y * volumeSize.X + x;
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
                        if (huValue < result.MinHU)
                        {
                            result.MinHU = huValue;
                        }
                        if (huValue > result.MaxHU)
                        {
                            result.MaxHU = huValue;
                        }
                        result.HuSum += huValue;
                        result.HuSumSq += huValue * huValue;
                    }
                }
            }

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
        /// <param name="layerPixels">层像素指针</param>
        /// <param name="markValue">标记值</param>
        /// <returns>统计结果</returns>
        public static StatisticResult ApplyRectangleAnalyse(this VolumeData volumeData, in Vector2 pointA, in Vector2 pointB, in Vector2 pointC, in Vector2 pointD, int viewportWidth, int viewportHeight, byte[] layerPixels, byte? markValue)
        {
            Vector2[] screenCorners = [pointA, pointB, pointC, pointD];

            //计算矩形屏幕包围盒
            float minX = Math.Min(Math.Min(pointA.X, pointB.X), Math.Min(pointC.X, pointD.X));
            float maxX = Math.Max(Math.Max(pointA.X, pointB.X), Math.Max(pointC.X, pointD.X));
            float minY = Math.Min(Math.Min(pointA.Y, pointB.Y), Math.Min(pointC.Y, pointD.Y));
            float maxY = Math.Max(Math.Max(pointA.Y, pointB.Y), Math.Max(pointC.Y, pointD.Y));

            //裁剪到视口范围
            int startX = (int)Math.Floor(Math.Max(0, minX));
            int endX = (int)Math.Ceiling(Math.Min(viewportWidth - 1, maxX));
            int startY = (int)Math.Floor(Math.Max(0, minY));
            int endY = (int)Math.Ceiling(Math.Min(viewportHeight - 1, maxY));

            //遍历包围盒范围内像素
            StatisticResult result = new StatisticResult();
            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
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

                    //统计
                    if (huValue < result.MinHU)
                    {
                        result.MinHU = huValue;
                    }
                    if (huValue > result.MaxHU)
                    {
                        result.MaxHU = huValue;
                    }
                    result.HuSum += huValue;
                    result.HuSumSq += huValue * huValue;
                }
            }

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
        /// <param name="layerPixels">层像素指针</param>
        /// <param name="markValue">标记值</param>
        /// <returns>统计结果</returns>
        public static StatisticResult ApplyCircleAnalyse(this VolumeData volumeData, Vector2 center, float radius, int viewportWidth, int viewportHeight, byte[] layerPixels, byte? markValue)
        {
            float radiusSq = radius * radius;

            //计算圆形屏幕包围盒
            int minX = (int)Math.Floor(center.X - radius - 1);
            int maxX = (int)Math.Ceiling(center.X + radius + 1);
            int minY = (int)Math.Floor(center.Y - radius - 1);
            int maxY = (int)Math.Ceiling(center.Y + radius + 1);

            //裁剪到视口范围
            int startX = Math.Max(0, minX);
            int endX = Math.Min(viewportWidth - 1, maxX);
            int startY = Math.Max(0, minY);
            int endY = Math.Min(viewportHeight - 1, maxY);

            //遍历包围盒范围内像素
            StatisticResult result = new StatisticResult();
            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
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

                    //统计
                    if (huValue < result.MinHU)
                    {
                        result.MinHU = huValue;
                    }
                    if (huValue > result.MaxHU)
                    {
                        result.MaxHU = huValue;
                    }
                    result.HuSum += huValue;
                    result.HuSumSq += huValue * huValue;
                }
            }

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
        /// <param name="layerPixels">层像素指针</param>
        /// <param name="markValue">标记值</param>
        /// <returns>统计结果</returns>
        public static StatisticResult ApplyEllipseAnalyse(this VolumeData volumeData, Vector2 center, float halfWidth, float halfHeight, int viewportWidth, int viewportHeight, byte[] layerPixels, byte? markValue)
        {
            //椭圆方程参数
            float aSq = halfWidth * halfWidth;   // 半宽平方
            float bSq = halfHeight * halfHeight; // 半高平方

            //计算椭圆屏幕包围盒
            int minX = (int)Math.Floor(center.X - halfWidth - 1);
            int maxX = (int)Math.Ceiling(center.X + halfWidth + 1);
            int minY = (int)Math.Floor(center.Y - halfHeight - 1);
            int maxY = (int)Math.Ceiling(center.Y + halfHeight + 1);

            //裁剪到视口范围
            int startX = Math.Max(0, minX);
            int endX = Math.Min(viewportWidth - 1, maxX);
            int startY = Math.Max(0, minY);
            int endY = Math.Min(viewportHeight - 1, maxY);

            //遍历包围盒范围内像素
            StatisticResult result = new StatisticResult();
            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
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

                    //统计
                    if (huValue < result.MinHU)
                    {
                        result.MinHU = huValue;
                    }
                    if (huValue > result.MaxHU)
                    {
                        result.MaxHU = huValue;
                    }
                    result.HuSum += huValue;
                    result.HuSumSq += huValue * huValue;
                }
            }

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
        /// <param name="layerPixels">层像素指针</param>
        /// <param name="markValue">标记值</param>
        /// <returns>统计结果</returns>
        public static StatisticResult ApplyPolygonAnalyse(this VolumeData volumeData, Vector2[] screenVertices, int viewportWidth, int viewportHeight, byte[] layerPixels, byte? markValue)
        {
            //计算多边形屏幕包围盒
            float minX = screenVertices.Min(vertex => vertex.X);
            float maxX = screenVertices.Max(vertex => vertex.X);
            float minY = screenVertices.Min(vertex => vertex.Y);
            float maxY = screenVertices.Max(vertex => vertex.Y);

            //裁剪到视口范围
            int startX = (int)Math.Floor(Math.Max(0, minX));
            int endX = (int)Math.Ceiling(Math.Min(viewportWidth - 1, maxX));
            int startY = (int)Math.Floor(Math.Max(0, minY));
            int endY = (int)Math.Ceiling(Math.Min(viewportHeight - 1, maxY));

            //遍历包围盒范围内像素
            StatisticResult result = new StatisticResult();
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

                    //统计
                    if (huValue < result.MinHU)
                    {
                        result.MinHU = huValue;
                    }
                    if (huValue > result.MaxHU)
                    {
                        result.MaxHU = huValue;
                    }
                    result.HuSum += huValue;
                    result.HuSumSq += huValue * huValue;
                }
            }

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
            ////翻转生成SK图像
            //using SKBitmap bitmap = new SKBitmap(viewportWidth, viewportHeight, SKColorType.Rgba8888, SKAlphaType.Unpremul);
            //byte* targetPtr = (byte*)bitmap.GetPixels().ToPointer();
            //fixed (byte* sourcePtr = layerPixels)
            //{
            //    int stride = viewportWidth * 4;
            //    for (int y = 0; y < viewportHeight; y++)
            //    {
            //        int sourceY = viewportHeight - 1 - y;  //翻转Y轴
            //        byte* sourceRow = sourcePtr + sourceY * stride;
            //        byte* targetRow = targetPtr + y * stride;

            //        //复制整行（RGBA -> RGBA，顺序相同）
            //        Buffer.MemoryCopy(sourceRow, targetRow, stride, stride);
            //    }
            //}

            ////定义矩形
            //int minX = (int)Math.Min(Math.Min(screenCorners[0].X, screenCorners[1].X), Math.Min(screenCorners[2].X, screenCorners[3].X));
            //int maxX = (int)Math.Max(Math.Max(screenCorners[0].X, screenCorners[1].X), Math.Max(screenCorners[2].X, screenCorners[3].X));
            //int minY = (int)Math.Min(Math.Min(screenCorners[0].Y, screenCorners[1].Y), Math.Min(screenCorners[2].Y, screenCorners[3].Y));
            //int maxY = (int)Math.Max(Math.Max(screenCorners[0].Y, screenCorners[1].Y), Math.Max(screenCorners[2].Y, screenCorners[3].Y));
            //SKRect reactangle = SKRect.Create(minX, minY, maxX - minX, maxY - minY);

            ////绘制矩形
            //using SKCanvas canvas = new SKCanvas(bitmap);
            //using SKPaint fill = new SKPaint();
            //using SKPaint stroke = new SKPaint();
            //fill.Style = SKPaintStyle.Fill;
            //fill.Color = SKColors.White;
            //fill.IsAntialias = true;
            //stroke.Style = SKPaintStyle.Stroke;
            //stroke.Color = SKColors.Black;
            //stroke.StrokeWidth = 1;
            //stroke.IsAntialias = true;
            //canvas.DrawRect(reactangle, fill);
            //canvas.DrawRect(reactangle, stroke);

            ////保存文件
            //using FileStream stream = File.OpenWrite("MPR.png");
            //bitmap.Encode(SKEncodedImageFormat.Png, 80).SaveTo(stream);
        }
        #endregion

        #region # 保存图像 —— static void SaveImage(int viewportWidth, int viewportHeight...
        /// <summary>
        /// 保存图像
        /// </summary>
        /// <remarks>用于调试</remarks>
        public static unsafe void SaveImage(int viewportWidth, int viewportHeight, byte[] layerPixels, Vector2 center, float radius)
        {
            ////翻转生成SK图像
            //using SKBitmap bitmap = new SKBitmap(viewportWidth, viewportHeight, SKColorType.Rgba8888, SKAlphaType.Unpremul);
            //byte* targetPtr = (byte*)bitmap.GetPixels().ToPointer();
            //fixed (byte* sourcePtr = layerPixels)
            //{
            //    int stride = viewportWidth * 4;
            //    for (int y = 0; y < viewportHeight; y++)
            //    {
            //        int sourceY = viewportHeight - 1 - y;  //翻转Y轴
            //        byte* sourceRow = sourcePtr + sourceY * stride;
            //        byte* targetRow = targetPtr + y * stride;

            //        //复制整行（RGBA -> RGBA，顺序相同）
            //        Buffer.MemoryCopy(sourceRow, targetRow, stride, stride);
            //    }
            //}

            ////绘制圆形
            //using SKCanvas canvas = new SKCanvas(bitmap);
            //using SKPaint fill = new SKPaint();
            //using SKPaint stroke = new SKPaint();
            //fill.Style = SKPaintStyle.Fill;
            //fill.Color = SKColors.White;
            //fill.IsAntialias = true;
            //stroke.Style = SKPaintStyle.Stroke;
            //stroke.Color = SKColors.Black;
            //stroke.StrokeWidth = 1;
            //stroke.IsAntialias = true;
            //canvas.DrawCircle(center.X, center.Y, radius, fill);
            //canvas.DrawCircle(center.X, center.Y, radius, stroke);

            ////保存文件
            //using FileStream stream = File.OpenWrite("MPR-Circle.png");
            //bitmap.Encode(SKEncodedImageFormat.Png, 80).SaveTo(stream);
        }
        #endregion
    }
}
