using System.Windows.Controls;

namespace YoYo.ToolkitSample.Views;

/// <summary>网络页。默认本地模拟 DoWorkerAsync 的五个阶段，取消「本地模拟」走真实调用。</summary>
public partial class NetView : UserControl
{
    public NetView() => InitializeComponent();
}
