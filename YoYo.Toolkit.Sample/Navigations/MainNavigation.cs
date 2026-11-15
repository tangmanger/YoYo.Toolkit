using System;
using System.Collections.Generic;
using YoYo.Toolkit.Interfaces.Navigations;
using YoYo.Toolkit.Models.Navigations;
using YoYo.Toolkit.Navigations;
using YoYo.ToolkitSample.Views;

namespace YoYo.ToolkitSample.Navigations;

/// <summary>
/// 主导航：一个 <see cref="NavigateModel"/> 就是侧栏的一行。
/// <para>
/// 注意两个坑（示例里刻意绕开）：
/// 1. 基类构造函数会调用 <see cref="InitView"/>，此时派生字段还没初始化，所以 <see cref="Navigates"/> 用懒加载，
///    InitView 里也绝不碰它；
/// 2. <c>ViewType</c> 是唯一 key，字符串匹配，找不到就返回 null，不抛异常。
/// </para>
/// </summary>
public sealed class MainNavigation : BaseNavigation
{
    private List<NavigateModel>? _navigates;

    public MainNavigation(IGetService service) : base(service) { }

    /// <summary>日志出口，外壳订阅后写到状态栏与日志页。</summary>
    public event Action<string, bool>? Logged;

    public override string NavigationName => "Main";

    public override List<NavigateModel> Navigates => _navigates ??= Build();

    /// <summary>侧栏只显示这 8 页；DeviceView 是参数导航演示页，不进侧栏。</summary>
    public static readonly string[] SidebarViewTypes =
    {
        "OverviewView", "NavigationView", "ControlsView", "ProgressView",
        "NotifiesView", "NetView", "SystemView", "CryptoView"
    };

    private static List<NavigateModel> Build() => new()
    {
        new NavigateModel
        {
            ViewType = "OverviewView",
            Type = typeof(OverviewView),
            VMType = typeof(ViewModels.OverviewViewModel),
            Name = "概览"
        },
        new NavigateModel
        {
            ViewType = "NavigationView",
            Type = typeof(NavigationView),
            VMType = typeof(ViewModels.NavigationViewModel),
            Name = "导航与窗口"
        },
        new NavigateModel
        {
            ViewType = "ControlsView",
            Type = typeof(ControlsView),
            VMType = typeof(ViewModels.ControlsViewModel),
            Name = "控件"
        },
        new NavigateModel
        {
            ViewType = "ProgressView",
            Type = typeof(ProgressView),
            VMType = typeof(ViewModels.ProgressViewModel),
            Name = "进度"
        },
        new NavigateModel
        {
            ViewType = "NotifiesView",
            Type = typeof(NotifiesView),
            VMType = typeof(ViewModels.NotifiesViewModel),
            Name = "通知"
        },
        new NavigateModel
        {
            ViewType = "NetView",
            Type = typeof(NetView),
            VMType = typeof(ViewModels.NetViewModel),
            Name = "网络"
        },
        new NavigateModel
        {
            ViewType = "SystemView",
            Type = typeof(SystemView),
            VMType = typeof(ViewModels.SystemViewModel),
            Name = "系统与工具"
        },
        new NavigateModel
        {
            ViewType = "CryptoView",
            Type = typeof(CryptoView),
            VMType = typeof(ViewModels.CryptoViewModel),
            Name = "加密"
        },
        new NavigateModel
        {
            ViewType = "DeviceView",
            Type = typeof(DeviceView),
            VMType = typeof(ViewModels.DeviceViewModel),
            Name = "设备详情（参数演示）"
        }
    };

    /// <summary>不预加载视图：页面在第一次 GoTo 时才创建。</summary>
    public override void InitView() { }

    public override void LogWrite(string? msg)
    {
        if (!string.IsNullOrWhiteSpace(msg)) Logged?.Invoke(msg!, false);
    }

    public override void LogWrite(string? msg, Exception? exception)
        => Logged?.Invoke($"{msg} · {exception?.Message}", true);
}
