using System.Windows.Controls;

namespace YoYo.ToolkitSample.Views;

/// <summary>概览页。DataContext 由 ShellService 注入，视图本身没有逻辑。</summary>
public partial class OverviewView : UserControl
{
    public OverviewView() => InitializeComponent();
}
