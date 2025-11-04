using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace YoYo.Toolkit.Controls.Button
{
    /// <summary>
    /// author:TT
    /// time:2024/12/6 16:35:54
    /// desc:IconButton
    /// </summary>
    public class IconRadioButton : RadioButton
    {


        public string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(string), typeof(IconRadioButton), new PropertyMetadata(""));





        public SolidColorBrush IconForeground
        {
            get { return (SolidColorBrush)GetValue(IconForegroundProperty); }
            set { SetValue(IconForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IconForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconForegroundProperty =
            DependencyProperty.Register("IconForeground", typeof(SolidColorBrush), typeof(IconRadioButton), new PropertyMetadata(Brushes.Black));




    }
}
