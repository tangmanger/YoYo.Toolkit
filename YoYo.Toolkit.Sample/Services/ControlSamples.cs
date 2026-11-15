using System;
using System.Collections.Generic;
using System.Linq;

namespace YoYo.ToolkitSample.Services;

/// <summary>一个控件的用法条目：控件名 + 归属模块 + 若干「最小可用」标记片段。</summary>
internal sealed class ControlSample
{
    public ControlSample(string name, string module, string summary, string xmlns,
        IReadOnlyList<ControlVariant> variants, string? hostNote = null, string? page = null)
    {
        Name = name;
        Module = module;
        Summary = summary;
        Xmlns = xmlns;
        Variants = variants;
        HostNote = hostNote;
        Page = page;
    }

    /// <summary>控件类型名，如 <c>ProgressIndicator</c>。</summary>
    public string Name { get; }

    /// <summary>源码里的命名空间，如 <c>YoYo.Toolkit.Controls.Progress</c>。</summary>
    public string Module { get; }

    /// <summary>一句话说明这个控件解决什么。</summary>
    public string Summary { get; }

    /// <summary>使用前需要的那行 xmlns 声明。</summary>
    public string Xmlns { get; }

    /// <summary>各用法变体（最小可复制片段）。</summary>
    public IReadOnlyList<ControlVariant> Variants { get; }

    /// <summary>宿主必须做的事（例如补 Style），没有则为 null。</summary>
    public string? HostNote { get; }

    /// <summary>示例程序里演示它的页面（ViewType），用于打开查看器时定位。</summary>
    public string? Page { get; }

    /// <summary>列表副标题：短模块名 + 变体数（去掉 YoYo.Toolkit. 前缀，避免目录里被省略号截断）。</summary>
    public string Badge => $"{ShortModule} · {Variants.Count} 种用法";

    private string ShortModule => Module.StartsWith("YoYo.Toolkit.", StringComparison.Ordinal)
        ? Module["YoYo.Toolkit.".Length..]
        : Module;

    public override string ToString() => Name;
}

/// <summary>某个控件的一种用法。</summary>
internal sealed class ControlVariant
{
    public ControlVariant(string title, string xaml, string? note = null)
    {
        Title = title;
        Xaml = xaml;
        Note = note;
    }

    public string Title { get; }

    public string Xaml { get; }

    /// <summary>这条用法的关键点，渲染成灰字说明（不是代码）。</summary>
    public string? Note { get; }
}

/// <summary>
/// 控件用法目录：只放「一个控件怎么用」的最小片段，而不是整页源码。
/// 每个条目都能直接粘进宿主工程，变体覆盖最常踩的那几种场景。
/// </summary>
internal static class ControlSamples
{
    private const string ProgressXmlns = "xmlns:p=\"clr-namespace:YoYo.Toolkit.Controls.Progress;assembly=YoYo.Toolkit\"";
    private const string TextXmlns = "xmlns:txt=\"clr-namespace:YoYo.Toolkit.Controls.Text;assembly=YoYo.Toolkit\"";
    private const string ComboXmlns = "xmlns:yy=\"clr-namespace:YoYo.Toolkit.Controls.ComboBox;assembly=YoYo.Toolkit\"";
    private const string ButtonXmlns = "xmlns:ibtn=\"clr-namespace:YoYo.Toolkit.Controls.Button;assembly=YoYo.Toolkit\"";
    private const string RingXmlns = "xmlns:bi=\"clr-namespace:YoYo.Toolkit.Controls.BusyIndicators;assembly=YoYo.Toolkit\"";
    private const string InputXmlns = "xmlns:input=\"clr-namespace:YoYo.Toolkit.Input;assembly=YoYo.Toolkit\"";
    private const string ConvXmlns = "xmlns:conv=\"clr-namespace:YoYo.Toolkit.Converters;assembly=YoYo.Toolkit\"";
    private const string NotifyXmlns = "xmlns:notifies=\"clr-namespace:YoYo.Toolkit.Notifies;assembly=YoYo.Toolkit\"";

