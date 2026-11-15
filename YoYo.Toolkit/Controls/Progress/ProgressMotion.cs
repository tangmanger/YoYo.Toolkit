namespace YoYo.Toolkit.Controls.Progress;

/// <summary>
/// 动效强度。用于无障碍降级：<see cref="Auto"/> 跟随系统“在 Windows 中显示动画”设置。
/// </summary>
public enum ProgressMotion
{
    /// <summary>跟随 <c>SystemParameters.ClientAreaAnimation</c>。</summary>
    Auto = 0,

    /// <summary>始终播放完整动效。</summary>
    Full,

    /// <summary>降级为静止帧，不启动故事板。模板未实现 <c>RunningReduced</c> 状态时停在首帧。</summary>
    Reduced
}
