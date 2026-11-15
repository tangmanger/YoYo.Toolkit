using System;
using System.Globalization;
using System.Windows.Data;

namespace YoYo.Toolkit.Converters;

/// <summary>
/// 把完成比例（0~1）格式化为百分比文案，供确定态模板中央的数值使用。
/// <para>
/// ConverterParameter 可给小数位（默认 0），例如 <c>ConverterParameter="1"</c> 得到 <c>68.5%</c>。
/// </para>
/// </summary>
public sealed class PercentToTextConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var percent = value switch
        {
            double d => d,
            float f => f,
            decimal m => (double)m,
            int i => i,
            _ => double.NaN
        };

        if (double.IsNaN(percent))
            return string.Empty;

        var digits = 0;
        if (parameter is string s && int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
            digits = Math.Clamp(parsed, 0, 2);
        else if (parameter is int p)
            digits = Math.Clamp(p, 0, 2);

        var clamped = Math.Clamp(percent, 0d, 1d);
        return (clamped * 100d).ToString("F" + digits.ToString(CultureInfo.InvariantCulture), culture) + "%";
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
