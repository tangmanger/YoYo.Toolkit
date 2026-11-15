using System.Collections.Generic;
using YoYo.ToolkitSample.Models;

namespace YoYo.ToolkitSample.ViewModels;

/// <summary>
/// 参数导航演示页：它不在侧栏里，只能由 <c>GoTo("DeviceView", "SN-0001")</c> 进入。
/// <para>
/// 生命周期会**两次**触发：<c>GoTo</c> 先按 VMType 取一次 VM 调无参 <c>NavigateIn()</c>，
/// 再取一次调 <c>NavigateIn&lt;T&gt;(param)</c>。所以这里把有参回调当作唯一入口，
/// 无参回调只记录一行日志，避免重复加载。
/// </para>
/// </summary>
public sealed class DeviceViewModel : PageViewModelBase<string>
{
    private string _serialNumber = "—";
    private string _enteredAt = "—";
    private bool _isPolling;
    private string _pollState = "未在轮询";

    public DeviceViewModel(INavigationHost? host) : base(host)
    {
        StopPollingCommand = new RelayCommand(() =>
        {
            IsPolling = false;
            PollState = "已停止轮询";
            Log("DeviceViewModel · 手动停止轮询");
        });
    }

    public override string ViewType => "DeviceView";

    public override string Title => "设备详情（参数演示）";

    public override string Lede => "由 GoTo(\"DeviceView\", \"SN-0001\") 带参进入；离开时停轮询、退订阅。";

    public string SerialNumber
    {
        get => _serialNumber;
        private set => SetField(ref _serialNumber, value);
    }

    public string EnteredAt
    {
        get => _enteredAt;
        private set => SetField(ref _enteredAt, value);
    }

    public bool IsPolling
    {
        get => _isPolling;
        private set
        {
            if (!SetField(ref _isPolling, value)) return;
            PollState = value ? "轮询中 · 1 Hz" : "已停止轮询";
        }
    }

    public string PollState
    {
        get => _pollState;
        private set => SetField(ref _pollState, value);
    }

    public System.Windows.Input.ICommand StopPollingCommand { get; }

    public IReadOnlyList<TraceEntry> Trace { get; } = new List<TraceEntry>();

    /// <summary>有参回调：真正接参的地方。</summary>
    protected override void OnNavigatedIn(string? param)
    {
        SerialNumber = string.IsNullOrWhiteSpace(param) ? "（未传参）" : param!;
        EnteredAt = System.DateTime.Now.ToString("HH:mm:ss");
        IsPolling = true;
        Log($"NavigateIn<string>(\"{SerialNumber}\") · 开始加载配置");
    }

    /// <summary>无参回调：GoTo 里先调的那一次，这里不重复加载。</summary>
    protected override void OnNavigatedIn()
        => Log("NavigateIn() · 无参回调先到，跳过加载");

    protected override void OnNavigatedOut()
    {
        IsPolling = false;
        Log($"NavigateOut() · 释放 {SerialNumber} 的订阅与轮询");
    }
}

/// <summary>生命周期轨迹的一行（占位，供视图展示）。</summary>
public sealed record TraceEntry(string Time, string Message);
