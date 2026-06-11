using Avalonia.Media;
using MedicalSharp.Primitives.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MedicalSharp.Controls.UserControls
{
    /// <summary>
    /// 颜色控制点
    /// </summary>
    public class ColorControlPoint : INotifyPropertyChanged
    {
        #region # 字段及构造器

        /// <summary>
        /// 属性改变事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 默认构造器
        /// </summary>
        public ColorControlPoint()
        {

        }

        #endregion

        #region # 属性

        #region HU值 —— short HU
        /// <summary>
        /// HU值
        /// </summary>
        public short HU
        {
            get;
            set
            {
                field = value;
                this.OnPropertyChanged(nameof(this.HU));
            }
        }
        #endregion

        #region 颜色 —— Color Color
        /// <summary>
        /// 颜色
        /// </summary>
        public Color Color
        {
            get;
            set
            {
                field = value;
                this.OnPropertyChanged(nameof(this.Color));
            }
        }
        #endregion

        #endregion

        #region # 方法

        #region 插值颜色 —— Color InterpolateColor(IReadOnlyList<ColorControlPoint> points, short hu)
        /// <summary>
        /// 插值颜色
        /// </summary>
        public static Color InterpolateColor(IReadOnlyList<ColorControlPoint> controlPoints, short hu, InterpolationMode interpolationMode)
        {
            #region # 验证

            if (!controlPoints.Any())
            {
                return Colors.White;
            }
            if (hu <= controlPoints[0].HU)
            {
                return controlPoints[0].Color;
            }
            if (hu >= controlPoints[^1].HU)
            {
                return controlPoints[^1].Color;
            }

            #endregion

            for (int index = 0; index < controlPoints.Count - 1; index++)
            {
                if (hu >= controlPoints[index].HU && hu <= controlPoints[index + 1].HU)
                {
                    double range = controlPoints[index + 1].HU - controlPoints[index].HU;
                    double t = range > 0 ? (hu - controlPoints[index].HU) / range : 0.0;

                    //根据插值模式调整t值
                    t = interpolationMode switch
                    {
                        InterpolationMode.Linear => t,
                        InterpolationMode.Step => t < 0.5 ? 0.0 : 1.0,
                        InterpolationMode.SmoothStep => t * t * (3.0 - 2.0 * t),
                        _ => t
                    };

                    byte r = (byte)(controlPoints[index].Color.R + (controlPoints[index + 1].Color.R - controlPoints[index].Color.R) * t);
                    byte g = (byte)(controlPoints[index].Color.G + (controlPoints[index + 1].Color.G - controlPoints[index].Color.G) * t);
                    byte b = (byte)(controlPoints[index].Color.B + (controlPoints[index + 1].Color.B - controlPoints[index].Color.B) * t);
                    Color color = Color.FromRgb(r, g, b);

                    return color;
                }
            }

            return Colors.White;
        }
        #endregion 

        #region 属性改变事件 —— void OnPropertyChanged(string propertyName = null)
        /// <summary>
        /// 属性改变事件
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #endregion
    }
}
