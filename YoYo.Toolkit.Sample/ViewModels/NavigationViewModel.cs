using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using YoYo.ToolkitSample.Models;

namespace YoYo.ToolkitSample.ViewModels;

/// <summary>
/// 导航页：把 BaseNavigation 的一次 GoTo 拆成四个可观察的步骤，逐节点点亮。
/// 三个跳转按钮是真的在调 GoTo —— 切页、参数注入、生命周期回调都会真实发生。
/// </summary>
public sealed class NavigationViewModel : PageViewModelBase
{
    private int _activeStep = -1;
    private bool _isPlaying;
    private string _mountView = "—";
    private string _mountParam = "null";
    private string _mountVm = "—";

    public NavigationViewModel(INavigationHost? host) : base(host)
    {
        Stages = new[]
        {
            new NavStage("1", "NavigateOut", "旧 VM 释放"),
            new NavStage("2", "NavigateIn<T>", "新 VM 收参"),
            new NavStage("3", "GetService", "解析视图实例"),
            new NavStage("4", "SetView", "挂载到宿主")
        };

        Targets = new[]
        {
            new NavTarget("OverviewView", "GoTo(\"OverviewView\")", null, false),
            new NavTarget("DeviceView", "GoTo(\"DeviceView\", \"SN-0001\")", "SN-0001", true),
            new NavTarget("DeviceView", "GoTo(\"DeviceView\", \"SN-0042\")", "SN-0042", false)
        };

        PlayCommand = new RelayCommand<NavTarget>(async target =>
        {
            if (target is not null) await PlayAsync(target);
        });

        // WindowService 是真演示：打开真窗口 → RegisterWindow(id) → CloseWindow(id)，
        // 而不是只改一行状态文字。窗口由视图层创建，这里只调服务。
        OpenWindowCommand = new RelayCommand<RegisteredWindow>(window =>
        {
            if (window is null || !window.CanOpen) return;
            if (Host is not ShellViewModel shell || shell.OpenDemoWindow is null)
            {
                Log("宿主未注入开窗动作，无法演示 RegisterWindow", true);
                return;
            }

            var instance = shell.OpenDemoWindow(window.WindowId!, window.Name);
            if (instance is null) return;

            window.Attach(instance);
            shell.WindowService.RegisterWindow(instance, window.WindowId!);
            instance.Show();
            window.IsOpen = true;
            window.StateText = "已注册 · 运行中";
            Log($"WindowService.RegisterWindow(window, \"{window.WindowId}\") · Show()");
        });

        CloseWindowCommand = new RelayCommand<RegisteredWindow>(window => Close(window));

        // 设备线程里调用：WindowService 内部用 window.Dispatcher.Invoke 回主线程关窗
        CloseFromWorkerCommand = new RelayCommand(async () =>
            await CloseFromWorkerAsync(Windows.FirstOrDefault(w => w.WindowId == "log-viewer")));

        // 导航表点击：直接按 ViewType 跳，不走时序动画
        GoToCommand = new RelayCommand<string>(viewType =>
        {
            if (!string.IsNullOrWhiteSpace(viewType)) Host?.GoTo(viewType!);
        });
    }

    public override string ViewType => "NavigationView";

    public override string Title => "导航与窗口";

    public override string Lede => "一次 GoTo 的完整时序、强类型参数、多窗口注册与跨线程关闭。";

    public IReadOnlyList<NavStage> Stages { get; }

    public IReadOnlyList<NavTarget> Targets { get; }

    public ObservableCollection<RegisteredWindow> Windows { get; } = new()
    {
        new RegisteredWindow("MainWindow", "null", null, true),
        new RegisteredWindow("DevicePanel", "\"device-panel\"", "device-panel", false),
        new RegisteredWindow("LogViewer", "\"log-viewer\"", "log-viewer", false)
    };

    public ICommand PlayCommand { get; }

    public ICommand OpenWindowCommand { get; }

    public ICommand CloseWindowCommand { get; }

    /// <summary>在后台线程里关 log-viewer，验证 WindowService 的 Dispatcher 回主线程。</summary>
    public ICommand CloseFromWorkerCommand { get; }

    public ICommand GoToCommand { get; }

    /// <summary>当前点亮的节点下标，-1 表示未开始。</summary>
    public int ActiveStep
    {
        get => _activeStep;
        private set
        {
            if (!SetField(ref _activeStep, value)) return;
            for (var i = 0; i < Stages.Count; i++)
                Stages[i].IsLit = i <= _activeStep;
        }
    }

    public bool IsPlaying
    {
        get => _isPlaying;
        private set => SetField(ref _isPlaying, value);
    }

    public string MountView
    {
        get => _mountView;
        private set => SetField(ref _mountView, value);
    }

    public string MountParam
    {
        get => _mountParam;
        private set => SetField(ref _mountParam, value);
    }

    public string MountVm
    {
        get => _mountVm;
        private set => SetField(ref _mountVm, value);
    }

    /// <summary>导航表：直接读 MainNavigation.Navigates，不做第二份数据。</summary>
    public IReadOnlyList<NavigateRow> Navigates { get; private set; } = Array.Empty<NavigateRow>();

