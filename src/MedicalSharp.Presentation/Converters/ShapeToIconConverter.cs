using Avalonia;
using Avalonia.Data.Converters;
using MedicalSharp.Controls.Visual3Ds;
using System;
using System.Globalization;

namespace MedicalSharp.Presentation.Converters
{
    /// <summary>
    /// 形状To图标转换器
    /// </summary>
    public class ShapeToIconConverter : IValueConverter
    {
        /// <summary>
        /// 转换
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Func<string, object> getResource = icon =>
            {
                icon = string.IsNullOrWhiteSpace(icon) ? "Icon-Shape" : icon;
                if (Application.Current!.Resources.TryGetResource(icon, null, out object resource))
                {
                    return resource;
                }

                return null;
            };

            string icon = value switch
            {
                TextVisual3D => "Icon-Text",
                PointVisual3D => "Icon-Point",
                LineSegmentVisual3D => "Icon-LineSegment",
                PolylineVisual3D polyline when !polyline.Closed => "Icon-Polyline",
                CurveVisual3D curve when !curve.Closed => "Icon-Curve",
                RectangleVisual3D => "Icon-Rectangle",
                CircleVisual3D => "Icon-Circle",
                EllipseVisual3D => "Icon-Ellipse",
                PolylineVisual3D polyline when polyline.Closed => "Icon-Polygon",
                CurveVisual3D curve when curve.Closed => "Icon-ClosedCurve",
                BoundingBoxVisual3D => "Icon-Cube",
                BoundingSphereVisual3D => "Icon-Sphere",
                CylinderVisual3D => "Icon-Cylinder",
                ConvexPolyhedronVisual3D => "Icon-Polyhedron",
                _ => null
            };
            object resource = getResource(icon);

            return resource;
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
