using System.ComponentModel;

namespace YoYo.Toolkit.Controls.Progress;

/// <summary>
/// 进度指示器的视觉语言。对应设计稿 04–24 中的动效编号（04 由 <see cref="StepTracker"/> 承载）。
/// 每个值对应一个资源键 <c>Progress.Template.{Kind}.{Indeterminate|Determinate}</c>，
/// 宿主可在自己的资源中定义同名模板覆盖，无需改动控件。
/// </summary>
public enum ProgressKind
{
    /// <summary>08 · 旋转弧环。单段弧绕轴匀速旋转，最通用的等待反馈。</summary>
    [Description("旋转弧环")]
    Arc = 0,

    /// <summary>07 · 流动扫光条。一段渐变光带在线性轨道上掠过。</summary>
    [Description("流动扫光条")]
    FlowBar,

    /// <summary>09 · 三点脉动。三个圆点按固定时差依次缩放。</summary>
    [Description("三点脉动")]
    BounceDots,

    /// <summary>10 · 声纳波纹。中心的点向外扩散出同心圆环。</summary>
    [Description("声纳波纹")]
    Sonar,

    /// <summary>11 · 骨架扫光。内容占位块的扫光，用于列表/报表加载。</summary>
    [Description("骨架扫光")]
    Skeleton,

    /// <summary>12 · 行进虚线环。虚线描边整体旋转，像行军蚁。</summary>
    [Description("行进虚线环")]
    MarchingRing,

    /// <summary>13 · 对向游走。两段色带在轨道上反向对流。</summary>
    [Description("对向游走")]
    CounterFlow,

    /// <summary>14 · 打字省略号。文案后三点依次点亮。</summary>
    [Description("打字省略号")]
    Typing,

    /// <summary>15 · 刻度游标环。一圈静态刻度 + 一个高亮指针旋转。</summary>
    [Description("刻度游标环")]
    TickRing,

    /// <summary>16 · 行波线。连续正弦波整体平移。</summary>
    [Description("行波线")]
    Wave,

    /// <summary>17 · 点阵涟漪。点阵按到中心的距离错相位收缩。</summary>
    [Description("点阵涟漪")]
    DotMatrix,

    /// <summary>18 · 雷达扫描臂。扇形光幕绕中心旋转。</summary>
    [Description("雷达扫描臂")]
    Radar,

    /// <summary>19 · 对流双环。双色弧环反向旋转。</summary>
    [Description("对流双环")]
    DualRing,

    /// <summary>20 · 流星尾迹。带头部与渐隐拖尾的圆周运动。</summary>
    [Description("流星尾迹")]
    Comet,

    /// <summary>21 · 呼吸光球。径向渐变球体柔和涨落。</summary>
    [Description("呼吸光球")]
    Breathing,

    /// <summary>22 · 屏幕扫描。发光扫描线在矩形区域内上下移动。</summary>
    [Description("屏幕扫描")]
    Scan,

    /// <summary>23 · 花瓣绽放。五瓣绕轴旋转并逐瓣缩放。</summary>
    [Description("花瓣绽放")]
    Petal,

    /// <summary>24 · 流光环。彗尾式渐变弧沿环轨道运行。</summary>
    [Description("流光环")]
    GlowRing,

    /// <summary>05 · 信号带。多根柱体以错开相位起伏。</summary>
    [Description("信号带")]
    Signal,

    /// <summary>06 · 轨道点。多个点以不同半径与速度绕中心运行。</summary>
    [Description("轨道点")]
    Orbit
}