    /// <summary>由外壳在构造后注入导航表，保证与 NavigateModel 完全同源。</summary>
    public void LoadNavigates(IEnumerable<YoYo.Toolkit.Models.Navigations.NavigateModel> models)
    {
        Navigates = models.Select(m => new NavigateRow(
            m.Name ?? string.Empty,
            m.ViewType ?? string.Empty,
            m.VMType?.Name ?? string.Empty,
            m.ViewType ?? string.Empty)).ToArray();
        OnPropertyChanged(nameof(Navigates));
    }

    private void Close(RegisteredWindow? window)
    {
        if (window is null || !window.CanClose || !window.IsOpen) return;
        if (Host is not ShellViewModel shell)
        {
            Log("未找到 IWindowService", true);
            return;
        }

        shell.WindowService.CloseWindow(window.WindowId!);
        Log($"WindowService.CloseWindow(\"{window.WindowId}\") · Dispatcher 回主线程", true);
    }

    private async Task CloseFromWorkerAsync(RegisteredWindow? window)
    {
        if (window is null || !window.IsOpen)
        {
            Log("log-viewer 尚未打开，先点「打开」", true);
            return;
        }
        if (Host is not ShellViewModel shell) return;

        Log("设备线程：Task.Run → WindowService.CloseWindow(\"log-viewer\")");
        await Task.Run(() => shell.WindowService.CloseWindow(window.WindowId!));
        Log("Dispatcher.Invoke(window.Close) 已在 UI 线程完成", true);
    }

    private async Task PlayAsync(NavTarget target)
    {
        if (IsPlaying) return;
        IsPlaying = true;
        ActiveStep = -1;

        const int stepDelay = 320;

        ActiveStep = 0;
        MountView = "—";
        MountParam = "—";
        MountVm = "—";
        Log($"GoTo(\"{target.ViewType}\"{(target.Param is null ? "" : $", \"{target.Param}\"")})");
        await Task.Delay(stepDelay);

        ActiveStep = 1;
        MountParam = target.Param ?? "null";
        await Task.Delay(stepDelay);

        ActiveStep = 2;
        await Task.Delay(stepDelay);

        ActiveStep = 3;
        await Task.Delay(stepDelay);

        IsPlaying = false;

        // 真的跳过去：参数版走 INavigateIn<T>，无参版走 INavigateIn
        if (target.Param is null)
            Host?.GoTo(target.ViewType);
        else
            (Host as ShellViewModel)?.GoToWithParam(target.ViewType, target.Param);
    }

    protected override void OnNavigatedIn()
    {
        if (Navigates.Count > 0) return;

        if (Host is ShellViewModel shell)
            LoadNavigates(shell.Navigation.Navigates);
    }
}

/// <summary>时序上的一个节点。</summary>
public sealed class NavStage : System.ComponentModel.INotifyPropertyChanged
{
    private bool _isLit;

    public NavStage(string ordinal, string title, string description)
    {
        Ordinal = ordinal;
        Title = title;
        Description = description;
    }

    public string Ordinal { get; }
    public string Title { get; }
    public string Description { get; }

    public bool IsLit
    {
        get => _isLit;
        set
        {
            if (_isLit == value) return;
            _isLit = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(IsLit)));
        }
    }

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
}

/// <summary>一个跳转目标。</summary>
public sealed class NavTarget
{
    public NavTarget(string viewType, string label, string? param, bool isPrimary)
    {
        ViewType = viewType;
        Label = label;
        Param = param;
        IsPrimary = isPrimary;
    }

    public string ViewType { get; }
    public string Label { get; }
    public string? Param { get; }
    public bool IsPrimary { get; }
}

/// <summary>IWindowService 里登记的一行窗口（状态由真实窗口的 Closed 事件回写）。</summary>
public sealed class RegisteredWindow : System.ComponentModel.INotifyPropertyChanged
{
    private bool _isOpen;
    private string _stateText;

    public RegisteredWindow(string name, string id, string? windowId, bool isOpen)
    {
        Name = name;
        Id = id;
        WindowId = windowId;
        _isOpen = isOpen;
        _stateText = isOpen ? "已注册" : "未打开";
    }

    public string Name { get; }

    /// <summary>界面上显示的 id 文本（带引号或 null）。</summary>
    public string Id { get; }

    /// <summary>真正传给 WindowService 的 id；null = 主窗口，不演示关闭。</summary>
    public string? WindowId { get; }

    public bool CanClose => WindowId is not null;

    public bool CanOpen => CanClose && !_isOpen;

    public bool CanCloseNow => CanClose && _isOpen;

    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            if (_isOpen == value) return;
            _isOpen = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(IsOpen)));
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(CanOpen)));
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(CanCloseNow)));
        }
    }

    public string StateText
    {
        get => _stateText;
        set
        {
            if (_stateText == value) return;
            _stateText = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(StateText)));
        }
    }

    /// <summary>订阅真实窗口的 Closed，服务关窗和用户直接关窗都会回写状态。</summary>
    public void Attach(System.Windows.Window window)
    {
        window.Closed += (_, _) =>
        {
            IsOpen = false;
            StateText = "已关闭";
        };
    }

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
}

/// <summary>导航表的一行。</summary>
public sealed record NavigateRow(string Name, string ViewType, string VmType, string Key);
