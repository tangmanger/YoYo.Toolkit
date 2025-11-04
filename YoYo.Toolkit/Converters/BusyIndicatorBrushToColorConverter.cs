using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace YoYo.Toolkit.Converters
{
    public class BusyIndicatorBrushToColorConverter : IValueConverter
    {
        /// <summary>
        /// Converts brush to color
        /// </summary>
        /// <param name="value">input value</param>
        /// <param name="targetType"> target type</param>
        /// <param name="parameter">input parameter</param>
        /// <param name="language">input language</param>
        /// <returns>converted value as object</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            SolidColorBrush? solidColorBrush = value as SolidColorBrush;
            if (solidColorBrush == null && parameter != null && parameter.ToString() == "AccentBrushnull")
            {
                return new SolidColorBrush(Colors.Transparent);
            }
            if (solidColorBrush == null && parameter != null && parameter.ToString() == "ContentBrushnull")
            {
                return Colors.Black;
            }
            if (solidColorBrush != null && parameter != null && parameter.ToString() == "ContentBrushnull")
            {
                return solidColorBrush.Color;
            }
            if (solidColorBrush != null && parameter != null && parameter.ToString() == "AccentBrushnull")
            {
                Color color = Color.FromArgb(solidColorBrush.Color.A, solidColorBrush.Color.R, solidColorBrush.Color.G, solidColorBrush.Color.B);
                return new SolidColorBrush(color);
            }
            if (solidColorBrush == null)
            {
                return Colors.Transparent;
            }
            return Color.FromArgb(solidColorBrush.Color.A, solidColorBrush.Color.R, solidColorBrush.Color.G, solidColorBrush.Color.B);
        }

        /// <summary>
        /// Converts back into default type
        /// </summary>
        /// <param name="value">input value</param>
        /// <param name="targetType"> target type</param>
        /// <param name="parameter">input parameter</param>
        /// <param name="language">input language</param>
        /// <returns>converted value as object</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            return null;
        }
    }
}
