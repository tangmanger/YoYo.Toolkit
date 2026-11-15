using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using YoYo.Toolkit.Notifies;
using YoYo.ToolkitSample.Models;

namespace YoYo.ToolkitSample.ViewModels;

/// <summary>
/// 通知页：四级消息框都走 <see cref="TMessageBox.ShowMsg(string,string,TMessageBox.MessageLevel)"/>，
/// 真正显示的是宿主注册的 <see cref="YoYo.ToolkitSample.Views.MyMessageDialog"/>——不是系统 MessageBox。
/// </summary>
public sealed class NotifiesViewModel : PageViewModelBase
{
    public NotifiesViewModel(INavigationHost? host) : base(host)
    {
        Levels = new[]
        {
            new MessageLevelOption("信息", "Information", TMessageBox.MessageLevel.Information, "Icon.Bang"),
            new MessageLevelOption("询问", "Question", TMessageBox.MessageLevel.Question, "Icon.Question"),
            new MessageLevelOption("警告", "Warning", TMessageBox.MessageLevel.Warning, "Icon.Bang"),
            new MessageLevelOption("错误", "Error", TMessageBox.MessageLevel.Error, "Icon.Bang")
        };

        ShowCommand = new RelayCommand<MessageLevelOption>(Show);
        ClearCommand = new RelayCommand(() => Results.Clear());
    }

    public override string ViewType => "NotifiesView";

    public override string Title => "通知";

    public override string Lede => "不依赖系统 MessageBox——外观由宿主注册的窗口决定，返回值仍是 TMessageBoxResult。";

    public System.Collections.Generic.IReadOnlyList<MessageLevelOption> Levels { get; }

    public ObservableCollection<ResultEntry> Results { get; } = new();

    public ICommand ShowCommand { get; }

    public ICommand ClearCommand { get; }

    private void Show(MessageLevelOption? option)
    {
        if (option is null) return;

        TMessageBox.MessageBoxOwner = Application.Current?.MainWindow;

        var caption = option.Level switch
        {
            TMessageBox.MessageLevel.Question => "开始运行",
            TMessageBox.MessageLevel.Warning => "参数越界",
            TMessageBox.MessageLevel.Error => "采集失败",
            _ => "提示"
        };

        var message = option.Level switch
        {
            TMessageBox.MessageLevel.Question => "确认以当前参数启动采集？",
            TMessageBox.MessageLevel.Warning => "循环数 60 超出建议区间 35–50，继续可能延长单次运行时长。",
            TMessageBox.MessageLevel.Error => "设备 A-02 未响应，最后一次心跳距今 12s。",
            _ => "运行参数已保存，将在下一次启动时生效。"
        };

        var result = TMessageBox.ShowMsg(caption, message, option.Level);

        Results.Insert(0, new ResultEntry(System.DateTime.Now.ToString("HH:mm:ss"), option.LevelName, result.ToString()));
        while (Results.Count > 12) Results.RemoveAt(Results.Count - 1);

        Log($"TMessageBox.ShowMsg(\"{caption}\", {option.LevelName}) → {result}");
    }
}

/// <summary>一个按钮对应一个级别。</summary>
public sealed class MessageLevelOption
{
    public MessageLevelOption(string label, string levelName, TMessageBox.MessageLevel level, string iconKey)
    {
        Label = label;
        LevelName = levelName;
        Level = level;
        IconKey = iconKey;
    }

    public string Label { get; }
    public string LevelName { get; }
    public TMessageBox.MessageLevel Level { get; }
    public string IconKey { get; }

    public string Caption => $"TMessageBox.MessageLevel.{LevelName}";
}

/// <summary>一次调用的返回值。</summary>
public sealed class ResultEntry
{
    public ResultEntry(string time, string level, string result)
    {
        Time = time;
        Level = level;
        Result = result;
    }

    public string Time { get; }
    public string Level { get; }
    public string Result { get; }
}
