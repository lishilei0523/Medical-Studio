using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace MedicalSharp.Presentation.Converters
{
    /// <summary>
    /// 字符串转几何转换器
    /// </summary>
    public class StringToGeometryConverter : IValueConverter
    {
        /// <summary>
        /// 转换
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string icon)
            {
                return null;
            }

            //从当前控件往上逐级查找
            if (parameter is Control control)
            {
                if (control.TryFindResource(icon, out object resource))
                {
                    return resource;
                }
            }

            //回退到Application资源
            if (Application.Current!.Resources.TryGetResource(icon, null, out object appResource))
            {
                return appResource;
            }

            return null;
        }

        /// <summary>
        /// 转换回
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
