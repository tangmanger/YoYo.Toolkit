namespace YoYo.Toolkit.Controls.Progress;

/// <summary>
/// <see cref="ProgressPainter"/> 的绘制模式。这些动效的元素数量或相位由参数决定，
/// 无法用静态 XAML 表达，统一收敛到一个自绘元素 + 一个 <c>Phase</c> 动画契约。
/// </summary>
public enum ProgressPainterPattern
{
    /// <summary>05 · 信号带：多根柱体以错开相位起伏。</summary>
    Signal = 0,

    /// <summary>15 · 刻度游标环：一圈静态刻度 + 一个高亮指针。</summary>
    Ticks,

    /// <summary>16 · 行波线：连续正弦波整体平移。</summary>
    Wave,

    /// <summary>17 · 点阵涟漪：点阵按到中心的距离错相位收缩。</summary>
    Matrix,

    /// <summary>20 · 流星尾迹：头部带渐隐拖尾的圆周运动。</summary>
    Comet,

    /// <summary>确定态刻度环：按 Phase（此处即百分比）点亮前 N 条刻度，不做循环动画。</summary>
    TicksProgress,

    /// <summary>10 · 声纳波纹：两圈同心环按相位扩散并淡出。</summary>
    Sonar,

    /// <summary>21 · 呼吸光球：球体半径与亮度随相位起伏。</summary>
    Breathing
}
