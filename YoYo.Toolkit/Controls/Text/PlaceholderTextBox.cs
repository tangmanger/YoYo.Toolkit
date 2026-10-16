using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace YoYo.Toolkit.Controls.Text
{
    [TemplatePart(Name = "PART_BottomBorder", Type = typeof(Border))]
    public class PlaceholderTextBox : TextBox
    {

        Border? border = null;
        public PlaceholderTextBox()
        {
            this.IsKeyboardFocusedChanged += PlaceholderTextBox_IsKeyboardFocusedChanged;
            this.TextChanged += PlaceholderTextBox_TextChanged;
            // 折叠→恢复可见时刷新水印（编辑区收起后重开、清空重录场景，避免复用旧 brush 渲染偏移）
            this.IsVisibleChanged += PlaceholderTextBox_IsVisibleChanged;
            this.Loaded += PlaceholderTextBox_Loaded;
        }


        public double TransformX
        {
            get { return (double)GetValue(TransformXProperty); }
            set { SetValue(TransformXProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TransformX.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TransformXProperty =
            DependencyProperty.Register("TransformX", typeof(double), typeof(PlaceholderTextBox), new PropertyMetadata(16d));



        public double TransformY
        {
            get { return (double)GetValue(TransformYProperty); }
            set { SetValue(TransformYProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TransformY.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TransformYProperty =
            DependencyProperty.Register("TransformY", typeof(double), typeof(PlaceholderTextBox), new PropertyMetadata(15d));



        private void PlaceholderTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshPlaceholder();
        }

        private void PlaceholderTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            RefreshPlaceholder();
        }

        private void PlaceholderTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshPlaceholder();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            border = GetTemplateChild("PART_DropButton") as Border;
            RefreshPlaceholder();
        }
        private void PlaceholderTextBox_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (border == null) return;
            if ((bool)e.NewValue == false && string.IsNullOrWhiteSpace(Text))
            {
                RefreshPlaceholder();
            }
            else
            {
                border.Background = Brushes.Transparent;
            }
        }

        /// <summary>
        /// 按当前参数（PlaceHolderText/TransformX/TransformY）重建占位水印；
        /// 每次显示均新建 VisualBrush/TextBlock/TranslateTransform（对照 PasswordHelper 每次显示重建的做法），
        /// 避免复用同一 brush 实例在「折叠→恢复可见」后渲染位置偏移
        /// </summary>
        private void RefreshPlaceholder()
        {
            if (border == null) return;
            if (!string.IsNullOrWhiteSpace(Text) || string.IsNullOrEmpty(PlaceHolderText))
            {
                border.Background = Brushes.Transparent;
                return;
            }

            TextBlock textBlock = new TextBlock();
            textBlock.Margin = new Thickness(8, 5, 8, 0);
            textBlock.Visibility = Visibility.Visible;
            textBlock.FontSize = 14;
            textBlock.Text = PlaceHolderText;
            textBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C6C6C6"));

            VisualBrush brush = new VisualBrush();
            brush.TileMode = TileMode.None;
            brush.AutoLayoutContent = true;
            brush.AlignmentX = AlignmentX.Left;
            brush.AlignmentY = AlignmentY.Top;
            brush.Stretch = Stretch.None;
            brush.Visual = textBlock;
            brush.Transform = new TranslateTransform(TransformX, TransformY);
            border.Background = brush;
        }


        public string PlaceHolderText
        {
            get
            {
                var result = (string)GetValue(PlaceHolderTextProperty);
                return result;
            }
            set
            {
                SetValue(PlaceHolderTextProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for PlaceHolderText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlaceHolderTextProperty =
            DependencyProperty.Register("PlaceHolderText", typeof(string), typeof(PlaceholderTextBox), new PropertyMetadata("", new PropertyChangedCallback(PlaceHolderTextChangedCallBack)));

        private static void PlaceHolderTextChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PlaceholderTextBox)?.RefreshPlaceholder();
        }


        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CornerRadius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(PlaceholderTextBox), new PropertyMetadata(new CornerRadius(4)));


        public bool IsShowError
        {
            get { return (bool)GetValue(IsShowErrorProperty); }
            set { SetValue(IsShowErrorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsShowError.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsShowErrorProperty =
            DependencyProperty.Register("IsShowError", typeof(bool), typeof(PlaceholderTextBox), new PropertyMetadata(false));



    }
}
