using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace YoYo.Toolkit.Converters
{
    /// <summary>
    /// author:TT
    /// time:2021/6/30 0:27:03
    /// desc:NullToVisibilityConverter
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null)
            {
                if (parameter.ToString() == "Inverse")
                {
                    if (value == null) return Visibility.Visible;
                    else return Visibility.Collapsed;
                }
            }
            if (value == null) return Visibility.Collapsed;
            else return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
