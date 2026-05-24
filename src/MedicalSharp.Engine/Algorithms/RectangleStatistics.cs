using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using SkiaSharp;
using System;
using System.IO;

namespace MedicalSharp.Engine.Algorithms
{
    /// <summary>
    /// 矩形统计算法
    /// </summary>
    public static class RectangleStatistics
    {
        //Public

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
                    if (!IsPointInPolygon(pixelPosition, screenCorners))
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
                    if (IsPointOnPolygonEdge(pixelPosition, screenCorners, 0.5f))
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


        //Private

        #region # 判断点是否在多边形内 —— static bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
        /// <summary>
        /// 判断点是否在多边形内
        /// </summary>
        /// <remarks>射线投射法</remarks>
        private static bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                Vector2 vi = polygon[i];
                Vector2 vj = polygon[j];
                bool intersect = ((vi.Y > point.Y) != (vj.Y > point.Y)) &&
                                 (point.X < (vj.X - vi.X) * (point.Y - vi.Y) / (vj.Y - vi.Y) + vi.X);
                if (intersect)
                {
                    inside = !inside;
                }
            }

            return inside;
        }
        #endregion

        #region # 判断点是否在多边形边上 —— static bool IsPointOnPolygonEdge(Vector2 point...
        /// <summary>
        /// 判断点是否在多边形边上
        /// </summary>
        /// <remarks>有容差</remarks>
        private static bool IsPointOnPolygonEdge(Vector2 point, Vector2[] polygon, float tolerance)
        {
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if (IsPointOnSegment(point, polygon[i], polygon[j], tolerance))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region # 判断点是否在线段上 —— static bool IsPointOnSegment(Vector2 point, Vector2 start...
        /// <summary>
        /// 判断点是否在线段上
        /// </summary>
        private static bool IsPointOnSegment(Vector2 point, Vector2 start, Vector2 end, float tolerance)
        {
            //叉积判断是否共线
            float cross = (end.X - start.X) * (point.Y - start.Y) - (end.Y - start.Y) * (point.X - start.X);
            if (Math.Abs(cross) > tolerance)
            {
                return false;
            }

            //点积判断是否在线段范围内
            float dot = (point.X - start.X) * (end.X - start.X) + (point.Y - start.Y) * (end.Y - start.Y);
            if (dot < 0)
            {
                return false;
            }

            float squaredLength = (end.X - start.X) * (end.X - start.X) + (end.Y - start.Y) * (end.Y - start.Y);
            if (dot > squaredLength)
            {
                return false;
            }

            return true;
        }
        #endregion

        #region # 保存图像 —— static void SaveImage(int viewportWidth, int viewportHeight...
        /// <summary>
        /// 保存图像
        /// </summary>
        /// <remarks>用于调试</remarks>
        private static unsafe void SaveImage(int viewportWidth, int viewportHeight, byte[] layerPixels, Vector2[] screenCorners)
        {
            using SKBitmap bitmap = new SKBitmap(viewportWidth, viewportHeight, SKColorType.Bgra8888, SKAlphaType.Unpremul);
            byte* targetPtr = (byte*)bitmap.GetPixels().ToPointer();
            fixed (byte* sourcePtr = layerPixels)
            {
                int stride = viewportWidth * 4;
                for (int y = 0; y < viewportHeight; y++)
                {
                    int srcY = viewportHeight - 1 - y;  //翻转Y轴
                    byte* srcRow = sourcePtr + srcY * stride;
                    byte* dstRow = targetPtr + y * stride;

                    //复制整行（RGB -> RGBA，顺序相同）
                    Buffer.MemoryCopy(srcRow, dstRow, stride, stride);
                }
            }

            using SKCanvas canvas = new SKCanvas(bitmap);
            SKPaint fill = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            SKPaint stroke = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            // 绘制矩形
            int minX = (int)Math.Min(Math.Min(screenCorners[0].X, screenCorners[1].X), Math.Min(screenCorners[2].X, screenCorners[3].X));
            int maxX = (int)Math.Max(Math.Max(screenCorners[0].X, screenCorners[1].X), Math.Max(screenCorners[2].X, screenCorners[3].X));
            int minY = (int)Math.Min(Math.Min(screenCorners[0].Y, screenCorners[1].Y), Math.Min(screenCorners[2].Y, screenCorners[3].Y));
            int maxY = (int)Math.Max(Math.Max(screenCorners[0].Y, screenCorners[1].Y), Math.Max(screenCorners[2].Y, screenCorners[3].Y));
            SKRect rect = SKRect.Create(minX, minY, maxX - minX, maxY - minY);
            canvas.DrawRect(rect, fill);      //填充
            canvas.DrawRect(rect, stroke);    //边框

            using FileStream stream = File.OpenWrite("MPR.png");
            bitmap.Encode(SKEncodedImageFormat.Png, 80).SaveTo(stream);
        }
        #endregion
    }
}
