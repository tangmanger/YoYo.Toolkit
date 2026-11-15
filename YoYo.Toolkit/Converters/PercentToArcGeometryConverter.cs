using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace YoYo.Toolkit.Converters;

/// <summary>
/// 把完成比例转换为环形进度所需的弧线几何。
/// <para>
/// MultiBinding 依次传入：Percent、元素宽度、元素高度；线宽通过 ConverterParameter 传入。
/// 这是本套件唯一新增的转换器——条形进度用 <c>ScaleTransform.ScaleX</c> 直接绑定 Percent，
/// 不需要转换。
/// </para>
/// </summary>
public sealed class PercentToArcGeometryConverter : IMultiValueConverter
{
    private const double DefaultThickness = 4d;

    /// <inheritdoc />
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is null || values.Length < 3)
            return Geometry.Empty;

        var percent = ToDouble(values[0]);
        var width = ToDouble(values[1]);
        var height = ToDouble(values[2]);
        var thickness = ParseThickness(parameter);

        if (width <= 0 || height <= 0)
            return Geometry.Empty;

        var radius = Math.Min(width, height) / 2 - thickness / 2;
        if (radius <= 0)
            return Geometry.Empty;

        var center = new Point(width / 2, height / 2);
        percent = double.IsNaN(percent) ? 0 : Math.Clamp(percent, 0d, 1d);

        if (percent <= 0)
            return Geometry.Empty;

        // 整圈用 ArcSegment 会因起终点重合而退化，直接给椭圆
        if (percent >= 1)
        {
            var full = new EllipseGeometry(center, radius, radius);
            full.Freeze();
            return full;
        }

        const double startAngle = -90d;   // 从 12 点方向起算
        var sweep = percent * 360d;

        var figure = new PathFigure
        {
            StartPoint = Polar(center, radius, startAngle),
            IsClosed = false,
            IsFilled = false
        };

        figure.Segments.Add(new ArcSegment(
            Polar(center, radius, startAngle + sweep),
            new Size(radius, radius),
            0d,
            sweep > 180d,
            SweepDirection.Clockwise,
            true));

        var geometry = new PathGeometry();
        geometry.Figures.Add(figure);
        geometry.Freeze();
        return geometry;
    }

    /// <inheritdoc />
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();

    private static Point Polar(Point center, double radius, double angleDegrees)
    {
        var radians = angleDegrees * Math.PI / 180d;
        return new Point(center.X + radius * Math.Cos(radians), center.Y + radius * Math.Sin(radians));
    }

    private static double ToDouble(object? value)
        => value switch
        {
            double d => d,
            float f => f,
            int i => i,
            _ => 0d
        };

    private static double ParseThickness(object? parameter)
    {
        if (parameter is double d) return d;
        if (parameter is string s && double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            return parsed;
        return DefaultThickness;
    }
}
