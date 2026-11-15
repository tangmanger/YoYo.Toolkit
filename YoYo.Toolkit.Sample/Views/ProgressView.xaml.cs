using System.Windows.Controls;

namespace YoYo.ToolkitSample.Views;

/// <summary>
/// 进度页。所有指示器的 IsActive 都绑 <c>IsRunning</c>，暂停即整体停故事板——
/// 不再像早期版本那样遍历视觉树逐个关。
/// </summary>
public partial class ProgressView : UserControl
{
    public ProgressView() => InitializeComponent();
}
