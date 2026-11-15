using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using YoYo.Toolkit.Interfaces;
using YoYo.ToolkitSample.Navigations;
using YoYo.ToolkitSample.Models;
using YoYo.ToolkitSample.Services;
using YoYo.ToolkitSample.ViewModels;
using YoYo.ToolkitSample.Views;

namespace YoYo.ToolkitSample.ViewModels;

/// <summary>
/// 外壳：侧栏、页头、状态栏、主题，以及「谁负责把视图挂上去」。
/// 它自己不做导航决策——真正的跳转交给 <see cref="MainNavigation"/>，
/// 外壳只订阅结果（ViewChanged / Logged），这也是宿主接入这套导航需要写的全部代码。
/// </summary>
public sealed class ShellViewModel : INotifyPropertyChanged, INavigationHost
{
    private static readonly Uri DarkTokensUri = new("Themes/Tokens.Dark.xaml", UriKind.Relative);
    private static readonly Uri DarkProgressTokensUri =
        new("/YoYo.Toolkit;component/Themes/Progress.Tokens.Dark.xaml", UriKind.Relative);

    private readonly ShellService _service;
    private readonly MainNavigation _navigation;

    private string _pageTitle = "概览";
    private string _pageLede = string.Empty;
    private string _pageKey = "OverviewView";
    private string _lastLog = "就绪";
    private string _lastLogTime = "--:--:--";
    private string _filterText = string.Empty;
    private bool _isDark;
    private bool _isRail;
    private object? _content;

    public ShellViewModel(IWindowService? windowService = null)
    {
        WindowService = windowService ?? new YoYo.Toolkit.Services.WindowService();
        _service = new ShellService();
        _navigation = new MainNavigation(_service);
        Progress = new ProgressViewModel(this);

        _service.ViewChanged += OnViewChanged;
        _navigation.Logged += (message, isError) => Log(message, isError);

        RegisterPages();

        Groups = BuildSidebar();
        Items = Groups.SelectMany(g => g.Items).ToList();

        ToggleThemeCommand = new RelayCommand(() => IsDark = !IsDark);
        ToggleRailCommand = new RelayCommand(() => IsRail = !IsRail);
        NavigateCommand = new RelayCommand<string>(viewType =>
        {
            if (!string.IsNullOrEmpty(viewType)) GoTo(viewType);
        });
        ShowUsageCommand = new RelayCommand(() => OpenUsageViewer?.Invoke(PageKey));

        Log("MainNavigation 已注册 " + _navigation.Navigates.Count + " 个 NavigateModel", true);
    }

    /// <summary>暴露给 App 的命令行验收：截图/诊断需要直接操作进度页。</summary>
    public MainNavigation Navigation => _navigation;

    public IReadOnlyList<NavGroup> Groups { get; }

    public IReadOnlyList<SidebarItem> Items { get; }

    /// <summary>进度页单例：命令行截图与「暂停全部动画」需要直接操作它。</summary>
    public ProgressViewModel Progress { get; }

    public ObservableCollection<LogEntry> Logs { get; } = new();

    public string PageTitle
    {
        get => _pageTitle;
        private set => SetField(ref _pageTitle, value);
    }

    public string PageLede
    {
        get => _pageLede;
        private set => SetField(ref _pageLede, value);
    }

    public string PageKey
    {
        get => _pageKey;
        private set => SetField(ref _pageKey, value);
    }

    public string LastLog
    {
        get => _lastLog;
        private set => SetField(ref _lastLog, value);
    }

    public string LastLogTime
    {
        get => _lastLogTime;
        private set => SetField(ref _lastLogTime, value);
    }

    public string NavigateCount => _navigation.Navigates.Count.ToString();

    public object? Content
    {
        get => _content;
        private set => SetField(ref _content, value);
    }

    public bool IsDark
    {
        get => _isDark;
        set
        {
            if (!SetField(ref _isDark, value)) return;
            OnPropertyChanged(nameof(IsLight));
            OnPropertyChanged(nameof(ThemeLabel));
            ApplyTheme(value);
        }
    }

    public bool IsLight
    {
        get => !_isDark;
        set { if (value) IsDark = false; }
    }

    public string ThemeLabel => _isDark ? "深色令牌" : "浅色令牌";

    public bool IsRail
    {
        get => _isRail;
        set
        {
            if (!SetField(ref _isRail, value)) return;
            OnPropertyChanged(nameof(RailLabel));
        }
    }

    public string RailLabel => _isRail ? "展开导航" : "折叠导航";

    public string FilterText
    {
        get => _filterText;
        set
        {
            if (!SetField(ref _filterText, value)) return;
            ApplyFilter();
        }
    }

    /// <summary>视图层动作由宿主注入，VM 不创建任何 Window。</summary>
    public Action<string>? OpenUsageViewer { get; set; }

    /// <summary>演示 IWindowService 用的单例：登记与关闭必须是同一本字典。</summary>
    public IWindowService WindowService { get; }

    /// <summary>
    /// 视图层注入：按 (windowId, 标题) 造一个真实子窗口。
    /// 导航页拿到它后调用 RegisterWindow，再自己 Show()。
    /// </summary>
    public Func<string, string, Window?>? OpenDemoWindow { get; set; }

