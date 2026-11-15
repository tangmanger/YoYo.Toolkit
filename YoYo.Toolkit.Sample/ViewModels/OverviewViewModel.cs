using System.Collections.Generic;
using System.Windows.Input;
using YoYo.ToolkitSample.Models;

namespace YoYo.ToolkitSample.ViewModels;

/// <summary>概览页：模块索引 + 接线说明。卡片点击即一次 GoTo。</summary>
public sealed class OverviewViewModel : PageViewModelBase
{
    public OverviewViewModel(INavigationHost? host) : base(host)
    {
        Modules = new[]
        {
            new ModuleCard("01", "概览", "当前页。模块索引、启动代码与设计令牌", "OverviewView", "Icon.Home"),
            new ModuleCard("02", "导航与窗口", "生命周期钩子、强类型参数、多窗口注册与跨线程关闭", "NavigationView", "Icon.Route"),
            new ModuleCard("03", "控件", "水印输入框、树形下拉、图标选项、描边文字、圆环", "ControlsView", "Icon.Sliders"),
            new ModuleCard("04", "进度", "20 种动效 + 三套确定态 + 流程步骤条，换肤只换令牌", "ProgressView", "Icon.Loader"),
            new ModuleCard("05", "通知", "四级消息框，外观由宿主窗口决定，不依赖系统 MessageBox", "NotifiesView", "Icon.Message"),
            new ModuleCard("06", "网络", "请求 / 响应基类、同步与异步调用、统一错误出口", "NetView", "Icon.Net"),
            new ModuleCard("07", "系统与工具", "硬盘序列号、主板 ID、Unix 时间戳、位图加载、值转换器", "SystemView", "Icon.Cpu"),
            new ModuleCard("08", "加密", "MD5 摘要（32 / 16 位、大小写）与 AES 加解密", "CryptoView", "Icon.Lock")
        };

        NavigateCommand = new RelayCommand<string>(viewType =>
        {
            if (!string.IsNullOrWhiteSpace(viewType)) Go(viewType!);
        });
    }

    public override string ViewType => "OverviewView";

    public override string Title => "概览";

    public override string Lede => "八个模块、一个导航容器：示例程序本身就用 BaseNavigation 组织。";

    public IReadOnlyList<ModuleCard> Modules { get; }

    public ICommand NavigateCommand { get; }

    public string SourceFileCount => "43";
    public string PublicTypeCount => "27";
    public string MotionCount => "20";
    public string DependencyCount => "0";

    private void Go(string viewType) => Host?.GoTo(viewType);
}

/// <summary>概览页的模块卡。</summary>
public sealed class ModuleCard
{
    public ModuleCard(string index, string title, string description, string viewType, string iconKey)
    {
        Index = index;
        Title = title;
        Description = description;
        ViewType = viewType;
        IconKey = iconKey;
    }

    public string Index { get; }
    public string Title { get; }
    public string Description { get; }
    public string ViewType { get; }
    public string IconKey { get; }
}
