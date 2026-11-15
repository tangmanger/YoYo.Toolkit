using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace YoYo.ToolkitSample;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 消息框实现必须在使用前注册一次；签名固定为 (caption, msg, level)
        YoYo.Toolkit.Notifies.TMessageBox.Init(typeof(Views.MyMessageDialog));

        if (e.Args.Contains("--diag", StringComparer.OrdinalIgnoreCase))
        {
            RunDiagnostics();
            Shutdown();
            return;
        }

        if (e.Args.Contains("--cpu", StringComparer.OrdinalIgnoreCase))
        {
            RunCpuProbe();
            return;
        }

        // 连通性检测冒烟：Options 为 null 的应答不能再抛 NRE
        if (e.Args.Contains("--ping", StringComparer.OrdinalIgnoreCase))
        {
            RunPingProbe();
            return;
        }

        // 窗口外框冒烟：读回 DWM 圆角偏好与窗口样式，确认阴影前提条件
        if (e.Args.Contains("--frame", StringComparer.OrdinalIgnoreCase))
        {
            var frameWindow = new MainWindow
            {
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -4000,
                Top = 0,
                ShowInTaskbar = false
            };

            frameWindow.Show();
            frameWindow.UpdateLayout();

            File.WriteAllLines(@"D:\GitHub\YoYo.Toolkit\documents\frame.txt",
                Services.WindowFrame.Describe(frameWindow));
            Current.Shutdown();
            return;
        }

        var shotPath = GetArgument(e.Args, "--shot");

        // 单独渲染控件用法目录，便于核对片段排版与深浅色
        var viewerType = GetArgument(e.Args, "--viewer");
        if (viewerType is not null && shotPath is not null)
        {
            RunViewerProbe(viewerType, shotPath, e.Args);
            return;
        }

        // 单独渲染一个消息框窗口，便于截图核对样式（正常使用时由 TMessageBox 反射创建）
        var msgLevel = GetArgument(e.Args, "--msg");
        if (msgLevel is not null && shotPath is not null)
        {
            RunMessageProbe(msgLevel, shotPath, e.Args);
            return;
        }

        // 单控件探针：--probe Arc --probe-size 400 --shot xxx.png，只渲染一个控件，便于逐套核对几何
        var probeKind = GetArgument(e.Args, "--probe");
        if (probeKind is not null && shotPath is not null)
        {
            RunProbe(probeKind, shotPath, e.Args);
            return;
        }

        var window = new MainWindow();

        // 截图/回归脚本可以指定落地页：--page ProgressView [--param SN-0001]
        var startPage = GetArgument(e.Args, "--page");
        var startParam = GetArgument(e.Args, "--param");

        if (shotPath is null)
        {
            // --front 3000：置顶显示指定毫秒后自动关闭，用于真实桌面截图（DWM 圆角/阴影只能用屏幕抓图验证）
            if (int.TryParse(GetArgument(e.Args, "--front"), out var frontMs) && frontMs > 0)
            {
                window.WindowStartupLocation = WindowStartupLocation.Manual;
                window.Left = 60;
                window.Top = 60;
                window.Width = 980;
                window.Height = 640;
                window.Topmost = true;
                window.Show();
                if (e.Args.Contains("--maximized", StringComparer.OrdinalIgnoreCase))
                    window.WindowState = WindowState.Maximized;
                if (startPage is not null) window.NavigateTo(startPage);

                var closeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(frontMs) };
                closeTimer.Tick += (_, _) =>
                {
                    closeTimer.Stop();
                    Shutdown();
                };
                closeTimer.Start();
                return;
            }

            window.Show();
            if (startPage is not null) window.NavigateTo(startPage);
            if (startPage is not null && startParam is not null) window.NavigateToWithParam(startPage, startParam);
            return;
        }

        var dark = e.Args.Contains("--dark", StringComparer.OrdinalIgnoreCase);
        var paused = e.Args.Contains("--paused", StringComparer.OrdinalIgnoreCase);
        var delay = int.TryParse(GetArgument(e.Args, "--delay"), out var ms) ? ms : 900;

        // 截图模式：离屏渲染当前帧，不抢焦点，便于自动化回归比对
        window.ShowInTaskbar = false;
        window.WindowStartupLocation = WindowStartupLocation.Manual;
        window.Left = -4000;
        window.Top = 0;

        // 全页长图：临时拉高窗口，让 ScrollViewer 一次性铺开
        if (int.TryParse(GetArgument(e.Args, "--height"), out var height) && height > 0)
        {
            window.Height = height;
        }

        window.Show();

        // 最大化验收：让系统按工作区算尺寸，截图尺寸就是实际铺开的范围
        if (e.Args.Contains("--maximized", StringComparer.OrdinalIgnoreCase))
            window.WindowState = WindowState.Maximized;

        if (startPage is not null) window.NavigateTo(startPage);
        if (startPage is not null && startParam is not null) window.NavigateToWithParam(startPage, startParam);

        if (dark) window.SetDarkTheme(true);

        // 描边三档的命令行注入，便于逐档截图比对
        var stroke = GetArgument(e.Args, "--stroke");
        if (stroke is not null) window.SetStroke(stroke);

        // 确定态边界值 / 步骤下标的命令行注入，便于脚本化验收
        if (double.TryParse(GetArgument(e.Args, "--value"), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var value))
            window.ProgressValue = value;

        if (int.TryParse(GetArgument(e.Args, "--step"), out var step))
            window.CurrentStep = step;

        if (paused)
        {
            // 先让动画跑起来，再统一停住，验证停的是真实运行中的故事板
            var stopTimer = new DispatcherTimer(DispatcherPriority.Normal) { Interval = TimeSpan.FromMilliseconds(delay / 2d) };
            stopTimer.Tick += (_, _) =>
            {
                stopTimer.Stop();
                window.SetAllPaused(true);
            };
            stopTimer.Start();
        }

        var timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle) { Interval = TimeSpan.FromMilliseconds(delay) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            Capture(window, shotPath);
            Shutdown();
        };
        timer.Start();
    }

    /// <summary>
    /// 只渲染控件用法目录窗口并截图，用于核对目录、片段排版与深浅色。
    /// <c>--viewer</c> 的值既是落地页（决定默认选中哪个控件），也是截图文件名前缀。
    /// </summary>
    private static void RunViewerProbe(string viewType, string shotPath, string[] args)
    {
        var delay = int.TryParse(GetArgument(args, "--delay"), out var ms) ? ms : 900;

        if (args.Contains("--dark", StringComparer.OrdinalIgnoreCase))
        {
            Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("Themes/Tokens.Dark.xaml", UriKind.Relative)
            });
        }

        var viewer = new Views.XamlViewerWindow(viewType)
        {
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = -4000,
            Top = 0,
            ShowInTaskbar = false
        };

        viewer.Show();

        var timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle) { Interval = TimeSpan.FromMilliseconds(delay) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            Capture(viewer, shotPath);
            Current.Shutdown();
        };
        timer.Start();
    }

    /// <summary>
    /// 连通性冒烟：分别打本机回环、主机名、IPv6 回环和一个不可达的 TEST-NET 地址。
    /// 重点是「Options 为 null 时不再抛 NullReferenceException」，结果写进 documents/ping.txt。
    /// </summary>
    private static void RunPingProbe()
    {
        var hosts = new[] { "127.0.0.1", "localhost", "::1", "192.0.2.1" };

        // 不能在 UI 线程上 .Result / .GetResult()：PingHost 是 async，await 会切回 Dispatcher，直接死锁
        Task.Run(async () =>
        {
            var lines = new List<string>();

            foreach (var host in hosts)
            {
                try
                {
                    var reply = await YoYo.Toolkit.Net.Pings.NetworkPing.PingHost(host, 1200);
                    lines.Add(reply is null
                        ? $"{host,-14} 不可达（返回 null）"
                        : $"{host,-14} {reply.RoundtripTime,5} ms · TTL {(reply.Options is null ? "无 Options" : reply.Options.Ttl.ToString())} · {reply.Address}");
                }
                catch (Exception ex)
                {
                    lines.Add($"{host,-14} 抛异常 {ex.GetType().Name}: {ex.Message}");
                }
            }

            File.WriteAllLines(@"D:\GitHub\YoYo.Toolkit\documents\ping.txt", lines);
        }).GetAwaiter().OnCompleted(() => Current.Dispatcher.Invoke(() => Current.Shutdown()));
    }

    /// <summary>
    /// 参数化阵列的 CPU 基线：同时跑 6 个自绘阵列控件，测 4 秒内的进程 CPU 占比。
    /// 只计 UI 线程的渲染开销，用于验收"阵列类 CPU &lt; 2%"。
    /// </summary>
    private static void RunCpuProbe()
    {
        var kinds = new[]
        {
            YoYo.Toolkit.Controls.Progress.ProgressKind.Signal,
            YoYo.Toolkit.Controls.Progress.ProgressKind.TickRing,
            YoYo.Toolkit.Controls.Progress.ProgressKind.Wave,
            YoYo.Toolkit.Controls.Progress.ProgressKind.DotMatrix,
            YoYo.Toolkit.Controls.Progress.ProgressKind.Comet,
            YoYo.Toolkit.Controls.Progress.ProgressKind.MarchingRing
        };

        var panel = new System.Windows.Controls.Primitives.UniformGrid { Columns = 3, Rows = 2 };
        foreach (var kind in kinds)
        {
            panel.Children.Add(new YoYo.Toolkit.Controls.Progress.ProgressIndicator
            {
                Kind = kind,
                IsActive = true,
                Width = 140,
                Height = 140,
                Margin = new Thickness(8)
            });
        }

        var host = new Window
        {
            Width = 520,
            Height = 380,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = -4000,
            Top = 0,
            Content = panel
        };

        host.Show();
        host.UpdateLayout();

        var process = System.Diagnostics.Process.GetCurrentProcess();
        var indicators = panel.Children.OfType<YoYo.Toolkit.Controls.Progress.ProgressIndicator>().ToArray();
        var cores = Environment.ProcessorCount;
        var tier = System.Windows.Media.RenderCapability.Tier >> 16;

        var lines = new List<string>
        {
            $"WPF 渲染层级: Tier {tier} ({(tier == 0 ? "软件渲染" : "硬件加速")}), 逻辑核: {cores}",
            $"控件数: {indicators.Length} × 140px, 阵列绘制 (Signal / TickRing / Wave / DotMatrix / Comet / MarchingRing)",
            string.Empty
        };

        // 两段测量：先静止基线，再全速动画，差值即阵列重绘的真实成本
        double Measure(bool active, double seconds, out double cpu, out double wall)
        {
            foreach (var indicator in indicators) indicator.IsActive = active;

            // 让布局与首帧先落定，再开始计时
            Pump(0.35);

            process.Refresh();
            var cpuStart = process.TotalProcessorTime;
            var wallStart = DateTime.UtcNow;

            Pump(seconds);

            process.Refresh();
            cpu = (process.TotalProcessorTime - cpuStart).TotalSeconds;
            wall = (DateTime.UtcNow - wallStart).TotalSeconds;
            return cpu / wall * 100d;
        }

        // 在 UI 线程上推进消息泵，保证动画帧真的在渲染，而不是被 Thread.Sleep 卡住
        static void Pump(double seconds)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            while (watch.Elapsed.TotalSeconds < seconds)
            {
                var frame = new DispatcherFrame();
                var slice = new DispatcherTimer(
                    TimeSpan.FromMilliseconds(10),
                    DispatcherPriority.Background,
                    (_, _) => frame.Continue = false,
                    Dispatcher.CurrentDispatcher);
                slice.Start();
                Dispatcher.PushFrame(frame);
                slice.Stop();
            }
        }

        // 基线
        var idlePercent = Measure(false, 1.5, out var idleCpu, out var idleWall);
        lines.Add($"静止基线 : CPU {idleCpu:F3}s / {idleWall:F2}s = 单核 {idlePercent:F2}%");

        // 动画
        var runPercent = Measure(true, 4d, out var runCpu, out var runWall);
        lines.Add($"动画运行 : CPU {runCpu:F3}s / {runWall:F2}s = 单核 {runPercent:F2}%");

        var deltaPercent = runPercent - idlePercent;
        var perControl = deltaPercent / indicators.Length;
        lines.Add($"动画净开销: 单核 {deltaPercent:F2}%，单控件 {perControl:F2}%");
        lines.Add($"整机占用 : {(idlePercent + deltaPercent) / cores:F2}%");
        lines.Add(string.Empty);
        lines.Add($"验收线(单控件单核 < 2%): {(perControl < 2d ? "PASS" : "FAIL")}");

        File.WriteAllLines(@"D:\GitHub\YoYo.Toolkit\documents\cpu.txt", lines);
        Current.Shutdown();
    }

    /// <summary>
    /// 只渲染一个消息框窗口并截图，用于核对四级样式与深浅令牌。
    /// 正常运行时由 <c>TMessageBox</c> 通过反射创建同一个类型，这里只是绕开模态阻塞。
    /// </summary>
    private static void RunMessageProbe(string levelName, string shotPath, string[] args)
    {
        var level = Enum.TryParse<YoYo.Toolkit.Notifies.TMessageBox.MessageLevel>(levelName, true, out var parsed)
            ? parsed
            : YoYo.Toolkit.Notifies.TMessageBox.MessageLevel.Information;

        var delay = int.TryParse(GetArgument(args, "--delay"), out var ms) ? ms : 800;

        var dialog = new Views.MyMessageDialog(
            level == YoYo.Toolkit.Notifies.TMessageBox.MessageLevel.Error ? "采集失败" : "开始运行",
            level == YoYo.Toolkit.Notifies.TMessageBox.MessageLevel.Question
                ? "确认以当前参数启动采集？"
                : "循环数 60 超出建议区间 35–50，继续可能延长单次运行时长。",
            level)
        {
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = -4000,
            Top = 0,
            ShowInTaskbar = false
        };

        if (args.Contains("--dark", StringComparer.OrdinalIgnoreCase))
        {
            Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("Themes/Tokens.Dark.xaml", UriKind.Relative)
            });
        }

        dialog.Show();

        var timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle) { Interval = TimeSpan.FromMilliseconds(delay) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            Capture(dialog, shotPath);
            Current.Shutdown();
        };
        timer.Start();
    }

    private static void RunProbe(string kindOrKey, string shotPath, string[] args)
    {
        var size = int.TryParse(GetArgument(args, "--probe-size"), out var parsedSize) ? parsedSize : 360;
        var dark = args.Contains("--dark", StringComparer.OrdinalIgnoreCase);
        var delay = int.TryParse(GetArgument(args, "--delay"), out var ms) ? ms : 800;

        var indicator = new YoYo.Toolkit.Controls.Progress.ProgressIndicator
        {
            Width = size,
            Height = size,
            IsActive = !args.Contains("--paused", StringComparer.OrdinalIgnoreCase)
        };

        if (Enum.TryParse<YoYo.Toolkit.Controls.Progress.ProgressKind>(kindOrKey, true, out var kind))
            indicator.Kind = kind;
        else
            indicator.TemplateKey = kindOrKey;

        if (double.TryParse(GetArgument(args, "--value"), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var value))
        {
            indicator.IsIndeterminate = false;
            indicator.Value = value;
        }

        var host = new Window
        {
            Width = size + 40,
            Height = size + 40,
            Background = System.Windows.Media.Brushes.White,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = -4000,
            Top = 0,
            Content = indicator
        };

        if (dark)
        {
            host.Background = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#0D1011"));
            Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("/YoYo.Toolkit;component/Themes/Progress.Tokens.Dark.xaml", UriKind.Relative)
            });
        }

        host.Show();

        var timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle) { Interval = TimeSpan.FromMilliseconds(delay) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            Capture(host, shotPath);
            Current.Shutdown();
        };
        timer.Start();
    }

    private static void RunDiagnostics()
    {
        var logPath = @"D:\GitHub\YoYo.Toolkit\documents\diag.txt";
        var report = new System.Text.StringBuilder();
        var current = "?";

        void Flush() => File.WriteAllText(logPath, report.ToString());

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            report.AppendLine($"{current}: FATAL {args.ExceptionObject.GetType().Name} - {args.ExceptionObject}");
            Flush();
        };

        Current.DispatcherUnhandledException += (_, args) =>
        {
            report.AppendLine($"{current}: FAIL {args.Exception.GetType().Name} - {args.Exception.Message}");
            args.Handled = true;
            Flush();
        };

        var host = new Window
        {
            Width = 240,
            Height = 240,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = -4000,
            Top = 0
        };

        foreach (YoYo.Toolkit.Controls.Progress.ProgressKind kind in Enum.GetValues(typeof(YoYo.Toolkit.Controls.Progress.ProgressKind)))
        {
            current = kind.ToString();
            var before = report.Length;

            var indicator = new YoYo.Toolkit.Controls.Progress.ProgressIndicator
            {
                Kind = kind,
                IsActive = true,
                Width = 120,
                Height = 120
            };

            host.Content = indicator;

            try
            {
                host.Show();
                host.UpdateLayout();
                indicator.UpdateLayout();
            }
            catch (Exception ex)
            {
                report.AppendLine($"{current}: THROW {ex.GetType().Name} - {ex.Message}");
            }

            if (report.Length == before)
                report.AppendLine($"{current}: OK");

            Flush();

            try
            {
                host.Hide();
                host.Content = null;
            }
            catch
            {
                // 忽略清理阶段的异常
            }
        }

        Flush();
    }

    private static string? GetArgument(string[] args, string name)
    {
        var index = Array.FindIndex(args, a => string.Equals(a, name, StringComparison.OrdinalIgnoreCase));
        return index >= 0 && index + 1 < args.Length && !args[index + 1].StartsWith("--", StringComparison.Ordinal)
            ? args[index + 1]
            : null;
    }

    private static void Capture(Window window, string path)
    {
        window.UpdateLayout();

        var dpi = VisualTreeHelper.GetDpi(window);
        var width = (int)Math.Ceiling(window.ActualWidth * dpi.DpiScaleX);
        var height = (int)Math.Ceiling(window.ActualHeight * dpi.DpiScaleY);

        if (width <= 0 || height <= 0) return;

        var bitmap = new RenderTargetBitmap(width, height, 96 * dpi.DpiScaleX, 96 * dpi.DpiScaleY, PixelFormats.Pbgra32);
        bitmap.Render(window);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

        using var stream = File.Create(path);
        encoder.Save(stream);
    }
}
