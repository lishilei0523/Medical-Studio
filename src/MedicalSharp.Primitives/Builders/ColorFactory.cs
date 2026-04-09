using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Primitives.Builders
{
    /// <summary>
    /// 颜色工厂
    /// </summary>
    public static class ColorFactory
    {
        #region # 红色 —— static Vector4 Red(float opacity = 1.0f)
        /// <summary>
        /// 红色
        /// </summary>
        /// <param name="opacity">不透明度 (0-1)</param>
        public static Vector4 Red(float opacity = 1.0f)
        {
            return new Vector4(1.0f, 0.0f, 0.0f, opacity);
        }
        #endregion

        #region # 绿色 —— static Vector4 Green(float opacity = 1.0f)
        /// <summary>
        /// 绿色
        /// </summary>
        /// <param name="opacity">不透明度 (0-1)</param>
        public static Vector4 Green(float opacity = 1.0f)
        {
            return new Vector4(0.0f, 1.0f, 0.0f, opacity);
        }
        #endregion

        #region # 蓝色 —— static Vector4 Blue(float opacity = 1.0f)
        /// <summary>
        /// 蓝色
        /// </summary>
        /// <param name="opacity">不透明度 (0-1)</param>
        public static Vector4 Blue(float opacity = 1.0f)
        {
            return new Vector4(0.0f, 0.0f, 1.0f, opacity);
        }
        #endregion

        #region # 白色 —— static Vector4 White(float opacity = 1.0f)
        /// <summary>
        /// 白色
        /// </summary>
        /// <param name="opacity">不透明度 (0-1)</param>
        public static Vector4 White(float opacity = 1.0f)
        {
            return new Vector4(1.0f, 1.0f, 1.0f, opacity);
        }
        #endregion

        #region # 黑色 —— static Vector4 Black(float opacity = 1.0f)
        /// <summary>
        /// 黑色
        /// </summary>
        /// <param name="opacity">不透明度 (0-1)</param>
        public static Vector4 Black(float opacity = 1.0f)
        {
            return new Vector4(0.0f, 0.0f, 0.0f, opacity);
        }
        #endregion

        #region # 黄色 —— static Vector4 Yellow(float opacity = 1.0f)
        /// <summary>
        /// 黄色
        /// </summary>
        /// <param name="opacity">不透明度 (0-1)</param>
        public static Vector4 Yellow(float opacity = 1.0f)
        {
            return new Vector4(1.0f, 1.0f, 0.0f, opacity);
        }
        #endregion

        #region # 青色 —— static Vector4 Cyan(float opacity = 1.0f)
        /// <summary>
        /// 青色
        /// </summary>
        /// <param name="opacity">不透明度 (0-1)</param>
        public static Vector4 Cyan(float opacity = 1.0f)
        {
            return new Vector4(0.0f, 1.0f, 1.0f, opacity);
        }
        #endregion

        #region # 品红色 —— static Vector4 Magenta(float opacity = 1.0f)
        /// <summary>
        /// 品红色
        /// </summary>
        /// <param name="opacity">不透明度 (0-1)</param>
        public static Vector4 Magenta(float opacity = 1.0f)
        {
            return new Vector4(1.0f, 0.0f, 1.0f, opacity);
        }
        #endregion

        #region # 灰色 —— static Vector4 Gray(float opacity = 1.0f)
        /// <summary>
        /// 灰色
        /// </summary>
        /// <param name="opacity">不透明度 (0-1)</param>
        public static Vector4 Gray(float opacity = 1.0f)
        {
            return new Vector4(0.5f, 0.5f, 0.5f, opacity);
        }
        #endregion

        #region # 橙色 —— static Vector4 Orange(float opacity = 1.0f)
        /// <summary>
        /// 橙色
        /// </summary>
        /// <param name="opacity">不透明度 (0-1)</param>
        public static Vector4 Orange(float opacity = 1.0f)
        {
            return new Vector4(1.0f, 0.5f, 0.0f, opacity);
        }
        #endregion

        #region # 紫色 —— static Vector4 Purple(float opacity = 1.0f)
        /// <summary>
        /// 紫色
        /// </summary>
        /// <param name="opacity">不透明度 (0-1)</param>
        public static Vector4 Purple(float opacity = 1.0f)
        {
            return new Vector4(0.5f, 0.0f, 0.5f, opacity);
        }
        #endregion

        #region # 粉色 —— static Vector4 Pink(float opacity = 1.0f)
        /// <summary>
        /// 粉色
        /// </summary>
        /// <param name="opacity">不透明度 (0-1)</param>
        public static Vector4 Pink(float opacity = 1.0f)
        {
            return new Vector4(1.0f, 0.75f, 0.8f, opacity);
        }
        #endregion

        #region # 棕色 —— static Vector4 Brown(float opacity = 1.0f)
        /// <summary>
        /// 棕色
        /// </summary>
        /// <param name="opacity">不透明度 (0-1)</param>
        public static Vector4 Brown(float opacity = 1.0f)
        {
            return new Vector4(0.65f, 0.16f, 0.16f, opacity);
        }
        #endregion

        #region # 酸橙色 —— static Vector4 Lime(float opacity = 1.0f)
        /// <summary>
        /// 酸橙色
        /// </summary>
        /// <param name="opacity">不透明度 (0-1)</param>
        public static Vector4 Lime(float opacity = 1.0f)
        {
            return new Vector4(0.75f, 1.0f, 0.0f, opacity);
        }
        #endregion

        #region # 蓝绿色 —— static Vector4 Teal(float opacity = 1.0f)
        /// <summary>
        /// 蓝绿色
        /// </summary>
        /// <param name="opacity">不透明度 (0-1)</param>
        public static Vector4 Teal(float opacity = 1.0f)
        {
            return new Vector4(0.0f, 0.5f, 0.5f, opacity);
        }
        #endregion

        #region # 橄榄色 —— static Vector4 Olive(float opacity = 1.0f)
        /// <summary>
        /// 橄榄色
        /// </summary>
        /// <param name="opacity">不透明度 (0-1)</param>
        public static Vector4 Olive(float opacity = 1.0f)
        {
            return new Vector4(0.5f, 0.5f, 0.0f, opacity);
        }
        #endregion

        #region # 深蓝色 —— static Vector4 Navy(float opacity = 1.0f)
        /// <summary>
        /// 深蓝色
        /// </summary>
        /// <param name="opacity">不透明度 (0-1)</param>
        public static Vector4 Navy(float opacity = 1.0f)
        {
            return new Vector4(0.0f, 0.0f, 0.5f, opacity);
        }
        #endregion

        #region # 银色 —— static Vector4 Silver(float opacity = 1.0f)
        /// <summary>
        /// 银色
        /// </summary>
        /// <param name="opacity">不透明度 (0-1)</param>
        public static Vector4 Silver(float opacity = 1.0f)
        {
            return new Vector4(0.75f, 0.75f, 0.75f, opacity);
        }
        #endregion

        #region # 金色 —— static Vector4 Gold(float opacity = 1.0f)
        /// <summary>
        /// 金色
        /// </summary>
        /// <param name="opacity">不透明度 (0-1)</param>
        public static Vector4 Gold(float opacity = 1.0f)
        {
            return new Vector4(1.0f, 0.84f, 0.0f, opacity);
        }
        #endregion

        #region # 从RGB值创建颜色 —— static Vector4 FromRGB(float r, float g, float b...
        /// <summary>
        /// 从RGB值创建颜色
        /// </summary>
        /// <param name="r">红色分量 (0-1)</param>
        /// <param name="g">绿色分量 (0-1)</param>
        /// <param name="b">蓝色分量 (0-1)</param>
        /// <param name="opacity">不透明度 (0-1)</param>
        public static Vector4 FromRGB(float r, float g, float b, float opacity = 1.0f)
        {
            return new Vector4(r, g, b, opacity);
        }
        #endregion

        #region # 从HSV值创建颜色 —— static Vector4 FromHSV(float h, float s, float v...
        /// <summary>
        /// 从HSV值创建颜色
        /// </summary>
        /// <param name="h">色相 (0-360)</param>
        /// <param name="s">饱和度 (0-1)</param>
        /// <param name="v">明度 (0-1)</param>
        /// <param name="opacity">不透明度 (0-1)</param>
        public static Vector4 FromHSV(float h, float s, float v, float opacity = 1.0f)
        {
            //确保参数在有效范围内
            h = MathHelper.Clamp(h, 0f, 360f);
            s = MathHelper.Clamp(s, 0f, 1f);
            v = MathHelper.Clamp(v, 0f, 1f);

            float c = v * s;
            float x = c * (1 - Math.Abs((h / 60.0f) % 2 - 1));
            float m = v - c;

            Vector3 rgb;

            if (h < 60)
            {
                rgb = new Vector3(c, x, 0);
            }
            else if (h < 120)
            {
                rgb = new Vector3(x, c, 0);
            }
            else if (h < 180)
            {
                rgb = new Vector3(0, c, x);
            }
            else if (h < 240)
            {
                rgb = new Vector3(0, x, c);
            }
            else if (h < 300)
            {
                rgb = new Vector3(x, 0, c);
            }
            else
            {
                rgb = new Vector3(c, 0, x);
            }

            return new Vector4(rgb.X + m, rgb.Y + m, rgb.Z + m, opacity);
        }
        #endregion

        #region # 线性插值两个颜色 —— static Vector4 Lerp(Vector4 color1, Vector4 color2...
        /// <summary>
        /// 线性插值两个颜色
        /// </summary>
        /// <param name="color1">颜色1</param>
        /// <param name="color2">颜色2</param>
        /// <param name="factor">插值因子 (0~1)</param>
        public static Vector4 Lerp(Vector4 color1, Vector4 color2, float factor)
        {
            factor = Math.Clamp(factor, 0.0f, 1.0f);

            return color1 * (1.0f - factor) + color2 * factor;
        }
        #endregion
    }
}