    public ICommand ToggleThemeCommand { get; }
    public ICommand ToggleRailCommand { get; }
    public ICommand NavigateCommand { get; }
    public ICommand ShowUsageCommand { get; }

    /// <summary>命令行验收注入：--stroke Outside/Center/Inside。</summary>
    public void SetStroke(string kind)
    {
        if (Content is FrameworkElement { DataContext: ControlsViewModel vm }
            && Enum.TryParse<StrokeKind>(kind, true, out var parsed))
            vm.SetStroke(parsed);
    }

    /// <summary>侧栏点击、概览页卡片、导航页表格都走这里。</summary>
    public void GoTo(string viewType) => _navigation.GoTo(viewType);

    /// <summary>强类型参数导航，命中 ViewModel 的 <c>INavigateIn&lt;T&gt;</c>。</summary>
    public void GoToWithParam(string viewType, string param) => _navigation.GoTo(viewType, param);

    public void Log(string message, bool isError = false)
    {
        var time = DateTime.Now.ToString("HH:mm:ss");
        Logs.Add(new LogEntry(time, message, isError));
        while (Logs.Count > 80) Logs.RemoveAt(0);

        LastLogTime = time;
        LastLog = message;
    }

    /// <summary>启动后默认落在概览页。</summary>
    public void Start() => GoTo("OverviewView");

    private void OnViewChanged(FrameworkElement? view)
    {
        Content = view;

        if (view?.DataContext is PageViewModelBase page)
        {
            PageKey = page.ViewType;
            PageTitle = page.Title;
            PageLede = page.Lede;
        }

        foreach (var item in Items)
            item.IsActive = string.Equals(item.ViewType, PageKey, StringComparison.Ordinal);
    }

    private void RegisterPages()
    {
        _service.RegisterPage<OverviewView, OverviewViewModel>(() => new OverviewViewModel(this));
        _service.RegisterPage<NavigationView, NavigationViewModel>(() => new NavigationViewModel(this));
        _service.RegisterPage<ControlsView, ControlsViewModel>(() => new ControlsViewModel(this));
        _service.RegisterPage<ProgressView, ProgressViewModel>(() => Progress);
        _service.RegisterPage<NotifiesView, NotifiesViewModel>(() => new NotifiesViewModel(this));
        _service.RegisterPage<NetView, NetViewModel>(() => new NetViewModel(this));
        _service.RegisterPage<SystemView, SystemViewModel>(() => new SystemViewModel(this));
        _service.RegisterPage<CryptoView, CryptoViewModel>(() => new CryptoViewModel(this));
        _service.RegisterPage<DeviceView, DeviceViewModel>(() => new DeviceViewModel(this));
    }

    private static IReadOnlyList<NavGroup> BuildSidebar() => new List<NavGroup>
    {
        new("开始", new List<SidebarItem>
        {
            new("开始", "概览", "Icon.Home", "OverviewView")
        }),
        new("架构", new List<SidebarItem>
        {
            new("架构", "导航与窗口", "Icon.Route", "NavigationView")
        }),
        new("组件", new List<SidebarItem>
        {
            new("组件", "控件", "Icon.Sliders", "ControlsView"),
            new("组件", "进度", "Icon.Loader", "ProgressView"),
            new("组件", "通知", "Icon.Message", "NotifiesView")
        }),
        new("基础设施", new List<SidebarItem>
        {
            new("基础设施", "网络", "Icon.Net", "NetView"),
            new("基础设施", "系统与工具", "Icon.Cpu", "SystemView"),
            new("基础设施", "加密", "Icon.Lock", "CryptoView")
        })
    };

    private void ApplyFilter()
    {
        var query = _filterText.Trim();
        foreach (var group in Groups)
        {
            var any = false;
            foreach (var item in group.Items)
            {
                var hit = query.Length == 0
                          || item.Label.Contains(query, StringComparison.OrdinalIgnoreCase)
                          || item.ViewType.Contains(query, StringComparison.OrdinalIgnoreCase);
                item.IsVisible = hit;
                any |= hit;
            }

            group.IsVisible = any;
        }
    }

    private static void ApplyTheme(bool dark)
    {
        var merged = Application.Current?.Resources.MergedDictionaries;
        if (merged is null) return;

        SetDictionary(merged, DarkTokensUri, dark);
        SetDictionary(merged, DarkProgressTokensUri, dark);
    }

    private static void SetDictionary(IList<ResourceDictionary> merged, Uri uri, bool present)
    {
        var existing = merged.FirstOrDefault(d =>
            d.Source is not null && d.Source.OriginalString.EndsWith(uri.OriginalString, StringComparison.OrdinalIgnoreCase));

        if (present && existing is null)
            merged.Add(new ResourceDictionary { Source = uri });
        else if (!present && existing is not null)
            merged.Remove(existing);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

/// <summary>一条日志。</summary>
public sealed class LogEntry
{
    public LogEntry(string time, string message, bool isError)
    {
        Time = time;
        Message = message;
        IsError = isError;
    }

    public string Time { get; }
    public string Message { get; }
    public bool IsError { get; }

    public Brush Brush => IsError
        ? (Brush)Application.Current.FindResource("Sample.Err")
        : (Brush)Application.Current.FindResource("Sample.Ink2");
}
