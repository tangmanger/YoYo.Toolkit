using System;
using System.Diagnostics;
using System.Windows;

namespace YoYo.Toolkit.Controls.Progress;

/// <summary>
/// 把进度控件的 Token 与模板字典挂到应用资源，使 <c>TryFindResource</c> 可见。
/// <para>
/// 原因：<c>Themes/Generic.xaml</c> 只服务于 <c>DefaultStyleKey</c> 的主题样式解析，
/// 其中的资源不参与元素的常规资源查找链，因此按 <see cref="ProgressKind"/> 动态取模板会落空。
/// </para>
/// <para>
/// 策略：首次实例化时把字典 <c>Insert(0, ...)</c>，使宿主在 <c>App.xaml</c> 中后续 merge 的
/// 同名资源排在查找序列更后（WPF 倒序查找），从而让宿主覆盖始终优先。
/// </para>
/// </summary>
internal static class ProgressTheme
{
    private const string TemplatesUri = "/YoYo.Toolkit;component/Themes/Progress.Templates.xaml";
    private const string TokensUri = "/YoYo.Toolkit;component/Themes/Progress.Tokens.Light.xaml";

    private static bool _loaded;
    private static bool _reported;

    internal static void EnsureLoaded()
    {
        if (_loaded) return;
        _loaded = true;

        var app = Application.Current;
        if (app is null)
        {
            // 设计器或未创建 Application 的宿主，退化为无默认视觉
            ReportOnce("当前没有 Application 实例，进度控件资源未挂载。");
            return;
        }

        EnsureDictionary(app, TemplatesUri);
        EnsureDictionary(app, TokensUri);
    }

    private static void EnsureDictionary(Application app, string uri)
    {
        var dictionaries = app.Resources.MergedDictionaries;
        var fileName = uri[(uri.LastIndexOf('/') + 1)..];

        foreach (var dictionary in dictionaries)
        {
            var source = dictionary.Source?.OriginalString;
            if (source is not null && source.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
                return;
        }

        try
        {
            dictionaries.Insert(0, new ResourceDictionary { Source = new Uri(uri, UriKind.Relative) });
        }
        catch (Exception ex)
        {
            ReportOnce($"加载 {fileName} 失败：{ex.Message}");
        }
    }

    internal static void ReportOnce(string message)
    {
        if (_reported) return;
        _reported = true;
        Debug.WriteLine($"[YoYo.Toolkit.Progress] {message}");
    }
}
