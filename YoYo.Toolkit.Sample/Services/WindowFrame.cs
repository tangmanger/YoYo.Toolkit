using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Shell;

namespace YoYo.ToolkitSample.Services;

/// <summary>
/// 窗口外框助手：圆角与阴影一律交给系统（DWM），不再用 WPF 的 Effect / 透明窗口伪造。
/// <para>
/// 约定：
/// 1. 窗口用 <see cref="WindowChrome"/> 保留系统的拖拽/缩放/贴边能力，<c>AllowsTransparency</c> 必须为 false；
/// 2. Win11（build 22000+）用 DWMWA_WINDOW_CORNER_PREFERENCE 做真正的抗锯齿圆角，阴影由 DWM 合成；
/// 3. Win10 没有该属性，退回 <see cref="WindowChrome.CornerRadius"/> 的区域裁剪（边缘略硬，但形状正确）；
/// 4. 想让 <c>DropShadowEffect</c> 出效果时，只允许挂在一个「没有子元素、只负责画阴影」的 Border 上，
///    绝不挂在承载文字的 Border/Grid 上——effect 会把整棵子树先渲染到中间位图，文字必然发虚。
/// </para>
/// </summary>
internal static class WindowFrame
{
    private const int DwmwaUseImmersiveDarkMode = 20;
    private const int DwmwaWindowCornerPreference = 33;
    private const int DwmwaBorderColor = 34;

    private const int DwmwcpRound = 2;

    /// <summary>系统边框颜色哨兵值：表示"不画边框"。</summary>
    private static readonly int BorderColorNone = unchecked((int)0xFFFFFFFE);

    /// <summary>Win11 及以上才支持 DWM 圆角偏好。</summary>
    public static bool HasDwmRoundedCorners => Environment.OSVersion.Version.Build >= 22000;

    /// <summary>
    /// 挂到窗口上：句柄就绪时设置圆角与边框，并按当前主题同步系统深色模式。
    /// </summary>
    /// <param name="window">目标窗口（<c>WindowStyle=None</c> + <see cref="WindowChrome"/>）。</param>
    /// <param name="chrome">窗口自己的 WindowChrome 实例，Win10 下用它兜底圆角。</param>
    /// <param name="isDark">读取当前主题，用于系统侧深色模式。</param>
    public static void Attach(Window window, WindowChrome chrome, Func<bool> isDark)
    {
        window.SourceInitialized += (_, _) =>
        {
            var handle = new WindowInteropHelper(window).Handle;

            if (HasDwmRoundedCorners)
            {
                SetInt(handle, DwmwaWindowCornerPreference, DwmwcpRound);
                SetValue(handle, DwmwaBorderColor, BorderColorNone);
            }
            else
            {
                chrome.CornerRadius = new CornerRadius(8);
            }

            SetDarkMode(window, isDark());
        };
    }

    /// <summary>同步系统深色模式（影响窗口阴影/贴边预览等系统绘制部分）。</summary>
    public static void SetDarkMode(Window window, bool dark)
    {
        if (!HasDwmRoundedCorners) return;

        var handle = new WindowInteropHelper(window).Handle;
        if (handle != IntPtr.Zero) SetInt(handle, DwmwaUseImmersiveDarkMode, dark ? 1 : 0);
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    /// <summary>
    /// 读回窗口的 DWM 实际状态，供命令行验收（<c>--frame</c>）核对圆角/阴影前提条件。
    /// </summary>
    public static System.Collections.Generic.IEnumerable<string> Describe(Window window)
    {
        var handle = new WindowInteropHelper(window).Handle;
        var lines = new System.Collections.Generic.List<string>();

        lines.Add($"系统: {Environment.OSVersion.Version} (build {Environment.OSVersion.Version.Build}), 支持 DWM 圆角: {HasDwmRoundedCorners}");

        var corner = 0;
        var result = DwmGetWindowAttribute(handle, DwmwaWindowCornerPreference, out corner, sizeof(int));
        lines.Add($"DWMWA_WINDOW_CORNER_PREFERENCE: hr=0x{result:X8} value={corner} ({(corner == DwmwcpRound ? "DWMWCP_ROUND" : "其它")})");

        var cloaked = 0;
        _ = DwmGetWindowAttribute(handle, 14, out cloaked, sizeof(int));
        lines.Add($"DWMWA_CLOAKED: {cloaked}");

        lines.Add($"WS_EX_LAYERED: {Layered(handle)}（分层窗口没有系统阴影）");
        lines.Add($"WS_THICKFRAME: {ThickFrame(handle)}（保留系统可缩放边框，DWM 才会绘制窗口阴影）");

        return lines;
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmGetWindowAttribute(IntPtr hwnd, int attribute, out int value, int size);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hwnd, int index);

    private static bool Layered(IntPtr handle) => (GetWindowLong(handle, -20) & 0x00080000) != 0;

    private static bool ThickFrame(IntPtr handle) => (GetWindowLong(handle, -16) & 0x00040000) != 0;

    private static void SetInt(IntPtr handle, int attribute, int value) => SetValue(handle, attribute, value);

    private static void SetValue(IntPtr handle, int attribute, int value)
    {
        // 旧系统上没有这个属性，调用会失败；失败无所谓，保持系统默认外观即可
        _ = DwmSetWindowAttribute(handle, attribute, ref value, sizeof(int));
    }
}