    private static readonly IReadOnlyList<ControlSample> All = new List<ControlSample>
    {
        new("ProgressIndicator", "YoYo.Toolkit.Controls.Progress",
            "20 种不确定动效 + 确定态，全部由 Kind 切换，自带浅/深令牌。",
            ProgressXmlns,
            new List<ControlVariant>
            {
                new("不确定态 · 换 Kind 即换动效",
                    """
                    <p:ProgressIndicator Kind="Arc"
                                         IsActive="True"
                                         Width="40" Height="40" />
                    """,
                    "Kind 是枚举：环形 12 种、条形与波形 6 种、行内小型 2 种。"),

                new("带状态文案",
                    """
                    <p:ProgressIndicator Kind="Signal"
                                         IsActive="True"
                                         StatusText="正在采集"
                                         Width="190" Height="44" />
                    """,
                    "StatusText 显示在视觉右侧；ShowStatusText=\"False\" 可只留图形。"),

                new("确定态（Value 走 Minimum/Maximum）",
                    """
                    <p:ProgressIndicator Kind="FlowBar"
                                         IsIndeterminate="False"
                                         Minimum="0" Maximum="100" Value="62"
                                         Width="180" Height="20" />
                    """,
                    "只读属性 Percent（0~1）供模板画弧线/读数，Maximum==Minimum 时不会除零。"),

                new("用 IsActive 统一暂停",
                    """
                    <p:ProgressIndicator Kind="Breathing"
                                         IsActive="{Binding IsRunning}"
                                         Width="48" Height="48" />
                    """,
                    "IsActive=False 是停故事板，不是把 Opacity 设 0 —— 停住的画面不再产生渲染开销。"),

                new("换成自己的模板",
                    """
                    <Window.Resources>
                        <ControlTemplate x:Key="MyBrandSpinner"
                                         TargetType="{x:Type p:ProgressIndicator}">
                            <!-- 状态组必须叫 ProgressStates，只动 RenderTransform / Opacity -->
                        </ControlTemplate>
                    </Window.Resources>

                    <p:ProgressIndicator TemplateKey="MyBrandSpinner"
                                         Width="40" Height="40" />
                    """,
                    "四级回退：显式 Template > TemplateKey > Progress.Template.{Kind}.{Mode} > Progress.Template.Default。"),
            },
            hostNote: "开箱即用：Generic.xaml 的隐式样式会自动挂上模板与令牌，宿主不需要写 Style。",
            page: "ProgressView"),

        new("StepTracker", "YoYo.Toolkit.Controls.Progress",
            "流程步骤条：回答「走在第几步」，与「还要等多久」的 ProgressIndicator 正交。",
            ProgressXmlns,
            new List<ControlVariant>
            {
                new("最小用法",
                    """
                    <p:StepTracker Steps="{Binding Steps}"
                                   CurrentIndex="{Binding CurrentStep}" />
                    """,
                    "各步的 State 由 CurrentIndex 推导：index 之前 Done、当前 Active、之后 Waiting。"),

                new("显式锁住失败/跳过",
                    """
                    <!-- ViewModel：显式设过 State 的步骤不再被 CurrentIndex 覆盖 -->
                    new ProgressStep { Label = "完成" },
                    new ProgressStep { Label = "失败", State = ProgressStepState.Error },
                    new ProgressStep { Label = "跳过", State = ProgressStepState.Skipped },
                    new ProgressStep { Label = "进行中" },
                    """,
                    "否则「失败后重试」的流程无法如实呈现：重试会把 Error 洗成 Done。"),

                new("竖排 / 关掉连线",
                    """
                    <p:StepTracker Steps="{Binding Steps}"
                                   CurrentIndex="3"
                                   Orientation="Vertical"
                                   ShowConnector="False" />
                    """,
                    "横向按可用宽度等分单元格，标签不会被裁；竖排自动换成「节点在上、标签在下」的项模板。"),

                new("换主色",
                    """
                    <p:StepTracker Steps="{Binding Steps}"
                                   CurrentIndex="2"
                                   AccentBrush="{DynamicResource Brand.Accent}" />
                    """,
                    "没给 AccentBrush 时由样式从 Progress.Brush.Accent 注入，跟随深浅令牌。"),
            },
            page: "ProgressView"),

        new("PlaceholderTextBox", "YoYo.Toolkit.Controls.Text",
            "TextBox + 水印 + 圆角 + 错误态，水印由模板里的 Border 画出来。",
            TextXmlns,
            new List<ControlVariant>
            {
                new("基本：水印 + 双向绑定",
                    """
                    <txt:PlaceholderTextBox PlaceHolderText="请输入样本编号"
                                            Text="{Binding SampleNo, Mode=TwoWay,
                                                   UpdateSourceTrigger=PropertyChanged}" />
                    """),

                new("错误态 / 胶囊圆角",
                    """
                    <txt:PlaceholderTextBox IsShowError="True"
                                            Text="{Binding ErrorText, Mode=TwoWay}" />

                    <txt:PlaceholderTextBox PlaceHolderText="搜索…"
                                            CornerRadius="14" />
                    """),

                new("水印对齐（宿主必做）",
                    """
                    <!-- 库默认 TransformY=15 会把水印压到输入框下沿被裁；
                         32px 高的输入框里改成 7 左右才与输入文字同一基线。
                         TransformX 默认 16 正好补输入文字的左内边距，不用动 -->
                    <txt:PlaceholderTextBox PlaceHolderText="请输入样本编号"
                                            TransformY="7" />
                    """,
                    "水印是 PART_DropButton 这个 Border 的 VisualBrush；VisualBrush 自动布局时 TextBlock 的 Margin 基本不进内容边界，水印实际从近 (0,0) 画起，所以 TransformX/Y 是补内边距而非额外偏移。垂直居中值 ≈（输入框内部高 − 水印字形高）/ 2。改位置只需调这两个属性，不用改控件源码。"),

                new("在此基础上微调",
                    """
                    <txt:PlaceholderTextBox PlaceHolderText="再右移 4px / 下移 2px"
                                            TransformX="20" TransformY="9" />
                    """,
                    "两个值可正可负，在对齐位置上做增量调整。"),
            },
            hostNote: "必须由宿主提供 Style，且模板里要有名为 PART_DropButton 的 Border，否则没有水印；水印偏下时调 TransformY（默认 15，32px 高输入框约需 7），TransformX 默认 16 通常不用动。",
            page: "ControlsView"),

        new("TreeComboBox", "YoYo.Toolkit.Controls.ComboBox",
            "带树形下拉的选择控件：层级数据 + 只允许选叶子节点。",
            ComboXmlns,
            new List<ControlVariant>
            {
                new("层级数据 + 选中回传",
                    """
                    <yy:TreeComboBox ItemsSource="{Binding DeviceRoots}"
                                     SelectCommand="{Binding SelectDeviceCommand}"
                                     IsDorpDown="{Binding IsDropDown, Mode=TwoWay}"
                                     IsOnlyLastNode="True">
                        <yy:TreeComboBox.ItemTemplate>
                            <HierarchicalDataTemplate ItemsSource="{Binding Children}">
                                <TextBlock Text="{Binding DisplayName}" />
                            </HierarchicalDataTemplate>
                        </yy:TreeComboBox.ItemTemplate>
                    </yy:TreeComboBox>
                    """,
                    "IsDorpDown 是历史拼写（不是 IsDropDown）；选中结果通过 SelectCommand 回传，不走 SelectedItem。"),

                new("只读回显选中项",
                    """
                    <TextBlock Text="{Binding ElementName=DeviceTree, Path=Text}" />
                    """,
                    "控件把选中节点的文本暴露成 Text，可直接拿来做只读回显。"),
            },
            hostNote: "节点实现 ITreeModel；样式必须由宿主提供（示例见 Themes/Controls.xaml 的 Sample.TreePanel）。",
            page: "ControlsView"),

        new("IconRadioButton", "YoYo.Toolkit.Controls.Button",
            "带矢量图标的单选块，用来做模式切换这类「图标 + 文案」的选择。",
            ButtonXmlns,
            new List<ControlVariant>
            {
                new("基本",
                    """
                    <ibtn:IconRadioButton GroupName="SampleMode"
                                          Content="自动"
                                          Icon="M4,12 L12,4 L20,12"
                                          IsChecked="{Binding IsAuto, Mode=TwoWay}" />
                    """,
                    "Icon 是 24×24 的描边路径数据字符串，颜色由 IconForeground 控制。"),

                new("同组多个选项",
                    """
                    <ibtn:IconRadioButton GroupName="SampleMode" Content="手动" Icon="M6,6 L18,18" />
                    <ibtn:IconRadioButton GroupName="SampleMode" Content="暂停" Icon="M8,5 L8,19" />
                    """,
                    "同一 GroupName 内互斥，IsChecked 双向绑定各自的 ViewModel 属性。"),
            },
            hostNote: "必须由宿主提供 Style；Icon 是 string，TemplateBinding 到 Path.Data 不会走类型转换器，模板里要自备 string→Geometry。",
            page: "ControlsView"),

        new("OutlineText", "YoYo.Toolkit.Controls.Text",
            "可描边的文字，用于仪器面板上的大字号标签。",
            TextXmlns,
            new List<ControlVariant>
            {
                new("描边字",
                    """
                    <txt:OutlineText Text="SYSTEM READY"
                                     FontSize="25" FontWeight="SemiBold"
                                     Fill="{DynamicResource Sample.Surface2}"
                                     Stroke="{DynamicResource Sample.Accent}"
                                     StrokeThickness="1.1"
                                     StrokePosition="Outside" />
                    """,
                    "StrokePosition：Outside / Center / Inside；Fill 用背景色即可得到「空心字」。"),
            },
            page: "ControlsView"),

        new("TProgressRing", "YoYo.Toolkit.Controls.BusyIndicators",
            "最轻的等待指示环，只有 IsActive 一个开关，外观全交给宿主模板。",
            RingXmlns,
            new List<ControlVariant>
            {
                new("基本",
                    """
                    <bi:TProgressRing IsActive="{Binding IsBusy}"
                                      Width="40" Height="40"
                                      Foreground="{DynamicResource Sample.Accent}" />
                    """,
                    "尺寸走 Width/Height，颜色走 Foreground（模板里绑定它）。"),
            },
            hostNote: "必须由宿主提供 Style；示例模板是「虚线弧 + RotateTransform 旋转」，见 Sample.Ring。",
            page: "ControlsView"),

        new("PasswordHelper", "YoYo.Toolkit.Input",
            "把 PasswordBox 的密码变成可绑定的附加属性（PasswordBox.Password 不是依赖属性）。",
            InputXmlns,
            new List<ControlVariant>
            {
                new("附加属性绑定",
                    """
                    <PasswordBox input:PasswordHelper.Attach="True"
                                 input:PasswordHelper.Password="{Binding Password, Mode=TwoWay}" />
                    """,
                    "Attach=True 后控件内部同步 PasswordChanged 与绑定值，ViewModel 拿到的就是明文。"),
            },
            page: "ControlsView"),

        new("进度转换器", "YoYo.Toolkit.Converters",
            "确定态模板里把 Percent 变成读数或弧线，省掉 ViewModel 里再维护一份文案。",
            ConvXmlns,
            new List<ControlVariant>
            {
                new("Percent → 读数",
                    """
                    <conv:PercentToTextConverter x:Key="PercentText" />

                    <TextBlock Text="{Binding Percent, ElementName=Ring,
                                      Converter={StaticResource PercentText},
                                      ConverterParameter=1}" />
                    """,
                    "ConverterParameter 是小数位（0~2），上面这条得到 68.5% 这种形式。"),

                new("Percent → 弧线几何",
                    """
                    <conv:PercentToArcGeometryConverter x:Key="PercentArc" />

                    <Path StrokeThickness="6" Stroke="{DynamicResource Sample.Accent}">
                        <Path.Data>
                            <MultiBinding Converter="{StaticResource PercentArc}"
                                          ConverterParameter="6">
                                <Binding Path="Percent" ElementName="Ring" />
                                <Binding Path="ActualWidth" ElementName="Ring" />
                                <Binding Path="ActualHeight" ElementName="Ring" />
                            </MultiBinding>
                        </Path.Data>
                    </Path>
                    """,
                    "多值转换器：依次传 Percent / 宽 / 高，ConverterParameter 是线宽；0 返回空几何、1 返回整圆。"),
            },
            page: "ProgressView"),

        new("BaseMessageDialog", "YoYo.Toolkit.Notifies",
            "自绘消息框的宿主实现基类：标题、正文、按钮全由你的 XAML 决定。",
            NotifyXmlns,
            new List<ControlVariant>
            {
                new("宿主实现的根元素",
                    """
                    <notifies:BaseMessageDialog x:Class="YourApp.Views.MyMessageDialog"
                                                Width="404" SizeToContent="Height"
                                                WindowStyle="None" ResizeMode="NoResize"
                                                WindowStartupLocation="CenterOwner">
                        <!-- 自绘标题 / 正文 / 按钮；关闭前给 MessageBoxResult 赋值 -->
                    </notifies:BaseMessageDialog>
                    """,
                    "根元素必须是 BaseMessageDialog，否则生成的分部类基类不一致会报 CS0263。"),

                new("启动时注册 + 调用",
                    """
                    // App.OnStartup
                    TMessageBox.Init(typeof(MyMessageDialog));

                    // 任意位置
                    var result = TMessageBox.ShowMsg("开始运行", "确认以当前参数启动采集？",
                                                     TMessageBox.MessageLevel.Question);
                    """,
                    "必须是 (string caption, string msg, MessageLevel level) 这个构造函数签名，反射按它取窗口。"),
            },
            page: "NotifiesView"),
    };

    /// <summary>目录里的全部控件用法。</summary>
    public static IReadOnlyList<ControlSample> Catalog => All;

    /// <summary>按页面定位默认展示的控件；页面没有专属控件时返回第一个。</summary>
    public static ControlSample DefaultFor(string? viewType)
    {
        if (!string.IsNullOrWhiteSpace(viewType))
        {
            var hit = All.FirstOrDefault(sample =>
                string.Equals(sample.Page, viewType, StringComparison.OrdinalIgnoreCase));

            if (hit is not null) return hit;
        }

        return All[0];
    }
}
