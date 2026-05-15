using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Primitives.Builders
{
    /// <summary>
    /// 颜色工厂
    /// </summary>
    public static class ColorFactory
    {
        #region # 字段及构造器

        /// <summary>
        /// 默认标记颜色列表
        /// </summary>
        private static readonly Vector4[] _DefaultMarkColors;

        /// <summary>
        /// 标准标记颜色列表
        /// </summary>
        private static readonly Vector4[] _StandardMarkColors;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static ColorFactory()
        {
            _DefaultMarkColors = GetDefaultMarkColors();
            _StandardMarkColors = GetStandardMarkColors();
        }

        #endregion

        #region # 默认标记颜色列表 —— static Vector4[] DefaultMarkColors
        /// <summary>
        /// 默认标记颜色列表
        /// </summary>
        public static Vector4[] DefaultMarkColors
        {
            get => _DefaultMarkColors;
        }
        #endregion

        #region # 标准标记颜色列表 —— static Vector4[] StandardMarkColors
        /// <summary>
        /// 标准标记颜色列表
        /// </summary>
        public static Vector4[] StandardMarkColors
        {
            get => _StandardMarkColors;
        }
        #endregion

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

        #region # 取相反颜色 —— static Vector4 Invert(this Vector4 color)
        /// <summary>
        /// 取相反颜色
        /// </summary>
        /// <param name="color">原颜色</param>
        /// <returns>相反颜色</returns>
        public static Vector4 Invert(this Vector4 color)
        {
            Vector4 invertedColor = new Vector4(1.0f - color.X, 1.0f - color.Y, 1.0f - color.Z, color.W);
            float contrast = Math.Abs(invertedColor.X - color.X) +
                             Math.Abs(invertedColor.Y - color.Y) +
                             Math.Abs(invertedColor.Z - color.Z);
            if (contrast < 0.5f)
            {
                invertedColor = ColorFactory.Yellow(); //固定用亮黄色
            }

            return invertedColor;
        }
        #endregion

        #region # 获取默认标记颜色列表 —— static Vector4[] GetDefaultMarkColors(float opacity)
        /// <summary>
        /// 获取默认标记颜色列表
        /// </summary>
        /// <param name="opacity">不透明度(0~1)</param>
        /// <returns>颜色列表(固定长度256)</returns>
        public static Vector4[] GetDefaultMarkColors(float opacity = 0.6f)
        {
            Vector4[] colors = new Vector4[256];

            //索引 0：透明
            colors[0] = new Vector4(0, 0, 0, 0);

            //1-255：使用HSV色环，保证相邻标记值颜色有明显区别
            for (int index = 1; index < 256; index++)
            {
                //使用黄金角（137.5度）分布，保证颜色均匀且区分度高
                float hue = (index * 137.5f) % 360.0f;
                float saturation = 0.8f;
                float value = 0.9f;

                colors[index] = FromHSV(hue, saturation, value, opacity);
            }

            return colors;
        }
        #endregion

        #region # 获取标准标记颜色列表 —— static Vector4[] GetStandardMarkColors(float opacity)
        /// <summary>
        /// 获取标准标记颜色列表
        /// </summary>
        /// <param name="opacity">不透明度(0~1)</param>
        /// <returns>颜色列表(固定长度256)</returns>
        /// <remarks>前几个为常用颜色，后面自动生成</remarks>
        public static Vector4[] GetStandardMarkColors(float opacity = 0.6f)
        {
            Vector4[] colors = new Vector4[256];

            //索引0：透明
            colors[0] = new Vector4(0, 0, 0, 0);

            //预定义前20个常用颜色
            Vector4[] predefined =
            [
                Red(opacity),           //1:  红色
                Green(opacity),         //2:  绿色
                Blue(opacity),          //3:  蓝色
                Yellow(opacity),        //4:  黄色
                Cyan(opacity),          //5:  青色
                Magenta(opacity),       //6:  品红
                Orange(opacity),        //7:  橙色
                Purple(opacity),        //8:  紫色
                Lime(opacity),          //9:  酸橙
                Teal(opacity),          //10: 蓝绿
                Pink(opacity),          //11: 粉色
                Brown(opacity),         //12: 棕色
                Navy(opacity),          //13: 深蓝
                Olive(opacity),         //14: 橄榄
                Gold(opacity),          //15: 金色
                new Vector4(0.8f, 0.2f, 0.6f, opacity), //16: 紫罗兰
                new Vector4(0.2f, 0.8f, 0.3f, opacity), //17: 草绿
                new Vector4(0.3f, 0.5f, 0.9f, opacity), //18: 天蓝
                new Vector4(0.9f, 0.5f, 0.2f, opacity), //19: 橘红
                new Vector4(0.5f, 0.2f, 0.8f, opacity)  //20: 紫罗兰
            ];

            //填充预定义颜色
            for (int index = 1; index <= predefined.Length && index < 256; index++)
            {
                colors[index] = predefined[index - 1];
            }

            //剩余颜色用HSV色环填充
            for (int index = predefined.Length + 1; index < 256; index++)
            {
                float hue = (index - predefined.Length - 1) * 360.0f / (256 - predefined.Length - 1);
                colors[index] = FromHSV(hue, 0.8f, 0.9f, opacity);
            }

            return colors;
        }
        #endregion
    }
}
