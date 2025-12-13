using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace YoYo.Toolkit.Input
{
    public static class PasswordHelper
    {
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.RegisterAttached("Password",
            typeof(string), typeof(PasswordHelper),
            new FrameworkPropertyMetadata(string.Empty, OnPasswordPropertyChanged));

        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached("Attach",
            typeof(bool), typeof(PasswordHelper), new PropertyMetadata(false, Attach));

        private static readonly DependencyProperty IsUpdatingProperty =
           DependencyProperty.RegisterAttached("IsUpdating", typeof(bool),
           typeof(PasswordHelper));



        public static double GetPlaceHolderX(DependencyObject obj)
        {
            return (double)obj.GetValue(PlaceHolderXProperty);
        }

        public static void SetPlaceHolderX(DependencyObject obj, double value)
        {
            obj.SetValue(PlaceHolderXProperty, value);
        }

        // Using a DependencyProperty as the backing store for PlaceHolderX.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlaceHolderXProperty =
            DependencyProperty.RegisterAttached("PlaceHolderX", typeof(double), typeof(PasswordHelper), new PropertyMetadata(16d));




        public static double GetPlaceHolderY(DependencyObject obj)
        {
            return (double)obj.GetValue(PlaceHolderYProperty);
        }

        public static void SetPlaceHolderY(DependencyObject obj, double value)
        {
            obj.SetValue(PlaceHolderYProperty, value);
        }

        // Using a DependencyProperty as the backing store for PlaceHolderY.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlaceHolderYProperty =
            DependencyProperty.RegisterAttached("PlaceHolderY", typeof(double), typeof(PasswordHelper), new PropertyMetadata(15d));



        public static event Action<string, Exception>? ErrorOccurred;
        public static string GetPlaceHolderText(DependencyObject obj)
        {
            return (string)obj.GetValue(PlaceHolderTextProperty);
        }

        public static void SetPlaceHolderText(DependencyObject obj, string value)
        {
            obj.SetValue(PlaceHolderTextProperty, value);
        }

        // Using a DependencyProperty as the backing store for PlaceHolderText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlaceHolderTextProperty =
            DependencyProperty.RegisterAttached("PlaceHolderText", typeof(string), typeof(PasswordHelper));
        public static void SetAttach(DependencyObject dp, bool value)
        {
            dp.SetValue(AttachProperty, value);
        }

        public static bool GetAttach(DependencyObject dp)
        {
            return (bool)dp.GetValue(AttachProperty);
        }

        public static string GetPassword(DependencyObject dp)
        {
            return (string)dp.GetValue(PasswordProperty);
        }

        public static void SetPassword(DependencyObject dp, string value)
        {
            dp.SetValue(PasswordProperty, value);
        }

        private static bool GetIsUpdating(DependencyObject dp)
        {
            return (bool)dp.GetValue(IsUpdatingProperty);
        }

        private static void SetIsUpdating(DependencyObject dp, bool value)
        {
            dp.SetValue(IsUpdatingProperty, value);
        }

        private static void OnPasswordPropertyChanged(DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            PasswordBox? passwordBox = sender as PasswordBox;
            if (passwordBox == null) return;
            passwordBox.PasswordChanged -= PasswordChanged;

            if (!(bool)GetIsUpdating(passwordBox))
            {
                passwordBox.Password = (string)e.NewValue;
            }
            passwordBox.PasswordChanged += PasswordChanged;
        }

        private static void Attach(DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            PasswordBox? passwordBox = sender as PasswordBox;

            if (passwordBox == null)
                return;

            if ((bool)e.OldValue)
            {
                passwordBox.GotFocus -= GotFocus;
                passwordBox.PasswordChanged -= PasswordChanged;
                passwordBox.LostFocus -= PasswordBox_LostFocus;
                passwordBox.Loaded -= PasswordBox_Loaded;
            }

            if ((bool)e.NewValue)
            {
                passwordBox.Loaded += PasswordBox_Loaded;
                passwordBox.GotFocus += GotFocus;
                passwordBox.LostFocus += PasswordBox_LostFocus;
                passwordBox.PasswordChanged += PasswordChanged;
            }
        }
        private static void PasswordBox_Loaded(object sender, RoutedEventArgs e)
        {
            PasswordBox? passwordBox = sender as PasswordBox;
            if (passwordBox == null)
                return;
            if (string.IsNullOrWhiteSpace(passwordBox.Password))
                SetPlaceHolderTextBrush(passwordBox, true);
            else
            {
                SetPlaceHolderTextBrush(passwordBox, false);
            }
        }

        private static void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            PasswordBox? passwordBox = sender as PasswordBox;
            if (passwordBox == null)
                return;
            if (string.IsNullOrWhiteSpace(passwordBox.Password))
                SetPlaceHolderTextBrush(passwordBox, true);
            else
            {
                SetPlaceHolderTextBrush(passwordBox, false);
            }
        }
        private static void GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordBox? passwordBox = sender as PasswordBox;
            if (passwordBox == null)
                return;
            SetPlaceHolderTextBrush(passwordBox, false);
#if DEBUG
            return;
#endif
            try
            {
                StartOsk();
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke("打开虚拟键盘出错!", ex);
                //Log.Write("打开虚拟键盘出错!", ex);
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
                ErrorOccurred?.Invoke("打开osk键盘失败!", new FileNotFoundException("osk.exe not found in system directory."));
                //Log.Write("打开osk键盘失败!");
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
        private static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox? passwordBox = sender as PasswordBox;

            if (passwordBox == null)
                return;
            SetIsUpdating(passwordBox, true);
            SetPassword(passwordBox, passwordBox.Password);
            SetIsUpdating(passwordBox, false);
            if (string.IsNullOrWhiteSpace(passwordBox.Password))
                SetPlaceHolderTextBrush(passwordBox, true);
            else
            {
                SetPlaceHolderTextBrush(passwordBox, false);
            }
        }
        public static void SetPlaceHolderTextBrush(PasswordBox passwordBox, bool isShow)
        {
            var border = passwordBox.Template.FindName("PlaceHolderTextBorder", passwordBox) as Border;
            if (border != null)
            {
                if (isShow)
                {
                    var x = GetPlaceHolderX(passwordBox);
                    var y = GetPlaceHolderY(passwordBox);

                    VisualBrush visualBrush = new VisualBrush();
                    visualBrush.TileMode = TileMode.None;
                    visualBrush.AutoLayoutContent = true;
                    visualBrush.AlignmentX = AlignmentX.Left;
                    visualBrush.AlignmentY = AlignmentY.Top;
                    visualBrush.Stretch = Stretch.None;

                    TranslateTransform translateTransform = new TranslateTransform();
                    translateTransform.X = x;
                    translateTransform.Y = y;
                    visualBrush.Transform = translateTransform;
                    TextBlock textBlock = new TextBlock();
                    textBlock.Margin = new Thickness(5, 5, 8, 0);
                    textBlock.Visibility = Visibility.Visible;
                    textBlock.FontSize = 14;
                    textBlock.Text = GetPlaceHolderText(passwordBox);
                    textBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C6C6C6"));
                    visualBrush.Visual = textBlock;
                    border.Background = visualBrush;
                }
                else
                {
                    border.Background = Brushes.Transparent;
                }
            }
        }
    }
}
