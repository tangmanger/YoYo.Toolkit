using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shell;
using System.Windows.Threading;
using YoYo.ToolkitSample.Services;

namespace YoYo.ToolkitSample.Views;

/// <summary>
/// 控件用法查看器：左边是控件目录，右边是「这个控件怎么用」的最小 XAML 片段。
/// 不做整页源码浏览——需要的是能直接粘进宿主工程的那几行。
/// </summary>
public partial class XamlViewerWindow : Window
{
    private ControlSample? _current;
    private DispatcherTimer? _copyTimer;

    public XamlViewerWindow()
        : this(null)
    {
    }

    /// <param name="pageKey">当前页的 ViewType，用于把目录定位到该页演示的控件。</param>
    public XamlViewerWindow(string? pageKey)
    {
        InitializeComponent();

        WindowFrame.Attach(this, WindowChrome.GetWindowChrome(this), () => false);

        SampleList.ItemsSource = ControlSamples.Catalog;

        SampleList.SelectedItem = ControlSamples.DefaultFor(pageKey);

        PreviewKeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape) Close();
        };
    }

    private void OnSampleChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SampleList.SelectedItem is not ControlSample sample) return;

        _current = sample;

        NameText.Text = sample.Name;
        BadgeText.Text = sample.Module;
        SummaryText.Text = sample.Summary;
        HintText.Text = $"{sample.Name} · {sample.Variants.Count} 种用法";

        CodeHost.Document = XamlHighlighter.Build(sample);
        CodeHost.ScrollToHome();
    }

    private void OnCopy(object sender, RoutedEventArgs e)
    {
        if (_current is null) return;

        try
        {
            Clipboard.SetText(XamlHighlighter.Compose(_current));
        }
        catch (Exception)
        {
            // 剪贴板被其它进程占用时静默失败，避免弹框打断阅读
            return;
        }

        CopyLabel.Text = "已复制";

        _copyTimer?.Stop();
        _copyTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1200) };
        _copyTimer.Tick += (_, _) =>
        {
            _copyTimer?.Stop();
            CopyLabel.Text = "复制";
        };
        _copyTimer.Start();
    }

    private void OnClose(object sender, RoutedEventArgs e) => Close();
}
