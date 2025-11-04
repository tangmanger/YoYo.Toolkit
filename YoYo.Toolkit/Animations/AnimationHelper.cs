using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace YoYo.Toolkit.Animations
{
    public class AnimationHelper
    {




        public static bool GetAttach(DependencyObject obj)
        {
            return (bool)obj.GetValue(AttachProperty);
        }

        public static void SetAttach(DependencyObject obj, bool value)
        {
            obj.SetValue(AttachProperty, value);
        }

        // Using a DependencyProperty as the backing store for Attach.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached("Attach", typeof(bool), typeof(AnimationHelper), new PropertyMetadata(AttachCallBack));

        private static void AttachCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}
