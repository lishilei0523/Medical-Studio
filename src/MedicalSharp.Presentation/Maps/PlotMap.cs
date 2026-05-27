using Avalonia.Media.Imaging;
using ScottPlot;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MedicalSharp.Presentation.Maps
{
    /// <summary>
    /// 图表映射
    /// </summary>
    public static class PlotMap
    {
        #region # 生成直方图图像 —— static Bitmap GenerateHistogramImage(this uint[] histogram...
        /// <summary>
        /// 生成直方图图像
        /// </summary>
        /// <param name="histogram">直方图数组</param>
        /// <param name="width">直方图图像宽度</param>
        /// <param name="height">直方图图像高度</param>
        /// <returns>直方图图像矩阵</returns>
        public static Bitmap GenerateHistogramImage(this uint[] histogram, int width = 1024, int height = 768)
        {
            //ScottPlot绘图
            double[] values = histogram.Select(x => (double)x).ToArray();
            double[] positions = new double[values.Length];
            for (int index = 0; index < positions.Length; index++)
            {
                positions[index] = index - 1024;
            }

            using Plot plot = new Plot();
            plot.Add.Bars(positions, values);
            byte[] imageBytes = plot.GetImageBytes(width, height);

            //构建Avalonia图像
            using MemoryStream stream = new MemoryStream(imageBytes);
            Bitmap bitmap = new Bitmap(stream);

            return bitmap;
        }
        #endregion

        #region # 获取SKImage —— static SKImage GetSKImage(this Plot plot, int width, int height)
        /// <summary>
        /// 获取SKImage
        /// </summary>
        /// <param name="plot">绘图器</param>
        /// <param name="width">图像宽度</param>
        /// <param name="height">图像高度</param>
        /// <returns>SKImage</returns>
        public static SKImage GetSKImage(this Plot plot, int width = 1024, int height = 768)
        {
            Image image = plot.GetImage(width, height);
            Type imageType = image.GetType();
            PropertyInfo propertyInfo = imageType.GetProperty(nameof(SKImage), BindingFlags.Instance | BindingFlags.NonPublic);
            SKImage skImage = (SKImage)propertyInfo!.GetValue(image);

            return skImage;
        }
        #endregion
    }
}
