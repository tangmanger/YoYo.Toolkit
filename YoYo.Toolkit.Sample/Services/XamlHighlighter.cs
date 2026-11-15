using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace YoYo.ToolkitSample.Services;

/// <summary>
/// 极简 XAML 语法着色 + 用法文档排版：只认注释、字符串、标签名、属性名、尖括号这几类，
/// 够用且没有依赖——不引入 AvalonEdit 之类的第三方编辑器。
/// </summary>
internal static class XamlHighlighter
{
    private static readonly Regex Token = new(
        @"(?<comment><!--.*?-->)|(?<string>""[^""]*"")|(?<tag></?[A-Za-z_][\w:.\-]*)|(?<attr>[\w:.\-]+(?=\s*=))|(?<markup>[<>/=])",
        RegexOptions.Compiled);

    /// <summary>把某个控件的全部用法排成一份文档：命名空间 → 各变体 → 宿主提示。</summary>
    public static FlowDocument Build(ControlSample sample)
    {
        var document = CreateDocument();

        AddSection(document, "命名空间", null);
        AppendCode(document, sample.Xmlns);

        for (var i = 0; i < sample.Variants.Count; i++)
        {
            var variant = sample.Variants[i];
            AddSection(document, $"{i + 1:00} · {variant.Title}", variant.Note);
            AppendCode(document, variant.Xaml);
        }

        if (!string.IsNullOrWhiteSpace(sample.HostNote))
        {
            AddSection(document, "宿主必读", null);
            document.Blocks.Add(new Paragraph(new Run(sample.HostNote))
            {
                Margin = new Thickness(0, 0, 0, 14),
                FontSize = 13,
                Foreground = Brush("Sample.Warn")
            });
        }

        return document;
    }

    /// <summary>复制用的纯文本：每个变体前加一行 XAML 注释当标题。</summary>
    public static string Compose(ControlSample sample)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<!-- " + sample.Name + " · " + sample.Summary + " -->");
        builder.AppendLine(sample.Xmlns);
        builder.AppendLine();

        for (var i = 0; i < sample.Variants.Count; i++)
        {
            var variant = sample.Variants[i];
            builder.AppendLine($"<!-- {i + 1:00} · {variant.Title} -->");
            builder.AppendLine(variant.Xaml.Trim());
            builder.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(sample.HostNote))
            builder.AppendLine("<!-- 宿主必读：" + sample.HostNote + " -->");

        return builder.ToString().TrimEnd();
    }

    private static FlowDocument CreateDocument()
    {
        var font = Application.Current?.TryFindResource("Sample.MonoFontFamily") as FontFamily
                   ?? new FontFamily("Consolas");

        return new FlowDocument
        {
            FontFamily = font,
            FontSize = 13,
            PagePadding = new Thickness(0),
            LineHeight = 21,
            TextAlignment = TextAlignment.Left
        };
    }

    private static void AddSection(FlowDocument document, string title, string? note)
    {
        var accent = Brush("Sample.Accent");
        var muted = Brush("Sample.Ink3");

        var titleParagraph = new Paragraph(new Run(title))
        {
            Margin = new Thickness(0, document.Blocks.Count == 0 ? 0 : 10, 0, 5),
            FontSize = 12.5,
            FontWeight = FontWeights.SemiBold,
            Foreground = accent
        };

        document.Blocks.Add(titleParagraph);

        if (string.IsNullOrWhiteSpace(note)) return;

        document.Blocks.Add(new Paragraph(new Run(note))
        {
            Margin = new Thickness(0, 0, 0, 7),
            FontSize = 12.5,
            FontFamily = Application.Current?.TryFindResource("Sample.FontFamily") as FontFamily
                         ?? new FontFamily("Microsoft YaHei UI"),
            Foreground = muted
        });
    }

    private static void AppendCode(FlowDocument document, string xaml)
    {
        var body = Brush("Sample.Ink2");
        var tag = Brush("Sample.Syntax.Tag");
        var attr = Brush("Sample.Syntax.Attr");
        var value = Brush("Sample.Syntax.Value");
        var comment = Brush("Sample.Ink3");
        var markup = Brush("Sample.Ink3");

        // 代码块用一个带底色的段落区间表示：这里靠左右留白 + 等宽字体区分，避免再套一层 Border
        var lines = xaml.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

        foreach (var line in lines)
        {
            var paragraph = new Paragraph { Margin = new Thickness(0), LineHeight = 21 };
            AppendLine(paragraph, line, body, tag, attr, value, comment, markup);
            document.Blocks.Add(paragraph);
        }

        document.Blocks.Add(new Paragraph(new Run(" "))
        {
            Margin = new Thickness(0),
            FontSize = 5,
            LineHeight = 10
        });
    }

    private static void AppendLine(Paragraph paragraph, string line, Brush body, Brush tag, Brush attr,
        Brush value, Brush comment, Brush markup)
    {
        var cursor = 0;

        foreach (Match match in Token.Matches(line))
        {
            if (match.Index > cursor)
                paragraph.Inlines.Add(new Run(line[cursor..match.Index]) { Foreground = body });

            if (match.Groups["comment"].Success)
            {
                paragraph.Inlines.Add(new Run(match.Value) { Foreground = comment });
            }
            else if (match.Groups["string"].Success)
            {
                paragraph.Inlines.Add(new Run(match.Value) { Foreground = value });
            }
            else if (match.Groups["tag"].Success)
            {
                var head = match.Value.StartsWith("</", StringComparison.Ordinal) ? "</" : "<";
                paragraph.Inlines.Add(new Run(head) { Foreground = markup });
                paragraph.Inlines.Add(new Run(match.Value[head.Length..]) { Foreground = tag });
            }
            else if (match.Groups["attr"].Success)
            {
                paragraph.Inlines.Add(new Run(match.Value) { Foreground = attr });
            }
            else
            {
                paragraph.Inlines.Add(new Run(match.Value) { Foreground = markup });
            }

            cursor = match.Index + match.Length;
        }

        if (cursor < line.Length)
            paragraph.Inlines.Add(new Run(line[cursor..]) { Foreground = body });
    }

    private static Brush Brush(string key)
        => Application.Current?.TryFindResource(key) as Brush ?? Brushes.Gray;
}
