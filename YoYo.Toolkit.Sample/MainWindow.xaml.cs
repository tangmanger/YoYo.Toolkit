using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shell;
using YoYo.ToolkitSample.Services;
using YoYo.ToolkitSample.ViewModels;
using YoYo.ToolkitSample.Views;

namespace YoYo.ToolkitSample;

/// <summary>
/// 外壳窗口：只做视图层的事——窗口命令、DWM 同步、开窗、聚焦。
/// DataContext 由 App.xaml 的 locator 提供，导航与状态都在 <see cref="ShellViewModel"/> 里。
/// </summary>
public partial class MainWindow : Window
{
    private ShellViewModel Vm => (ShellViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();

        // 视图层动作注入：VM 不创建任何 Window
        Vm.OpenUsageViewer = pageKey => ShowXamlViewer(pageKey);
        Vm.OpenDemoWindow = CreateDemoWindow;
        Vm.PropertyChanged += OnViewModelPropertyChanged;

        WindowFrame.Attach(this, WindowChrome.GetWindowChrome(this), () => Vm.IsDark);

        StateChanged += OnStateChanged;
        Loaded += OnLoaded;

        Vm.Start();
    }

    /// <summary>系统侧深色模式属于视图层，跟着 VM 的主题状态同步。</summary>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ShellViewModel.IsDark))
            WindowFrame.SetDarkMode(this, Vm.IsDark);
    }

    /// <summary>与 PackageEasy 一致：四个窗口命令在 Loaded 里注册 CommandBinding。</summary>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, (_, _) => Close()));
        CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand,
            (_, _) => { WindowState = WindowState.Minimized; }));
        CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand,
            (_, _) => { WindowState = WindowState.Maximized; }));
        CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand,
            (_, _) => { WindowState = WindowState.Normal; }));

        OnStateChanged(this, EventArgs.Empty);
    }

    /// <summary>最大化时只切换按钮，不动 WindowChrome 的任何厚度——与 PackageEasy 相同。</summary>
    private void OnStateChanged(object? sender, EventArgs e)
    {
        var maximized = WindowState == WindowState.Maximized;
        MaxButton.Visibility = maximized ? Visibility.Collapsed : Visibility.Visible;
        RestoreButton.Visibility = maximized ? Visibility.Visible : Visibility.Collapsed;
    }

    // ---------- 供 App 的命令行验收（--shot / --diag / --value / --step / --stroke）使用 ----------

    public double ProgressValue
    {
        get => Vm.Progress.ProgressValue;
        set => Vm.Progress.ProgressValue = value;
    }

    public int CurrentStep
    {
        get => Vm.Progress.CurrentStep;
        set => Vm.Progress.CurrentStep = value;
    }

    /// <summary>当前页的 ViewType，即 XAML 查看器要展示的文件名。</summary>
    public string CurrentPageKey => Vm.PageKey;

    public void SetAllPaused(bool paused) => Vm.Progress.SetAllPaused(paused);

    public void SetDarkTheme(bool dark) => Vm.IsDark = dark;

    public void SetStroke(string kind) => Vm.SetStroke(kind);

    /// <summary>截图脚本用：先切到指定页面再抓图。</summary>
    public void NavigateTo(string viewType) => Vm.GoTo(viewType);

    /// <summary>截图脚本用：带参进入（如 DeviceView + SN-0001）。</summary>
    public void NavigateToWithParam(string viewType, string param) => Vm.GoToWithParam(viewType, param);

    // ---------- XAML 查看器 ----------

    private void ShowXamlViewer(string pageKey)
    {
        var viewer = new XamlViewerWindow(pageKey) { Owner = this };
        viewer.ShowDialog();
    }

    /// <summary>
    /// WindowService 演示用的真实子窗口。由视图层创建，VM 只负责登记 id 与调用 CloseWindow。
    /// </summary>
    private Window CreateDemoWindow(string windowId, string title)
    {
        var caption = new TextBlock
        {
            Text = title,
            FontSize = 15,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 9)
        };
        caption.SetResourceReference(TextBlock.ForegroundProperty, "Sample.Ink");

        var hint = new TextBlock
        {
            Text = $"windowId = \"{windowId}\"\n\n已登记到 IWindowService。\n在页面点「关闭」会调用 CloseWindow(\"{windowId}\")；\n直接关掉本窗口，状态也会由 Closed 事件回写。",
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 19
        };
        hint.SetResourceReference(TextBlock.ForegroundProperty, "Sample.Ink2");

        var panel = new StackPanel { Margin = new Thickness(18) };
        panel.Children.Add(caption);
        panel.Children.Add(hint);

        var root = new Border { Child = panel };
        root.SetResourceReference(Border.BackgroundProperty, "Sample.Surface");

        return new Window
        {
            Title = title,
            Width = 340,
            Height = 210,
            Owner = this,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = root
        };
    }

    /// <summary>打开控件用法目录（模态，默认定位到当前页演示的控件）。</summary>
    public void ShowXamlViewer() => ShowXamlViewer(CurrentPageKey);

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        // Ctrl+K 聚焦搜索框是纯视图动作；Ctrl+T / F12 已走 Window.InputBindings
        if (e.Key == Key.K && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            GlobalSearch.Focus();
            GlobalSearch.SelectAll();
            e.Handled = true;
        }
    }
}
