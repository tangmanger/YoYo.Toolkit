using YoYo.Toolkit.Controls.Progress;

namespace YoYo.ToolkitSample.Models;

/// <summary>演示卡片的数据项：一个 Kind + 显示名 + 建议尺寸。</summary>
public sealed class KindSample
{
    public KindSample(ProgressKind kind, string label, double width, double height)
    {
        Kind = kind;
        Label = label;
        Width = width;
        Height = height;
    }

    public ProgressKind Kind { get; }
    public string Label { get; }
    public double Width { get; }
    public double Height { get; }
}
