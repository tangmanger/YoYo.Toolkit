using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.IO;

namespace YoYo.Toolkit.Input
{
    /// <summary>
    /// author:TT
    /// time:2021/7/23 21:16:50
    /// desc:OSKHelper
    /// </summary>
    public class OSKHelper
    {
        public static event Action<string, Exception>? ErrorOccurred;



        public static bool GetUseTabKeyBorder(DependencyObject obj)
        {
            return (bool)obj.GetValue(UseTabKeyBorderProperty);
        }

        public static void SetUseTabKeyBorder(DependencyObject obj, bool value)
        {
            obj.SetValue(UseTabKeyBorderProperty, value);
        }

        // Using a DependencyProperty as the backing store for UseTabKeyBorder.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UseTabKeyBorderProperty =
            DependencyProperty.RegisterAttached("UseTabKeyBorder", typeof(bool), typeof(OSKHelper), new PropertyMetadata(false));


        public static readonly DependencyProperty AttachProperty =
          DependencyProperty.RegisterAttached("Attach",
          typeof(bool), typeof(OSKHelper), new PropertyMetadata(false, Attach));
        public static void SetAttach(DependencyObject dp, bool value)
        {
            dp.SetValue(AttachProperty, value);
        }

        public static bool GetAttach(DependencyObject dp)
        {
            return (bool)dp.GetValue(AttachProperty);
        }
        private static void Attach(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBox? textBox = d as TextBox;
#if DEBUG
            return;
#endif
            if (textBox == null)
                return;
            if ((bool)e.OldValue)
            {
                textBox.GotFocus -= GotFocus;
            }

            if ((bool)e.NewValue)
            {
                textBox.GotFocus += GotFocus;
            }
        }

        private static void GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBox? textBox = sender as TextBox;
                if (textBox == null) return;
                var result = GetUseTabKeyBorder(textBox);
                if (result)
                    StartTabOsk();
                else
                    StartOsk();
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke("打开虚拟键盘出错!", ex);
            }
        }
        public static void StartOsk()
        {
            var osk = Process.GetProcessesByName("osk");
            if (osk != null && osk.Length > 0)
            {
                osk.FirstOrDefault()?.Kill();
            }
            string oskPath = Path.Combine(Environment.SystemDirectory, "osk.exe");
            if (File.Exists(oskPath))
            {
                Process.Start(oskPath);
            }
            else
            {
                ErrorOccurred?.Invoke("打开osk键盘失败!", new Exception("打开osk键盘失败!"));
            }
            return;
        }
        public static void StartTabOsk()
        {
            var osk = Process.GetProcessesByName("tabtip");
            if (osk != null && osk.Length > 0)
            {
                osk.FirstOrDefault()?.Kill();
            }
            string oskPath = Path.Combine(Environment.SystemDirectory, "tabtip.exe");
            if (File.Exists(oskPath))
            {
                Process.Start(oskPath);
            }
            else
            {
                ErrorOccurred?.Invoke("打开tabtiposk键盘失败!", new FileNotFoundException("tabtip.exe not found in system directory."));
                //Log.Write("打开osk键盘失败!");
            }
            return;
        }
    }
}
