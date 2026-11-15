using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using YoYo.Toolkit.Notifies;
using YoYo.ToolkitSample.Services;

namespace YoYo.ToolkitSample.Views;

/// <summary>
/// 宿主的消息框实现。
/// <para>
/// 约定只有一条：必须提供 <c>(string caption, string msg, MessageLevel level)</c> 构造函数，
/// 因为 <see cref="TMessageBox"/> 是用反射按这个签名取窗口的。
/// </para>
/// <para>
/// 外观完全自绘——这里刻意不使用系统 MessageBox，标题栏、圆角、按钮都由令牌控制，深浅色自动跟随。
/// 关闭前把用户选择写回 <see cref="BaseMessageDialog.MessageBoxResult"/>。
/// </para>
/// </summary>
public partial class MyMessageDialog : BaseMessageDialog
{
    private const string IconBang = "M12,5.4 L12,13.6 M12,17 L12,17.6";
    private const string IconQuestion = "M9.4,9.2 A2.7,2.7 0 1 1 12.6,11.8 C12.1,12 11.8,12.5 11.8,13.2 L11.8,13.8 M12,17 L12,17.6";
    private const string IconCross = "M7,7 L17,17 M17,7 L7,17";

    public MyMessageDialog()
    {
        InitializeComponent();
        AttachWindowFrame();
        Apply(string.Empty, string.Empty, TMessageBox.MessageLevel.Information);
    }

    public MyMessageDialog(string caption, string msg, TMessageBox.MessageLevel level)
        : base(caption, msg, level)
    {
        InitializeComponent();
        AttachWindowFrame();
        Apply(caption, msg, level);
    }

    /// <summary>圆角 + 系统阴影（DWM），替代原来的透明窗口 + DropShadowEffect。</summary>
    private void AttachWindowFrame()
    {
        WindowFrame.Attach(this, WindowChrome.GetWindowChrome(this), () => false);
    }

    private void Apply(string caption, string msg, TMessageBox.MessageLevel level)
    {
        CaptionText.Text = string.IsNullOrWhiteSpace(caption) ? DefaultCaption(level) : caption;
        MessageText.Text = msg;

        var (accentKey, softKey, icon, primaryText) = level switch
        {
            TMessageBox.MessageLevel.Question => ("Sample.Accent", "Sample.AccentSoft", IconQuestion, "OK"),
            TMessageBox.MessageLevel.Warning => ("Sample.Warn", "Sample.WarnSoft", IconBang, "OK"),
            TMessageBox.MessageLevel.Error => ("Sample.Err", "Sample.ErrSoft", IconCross, "Close"),
            _ => ("Sample.Accent", "Sample.AccentSoft", IconBang, "OK")
        };

        IconBox.Background = Brush(softKey);
        IconPath.Data = Geometry.Parse(icon);
        IconPath.Stroke = Brush(accentKey);

        switch (level)
        {
            case TMessageBox.MessageLevel.Question:
                AddButton("取消", false, TMessageBox.TMessageBoxResult.Cancel, isDefault: false);
                AddButton("确定", true, TMessageBox.TMessageBoxResult.OK, isDefault: true);
                break;

            case TMessageBox.MessageLevel.QuestionNo:
                AddButton("否", false, TMessageBox.TMessageBoxResult.No, isDefault: false);
                AddButton("是", true, TMessageBox.TMessageBoxResult.OK, isDefault: true);
                break;

            case TMessageBox.MessageLevel.YesNoCancel:
                AddButton("取消", false, TMessageBox.TMessageBoxResult.Cancel, isDefault: false);
                AddButton("否", false, TMessageBox.TMessageBoxResult.No, isDefault: false);
                AddButton("是", true, TMessageBox.TMessageBoxResult.OK, isDefault: true);
                break;

            case TMessageBox.MessageLevel.Error:
                AddButton("关闭", true, TMessageBox.TMessageBoxResult.Close, isDefault: true);
                break;

            default:
                AddButton("确定", true, TMessageBox.TMessageBoxResult.OK, isDefault: true);
                break;
        }

        _ = primaryText;

        // TMessageBox 不设 Owner，这里补上，保证模态居中在外壳之上
        if (TMessageBox.MessageBoxOwner is { IsVisible: true } owner && !ReferenceEquals(owner, this))
        {
            try { Owner = owner; }
            catch (InvalidOperationException) { /* 已显示的窗口不可再设 Owner，忽略 */ }
        }

        Loaded += (_, _) => ButtonBar.Children[^1].Focus();
    }

    private static string DefaultCaption(TMessageBox.MessageLevel level) => level switch
    {
        TMessageBox.MessageLevel.Question => "请确认",
        TMessageBox.MessageLevel.Warning => "警告",
        TMessageBox.MessageLevel.Error => "错误",
        _ => "提示"
    };

    private static Brush Brush(string key)
        => Application.Current?.TryFindResource(key) as Brush ?? Brushes.Gray;

    private void AddButton(string text, bool primary, TMessageBox.TMessageBoxResult result, bool isDefault)
    {
        var button = new Button
        {
            Content = text,
            MinWidth = 68,
            IsDefault = isDefault,
            Margin = new Thickness(6, 0, 0, 0),
            Style = Application.Current?.TryFindResource(primary ? "Sample.Btn.Primary" : "Sample.Btn") as Style
        };

        button.Click += (_, _) =>
        {
            MessageBoxResult = result;
            Close();
        };

        ButtonBar.Children.Add(button);
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }
}
